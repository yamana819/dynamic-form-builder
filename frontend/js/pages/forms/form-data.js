import { api } from "../../shared/api.js";

const params = new URLSearchParams(window.location.search);
const formId = params.get('formId');

const formTitle = document.getElementById('form-title');
const formSubtitle = document.getElementById('form-subtitle');
const btnAddRecord = document.getElementById('btn-add-record');
const recordsTheadTr = document.getElementById('records-thead-tr');
const recordsTbody = document.getElementById('records-tbody');

const deleteConfirmModal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
const btnConfirmDelete = document.getElementById('btn-confirm-delete');

let currentForm = null;
let parsedSchemaObj = {};
let currentSchema = [];
let recordsList = [];
let recordsColumns = [];
let currentDeleteId = null;

async function init() {
    if (!formId) {
        showError("Geçersiz form ID.");
        return;
    }

    try {
        currentForm = await api.get(`/Form/${formId}`);
        formTitle.textContent = currentForm.formName;
        formSubtitle.textContent = `Toplam Kayıt: Yükleniyor...`;
        
        parsedSchemaObj = JSON.parse(currentForm.formSchema || "{}");
        currentSchema = parsedSchemaObj.components || [];
        
        // Yayımlanma kontrolü
        if (!currentForm.isPublished) {
            recordsTheadTr.parentElement.classList.add('d-none');
            showUnpublishedMessage();
            return; 
        }

        // View kontrolü
        if (!currentForm.viewName) {
            recordsTheadTr.parentElement.classList.add('d-none');
            showNoViewMessage();
            btnAddRecord.classList.remove('d-none'); // View yoksa bile Ana Tabloya kayıt eklenebilir
            return;
        }

        await loadRecords();
        btnAddRecord.classList.remove('d-none');
    } catch (error) {
        console.error(error);
        showError("Form bilgileri yüklenemedi. " + (error.message || ""));
    }
}

function showNoViewMessage() {
    formSubtitle.textContent = "Durum: View (Görünüm) Eksik";
    
    recordsTbody.innerHTML = `
        <tr>
            <td colspan="100%" class="text-center py-5 border-0">
                <div class="d-flex flex-column align-items-center justify-content-center py-4">
                    <div class="bg-light rounded-circle d-flex align-items-center justify-content-center mb-3" style="width: 80px; height: 80px;">
                        <i class="bi bi-table text-warning" style="font-size: 2.5rem;"></i>
                    </div>
                    <h4 class="fw-bold text-dark mb-2">Görünüm (View) Oluşturulmamış</h4>
                    <p class="text-muted text-center mx-auto" style="max-width: 450px;">
                        Bu form yayımlanmış ancak kayıtların listeleneceği veritabanı görünümü (View) henüz tanımlanmamış. Lütfen form düzenleme ekranından (Gelişmiş sekmesi) bir View adı girin.
                    </p>
                </div>
            </td>
        </tr>
    `;
}

function renderTableHeader() {
    recordsTheadTr.innerHTML = '';
    
    // Eğer View kolonları (başlıklar) yoksa thead'i tamamen gizle
    if (!recordsColumns || recordsColumns.length === 0) {
        recordsTheadTr.parentElement.classList.add('d-none');
        return;
    }
    
    recordsTheadTr.parentElement.classList.remove('d-none');

    const thPs4 = document.createElement('th');
    thPs4.className = "ps-4";
    thPs4.textContent = "#";
    thPs4.style.width = "50px";
    recordsTheadTr.appendChild(thPs4);

    // Backend'den dönen kolon isimlerini kullan (ilk 5 tanesi)
    const displayFields = recordsColumns.slice(0, 5);
    
    displayFields.forEach(key => {
        const th = document.createElement('th');
        th.textContent = key; // Direkt View'daki kolon adı (alias dahil) yazılacak
        recordsTheadTr.appendChild(th);
    });

    const thEnd = document.createElement('th');
    thEnd.className = "text-end pe-4";
    thEnd.textContent = "Detay";
    recordsTheadTr.appendChild(thEnd);
}

function showUnpublishedMessage() {
    formSubtitle.textContent = "Durum: Taslak (Yayımlanmadı)";
    
    recordsTbody.innerHTML = `
        <tr>
            <td colspan="100%" class="text-center py-5 border-0">
                <div class="d-flex flex-column align-items-center justify-content-center py-4">
                    <div class="bg-light rounded-circle d-flex align-items-center justify-content-center mb-3" style="width: 80px; height: 80px;">
                        <i class="bi bi-file-earmark-lock text-secondary" style="font-size: 2.5rem;"></i>
                    </div>
                    <h4 class="fw-bold text-dark mb-2">Form Henüz Yayımlanmadı</h4>
                    <p class="text-muted text-center mx-auto" style="max-width: 450px;">
                        Bu form şu anda taslak aşamasındadır. Veri girişi ve listeleme yapabilmek için lütfen form yönetimi ekranından formu <strong>yayımlayın</strong>.
                    </p>
                </div>
            </td>
        </tr>
    `;
}

