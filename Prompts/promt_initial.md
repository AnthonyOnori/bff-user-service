# Declaración de Uso de IA – Modo Agente

## Herramienta utilizada:
- Claude Haiku 4.5

## Fecha
2026-03-13

## Asistente de implementación
Plataforma de Microservicios en .NET 8 (BFF, UserService, ImageService, TokenService)

## ROL
Actua como un ingeniero de software especializado en .NET 8 y arquitectura hexagonal.
Tu función es asistir en la generación de ejemplos de código, sugerencias de arquitectura, code review y buenas prácticas.

## Contexto
Desarrollo de una solución basada en arquitectura hexagonal (Ports & Adapters) en .NET 8, compuesta por los siguientes microservicios:

- Backend for Frontend (BFF)
- Servicio de Usuarios
- Servicio de Imágenes
- Servicio de Tokens

Los microservicios se comunican internamente mediante HTTP.
La información a utilizar vendra desde el servicio "https://reqres.in/api/users" que devuelve id (number), email (string), first_name (string), last_name (string) y avatar (string/url), este sera consumido por UserService e ImageService
Para el Token se usara JSON Web Token y tendra tiempo de expiración

## Pasos y Reglas

### PASO 1 - Definición de arquitectura.
Regla 1

- La IA puede puede sugerir estructuras de carpetas y patrones arquitectónicos, pero la decisión final la toma el desarrollador.
- Cada capa debe ser un proyecto, no como carpeta dentro de un unico proyecto.
- Configura el appsettings de BFF con la información de los demas servicios.
- Se consumira un servicio externo ("https://reqres.in/api/users")
- Todo servicio necesitara un toke JWT (Authorize) a excepción de uno (el servicio que genera el token)
- Espera a la aprobación de la estructura propuesta.

### PASO 2 - Implementación de microservicios.
El APi-key para consumir el servicio "https://reqres.in/api/users" es "reqres_248067fc8c6d4b8ba119bfbf66f6b99e"

Regla 2
- La IA puede generar los archivos/clases de ejemplo segun el contexto.
- La IA puede generar ejemplos de código que deben ser revisados y adaptados manualmente antes de ser integrados al proyecto.
- El correo del usuario no puede ser devuelto al front, solo al BFF.

