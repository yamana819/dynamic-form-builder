# Dynamic Form Builder

**Dynamic Form Builder** is a comprehensive, scalable, and highly adaptable web application that allows system administrators to dynamically create forms and manage data structures without writing a single line of code. 

By leveraging a powerful `.NET 10` backend and a decoupled `Vanilla JavaScript` frontend, the system interprets admin-defined form schemas (text fields, numbers, dates, etc.) and translates them into physical database tables and views on the fly. This architecture ensures maximum performance, strict data integrity, and complete isolation of form records.

Beyond dynamic table generation, the project ships with a robust **Role-Based Access Control (RBAC)** system. Menus, form groups, and individual forms can be heavily restricted. A custom-built dynamic sidebar engine ensures users only see what they are authorized to see, securely validated by a robust JWT-based authorization pipeline.

## 🚀 Core Features

* **Dynamic Schema & Table Generation:** Admins can visually design forms. Upon publishing, the backend's custom `SqlHelper` service uses ADO.NET to automatically provision dedicated SQL Server Tables and Views tailored precisely to the form's schema.
* **Advanced RBAC (Role-Based Access Control):** Granular permissions (`CanView`, `CanCreate`, `CanEdit`, `CanDelete`) can be assigned to different roles. Every API endpoint enforcing these permissions utilizes custom ASP.NET Core Action Filters (`[RequirePermission]`).
* **Dynamic Sidebar Menus:** The frontend navigation is fully database-driven. Menus are retrieved based on the user's role and rendered recursively, guaranteeing a secure and clean UI.
* **Global Error & Exception Handling:** A highly polished exception handling architecture utilizing ASP.NET Core Middleware intercepts custom exceptions (`ResourceNotFoundException`, `ConflictException`, `BadRequestException`) and translates them into standard HTTP Problem Details, which are seamlessly parsed and displayed by SweetAlert2 on the frontend.
* **Soft Deletion & Audit Trails:** Entities (Users, Forms, Roles) are never physically deleted. A soft-delete mechanism (`IsDeleted`) ensures data history and referential integrity are strictly maintained.
* **JWT Bearer Authentication:** Secure, stateless authentication utilizing identity hashing protocols, complete with claim-based identity resolution.

---

## 📂 Project Architecture & Folder Structure

The repository is strictly divided into two decoupled monolithic layers: the Backend (API) and the Frontend (Client).

### 1. `Scripts/` (Database Initialization)
The project adopts a **Database-First** approach (or manual schema design) for its core structural tables (Users, Roles, Menus, Permissions). Instead of relying on EF Core Migrations, the initial database schema and seed data are strictly maintained through raw SQL scripts.

* **`Scripts/`**: Contains raw SQL files (e.g., `add_seed_data.sql`, `rerefences.sql`) necessary to bootstrap the database schema, constraints, and default administrative data before running the backend.

### 2. `DynamicFormBuilder.API/` (Backend Layer)
Built on **.NET 10** and **Entity Framework Core**, this layer acts as the brain of the application. It handles routing, business logic, security, and dynamic database interactions.

* **`Controllers/`**: Contains API Endpoints handling HTTP requests. Responsible for request routing, authorization attributes, and returning standardized HTTP responses (e.g., `UserController`, `FormController`, `MenuController`).
* **`Services/`**: The core Business Logic Layer. Contains the services that execute the heavy lifting, separating business rules from the controllers (e.g., `UserService`, `FormService`, `RecordService`).
* **`Data/`**: Contains the Entity Framework Core `DbContext` (`DynamicFormBuilderDbContext`) and the heavily customized `SqlHelper` class responsible for raw ADO.NET dynamic table generation and DDL/DML executions.
* **`Models/`**: The Domain Entities mapped to the database tables via EF Core (e.g., `User`, `Role`, `Form`, `Menu`).
* **`DTOs/`**: Data Transfer Objects used to shape data entering and leaving the API, ensuring sensitive domain models are never exposed directly to the client. Includes strict validation attributes.
* **`Exceptions/`**: Custom Exception classes (e.g., `ConflictException`) tailored to specific business logic failures, allowing semantic error throwing throughout the application.
* **`MiddleWare/`**: Contains the `GlobalExceptionMiddleware` that catches all unhandled exceptions and custom exceptions, formatting them into structured JSON responses.
* **`Filters/`**: Custom Action Filters, most notably the `RequirePermissionFilter`, which intercepts requests to validate JWT claims against the database RBAC engine before the controller executes.
* **`Constants/`**: Application-wide static definitions and Enums (e.g., `PermissionType`, default roles).

### 2. `Frontend/` (Client Layer)
A lightweight, lightning-fast Single Page Application (SPA) feel, built strictly with **Vanilla HTML, CSS, and ES6 JavaScript Modules**. It utilizes **Bootstrap 5** for layout and **SweetAlert2** for interactions, avoiding the overhead of heavy JavaScript frameworks.

* **`pages/`**: Contains the HTML views for different application routes.
  * `admin/`: Views dedicated to administrative tasks (`users.html`, `authorizations.html`).
  * `forms/`: Views for interacting with the dynamic forms (`index.html`, `design.html`, `form-data.html`, `record-edit.html`).
  * `login.html` & `profile.html`: Authentication and user preference views.
* **`js/`**: The JavaScript logic, heavily modularized using ES6 imports.
  * `pages/`: Page-specific logic scripts tied directly to their HTML counterparts (e.g., `users.js`, `form-data.js`). These handle DOM manipulation and event listeners.
  * `shared/`: Reusable core modules. 
    * `api.js`: A centralized `fetch` wrapper handling JWT injection, global HTTP error parsing, and session expiration logic.
    * `layout.js`: The engine responsible for dynamically building the sidebar, injecting partial HTMLs, and resolving UI-level permissions.
* **`partials/`**: Reusable HTML snippets (`header.html`, `sidebar.html`) injected into the DOM dynamically to maintain a DRY (Don't Repeat Yourself) codebase.
* **`assets/`**: Static assets including global stylesheets (`style.css`), images, and fonts.
