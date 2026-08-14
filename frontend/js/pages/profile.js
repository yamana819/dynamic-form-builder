import { api } from "../shared/api.js";

// Elementler
const displayUsername = document.getElementById('display-username');
const displayRole = document.getElementById('display-role');

const form = document.getElementById('form-profile-edit');
const inputUserName = document.getElementById('userName');
const usernameHelp = document.getElementById('username-help');

const btnEditProfile = document.getElementById('btn-edit-profile');
const btnSaveProfile = document.getElementById('btn-save-profile');
const btnChangePassword = document.getElementById('btn-change-password');

let originalUsername = '';

document.addEventListener('DOMContentLoaded', async () => {
    await loadMyProfile();
});

async function loadMyProfile() {
    try {
        const user = await api.get('/User/me');
        
        displayUsername.textContent = user.userName;
        displayRole.textContent = user.roleName.toUpperCase() + " ROLÜ";
        
        document.getElementById('roleName').value = user.roleName;

        inputUserName.value = user.userName;
        originalUsername = user.userName;

    } catch (error) {
        Swal.fire('Hata!', 'Profil bilgileri yüklenemedi. Lütfen tekrar giriş yapın.', 'error').then(() => {
            window.location.href = '/frontend/pages/login.html';
        });
    }
}

// Düzenleme Moduna Geçiş
btnEditProfile.addEventListener('click', () => {
    inputUserName.disabled = false;
    inputUserName.classList.remove('bg-light');
    usernameHelp.classList.remove('d-none');
    
    btnEditProfile.classList.add('d-none');
    btnSaveProfile.classList.remove('d-none');
    
    inputUserName.focus();
});

// Profil Kaydetme
form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const newUserName = inputUserName.value.trim();

    if (newUserName === originalUsername) {
        // Değişiklik yoksa modu kapat
        exitEditMode();
        return;
    }

    try {
        btnSaveProfile.disabled = true;
        btnSaveProfile.innerHTML = '<div class="spinner-border spinner-border-sm"></div> Kaydediliyor...';

        const updatedUser = await api.patch('/User/me', { userName: newUserName });

        // Header'daki ismi ve localStorage'ı da güncelle
        localStorage.setItem('userName', updatedUser.userName);
        sessionStorage.setItem('userName', updatedUser.userName);
        const headerUsernameEl = document.getElementById('header-user-name');
        if (headerUsernameEl) headerUsernameEl.textContent = updatedUser.userName;

        displayUsername.textContent = updatedUser.userName;
        originalUsername = updatedUser.userName;

        Swal.fire({
            toast: true,
            position: 'bottom-end',
            icon: 'success',
            title: 'Kullanıcı adı güncellendi!',
            showConfirmButton: false,
            timer: 3000
        });

        exitEditMode();

    } catch (error) {
        Swal.fire('Hata!', error.message || 'Güncellenirken bir hata oluştu.', 'error');
    } finally {
        btnSaveProfile.disabled = false;
        btnSaveProfile.innerHTML = '<i class="bi bi-save"></i> Kaydet';
    }
});

function exitEditMode() {
    inputUserName.disabled = true;
    inputUserName.classList.add('bg-light');
    usernameHelp.classList.add('d-none');
    
    btnSaveProfile.classList.add('d-none');
    btnEditProfile.classList.remove('d-none');
}

// Şifre Değiştirme Modal'ı
btnChangePassword.addEventListener('click', async () => {
    await Swal.fire({
        title: 'Şifre Değiştir',
        html:
            '<input id="swal-old-password" class="swal2-input" type="password" placeholder="Mevcut Şifre" required>' +
            '<input id="swal-new-password" class="swal2-input" type="password" placeholder="Yeni Şifre" required>' +
            '<input id="swal-confirm-password" class="swal2-input" type="password" placeholder="Yeni Şifre (Tekrar)" required>',
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: 'Şifreyi Güncelle',
        cancelButtonText: 'İptal',
        showLoaderOnConfirm: true,
        preConfirm: async () => {
            const oldPass = document.getElementById('swal-old-password').value;
            const newPass = document.getElementById('swal-new-password').value;
            const confirmPass = document.getElementById('swal-confirm-password').value;
            
            if (!oldPass || !newPass || !confirmPass) {
                Swal.showValidationMessage('Lütfen tüm alanları doldurun!');
                return false;
            }
            if (newPass !== confirmPass) {
                Swal.showValidationMessage('Girdiğiniz yeni şifreler birbiriyle eşleşmiyor!');
                return false;
            }
            
            try {
                // Hata mesajlarını modalın içinde (butonun üstünde) gösterebilmek için API çağrısını burada yapıyoruz.
                await api.patch('/User/me/change-password', { currentPassword: oldPass, password: newPass });
                return true;
            } catch (error) {
                // Backend'den dönen hata mesajı modalın içine basılır, modal kapanmaz.
                Swal.showValidationMessage(error.message || 'Şifre güncellenemedi.');
                return false;
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire(
                'Başarılı!',
                'Şifreniz başarıyla değiştirildi.',
                'success'
            );
        }
    });
});
