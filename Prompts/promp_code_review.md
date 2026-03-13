# Declaración de Uso de IA – Modo Agente

## Herramienta utilizada:
- Claude Haiku 4.5

## Fecha
2026-03-12

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
La información a utilizar vendra desde el servicio "https://reqres.in/api/users" que devuelve id, email, first_name, last_name y avatar, este sera consumido por UserService e ImageService
El APi-key para consumer el servicio es "reqres_248067fc8c6d4b8ba119bfbf66f6b99e"
Para el Token se usara JSON Web Token y tendra tiempo de expiración
Todos los servicios necesitan autorización (Authorize) a excepción de uno que genera el token
Para el servicio de imagenes, debe estar configuirado Rate Limite

## Pasos y Reglas

### PASO 1 - Revisar de arquitectura
Revisar la arquitectura del proyecto y sugerir correciones y/o mejoras en caso los hubiera.

Regla 1
- La IA no puede modificar el codigo fuente, solo indicar la observación y la sugerencia.
- Al finalizar espere instrucciones.

### PASO 2 - Revisar de codigo
Revisar el codigo desarrollado, verificar que cumpla con el contexto, buenas practicas y no se expongan vulnerabilidades.

Regla 2
- La IA no puede modificar el codigo fuente, solo indicar la observación y la sugerencia.
- Al finalizar espere instrucciones.

### PASO 3 - Pruebas unitarias
Sugerir estructuras y ejemplos de pruebas unitarias.

Regla 3
- La IA puede sugerir estructuras de pruebas unitarias, las pruebas deben estar en una carpeta independiente.
- Al finalizar espere instrucciones.

### PASO 4 - Documentación
Revisa la arquitectura, el codigo fuente y pruebas para generar la documentación.

Regla 4
- La IA puede ayudar en la redacción de documentación técnica.
- Solo genera la documentación, no generes un archivo resumen de los pasos que hiciste
