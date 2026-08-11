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
      window.location.href="frontend/pages/login.html";
      throw new Error("Oturum süreniz doldu.");
    }
    if ((response.status === 403)) {
      alert("Bu işlem için yetkiniz yok.");
      throw new Error("");
    }
    if ((response.status === 204)) {
      return null;
    }
    const data = await response.json();
    if (!response.ok) {
      throw new Error(data.message);
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