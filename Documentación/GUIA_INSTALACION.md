# Guía de Instalación y Ejecución

## 📋 Requisitos Previos

### Software Requerido
- ✅ **.NET 8 SDK** (o superior)
  - Descargar: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verificar: `dotnet --version`

- ✅ **Visual Studio 2022** (recomendado)
  - Community Edition es suficiente
  - O usar Visual Studio Code + C# Extension

- ✅ **Git** (opcional, para clonar el proyecto)
  - Descargar: https://git-scm.com

### Verificar Instalación
```powershell
# Verificar .NET
dotnet --version

# Verificar SDK
dotnet --list-sdks

# Verificar Runtime
dotnet --list-runtimes
```

---

## 📁 Estructura del Proyecto

```
bff-user-service3/
├── Documentación/
├── src/
│   ├── BFF/
│   │   ├── BFF.API.csproj
│   │   ├── BFF.Application.csproj
│   │   └── BFF.Infrastructure.csproj
│   ├── UserService/
│   │   ├── UserService.API.csproj
│   │   ├── UserService.Application.csproj
│   │   ├── UserService.Domain.csproj
│   │   └── UserService.Infrastructure.csproj
│   ├── TokenService/
│   │   ├── TokenService.API.csproj
│   │   ├── TokenService.Application.csproj
│   │   ├── TokenService.Domain.csproj
│   │   └── TokenService.Infrastructure.csproj
│   └── ImageService/
│       ├── ImageService.API.csproj
│       ├── ImageService.Application.csproj
│       ├── ImageService.Domain.csproj
│       └── ImageService.Infrastructure.csproj
├── tests/
│   ├── BFF.Tests/
│   ├── UserService.Tests/
│   ├── TokenService.Tests/
│   └── ImageService.Tests/
├── MS_BFF.sln
└── README.md
```

---

## 🚀 Opción 1: Visual Studio 2022 (Recomendado)

### Paso 1: Abrir la Solución
1. Abre Visual Studio 2022
2. Selecciona **File → Open → Project/Solution**
3. Navega a `d:\proyectos\propio\Evol\bff-user-service3\MS_BFF.sln`
4. Click en **Abrir**

### Paso 2: Restaurar Dependencias
Visual Studio restaurará automáticamente los paquetes NuGet.

Si no lo hace manualmente:
1. **Project → Restore NuGet Packages**
2. O click derecho en la solución → **Restore NuGet Packages**

### Paso 3: Configurar Proyectos de Inicio Múltiples

Para ejecutar todos los servicios simultáneamente:

1. Click derecho en la solución → **Properties**
2. Selecciona **Startup Project**
3. Selecciona **Multiple startup projects**
4. Cambia a **Start** para cada proyecto API:
   - `BFF.API`
   - `UserService.API`
   - `TokenService.API`
   - `ImageService.API`

5. Click **OK**

### Paso 4: Ejecutar
- **F5** o **Debug → Start Debugging**

**Puertos esperados:**
```
BFF.API → https://localhost:7061
UserService.API → https://localhost:5001
TokenService.API → https://localhost:5003
ImageService.API → https://localhost:5002
```

---

## 🖥️ Opción 2: .NET CLI (Terminal)

### Paso 1: Navegar al Directorio de Solución
```powershell
cd d:\proyectos\propio\Evol\bff-user-service3
```

### Paso 2: Restaurar Dependencias
```powershell
dotnet restore
```

### Paso 3: Compilar la Solución
```powershell
dotnet build
```

**Output esperado:**
```
Build succeeded with 0 warnings.
```

### Paso 4: Ejecutar Servicios (en ventanas separadas)

#### Terminal 1 - BFF
```powershell
cd src/BFF/BFF.API
dotnet run
```
**Esperado:** `Now listening on: https://localhost:7061`

#### Terminal 2 - UserService
```powershell
cd src/UserService/UserService.API
dotnet run
```
**Esperado:** `Now listening on: https://localhost:5001`

#### Terminal 3 - TokenService
```powershell
cd src/TokenService/TokenService.API
dotnet run
```
**Esperado:** `Now listening on: https://localhost:5003`

#### Terminal 4 - ImageService
```powershell
cd src/ImageService/ImageService.API
dotnet run
```
**Esperado:** `Now listening on: https://localhost:5002`

---

## 🔧 Configuración de Servicios

### Editar Configuración
Cada servicio tiene archivos de configuración:

```
src/{ServiceName}/{ServiceName}.API/appsettings.Development.json
```

