const BASE_URL = "http://localhost:5128/api";

const getToken = () => localStorage.getItem("token");

const removeToken = () => localStorage.removeItem("token");

async function request(endpoint, options = {}) {
  const cleanEndpoint = endpoint.replace(/^\/+/, '');
  const url = `${BASE_URL}/${cleanEndpoint}`;
  const headers = {
    "Content-Type": "application/json",
    ...options.headers,
  };
  const token = getToken();
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  const config = {
    ...options,
    headers: headers,
  };
  try {
    const response = await fetch(url, config);
    if ((response.status === 401)) {
      removeToken();
      sessionStorage.clear();
      
      // Eğer zaten login isteği yapıyorsak (şifre yanlışsa), sayfayı yenileme.
      if (!endpoint.toLowerCase().includes('login')) {
          window.location.href="/frontend/pages/login.html";
          throw new Error("Oturum süreniz doldu.");
      }
    }
    if ((response.status === 403)) {
      localStorage.removeItem('user_menus');
      sessionStorage.removeItem('user_menus');
      alert("Yetkileriniz güncellenmiş sayfa yenileniyor...");
      window.location.reload();
      throw new Error("Forbidden:Bu işlem için yetkiniz yok.");
    }
    if ((response.status === 204)) {
      return null;
    }
    let data = null;
    const text = await response.text();
    if (text) {
      try {
        data = JSON.parse(text);
      } catch {
        data = null; 
      }
    }
    if (!response.ok) {
      let errorMsg = data?.message || `HTTP ${response.status} hatası`;
      if (data?.errors && Array.isArray(data.errors) && data.errors.length > 0) {
        errorMsg = data.errors.join('<br>');
      }
      throw new Error(errorMsg);
    }
    return data;
  } catch (error) {
    console.error(error);
    throw error;
  }
}

export const api = {
  get: (endpoint)=>request(endpoint,{method:'GET'}),
  post:(endpoint,body)=>request(endpoint,{method:'POST',body:JSON.stringify(body)}),
  patch: (endpoint,body)=> request(endpoint,{method:'PATCH',body:JSON.stringify(body)}),
  delete:(endpoint)=>request(endpoint,{method:'DELETE'})
}