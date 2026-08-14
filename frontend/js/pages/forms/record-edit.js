import { api } from "../../shared/api.js";

const params = new URLSearchParams(window.location.search);
const formId = params.get('formId');
const recordId = params.get('recordId'); // Null ise Yeni Kayıt modudur

const pageTitle = document.getElementById('page-title');
const pageSubtitle = document.getElementById('page-subtitle');
const btnBack = document.getElementById('btn-back');
const dynamicFormContainer = document.getElementById('dynamic-form-container');
const btnSaveRecord = document.getElementById('btn-save-record');
const btnDeleteRecord = document.getElementById('btn-delete-record');
const recordError = document.getElementById('record-error');

const deleteConfirmModal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
const btnConfirmDelete = document.getElementById('btn-confirm-delete');

let currentForm = null;
let parsedSchemaObj = {};
let formioInstance = null;
let formPermissions = { canView: false, canCreate: false, canEdit: false, canDelete: false };

const pageLoader = document.getElementById('page-loader');
const loaderText = document.getElementById('loader-text');
const formContentArea = document.getElementById('form-content-area');

async function init() {
    if (!formId) {
        showError("Geçersiz form ID.");
        pageLoader.classList.add('d-none');
        return;
    }

    try {
        // Form şemasını al
        currentForm = await api.get(`/Form/${formId}`);
        pageSubtitle.textContent = `Form: ${currentForm.formName}`;
        parsedSchemaObj = JSON.parse(currentForm.formSchema || "{}");
        
        formPermissions = window.getUserPermissions(`/forms/${currentForm.formGroupCode}/${formId}`);

        if (recordId) {
            // Düzenleme Modu
            pageTitle.textContent = "Kaydı Görüntüle / Düzenle";
            
            if (formPermissions.canDelete) {
                btnDeleteRecord.classList.remove('d-none');
            } else {
                btnDeleteRecord.classList.add('d-none');
            }
            
            loaderText.textContent = "Veritabanından Kayıt Çekiliyor...";
            
            // Backend'den en taze kaydı çek
            const rawRecordData = await api.get(`/Record/${formId}/records/${recordId}`);
            loaderText.textContent = "Form Oluşturuluyor...";
            await renderDynamicForm(rawRecordData);
        } else {
            // Ekleme Modu
            pageTitle.textContent = "Yeni Kayıt Ekle";
            loaderText.textContent = "Form Oluşturuluyor...";
            await renderDynamicForm(null);
        }
        
        // Her şey bitince loader'ı gizle ve formu yavaşça göster
        pageLoader.classList.add('d-none');
        formContentArea.style.opacity = '1';
        
    } catch (error) {
        console.error(error);
        pageLoader.classList.add('d-none');
        showError("Bilgiler yüklenirken bir hata oluştu: " + (error.message || ""));
        btnSaveRecord.disabled = true;
    }
}

async function renderDynamicForm(recordData = null) {
    if (formioInstance) {
        formioInstance.destroy(); 
    }

    let isReadOnly = false;
    if (recordId && !formPermissions.canEdit) {
        isReadOnly = true;
        btnSaveRecord.classList.add('d-none'); // Kaydet butonunu gizle
    } else if (!recordId && !formPermissions.canCreate) {
        isReadOnly = true;
        btnSaveRecord.classList.add('d-none');
    } else {
        btnSaveRecord.classList.remove('d-none');
    }

    try {
        dynamicFormContainer.innerHTML = '';
        formioInstance = await Formio.createForm(dynamicFormContainer, parsedSchemaObj, {
            language: 'tr',
            noAlerts: true,
            readOnly: isReadOnly
        });

        if (recordData) {
            formioInstance.submission = { data: recordData };
        }
    } catch (err) {
        console.error("Formio oluşturulamadı:", err);
        dynamicFormContainer.innerHTML = '<div class="alert alert-danger">Form oluşturulurken bir hata meydana geldi.</div>';
    }
}

function showError(message) {
    recordError.textContent = message;
    recordError.classList.remove('d-none');
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function hideError() {
    recordError.classList.add('d-none');
    recordError.textContent = '';
}

async function handleSave() {
    try {
        if (!formioInstance) return;

        try {
            await formioInstance.submit();
        } catch (validationError) {
            showError("Lütfen formdaki zorunlu alanları doldurun veya hataları düzeltin.");
            return;
        }
        
        hideError();
        const btnOriginalText = btnSaveRecord.innerHTML;
        btnSaveRecord.disabled = true;
        btnSaveRecord.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Kaydediliyor...';
        
        try {
            const dataObj = formioInstance.submission?.data || formioInstance._data || {};
            if (dataObj.submit !== undefined) delete dataObj.submit;
            
            if (recordId) {
                // Düzenleme güncellemesi (PATCH)
                await api.patch(`/Record/${formId}/records/${recordId}`, dataObj);
                alert("Kayıt başarıyla güncellendi!");
            } else {
                // Yeni kayıt (POST)
                await api.post(`/Record/${formId}`, dataObj);
                alert("Yeni kayıt başarıyla eklendi!");
            }
            
            // Başarılı olunca listeye dön
            goBack();
        } catch (error) {
            showError(error?.message || "Kayıt işlemi başarısız oldu.");
        } finally {
            btnSaveRecord.disabled = false;
            btnSaveRecord.innerHTML = btnOriginalText;
        }
        
    } catch (fatalError) {
        console.error("KRİTİK HATA:", fatalError);
        alert("Yazılımsal bir hata oluştu: " + fatalError.message);
    }
}

async function handleDelete() {
    if (!recordId) return;
    
    deleteConfirmModal.hide();
    btnDeleteRecord.disabled = true;
    
    try {
        await api.delete(`/Record/${formId}/records/${recordId}`);
        alert("Kayıt başarıyla silindi!");
        goBack();
    } catch (error) {
        showError(error?.message || "Kayıt silinemedi.");
        btnDeleteRecord.disabled = false;
    }
}

function goBack() {
    window.location.href = `/frontend/pages/forms/form-data.html?formId=${formId}`;
}

btnBack.addEventListener('click', goBack);
btnSaveRecord.addEventListener('click', handleSave);

btnDeleteRecord.addEventListener('click', () => {
    deleteConfirmModal.show();
});

btnConfirmDelete.addEventListener('click', handleDelete);

document.addEventListener('DOMContentLoaded', init);
