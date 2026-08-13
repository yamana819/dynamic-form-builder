import { api } from '../shared/api.js';

const tbody = document.getElementById('form-groups-tbody');
const btnAddGroup = document.getElementById('btn-add-group');

const addGroupModalEl = document.getElementById('addGroupModal');
const addGroupModal = new bootstrap.Modal(addGroupModalEl);
const modalTitle = document.getElementById('addGroupModalLabel');
const btnSaveGroup = document.getElementById('btn-save-group');
const inputGroupName = document.getElementById('groupName');
const inputGroupCode = document.getElementById('groupCode');
const errorAlert = document.getElementById('add-group-error');

const formsModalEl = document.getElementById('formsModal');
const formsModal = new bootstrap.Modal(formsModalEl);
const formsModalTitle = document.getElementById('formsModalLabel');
const formsModalGroupCode = document.getElementById('formsModalGroupCode');
const formsTbody = document.getElementById('forms-tbody');
const btnAddForm = document.getElementById('btn-add-form');

let currentActiveGroupCode = null; 
let editingGroupCode = null;

async function refreshUserMenus() {
    try {
        const newMenus = await api.get('/Menu/me');
        if (sessionStorage.getItem('user_menus')) {
            sessionStorage.setItem('user_menus', JSON.stringify(newMenus));
        }
        if (localStorage.getItem('user_menus')) {
            localStorage.setItem('user_menus', JSON.stringify(newMenus));
        }
    } catch (error) {
        console.error("Menü yetkileri güncellenirken hata oluştu:", error);
    }
}

