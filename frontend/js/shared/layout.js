async function loadPartial(url, elementId) {
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`${url} yüklenemedi`);
        const html = await response.text();
        document.getElementById(elementId).innerHTML = html;
    } catch (error) {
        console.error("Bileşen yükleme hatası:", error);
    }
}


function buildFrontendUrl(href) {
    if (!href || href === '#') return '#';
    const parts = href.split('/').filter(Boolean);
    
    if (parts[0] === 'forms') {
        if (!parts[1]) {
            return '/frontend/pages/form-groups.html';
        }
        if (parts[2]) {
            return `/frontend/pages/forms/form-data.html?formId=${parts[2]}&groupCode=${parts[1]}`;
        }
        
        const base = '/frontend/pages/forms/index.html'; 
        return `${base}?code=${parts[1]}`;
    }
    
    return `/frontend/pages/${parts.join('/')}.html`;
}

function renderLayoutData() {
    const username = localStorage.getItem('userName') || sessionStorage.getItem('userName') || "Kullanıcı";
    const headerUsernameEl = document.getElementById('header-user-name');
    if (headerUsernameEl) {
        headerUsernameEl.textContent = username;
    }
    const menuContainer = document.getElementById('sidebar-menu-container');
    if (!menuContainer) return;

    const storedMenus = sessionStorage.getItem('user_menus') || localStorage.getItem('user_menus');
    
    if (!storedMenus) {
        menuContainer.innerHTML = `<li class="px-3 text-danger">Menü verisi bulunamadı. Lütfen tekrar giriş yapın.</li>`;
        return;
    }

    try {
        const menus = JSON.parse(storedMenus);
        menuContainer.innerHTML = '';
        function generateMenuHtml(menuList) {
            let html = '';
            menuList.forEach(menu => {
                const hasSubMenus = menu.subMenus && menu.subMenus.length > 0;
                if (hasSubMenus) {
                    html += `
                        <li class="nav-item mb-1">
                            <a class="nav-link text-white d-flex justify-content-between align-items-center dropdown-toggle" 
                               data-bs-toggle="collapse" 
                               href="#collapse-${menu.menuId}" 
                               role="button" 
                               aria-expanded="false">
                                ${menu.menuName}
                            </a>
                            <div class="collapse" id="collapse-${menu.menuId}">
                                <ul class="nav flex-column ms-3 mt-1 border-start border-secondary" style="font-size: 0.9em;">
                                    ${generateMenuHtml(menu.subMenus)}
                                </ul>
                            </div>
                        </li>
                    `;
                } else {
                    html += `
                        <li class="nav-item mb-1">
                            <a href="${buildFrontendUrl(menu.href)}" class="nav-link text-white">
                                ${menu.menuName}
                            </a>
                        </li>
                    `;
                }
            });
            return html;
        }
        menuContainer.innerHTML = generateMenuHtml(menus);

    } catch (e) {
        console.error("Menü parse hatası:", e);
    }

    const logoutBtn = document.getElementById('logout-button');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.clear();
            sessionStorage.clear();
            window.location.href = '/frontend/pages/login.html';
        });
    }
}

async function initLayout() {
    await loadPartial('/frontend/partials/sidebar.html', 'sidebar-placeholder');
    await loadPartial('/frontend/partials/header.html', 'header-placeholder');
    renderLayoutData();
    // Layout tam yüklendikten sonra sayfayı görünür yap (Göz kırpmayı önler)
    document.body.classList.add('loaded');
}
window.refreshSidebarMenu = renderLayoutData;

window.getUserPermissions = function(hrefSubstring) {
    const defaultPerms = { canView: false, canCreate: false, canEdit: false, canDelete: false };
    const storedMenus = sessionStorage.getItem('user_menus') || localStorage.getItem('user_menus');
    if (!storedMenus) return defaultPerms;

    try {
        const menus = JSON.parse(storedMenus);
        function findMenu(menuList) {
            for (const menu of menuList) {
                if (menu.href && menu.href.toLowerCase().includes(hrefSubstring.toLowerCase())) {
                    return menu;
                }
                if (menu.subMenus && menu.subMenus.length > 0) {
                    const found = findMenu(menu.subMenus);
                    if (found) return found;
                }
            }
            return null;
        }

        const foundMenu = findMenu(menus);
        if (foundMenu) {
            return {
                canView: foundMenu.canView === true,
                canCreate: foundMenu.canCreate === true,
                canEdit: foundMenu.canEdit === true,
                canDelete: foundMenu.canDelete === true
            };
        }
    } catch (e) {
        console.error("Yetki parse hatası:", e);
    }
    return defaultPerms;
};

initLayout();