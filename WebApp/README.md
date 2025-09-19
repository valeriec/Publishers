# WebApp (.NET 8 Razor Pages)

Estructura base para la aplicación web con Razor Pages, Bootstrap, jQuery y autenticación JWT.

## Estructura de carpetas

- wwwroot/: archivos estáticos (css, js, imágenes, bootstrap, jquery)
- Pages/: Razor Pages (artículos, detalles, administración, autenticación)
- Models/: ViewModels
- Services/: Consumo de APIs y lógica de negocio
- Tests/: Pruebas unitarias con xUnit y Moq

## Requisitos
- .NET 8 SDK
- Bootstrap y jQuery (vía LibMan o npm)

## Primeros pasos
1. dotnet restore
2. dotnet build
3. dotnet run
