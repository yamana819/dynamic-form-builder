import { api } from "../../shared/api.js";

function showToast(message, type = 'success') {
    const icon = type === 'danger' ? 'error' : type;
    Swal.fire({
        toast: true,
        position: 'bottom-end',
        showConfirmButton: false,
        timer: 3000,
        icon: icon,
        html: message
    });
}

const authForm = document.getElementById('auth-form');
const authRoleNameEl = document.getElementById('auth-role-name');
const authTbody = document.getElementById('auth-tbody');
const btnSaveAuth = document.getElementById('btn-save-auth');
const btnUncheckAllGlobal = document.getElementById('btn-uncheck-all-global');

let currentRoleId = null;
let currentAuthorizations = [];

document.addEventListener('DOMContentLoaded', async () => {
    // URL'den roleId parametresini al
    const urlParams = new URLSearchParams(window.location.search);
    const roleIdParam = urlParams.get('roleId');

    if (!roleIdParam) {
        Swal.fire('Hata', 'Geçersiz Rol ID!', 'error').then(() => {
            window.location.href = '/frontend/pages/admin/authorizations.html';
        });
        return;
    }

    currentRoleId = roleIdParam;
    await fetchRoleDetails();
    await fetchAuthorizations();
});

async function fetchRoleDetails() {
    try {
        const role = await api.get(`/Role/${currentRoleId}`);
        authRoleNameEl.textContent = role.roleName;
    } catch (error) {
        authRoleNameEl.textContent = "Bilinmeyen Rol";
        showToast('Rol bilgisi alınamadı.', 'danger');
    }
}

async function fetchAuthorizations() {
    try {
        currentAuthorizations = await api.get(`/Authorization/authorizations/${currentRoleId}`);
        renderAuthorizations();
    } catch (error) {
        authTbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger py-4">Yetkiler yüklenemedi. Lütfen tekrar deneyin.</td></tr>`;
        showToast('Yetki verileri alınamadı.', 'danger');
    }
}

function renderAuthorizations() {
    authTbody.innerHTML = '';
    
    if (currentAuthorizations.length === 0) {
        authTbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">Bu role atanabilecek menü bulunamadı.</td></tr>`;
        return;
    }

    currentAuthorizations.forEach((auth, index) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td class="fw-bold ps-4 text-dark py-3">${auth.menuName}</td>
            <td class="text-center py-3"><input class="form-check-input fs-5 auth-check" style="cursor:pointer;" type="checkbox" data-index="${index}" data-type="canView" ${auth.canView ? 'checked' : ''}></td>
            <td class="text-center py-3"><input class="form-check-input fs-5 auth-check" style="cursor:pointer;" type="checkbox" data-index="${index}" data-type="canCreate" ${auth.canCreate ? 'checked' : ''}></td>
            <td class="text-center py-3"><input class="form-check-input fs-5 auth-check" style="cursor:pointer;" type="checkbox" data-index="${index}" data-type="canEdit" ${auth.canEdit ? 'checked' : ''}></td>
            <td class="text-center py-3"><input class="form-check-input fs-5 auth-check" style="cursor:pointer;" type="checkbox" data-index="${index}" data-type="canDelete" ${auth.canDelete ? 'checked' : ''}></td>
            <td class="text-center pe-4 py-3">
                <div class="btn-group btn-group-sm shadow-sm" role="group">
                    <button type="button" class="btn btn-light text-success btn-row-check border px-3" data-index="${index}">Tümünü Ver</button>
                    <button type="button" class="btn btn-light text-danger btn-row-uncheck border px-3" data-index="${index}">Tümünü Al</button>
                </div>
            </td>
        `;
        authTbody.appendChild(tr);
    });

    // Bireysel Checkbox Değişimleri
    document.querySelectorAll('.auth-check').forEach(chk => {
        chk.addEventListener('change', (e) => {
            const index = e.target.getAttribute('data-index');
            const type = e.target.getAttribute('data-type');
            currentAuthorizations[index][type] = e.target.checked;
        });
    });

    // Satır Bazlı Tümünü Ver Butonu
    document.querySelectorAll('.btn-row-check').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const index = e.target.getAttribute('data-index');
            currentAuthorizations[index].canView = true;
            currentAuthorizations[index].canCreate = true;
            currentAuthorizations[index].canEdit = true;
            currentAuthorizations[index].canDelete = true;
            renderAuthorizations();
        });
    });

    // Satır Bazlı Tümünü Al Butonu
    document.querySelectorAll('.btn-row-uncheck').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const index = e.target.getAttribute('data-index');
            currentAuthorizations[index].canView = false;
            currentAuthorizations[index].canCreate = false;
            currentAuthorizations[index].canEdit = false;
            currentAuthorizations[index].canDelete = false;
            renderAuthorizations();
        });
    });
}

// Global (Tüm Menüler İçin) Tümünü Al
btnUncheckAllGlobal.addEventListener('click', () => {
    currentAuthorizations.forEach(a => {
        a.canView = false;
        a.canCreate = false;
        a.canEdit = false;
        a.canDelete = false;
    });
    renderAuthorizations();
});

// Yetkileri Kaydet (PATCH Request)
authForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!currentRoleId) return;

    const originalText = btnSaveAuth.innerHTML;
    btnSaveAuth.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Kaydediliyor...';
    btnSaveAuth.disabled = true;

    try {
        const payload = currentAuthorizations.map(a => ({
            menuId: a.menuId,
            canView: a.canView,
            canCreate: a.canCreate,
            canEdit: a.canEdit,
            canDelete: a.canDelete
        }));
        
        await api.patch(`/Authorization/${currentRoleId}`, payload);
        
        Swal.fire({
            title: 'Başarılı!',
            text: 'Yetkiler başarıyla güncellendi.',
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        }).then(() => {
            // Kayıttan sonra ana listeye geri dön
            window.location.href = '/frontend/pages/admin/authorizations.html';
        });

    } catch (error) {
        showToast(error.message || 'Yetkiler kaydedilirken hata oluştu.', 'danger');
        btnSaveAuth.innerHTML = originalText;
        btnSaveAuth.disabled = false;
    }
});