#### Ejemplo: appsettings.Development.json (UserService)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ServiceSettings": {
    "Port": 5001
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "DurationMinutes": 5
  },
  "ReqRes": {
    "BaseUrl": "https://reqres.in/api",
    "ApiKey": "reqres_248067fc8c6d4b8ba119bfbf66f6b99e"
  }
}
```

**Variables a configurar:**
- ✅ `Jwt.Key` - Mínimo 32 caracteres (secreto JWT)
- ✅ `Jwt.Issuer` - Emisor del token (ej: "MyApp")
- ✅ `Jwt.Audience` - Audiencia del token (ej: "MyAppUsers")
- ✅ `ReqRes.ApiKey` - API key para ReqRes (opcional)

---

## 📧 API Key de ReqRes

Si necesitas actualizar la API key:

1. Visita https://reqres.in
2. Obtén tu API key
3. Actualiza en `appsettings.Development.json`:

```json
"ReqRes": {
  "BaseUrl": "https://reqres.in/api",
  "ApiKey": "tu-nueva-api-key"
}
```

---

## 🧪 Ejecutar Pruebas Unitarias

### Opción 1: Visual Studio
1. **Test → Test Explorer**
2. Click en **Run All Tests** (▶️)
3. Espera los resultados

### Opción 2: .NET CLI
```powershell
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas de un proyecto específico
dotnet test tests/UserService.Tests/UserService.Tests.csproj

# Ejecutar pruebas con verbose output
dotnet test --verbosity detailed

# Ejecutar con cobertura de código
dotnet test /p:CollectCoverage=true
```

**Output esperado:**
```
Passed! - Failed: 0, Passed: 25, Skipped: 0
```

---

## 📊 Swagger - Documentación Interactiva

Cada servicio expone documentación automática:

- **BFF Swagger:** https://localhost:7061/swagger
- **UserService Swagger:** https://localhost:5001/swagger
- **TokenService Swagger:** https://localhost:5003/swagger
- **ImageService Swagger:** https://localhost:5002/swagger

**Para testear endpoints:**
1. Abre https://localhost:7061/swagger
2. Click en un endpoint
3. Click en **Try it out**
4. Completa parámetros
5. Click en **Execute**

---

## 🌐 Flujo de Autenticación Completo

### 1. Obtener Token
```powershell
curl -X GET https://localhost:7061/api/gateway/token \
  -k  # Ignore self-signed certificate
```

**Response:**
```json
{
  "Value": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "ExpiresAt": "2026-03-13T15:30:00Z"
}
```

### 2. Obtener Usuarios (usando token)
```powershell
$token = "tu-token-jwt"

curl -X GET https://localhost:7061/api/gateway/users \
  -H "Authorization: Bearer $token" \
  -k
```

**Response:**
```json
[
  {
    "id": 1,
    "firstName": "George",
    "lastName": "Bluth"
  }
]
```

### 3. Obtener Usuario Específico
```powershell
curl -X GET https://localhost:7061/api/gateway/users/1 \
  -H "Authorization: Bearer $token" \
  -k
```

**Response:**
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

---

## 🔐 Certificados SSL/TLS (Desarrollo)

En desarrollo, .NET genera certificados auto-firmados.

### Si tienes errores de certificado:

**Opción 1: Confiar en certificado (Windows)**
```powershell
dotnet dev-certs https --trust
```

**Opción 2: Desactivar validación (solo desarrollo)**
```powershell
$env:NODE_TLS_REJECT_UNAUTHORIZED='0'
```

**Opción 3: Usar HTTP en desarrollo**
Editar `appsettings.Development.json`:
```json
{
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http1AndHttp2"
    },
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:7061"
      }
    }
  }
}
```

---

## 🐛 Solución de Problemas

### Error: "Port already in use"
```powershell
# Encontrar proceso usando puerto 7061
netstat -ano | findstr :7061

# Terminar proceso (reemplazar PID)
taskkill /PID 12345 /F
```

### Error: "Package not found"
```powershell
dotnet nuget locals all --clear
dotnet restore
```

### Error: "Connection refused"
1. Verifica que los servicios estén ejecutándose
2. Verifica los puertos en `appsettings.Development.json`
3. Redeploy: `dotnet clean && dotnet build`

### Error: "Token validation failed"
1. Verifica que `Jwt.Key` sea idéntico en todos los servicios
2. Verifica que el token no haya expirado
3. Verifica que `Jwt.Issuer` y `Jwt.Audience` coincidan

---

## 📝 Archivos de Configuración Importantes

| Archivo | Propósito |
|---------|-----------|
| `appsettings.Development.json` | Configuración de desarrollo |
| `appsettings.json` | Configuración base |
| `Program.cs` | Configuración de servicios y middleware |
| `MS_BFF.sln` | Archivo de solución |

---

## ✅ Checklist de Instalación

- [ ] .NET 8 SDK instalado
- [ ] Solución abierta en Visual Studio
- [ ] Dependencias restauradas
- [ ] Proyectos compilados sin errores
- [ ] Servicios ejecutándose (4 ventanas/terminales)
- [ ] Swagger documentación accesible
- [ ] Token generado correctamente
- [ ] Endpoints protegidos requieren autenticación
- [ ] Pruebas ejecutándose correctamente

---

## 🚀 Siguientes Pasos

1. Consulta [API_ENDPOINTS.md](API_ENDPOINTS.md) para lista completa de endpoints
2. Lee [GUIA_SEGURIDAD.md](GUIA_SEGURIDAD.md) para consideraciones de seguridad
3. Lee [ARQUITECTURA.md](ARQUITECTURA.md) para entender el diseño

---

**Última actualización:** Marzo 2026
