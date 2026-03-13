# Referencia de Endpoints API

## 🌐 URLs Base por Servicio

| Servicio | URL Base | Puerto |
|----------|----------|--------|
| **BFF** | `https://localhost:7061` | 7061 |
| **UserService** | `https://localhost:5001` | 5001 |
| **ImageService** | `https://localhost:5002` | 5002 |
| **TokenService** | `https://localhost:5003` | 5003 |

---

## 🔐 Autenticación

### Tokens JWT
Todos los endpoints protegidos requieren header:
```
Authorization: Bearer {jwt_token}
```

**Ejemplo de token JWT:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Configuración (appsettings.json):**
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

## 📌 BFF - Gateway (Puerto 7061)

Punto de entrada único para clientes. Orquésta llamadas a todos los microservicios.

### 1. Obtener Todos los Usuarios
```
GET /api/gateway/users
Authorization: Bearer {token}
```

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "firstName": "George",
    "lastName": "Bluth"
  },
  {
    "id": 2,
    "firstName": "Janet",
    "lastName": "Weaver"
  }
]
```

**Notas:**
- ✅ Requiere autenticación
- ✅ No incluye emails (privacidad)
- ✅ Obtiene datos de UserService

---

### 2. Obtener Usuario Específico con Imagen
```
GET /api/gateway/users/{id}
Authorization: Bearer {token}
```

**Ejemplo:** `GET /api/gateway/users/1`

**Response 200 OK:**
```json
{
  "id": 1,
  "firstName": "George",
  "lastName": "Bluth",
  "Image": {
    "Base64": "iVBORw0KGgoAAAANSUhEUgAAA...",
    "ContentType": "image/jpg"
  }
}
```

**Errores:**
- ❌ 401 Unauthorized: Token inválido/expirado
- ❌ 404 Not Found: Usuario no existe
- ❌ 500 Internal Server Error: Error al obtener imagen

**Notas:**
- ✅ Requiere autenticación
- ✅ Orquésta 2 servicios: UserService + ImageService
- ✅ Retorna imagen en base64
- ✅ No incluye email

---

### 3. Obtener Token JWT
```
GET /api/gateway/token
```

**Response 200 OK:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-03-13T15:30:00Z"
}
```

**Notas:**
- ✅ NO requiere autenticación (endpoint público)
- ✅ Válido por 5 minutos
- ✅ Generad por TokenService

---

## 👥 UserService (Puerto 5001)

Gestiona usuarios desde API externa ReqRes.

### 1. Obtener Todos los Usuarios
```
GET /api/users
```

**Response 200 OK:**
```json
[
  {
    "id": 1,
    "email": "george.bluth@reqres.in",
    "firstName": "George",
    "lastName": "Bluth",
    "avatar": "https://reqres.in/img/faces/1-image.jpg"
  }
]
```

**Características:**
- 📌 Incluye email (nivel de servicio)
- 🔄 Cache en repositorio en memoria
- 🌐 Datos obtenidos de ReqRes API

---

### 2. Obtener Usuario por ID
```
GET /api/users/{id}
```

**Ejemplo:** `GET /api/users/1`

**Response 200 OK:**
```json
{
  "id": 1,
  "email": "george.bluth@reqres.in",
  "firstName": "George",
  "lastName": "Bluth",
  "avatar": "https://reqres.in/img/faces/1-image.jpg"
}
```

**Errores:**
- ❌ 404 Not Found: Usuario no existe

---

## 🎫 TokenService (Puerto 5003)

Genera y valida tokens JWT.

### 1. Generar Token
```
POST /api/tokens
Content-Type: application/json
```

**Response 200 OK:**
```json
{
  "Value": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "ExpiresAt": "2026-03-13T14:30:00Z"
}
```

**Notas:**
- ✅ Token válido por 5 minutos (configurable)
- ✅ HS256 algoritmo
- ✅ Almacenado en repositorio para validación

---

## 🖼️ ImageService (Puerto 5002)

Gestiona imágenes con rate limiting.

### Limitaciones de Rate Limiting
```
📊 5 requests por 10 segundos
⏰ Ventana fija
```

---

### 1. Obtener Imágene de Usuario
```
GET /api/images/process?url{url}
```

**Ejemplo:** `GET /api/images/process?url=https://reqres.in/img/faces/1-image.jpg`

**Response 200 OK:**
```json
{
  "id": 1,
  "image": 
  {
    "Base64": "iVBORw0KGgoAAAANSUhEUgAAA...",
    "ContentType": "image/jpg"
  }
}
```

**Errores:**
- ❌ 400 Bad Request: URL contiene caracteres maliciosos (SSRF protection)
- ❌ 429 Too Many Requests: Rate limit excedido

**Seguridad (SSRF Validation):**
- ✅ Rechaza URLs con caracteres especiales sospechosos
- ✅ Valida solo URLs HTTPS
- ✅ Whitelist de dominios permitidos

---

**Notas:**
- 🔒 Validación SSRF previene URLs maliciosas
- 🚦 Rate limiting protege el servicio
- 📸 Imagen convertida a base64 automáticamente

---

## 🔧 Ejemplos de Uso con cURL

### Obtener Token
```bash
curl -X GET https://localhost:7061/api/gateway/token
```

### Obtener Todos los Usuarios (con token)
```bash
curl -X GET https://localhost:7061/api/gateway/users \
  -H "Authorization: Bearer {token}" \
  -H "Accept: application/json"
```

### Obtener Usuario Específico
```bash
curl -X GET https://localhost:7061/api/gateway/users/1 \
  -H "Authorization: Bearer {token}" \
  -H "Accept: application/json"
```

---

## 🏥 Tratamiento de Errores

Todos los errores siguen el formato estándar:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request is invalid",
  "instance": "/api/gateway/users/999"
}
```

**Códigos HTTP estándar:**

| Código | Significado |
|--------|------------|
| 200 | OK - Solicitud exitosa |
| 400 | Bad Request - Request inválido |
| 401 | Unauthorized - Token inválido/ausente |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error - Error en el servidor |

**Notas de seguridad:**
- ✅ No exponen detalles de errores internos
- ✅ Mensajes genéricos para evitar information disclosure
- ✅ Stack traces solo en Development

---

**Última actualización:** Marzo 2026
