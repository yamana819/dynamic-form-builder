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
    
    if (response.status === 204) {
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

    let errorMsg = data?.message || `HTTP ${response.status} hatası`;
    
    if (data?.errors) {
      if (typeof data.errors === 'object' && !Array.isArray(data.errors)) {
        // ASP.NET Core ProblemDetails format: errors: { FieldName: ["Error 1", "Error 2"] }
        const messages = [];
        for (const key in data.errors) {
          if (Array.isArray(data.errors[key])) {
            messages.push(...data.errors[key]);
          } else {
            messages.push(data.errors[key]);
          }
        }
        if (messages.length > 0) errorMsg = messages.join('<br>');
      } else if (Array.isArray(data.errors) && data.errors.length > 0) {
        // Normal string array format
        errorMsg = data.errors.join('<br>');
      }
    }

    if ((response.status === 401) || (response.status === 403)) {
      removeToken();
      sessionStorage.clear();
      localStorage.clear();
      
      if (!endpoint.toLowerCase().includes('login')) {
          const alertMsg = data?.message ? data.message : "Oturum süreniz doldu veya yetkileriniz güncellendi. Güvenliğiniz için lütfen tekrar giriş yapın.";
          alert(alertMsg);
          window.location.href = "/frontend/pages/login.html";
      }
      throw new Error(errorMsg);
    }
    
    if (!response.ok) {
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