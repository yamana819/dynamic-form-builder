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

// Element Referansları
const form = document.getElementById('form-user-edit');
const inputUserName = document.getElementById('userName');
const selectRole = document.getElementById('roleId');
const inputPassword = document.getElementById('password');
const switchIsActive = document.getElementById('isActive');
const btnSave = document.getElementById('btn-save');

let targetUserId = null;

document.addEventListener('DOMContentLoaded', async () => {
    // 1. URL'den ID'yi al (tarayıcıdaki "=" mevzusu)
    const urlParams = new URLSearchParams(window.location.search);
    targetUserId = urlParams.get('id');

    if (!targetUserId) {
        Swal.fire('Hata!', 'Kullanıcı ID bulunamadı.', 'error').then(() => {
            window.location.href = '/frontend/pages/admin/users.html';
        });
        return;
    }

    // Yetki kontrolü (sadece butonu disable etmek/etmemek veya uyarı vermek için, asıl koruma backend'de)
    const perms = window.getUserPermissions('/admin/users');
    if (!perms.canEdit) {
        Swal.fire('Erişim Reddedildi', 'Bu kullanıcıyı düzenleme yetkiniz yok.', 'error').then(() => {
            window.location.href = '/frontend/pages/admin/users.html';
        });
        return;
    }

    // 2. Rolleri ve Kullanıcı verilerini yükle
    await loadRoles();
    await loadUserDetails();
});

async function loadRoles() {
    try {
        const roles = await api.get('/Role?pageNumber=1&pageSize=100');
        selectRole.innerHTML = '<option value="" disabled>Lütfen bir rol seçin</option>';
        
        roles.forEach(r => {
            const opt = document.createElement('option');
            opt.value = r.roleId;
            opt.textContent = r.roleName;
            selectRole.appendChild(opt);
        });
    } catch (error) {
        showToast('Roller yüklenirken hata oluştu: ' + error.message, 'danger');
        selectRole.innerHTML = '<option value="">Hata!</option>';
    }
}

async function loadUserDetails() {
    try {
        const user = await api.get(`/User/admin/${targetUserId}`);
        
        // Form alanlarını doldur
        inputUserName.value = user.userName;
        selectRole.value = user.roleId;
        
        // Admin'in kendi kendini yetkisizleştirmesini (rol düşürmesini) engelle
        if (user.userName.toLowerCase() === 'admin') {
            selectRole.disabled = true;
        } else {
            selectRole.disabled = false;
        }
        
        // Backend'de "IsDeleted" mantığı var, biz UI'da "IsActive" gibi tersini gösteriyoruz (Daha iyi UX için)
        switchIsActive.checked = !user.isDeleted;
        
    } catch (error) {
        Swal.fire('Hata!', error.message || 'Kullanıcı bilgileri alınamadı.', 'error').then(() => {
            window.location.href = '/frontend/pages/admin/users.html';
        });
    }
}

form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const payload = {
        userName: inputUserName.value.trim(),
        roleId: parseInt(selectRole.value),
        isDeleted: !switchIsActive.checked // Checkbox aktifse silinmemiştir (false), pasifse silinmiştir (true)
    };

    // Şifre alanı doldurulduysa payload'a ekle, boşsa gönderme (backend ona göre günceller/görmezden gelir)
    if (inputPassword.value.trim() !== "") {
        payload.password = inputPassword.value;
    }

    try {
        btnSave.disabled = true;
        btnSave.innerHTML = '<div class="spinner-border spinner-border-sm"></div> Kaydediliyor...';

        await api.patch(`/User/admin/${targetUserId}`, payload);

        Swal.fire({
            title: 'Başarılı!',
            text: 'Kullanıcı başarıyla güncellendi.',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false
        }).then(() => {
            // İsteğe bağlı: Liste sayfasına geri döndür veya burada kal
            window.location.href = '/frontend/pages/admin/users.html';
        });

    } catch (error) {
        Swal.fire('Hata!', error.message || 'Kullanıcı güncellenirken bir sorun oluştu.', 'error');
    } finally {
        btnSave.disabled = false;
        btnSave.innerHTML = '<i class="bi bi-save"></i> Değişiklikleri Kaydet';
    }
});
