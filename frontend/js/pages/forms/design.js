import { api } from '../../shared/api.js';
const pageTitle = document.getElementById('design-page-title');
const pageSubtitle = document.getElementById('design-page-subtitle');
const inputFormName = document.getElementById('formName');
const inputTargetTableName = document.getElementById('targetTableName');
const inputTargetPrimaryKey = document.getElementById('targetPrimaryKey');
const inputViewName = document.getElementById('viewName');
const btnSaveForm = document.getElementById('btn-save-form');
const errorAlert = document.getElementById('form-error-alert');
const urlParams = new URLSearchParams(window.location.search);
const groupCode = urlParams.get('groupCode');
const formId = urlParams.get('formId');
let formioBuilderInstance = null;
let currentFormGroupCode = groupCode; 
async function initDesignScreen() {
    formioBuilderInstance = await Formio.builder(document.getElementById('builder'), {}, {
        language: 'tr',
        noDefaultSubmitButton: true
    });
    if (formId) {
        try {
            const formData = await api.get(`/Form/${formId}`);
            currentFormGroupCode = formData.formGroupCode;
            pageTitle.textContent = "Formu Düzenle";
            pageSubtitle.textContent = `Form Grup Kodu: ${currentFormGroupCode}`;
            inputFormName.value = formData.formName || '';
            inputTargetTableName.value = formData.targetTableName || '';
            inputTargetPrimaryKey.value = formData.targetPrimaryKey || '';
            inputViewName.value = formData.viewName || '';
            const formSchemaString = formData.formSchema;
            if (formSchemaString) {
                const schemaJson = JSON.parse(formSchemaString);
                formioBuilderInstance.setForm(schemaJson);
            }
        } catch (error) {
            showError("Form verileri yüklenirken hata oluştu: " + error.message);
            btnSaveForm.disabled = true;
        }
    } else if (groupCode) {
        pageTitle.textContent = "Form Tasarımı";
        pageSubtitle.textContent = `Grup Kodu: ${groupCode}`;
    } else {
        showError("Geçersiz giriş: groupCode veya formId parametresi bulunamadı.");
        btnSaveForm.disabled = true;
    }
}
function showError(msg) {
    errorAlert.textContent = msg;
    errorAlert.classList.remove('d-none');
}
btnSaveForm.addEventListener('click', async () => {
    errorAlert.classList.add('d-none');
    const formName = inputFormName.value.trim();
    
    if (!formName) {
        showError("Lütfen Form Adını giriniz.");
        return;
    }
    const currentSchema = formioBuilderInstance.schema;
    const schemaString = JSON.stringify(currentSchema);
    const payload = {
        formName: formName,
        targetTableName: inputTargetTableName.value.trim() || null,
        targetPrimaryKey: inputTargetPrimaryKey.value.trim() || null,
        viewName: inputViewName.value.trim() || null,
        formSchema: schemaString
    };
    try {
        btnSaveForm.disabled = true;
        btnSaveForm.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Kaydediliyor...';
        if (formId) {
            await api.patch(`/Form/${formId}`, payload);
            alert("Form başarıyla güncellendi!");
        } else {
            payload.formGroupCode = currentFormGroupCode;
            await api.post('/Form', payload);
            alert("Yeni form başarıyla oluşturuldu!");
        }
        try {
            const newMenus = await api.get('/Menu/me');
            if (sessionStorage.getItem('user_menus')) sessionStorage.setItem('user_menus', JSON.stringify(newMenus));
            if (localStorage.getItem('user_menus')) localStorage.setItem('user_menus', JSON.stringify(newMenus));
            if (window.refreshSidebarMenu) window.refreshSidebarMenu();
        } catch (e) {
            console.error("Menü yenilenemedi:", e);
        }
        window.history.back();
    } catch (error) {
        showError("Kaydetme işlemi başarısız: " + error.message);
    } finally {
        btnSaveForm.disabled = false;
        btnSaveForm.innerHTML = '<i class="bi bi-save me-1"></i> Formu Kaydet';
    }
});
document.addEventListener('DOMContentLoaded', initDesignScreen);