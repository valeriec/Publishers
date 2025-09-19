Publishers App
Publishers App es una solución de ejemplo basada en .NET 8 que demuestra autenticación JWT, autorización por roles y gestión de artículos y opiniones, todo orquestado con Docker Compose.

Estructura del Proyecto
API1: Servicio de autenticación y gestión de usuarios (JWT, roles).
API2: Servicio para gestión de artículos y opiniones.
WebApp: Aplicación Razor Pages que consume ambas APIs.

Características
Autenticación y autorización JWT (Admin/User).
EF Core + SQLite para persistencia.
Seed automático de roles y usuario administrador (admin@demo.com / Admin123$).
APIs RESTful para login, artículos y opiniones.
Despliegue con Docker Compose (servicios aislados y comunicados en red interna).

Requisitos
.NET 8 SDK
Docker y Docker Compose
(Opcional) Git

Accede a la WebApp:
http://localhost:5000

APIs expuestas:
API1: http://localhost:5001/api
API2: http://localhost:5002/api

Usuarios y credenciales por defecto
Admin:
Usuario: admin
Email: admin@demo.com
Contraseña: Admin123$

Estructura de carpetas
Publishers/
├── API1/
├── API2/
├── WebApp/
├── docker-compose.yml