async function loadFormGroups() {
    try {
        tbody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-5 text-muted">
                    <div class="spinner-border text-primary mb-2" role="status"></div>
                    <div>Form grupları yükleniyor...</div>
                </td>
            </tr>
        `;
        const data = await api.get('/FormGroup?pageNumber=1&pageSize=50');
        
        tbody.innerHTML = ''; 

        if (!data || data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="3" class="text-center py-4 text-muted">Henüz form grubu bulunmuyor.</td></tr>`;
            return;
        }

        // Admin ekranı olduğu için "Yeni Grup Ekle" butonunu direkt gösteriyoruz
        btnAddGroup.classList.remove('d-none');

        data.forEach(group => {
            const actionButtons = `
                <button class="btn btn-sm btn-outline-primary me-2" onclick="openFormsModal('${group.formGroupCode}', '${group.formGroupName}')" title="Formları İncele">
                    <i class="bi bi-folder2-open"></i> Formlar
                </button>
                <button class="btn btn-sm btn-outline-warning me-2" onclick="editGroup('${group.formGroupCode}')" title="Grubu Düzenle">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-outline-danger" onclick="deleteGroup('${group.formGroupCode}')" title="Grubu Sil">
                    <i class="bi bi-trash"></i>
                </button>
            `;

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="ps-4 fw-medium text-dark">${group.formGroupName}</td>
                <td><span class="badge bg-secondary">${group.formGroupCode}</span></td>
                <td class="text-end pe-4">${actionButtons}</td>
            `;
            tbody.appendChild(tr);
        });

    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="3" class="text-center py-4 text-danger">Veriler yüklenirken bir hata oluştu.</td></tr>`;
        console.error("Gruplar çekilemedi:", error);
    }
}

btnAddGroup.addEventListener('click', () => {
    editingGroupCode = null; 
    modalTitle.textContent = "Yeni Form Grubu Ekle"; 
    document.getElementById('add-group-form').reset();
    errorAlert.classList.add('d-none');
    
    inputGroupCode.readOnly = false;
    inputGroupCode.classList.remove('bg-light');
    
    addGroupModal.show();
});

window.editGroup = async (formGroupCode) => {
    editingGroupCode = formGroupCode; 
    modalTitle.textContent = "Form Grubunu Düzenle";
    errorAlert.classList.add('d-none');
    document.getElementById('add-group-form').reset();

    try {
        inputGroupName.disabled = true;
        inputGroupCode.disabled = true;
        btnSaveGroup.disabled = true;
        
        addGroupModal.show();

        const groupData = await api.get(`/FormGroup/${formGroupCode}`);

        inputGroupName.value = groupData.formGroupName;
        inputGroupCode.value = groupData.formGroupCode;

        inputGroupCode.readOnly = true;
        inputGroupCode.classList.add('bg-light'); 

    } catch (error) {
        errorAlert.textContent = error.message || "Grup bilgileri getirilirken hata oluştu.";
        errorAlert.classList.remove('d-none');
    } finally {
        inputGroupName.disabled = false;
        inputGroupCode.disabled = false;
        btnSaveGroup.disabled = false;
    }
};

inputGroupCode.addEventListener('input', (e) => {
    let val = e.target.value;
    val = val.replace(/ı/g, 'i').replace(/i̇/g, 'i').replace(/ğ/g, 'g').replace(/ü/g, 'u').replace(/ş/g, 's').replace(/ö/g, 'o').replace(/ç/g, 'c');
    val = val.replace(/I/g, 'I').replace(/İ/g, 'I').replace(/Ğ/g, 'G').replace(/Ü/g, 'U').replace(/Ş/g, 'S').replace(/Ö/g, 'O').replace(/Ç/g, 'C');
    e.target.value = val.toUpperCase().replace(/[^A-Z0-9\-_]/g, '');
});

btnSaveGroup.addEventListener('click', async () => {
    const name = inputGroupName.value.trim();
    const code = inputGroupCode.value.trim();

    if (name.length < 6 || code.length < 2) {
        errorAlert.textContent = "Lütfen alanları istenilen uzunlukta doldurun.";
        errorAlert.classList.remove('d-none');
        return;
    }
    try {
        btnSaveGroup.disabled = true;
        btnSaveGroup.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Kaydediliyor...';

        if (editingGroupCode) {
            await api.patch(`/FormGroup/${editingGroupCode}`, {
                formGroupName: name,
                formGroupCode: code 
            });
        } else {
            await api.post('/FormGroup', {
                formGroupName: name,
                formGroupCode: code
            });
        }
        addGroupModal.hide();
        await refreshUserMenus();
        if (window.refreshSidebarMenu) window.refreshSidebarMenu();
        await loadFormGroups();
    } catch (error) {
        errorAlert.textContent = error.message || "İşlem sırasında bir hata meydana geldi.";
        errorAlert.classList.remove('d-none');
    } finally {
        btnSaveGroup.disabled = false;
        btnSaveGroup.innerHTML = '<i class="bi bi-check-lg me-1"></i> Kaydet';
    }
});

window.deleteGroup = async (formGroupCode) => {
    if (confirm(`'${formGroupCode}' kodlu grubu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`)) {
        try {
            await api.delete(`/FormGroup/${formGroupCode}`);
            await refreshUserMenus();
            if (window.refreshSidebarMenu) window.refreshSidebarMenu();
            await loadFormGroups();
            
        } catch (error) {
            alert(error.message || "Silme işlemi sırasında bir hata oluştu.");
        }
    }
};

window.openFormsModal = async (formGroupCode, formGroupName) => {
    currentActiveGroupCode = formGroupCode;
    formsModalTitle.textContent = `${formGroupName} Formları`;
    formsModalGroupCode.textContent = "Bu form grubuna ait formlar aranıyor...";
    
    formsTbody.innerHTML = `
        <tr>
            <td colspan="4" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-primary me-2"></div>
                Yükleniyor...
            </td>
        </tr>
    `;
    
    btnAddForm.classList.remove('d-none');
    formsModal.show();

    try {
        const forms = await api.get(`/Form/forms/${formGroupCode}?pageNumber=1&pageSize=50`);
        formsModalGroupCode.textContent = "";
        formsTbody.innerHTML = '';

        if (!forms || forms.length === 0) {
            formsTbody.innerHTML = `<tr><td colspan="4" class="text-center py-4 text-muted">Bu gruba ait henüz bir form bulunmuyor.</td></tr>`;
            return;
        }

        forms.forEach(form => {
            const formName = form.formName;
            const formId = form.formId;
            const isPublished = form.isPublished;
            const createdAt = new Date(form.createdAt).toLocaleDateString();

            const statusBadge = isPublished 
                ? `<span class="badge bg-success">Yayında</span>`
                : `<span class="badge bg-secondary">Taslak</span>`;
            let publishButton = '';
            if (!isPublished) {
                publishButton = `
                    <button class="btn btn-sm btn-outline-success me-1" onclick="publishForm('${formId}')" title="Yayınla">
                        <i class="bi bi-cloud-arrow-up"></i>
                    </button>
                `;
            } else {
                publishButton = `
                    <button class="btn btn-sm btn-outline-secondary me-1" onclick="unpublishForm('${formId}')" title="Yayından Kaldır">
                        <i class="bi bi-cloud-arrow-down"></i>
                    </button>
                `;
            }

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="ps-4 fw-medium text-dark">${formName}</td>
                <td>${statusBadge}</td>
                <td>${createdAt}</td>
                <td class="text-end pe-4">
                    ${publishButton}
                    <a href="/frontend/pages/forms/design.html?formId=${formId}" class="btn btn-sm btn-outline-warning me-1" title="Düzenle">
                        <i class="bi bi-pencil"></i>
                    </a>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteForm('${formId}')" title="Sil">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            formsTbody.appendChild(tr);
        });

    } catch (error) {
        formsTbody.innerHTML = `<tr><td colspan="4" class="text-center py-4 text-danger">Formlar yüklenirken bir hata oluştu.</td></tr>`;
        console.error("Formlar çekilemedi:", error);
    }
};

window.deleteForm = async (formId) => {
    if (confirm("Bu formu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.")) {
        try {
            await api.delete(`/Form/${formId}`);
            await refreshUserMenus();
            if (window.refreshSidebarMenu) window.refreshSidebarMenu();
            if (currentActiveGroupCode) {
                window.openFormsModal(currentActiveGroupCode); 
            }
        } catch (error) {
            alert("Form silinirken hata: " + error.message);
        }
    }
};

window.publishForm = async (formId) => {
    if (confirm("Bu formu yayınlamak istediğinize emin misiniz? Yayınlandıktan sonra tablosu oluşur ve şeması bir daha değiştirilemez.")) {
        try {
            await api.patch(`/Form/publish-form/${formId}`);
            if (currentActiveGroupCode) {
                window.openFormsModal(currentActiveGroupCode); 
            }
        } catch (error) {
            alert("Yayınlama hatası: " + error.message);
        }
    }
};

window.unpublishForm = async (formId) => {
    if (confirm("Bu formu yayından kaldırmak istediğinize emin misiniz? Form tekrar Taslak durumuna dönecektir.")) {
        try {
            await api.patch(`/Form/unpublish-form/${formId}`);
            if (currentActiveGroupCode) {
                window.openFormsModal(currentActiveGroupCode); 
            }
        } catch (error) {
            alert("Yayından kaldırma hatası: " + error.message);
        }
    }
};

btnAddForm.addEventListener('click', () => {
    if (currentActiveGroupCode) {
        window.location.href = `/frontend/pages/forms/design.html?groupCode=${currentActiveGroupCode}`;
    }
});

document.addEventListener('DOMContentLoaded', loadFormGroups);