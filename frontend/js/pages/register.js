import { api } from '../shared/api.js';

const form = document.getElementById('register-form');
const userNameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');
const passwordConfirmInput = document.getElementById('password-confirm');
const registerError = document.getElementById('register-error');
const signUpButton = document.getElementById('sign-up-submit-button');
const signUpSpinner = document.getElementById('sign-up-spinner');
const signUpText = document.getElementById('sign-up-text');

function validatePasswordMatch() {
    if (passwordInput.value !== passwordConfirmInput.value) {
        passwordConfirmInput.setCustomValidity('Şifreler eşleşmiyor.');
    } else {
        passwordConfirmInput.setCustomValidity('');
    }
}

passwordInput.addEventListener('input', validatePasswordMatch);
passwordConfirmInput.addEventListener('input', validatePasswordMatch);

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideError();

    validatePasswordMatch();

    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const userName = userNameInput.value.trim();
    const password = passwordInput.value;

    setLoading(true);

    try {
        await api.post('/User', { userName, password });
        showSuccess('Kayıt başarılı, giriş sayfasına yönlendiriliyorsunuz...');
        setTimeout(() => {
            window.location.href = '/frontend/pages/login.html';
        }, 1500);
    } catch (error) {
        showError(error?.message || 'Kayıt sırasında bir hata oluştu.');
        setLoading(false);
    }
});

function showError(message) {
    registerError.className = 'alert alert-danger';
    registerError.innerHTML = message;
    registerError.classList.remove('d-none');
}

function showSuccess(message) {
    registerError.className = 'alert alert-success';
    registerError.innerHTML = message;
    registerError.classList.remove('d-none');
}

function hideError() {
    registerError.classList.add('d-none');
    registerError.textContent = '';
}

function setLoading(isLoading) {
    signUpButton.disabled = isLoading;
    signUpSpinner.classList.toggle('d-none', !isLoading);
    signUpText.textContent = isLoading ? 'Kayıt olunuyor...' : 'Kayıt Ol';
}