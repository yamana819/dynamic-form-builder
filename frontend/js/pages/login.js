import { api } from "../shared/api.js";

const form = document.getElementById('login-form');
const usernameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');
const errorBox = document.getElementById('login-error');
const submitButton = document.getElementById('sign-in-button');
const submitSpinner = document.getElementById('sign-in-spinner');
const submitText = document.getElementById('sign-in-text');

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideError();

    const userName = usernameInput.value.trim();
    const password = passwordInput.value;

    if (!userName || !password) {
        showError('Kullanıcı adı ve şifre zorunludur.');
        return;
    }

    setLoading(true);

    try {
        const response = await api.post('/Authentication/login', { userName, password });
        localStorage.setItem('token', response.token);
        localStorage.setItem('userName', userName);

        const menus = await api.get('/Menu/me');
        sessionStorage.setItem('user_menus', JSON.stringify(menus));

        window.location.href = '/frontend/pages/dashboard.html';
    } catch (error) {
        showError(error?.message || 'Giriş yapılamadı, lütfen tekrar deneyin.');
        console.error(error);
    } finally {
        setLoading(false);
    }
});

function showError(message) {
    errorBox.innerHTML = message;
    errorBox.classList.remove('d-none');
}

function hideError() {
    errorBox.classList.add('d-none');
    errorBox.innerHTML = '';
}

function setLoading(isLoading) {
    submitButton.disabled = isLoading;
    submitSpinner.classList.toggle('d-none', !isLoading);
    submitText.textContent = isLoading ? 'Giriş yapılıyor...' : 'Giriş Yap';
}