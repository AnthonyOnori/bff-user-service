# Documentación Técnica - Sistema de Microservicios .NET 8

## 📋 Tabla de Contenidos
1. [Introducción](#introducción)
2. [Arquitectura General](#arquitectura-general)
3. [Microservicios](#microservicios)
4. [Funcionalidades](#funcionalidades)
5. [Stack Tecnológico](#stack-tecnológico)
6. [Estructura de Proyectos](#estructura-de-proyectos)
7. [Herramientas de IA](#herramientas-de-ia)

---

## Introducción

Este proyecto implementa una **solución de microservicios en .NET 8** basada en la **arquitectura hexagonal (Ports & Adapters)**, compuesta por 4 servicios independientes que se comunican mediante HTTP.

**Objetivo Principal:**
- Gestionar usuarios desde una API externa (ReqRes)
- Recuperar y procesar imágenes de usuarios
- Generar y validar tokens JWT
- Orquestar servicios mediante un Backend for Frontend (BFF)

---

## Arquitectura General

### Patrón: Puertos y Adaptadores (Hexagonal)

Cada microservicio sigue el patrón hexagonal con las siguientes capas:

```
┌─────────────────────────────────────────┐
│         API Layer (Controllers)          │
│    [Entrada de solicitudes HTTP]        │
└─────────────┬─────────────────────────┘
              │ (Inyección de Dependencias)
              ▼
┌─────────────────────────────────────────┐
│      Application Layer (Use Cases)      │
│  [Lógica de negocio/orquestación]       │
└─────────────┬─────────────────────────┘
              │ (Puertos/Interfaces)
              ▼
┌─────────────────────────────────────────┐
│       Domain Layer (Entidades)          │
│   [Modelos de negocio puros]            │
└─────────────┬─────────────────────────┘
              │ (Interfaz de repositorio)
              ▼
┌─────────────────────────────────────────┐
│   Infrastructure Layer (Adaptadores)    │
│ [Implementación tangible de Puertos]    │
│  (HTTP Clients, Repositories, etc)      │
└─────────────────────────────────────────┘
```

**Ventajas:**
- ✅ Desacoplamiento de capas
- ✅ Fácil de testear (mocks en puertos)
- ✅ Independencia de tecnologías externas
- ✅ Escalabilidad y mantenibilidad

---

## Microservicios

### 1️⃣ **UserService** (Puerto 5001)
**Responsabilidad:** Gestionar usuarios desde API externa ReqRes

**Componentes:**
- **Controllers:** `UserController`
- **Use Cases:** `GetAllUsersUseCase`, `GetUserByIdUseCase`
- **Entity:** `User` (id, email, firstName, lastName, avatar)
- **Adapter:** `ReqResHttpClient` - Integración con https://reqres.in/api/users
- **Repository:** `UserRepository` - Cache en memoria

**Endpoints:**
```
GET  /api/users           → Obtener todos los usuarios
GET  /api/users/{id}      → Obtener usuario por ID
```

---

### 2️⃣ **TokenService** (Puerto 5003)
**Responsabilidad:** Generar tokens JWT con expiración

**Componentes:**
- **Controllers:** `TokenController`
- **Use Cases:** `GenerateTokenUseCase`
- **Entity:** `Token` (Value, ExpiresAt)
- **Repository:** `TokenRepository` - Almacenamiento en memoria

**Endpoints:**
```
POST /api/tokens          → Generar token JWT
```

**Config JWT (appsettings.json):**
```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "DurationMinutes": 5
  }
}
```

---

### 3️⃣ **ImageService** (Puerto 5002)
**Responsabilidad:** Gestionar imágenes de usuarios con validación SSRF y rate limiting

**Componentes:**
- **Controllers:** `ImageController`
- **Use Cases:** `GetImageToBase64UseCase`
- **Entity:** `Image` (Base64, ContentType)
- **Repository:** `ImageRepository` - SSRF validation + Rate limiting
- **Middleware:** `ExceptionHandler`

**Endpoints:**
```
GET  /api/images/user/{userId}  → Obtener imágenes de usuario (base64)
```

**Seguridad:**
- 🔒 **SSRF Validation:** Previene URLs maliciosas
- 🚦 **Rate Limiting:** 5 requests / 10 segundos

---

### 4️⃣ **BFF - Backend for Frontend** (Puerto 7061)
**Responsabilidad:** Punto de entrada único que orquesa los 3 servicios

**Componentes:**
- **Controllers:** `GatewayController`
- **Use Cases:** 
  - `GetAllUsersUseCase`
  - `GetUserByIdUseCase`
  - `GetTokenUseCase`
- **Clients:** 
  - `UserServiceHttpClient`
  - `ImageServiceHttpClient`
  - `TokenServiceHttpClient`
- **Middleware:** `AuthHeaderHandler`, `ExceptionHandler`

**Endpoints:**
```
GET  /api/gateway/users              → Obtener todos los usuarios [Authorize]
GET  /api/gateway/users/{id}         → Obtener usuario + imagen [Authorize]
GET  /api/gateway/token              → Obtener token JWT
```

**Características:**
- 🔐 Validación JWT en endpoints protegidos
- 🔀 Orquestación de múltiples servicios
- 📤 Respuestas enriquecidas (usuario + imagen)

---

## Funcionalidades

### 👥 Gestión de Usuarios
- ✅ Obtener lista de usuarios desde ReqRes
- ✅ Filtrar usuario por ID
- ✅ Enriquecimiento de datos (usuario + imagen + avatar en base64)

### 🎫 Gestión de Tokens
- ✅ Generar JWT con expiración

### 🖼️ Gestión de Imágenes
- ✅ Obtener imágenes de usuarios
- ✅ Convertir a base64 para embeber en respuestas
- ✅ Validación SSRF (previene URLs maliciosas)
- ✅ Rate limiting para proteger recursos

### 🔐 Seguridad
- ✅ Autenticación JWT en BFF
- ✅ Manejo de errores genéricos (sin exposición de datos internos)
- ✅ SSRF validation en URLs de imágenes
- ✅ Rate limiting en ImageService

---

## Stack Tecnológico

| Componente | Versión | Propósito |
|-----------|---------|----------|
| **.NET** | 8.0 | Framework principal |
| **ASP.NET Core** | 8.0 | Web framework |
| **Entity Framework Core** | 8.0 | ORM (potencial) |
| **JWT** | - | Autenticación |
| **xUnit** | - | Testing framework |
| **Moq** | - | Mocking library |
| **Swagger/OpenAPI** | - | Documentación API |

---

## Estructura de Proyectos

```
bff-user-service3/
├── src/
│   ├── BFF/
│   │   ├── BFF.API/                 # Layer: Controllers
│   │   ├── BFF.Application/         # Layer: Use Cases
│   │   └── BFF.Infrastructure/      # Layer: HTTP Clients
│   │
│   ├── UserService/
│   │   ├── UserService.API/         # Layer: Controllers
│   │   ├── UserService.Application/ # Layer: Use Cases
│   │   ├── UserService.Domain/      # Layer: Entities
│   │   └── UserService.Infrastructure/ # Layer: ReqRes Adapter
│   │
│   ├── TokenService/
│   │   ├── TokenService.API/         # Layer: Controllers
│   │   ├── TokenService.Application/ # Layer: Use Cases
│   │   ├── TokenService.Domain/      # Layer: Entities
│   │   └── TokenService.Infrastructure/ # Layer: Repository
│   │
│   └── ImageService/
│       ├── ImageService.API/         # Layer: Controllers
│       ├── ImageService.Application/ # Layer: Use Cases
│       ├── ImageService.Domain/      # Layer: Entities
│       └── ImageService.Infrastructure/ # Layer: Repository + SSRF
│
├── tests/
│   ├── BFF.Tests/                    # Unit Tests
│   ├── UserService.Tests/            # Unit Tests
│   ├── TokenService.Tests/           # Unit Tests
│   └── ImageService.Tests/           # Unit Tests
│
├── Documentación/                    # Documentación técnica
├── Prompts/                          # Prompts de IA utilizados
│   ├── promt_initial.md              # Prompt inicial del proyecto
│   └── promp_code_review.md          # Prompt de code review
│
├── MS_BFF.sln                        # Solution file
└── README.md                         # Readme - este documento
```

---

## Herramientas de IA

Este proyecto fue desarrollado con asistencia de herramientas de IA generativa:

### Prompts Utilizados
En la carpeta `Prompts/` se encuentran los prompts utilizados:
- **promt_initial.md** - Define la visión inicial, requisitos y estructura base del proyecto
- **promp_code_review.md** - Especifica criterios de code review, seguridad y buenas prácticas

### Herramientas de IA Aplicadas
- **Claude Haiku 4.5** - Utilizado con los prompts para análisis de arquitectura, code review y documentación
- **GitHub Copilot** - Utilizado como asistente de revisión y código para autocompletado de implementación

---

## Próximos Pasos

Para más información detallada, consulta:
- 📐 [ARQUITECTURA.md](/Documentación/ARQUITECTURA.md) - Detalles de patrones y diseño
- 🔌 [API_ENDPOINTS.md](/Documentación/API_ENDPOINTS.md) - Referencia completa de endpoints
- 🚀 [GUIA_INSTALACION.md](/Documentación/GUIA_INSTALACION.md) - Cómo ejecutar el proyecto
- 🤖 [Prompts/](Prompts/) - Prompts de IA utilizados

---

**Última actualización:** Marzo 2026
