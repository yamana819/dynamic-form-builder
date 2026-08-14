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

function formatDate(dateString) {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('tr-TR', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric', 
        hour: '2-digit', 
        minute: '2-digit' 
    });
}

const usersTbody = document.getElementById('users-tbody');

document.addEventListener('DOMContentLoaded', async () => {
    await loadUsers();
});

async function loadUsers() {
    try {
        usersTbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4"><div class="spinner-border spinner-border-sm"></div> Kullanıcılar yükleniyor...</td></tr>`;
        
        // Backend'den kullanıcıları çek
        const users = await api.get('/User/admin?pageNumber=1&pageSize=100');
        
        usersTbody.innerHTML = '';
        if (users.length === 0) {
            usersTbody.innerHTML = `<tr><td colspan="6" class="text-center py-4">Sistemde henüz kullanıcı bulunmuyor.</td></tr>`;
            return;
        }

        const perms = window.getUserPermissions('/admin/users');

        users.forEach(user => {
            let actionButtons = '';
            
            // "Admin" rolündeki ilk kullanıcıyı korumak iyi bir fikirdir (Kendi kendini veya ana admini silmemesi için). 
            // Ancak şu an tüm kullanıcılara butonları yetkilere göre basıyoruz.
            if (perms.canEdit) {
                actionButtons += `
                    <a href="/frontend/pages/admin/user-edit.html?id=${user.userId}" class="btn btn-sm btn-outline-primary me-2" title="Kullanıcıyı Düzenle">
                        <i class="bi bi-pencil"></i> Düzenle
                    </a>
                `;
            }
            if (perms.canDelete) {
                // Eğer kullanıcı zaten silinmişse silme butonunu gizleyebiliriz veya pasif gösterebiliriz.
                if (!user.isDeleted) {
                    actionButtons += `
                        <button class="btn btn-sm btn-outline-danger btn-delete-user" data-id="${user.userId}" data-name="${user.userName}" title="Kullanıcıyı Sil">
                            <i class="bi bi-trash"></i> Sil
                        </button>
                    `;
                }
            }
            
            const tr = document.createElement('tr');
            
            // Eğer silinmişse satırı hafif soluk gösterelim
            if(user.isDeleted) {
                tr.classList.add('opacity-50');
            }

            tr.innerHTML = `
                <td class="fw-bold">${user.userName}</td>
                <td><span class="badge bg-secondary">${user.roleName}</span></td>
                <td>${formatDate(user.userStartDate)}</td>
                <td>${formatDate(user.userLastActiveDate)}</td>
                <td>
                    ${user.isDeleted 
                        ? '<span class="badge bg-danger">Silindi</span>' 
                        : '<span class="badge bg-success">Aktif</span>'}
                </td>
                <td class="text-end">
                    <div class="d-flex justify-content-end gap-2">
                        ${actionButtons}
                    </div>
                </td>
            `;
            usersTbody.appendChild(tr);
        });

        // Delete button listeners
        document.querySelectorAll('.btn-delete-user').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const userId = e.currentTarget.getAttribute('data-id');
                const userName = e.currentTarget.getAttribute('data-name');
                await deleteUser(userId, userName);
            });
        });

    } catch (error) {
        showToast(error.message || 'Kullanıcılar yüklenirken hata oluştu.', 'danger');
        usersTbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger py-4">Veriler yüklenemedi.</td></tr>`;
    }
}

async function deleteUser(userId, userName) {
    const result = await Swal.fire({
        title: 'Emin misiniz?',
        html: `<b>${userName}</b> adlı kullanıcıyı silmek istediğinize emin misiniz? (Kullanıcı pasife alınacaktır)`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet, Sil!',
        cancelButtonText: 'İptal'
    });

    if (result.isConfirmed) {
        try {
            // Backend "Soft Delete" yapıyor. Sadece isDeleted: true göndermemiz yeterli.
            await api.patch(`/User/admin/${userId}`, { isDeleted: true });
            Swal.fire('Silindi!', 'Kullanıcı başarıyla silindi (pasife alındı).', 'success');
            await loadUsers(); // Tabloyu yenile
        } catch (error) {
            Swal.fire('Hata!', error.message || 'Kullanıcı silinirken bir sorun oluştu.', 'error');
        }
    }
}
