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

const rolesTbody = document.getElementById('roles-tbody');
const btnAddRole = document.getElementById('btn-add-role');

document.addEventListener('DOMContentLoaded', async () => {
    await loadRoles();
});

async function loadRoles() {
    try {
        rolesTbody.innerHTML = `<tr><td colspan="3" class="text-center text-muted py-4"><div class="spinner-border spinner-border-sm"></div> Roller yükleniyor...</td></tr>`;
        const roles = await api.get('/Role?pageNumber=1&pageSize=100');
        
        rolesTbody.innerHTML = '';
        if (roles.length === 0) {
            rolesTbody.innerHTML = `<tr><td colspan="3" class="text-center py-4">Sistemde henüz rol bulunmuyor.</td></tr>`;
            return;
        }

        roles.forEach(role => {
            const isAdmin = role.roleName.toLowerCase() === 'admin' || role.roleId === 1;
            
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="text-muted fw-bold">#${role.roleId}</td>
                <td class="fw-bold">${role.roleName}</td>
                <td class="text-end">
                    ${isAdmin ? `
                        <span class="text-muted small fst-italic">Değiştirilemez</span>
                    ` : `
                        <button class="btn btn-sm btn-outline-warning btn-edit-role me-2" data-id="${role.roleId}" data-name="${role.roleName}" title="Rol İsmini Düzenle">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-primary btn-edit-auth me-2" data-id="${role.roleId}" data-name="${role.roleName}" title="Yetkileri Yönet">
                            <i class="bi bi-shield-lock"></i> Yetkiler
                        </button>
                        <button class="btn btn-sm btn-outline-danger btn-delete-role" data-id="${role.roleId}" title="Rolü Sil">
                            <i class="bi bi-trash"></i>
                        </button>
                    `}
                </td>
            `;
            rolesTbody.appendChild(tr);
        });

        // Event Listeners for action buttons
        document.querySelectorAll('.btn-edit-role').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const roleId = e.currentTarget.getAttribute('data-id');
                const roleName = e.currentTarget.getAttribute('data-name');
                editRoleName(roleId, roleName);
            });
        });

        document.querySelectorAll('.btn-edit-auth').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const roleId = e.currentTarget.getAttribute('data-id');
                window.location.href = `/frontend/pages/admin/authorizations-edit.html?roleId=${roleId}`;
            });
        });

        document.querySelectorAll('.btn-delete-role').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const roleId = e.currentTarget.getAttribute('data-id');
                await deleteRole(roleId);
            });
        });

    } catch (error) {
        showToast('Roller yüklenirken hata oluştu.', 'danger');
        rolesTbody.innerHTML = `<tr><td colspan="3" class="text-center text-danger py-4">Veriler yüklenemedi.</td></tr>`;
    }
}

btnAddRole.addEventListener('click', async () => {
    const { value: roleName } = await Swal.fire({
        title: 'Yeni Rol Ekle',
        input: 'text',
        inputLabel: 'Rol Adı',
        showCancelButton: true,
        confirmButtonText: 'Oluştur ve Yetkilendir',
        cancelButtonText: 'İptal',
        inputValidator: (value) => {
            if (!value || value.trim().length < 3) {
                return 'Rol adı en az 3 karakter olmalıdır!'
            }
        }
    });

    if (roleName) {
        try {
            // 1. Yeni rolü oluştur
            const payload = { roleName: roleName.trim() };
            const newRole = await api.post('/Role', payload);
            
            showToast('Rol oluşturuldu. Yetki ekranına yönlendiriliyorsunuz...', 'success');
            
            setTimeout(() => {
                window.location.href = `/frontend/pages/admin/authorizations-edit.html?roleId=${newRole.roleId}`;
            }, 1000);

        } catch (error) {
            showToast(error.message || 'Rol oluşturulurken hata oluştu.', 'danger');
        }
    }
});

// Rol Silme İşlemi
async function deleteRole(roleId) {
    const result = await Swal.fire({
        title: 'Emin misiniz?',
        text: "Bu rolü silerseniz, bu role sahip kullanıcıların yetkileri etkilenebilir!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet, Sil!',
        cancelButtonText: 'İptal'
    });

    if (result.isConfirmed) {
        try {
            await api.delete(`/Role/${roleId}`);
            Swal.fire('Silindi!', 'Rol başarıyla silindi.', 'success');
            await loadRoles();
        } catch (error) {
            Swal.fire('Hata!', error.message || 'Rol silinirken bir sorun oluştu.', 'error');
        }
    }
}

// Rol İsmi Düzenleme İşlemi
async function editRoleName(roleId, currentName) {
    const { value: newRoleName } = await Swal.fire({
        title: 'Rol İsmini Düzenle',
        input: 'text',
        inputValue: currentName,
        inputLabel: 'Yeni Rol Adı',
        showCancelButton: true,
        confirmButtonText: 'Güncelle',
        cancelButtonText: 'İptal',
        inputValidator: (value) => {
            if (!value || value.trim().length < 3) {
                return 'Rol adı en az 3 karakter olmalıdır!'
            }
        }
    });

    if (newRoleName && newRoleName.trim() !== currentName) {
        try {
            const payload = { roleName: newRoleName.trim() };
            await api.patch(`/Role/${roleId}`, payload);
            showToast('Rol ismi başarıyla güncellendi.', 'success');
            await loadRoles();
        } catch (error) {
            showToast(error.message || 'Rol güncellenirken hata oluştu.', 'danger');
        }
    }
}