async function loadRecords() {
    try {
        recordsTbody.innerHTML = `<tr><td colspan="100%" class="text-center py-5 text-muted"><div class="spinner-border spinner-border-sm text-primary me-2"></div>Kayıtlar yükleniyor...</td></tr>`;
        
        const data = await api.get(`/Record/${formId}`);
        // Yeni C# servisi bize { columns: [...], rows: [...] } şeklinde bir obje dönecek
        recordsList = data?.rows || [];
        recordsColumns = data?.columns || [];
        
        formSubtitle.textContent = `Toplam Kayıt: ${recordsList.length}`;
        
        renderTableHeader(); // Kayıtlar yüklendiğinde başlıkları çiz (veya gizle)
        renderTableBody();
    } catch (error) {
        console.error(error);
        recordsTbody.innerHTML = `<tr><td colspan="100%" class="text-center py-5 text-danger"><i class="bi bi-exclamation-triangle me-2"></i>Kayıtlar yüklenemedi.</td></tr>`;
    }
}

function renderTableBody() {
    recordsTbody.innerHTML = '';
    
    if (recordsList.length === 0) {
        recordsTbody.innerHTML = `<tr><td colspan="100%" class="text-center py-5 text-muted">Henüz hiç kayıt eklenmemiş.</td></tr>`;
        return;
    }

    // Başlıklarla uyuşması için view kolonlarını al
    const displayFields = recordsColumns.slice(0, 5);

    recordsList.forEach((record, index) => {
        const tr = document.createElement('tr');
        tr.className = "clickable-row";
        tr.addEventListener('click', () => openModalForEdit(record));

        const tdIndex = document.createElement('td');
        tdIndex.className = "ps-4 text-muted fw-semibold";
        tdIndex.textContent = index + 1;
        tr.appendChild(tdIndex);

        displayFields.forEach(field => {
            const td = document.createElement('td');
            const fieldKey = field; // field zaten string, field.key değil
            
            // Backend'den gelen JSON'da (Dictionary serialize edilirken) key'lerin casing'i değişmiş olabilir,
            // bu yüzden record objesinin property'lerinde case-insensitive eşleşme arıyoruz:
            const actualKey = Object.keys(record).find(k => k.toLowerCase() === fieldKey.toLowerCase());
            const val = record[actualKey || fieldKey];
            
            let displayVal = val;
            if (val !== null && typeof val === 'object') {
                displayVal = JSON.stringify(val);
            }

            const div = document.createElement('div');
            div.className = "text-truncate-custom";
            div.textContent = (displayVal !== null && displayVal !== undefined) ? displayVal : '-';
            td.appendChild(div);
            tr.appendChild(td);
        });

        const tdEnd = document.createElement('td');
        tdEnd.className = "text-end pe-4 text-primary";
        tdEnd.innerHTML = '<i class="bi bi-chevron-right"></i>';
        tr.appendChild(tdEnd);

        recordsTbody.appendChild(tr);
    });
}

function openModalForAdd() {
    window.location.href = `/frontend/pages/forms/record-edit.html?formId=${formId}`;
}

async function openModalForEdit(record) {
    const pkField = currentForm.targetPrimaryKey || 'id';
    
    // JSON property isminde camelCase dönüşümüne falan uğramış olma ihtimaline karşı case-insensitive arıyoruz:
    const actualPkKey = Object.keys(record).find(k => k.toLowerCase() === pkField.toLowerCase());
    const currentEditId = record[actualPkKey || pkField];
    
    if (!currentEditId) {
        showError("Bu kaydın Primary Key (ID) değeri bulunamadı.");
        return;
    }

    window.location.href = `/frontend/pages/forms/record-edit.html?formId=${formId}&recordId=${currentEditId}`;
}

function showError(message) {
    recordsTbody.innerHTML = `<tr><td colspan="100%" class="text-center py-5 text-danger">${message}</td></tr>`;
}

async function handleDelete() {
    if (!currentDeleteId) return;
    
    deleteConfirmModal.hide();
    
    try {
        await api.delete(`/Record/${formId}/records/${currentDeleteId}`);
        await loadRecords();
    } catch (error) {
        showError(error?.message || "Kayıt silinemedi.");
    }
}

// Bu dosyada silme işlemi tablodan yapılmıyor, ancak modal silme onayı için ekledik:
window.triggerDelete = (id) => {
    currentDeleteId = id;
    deleteConfirmModal.show();
};

btnAddRecord.addEventListener('click', openModalForAdd);
btnConfirmDelete.addEventListener('click', handleDelete);

document.addEventListener('DOMContentLoaded', init);