# Guía de Seguridad

## 🛡️ Resumen Ejecutivo

Este documento describe las medidas de seguridad implementadas en la solución y recomendaciones para producción.

---

## 🔐 1. Autenticación y Autorización

### 1.1 JWT (JSON Web Tokens)

**Implementación:**
- ✅ Algoritmo: **HS256** (HMAC SHA-256)
- ✅ Clave secreta: **32+ caracteres** (min. 256 bits)
- ✅ Expiración: **5 minutos** (configurable)
- ✅ Validación: Firma, emisor, audiencia, tiempo

**Configuración:**
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

**Validación implementada en Program.cs:**
```csharp
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
```

**Endpoints Protegidos:**
```
GET  /api/gateway/users          [Authorize] ✅
GET  /api/gateway/users/{id}     [Authorize] ✅
GET  /api/gateway/token          [Public] ✅
```

---

### 1.2 Mejoras Recomendadas para Producción

⚠️ **Cambiar a RS256 (Asymmetric)**

RS256 es más seguro que HS256 para arquitecturas distribuidas:

```csharp
// En lugar de usar una clave compartida
// Usar un certificado X.509

var cert = new X509Certificate2("path/to/cert.pfx", "password");
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new X509SecurityKey(cert),
    // ... resto de validación
};
```

⚠️ **Implementar Refresh Tokens**

Para permitir renovación segura sin relogueo:

```csharp
public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

⚠️ **Token Blacklist/Revocation**

Ya parcialmente implementado:
```csharp
public class Token
{
    public bool IsRevoked { get; set; }
}
```

Mejorar con Redis para revocación rápida.

---

## 🔒 2. Validación de Entrada

### 2.1 SSRF (Server-Side Request Forgery) Protection

**Implementado en ImageService:**

```csharp
// En ImageRepository
private bool ValidateSsrfUrl(string url)
{
    try
    {
        var uri = new Uri(url);
        
        // Rechazar URLs con caracteres sospechosos
        if (uri.Host.Contains("javascript:") || 
            uri.Host.Contains("data:") ||
            uri.Host.Contains("file://"))
            return false;
        
        // Solo HTTPS
        return uri.Scheme == "https";
    }
    catch
    {
        return false;
    }
}
```

**Validación adicional recomendada:**

```csharp
// Whitelist de dominios permitidos
private static readonly string[] AllowedDomains = 
{
    "reqres.in",
    "cdn.example.com",
    "images.example.com"
};

private bool IsWhitelistedDomain(Uri uri)
{
    return AllowedDomains.Any(d => 
        uri.Host.EndsWith(d, StringComparison.OrdinalIgnoreCase)
    );
}
```

---

### 2.2 Validación de Atributos (DTOs)

Usar `DataAnnotations` para validar entrada:

```csharp
public class CreateImageDto
{
    [Required]
    [Url] // Valida que sea URL válida
    [StringLength(500)]
    public string Url { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}
```

En controlador:
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateImageDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState); // Retorna 400 con detalles
}
```

---

## 🚦 3. Rate Limiting

### 3.1 Implementación (ImageService)

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("images", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromSeconds(10);
    });
});

// En controlador
[RateLimitPartition("images")]
[HttpGet("images/user/{userId}")]
public async Task<IActionResult> GetImages(int userId)
{
    // ...
}
```

**Límites actuales:**
- 📊 5 requests por 10 segundos
- 📋 Aplicado a endpoints de ImageService

**Headers de respuesta:**
```
RateLimit-Limit: 5
RateLimit-Remaining: 3
RateLimit-Reset: 1678790460
```

### 3.2 Mejoras Recomendadas

⚠️ **Rate Limiting por Usuario (IP)**

```csharp
options.OnRejected = async (context, token) =>
{
    context.HttpContext.Response.StatusCode = 429;
    await context.HttpContext.Response
        .WriteAsJsonAsync(new
        {
            message = "Too many requests. Please try again later.",
            retryAfter = context.HttpContext.Response
                .Headers["Retry-After"].ToString()
        }, cancellationToken: token);
};
```

⚠️ **Diferentes límites por rol**

```csharp
// Admin: 100 requests/min
// User: 10 requests/min
```

---

## 🔓 4. Manejo de Errores y Información

### 4.1 Errores Genéricos (Sin Exposición de Datos)

**Implementado correctamente:**

```csharp
// ✅ BIEN - Genérico
return BadRequest(new { message = "Invalid input" });

// ❌ MALO - Expone información
return BadRequest(new 
{ 
    message = "User not found in database table Users",
    exception = ex.ToString() // ¡NUNCA!
});
```

**Middleware de excepciones:**
```csharp
public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isProd = httpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsProduction();
        
        var response = new
        {
            message = isProd 
                ? "An error occurred" 
                : exception.Message,
            details = isProd ? null : exception.StackTrace
        };
        
        await httpContext.Response.WriteAsJsonAsync(response, 
            cancellationToken: cancellationToken);
        return true;
    }
}
```

---

### 4.2 Logging Seguro

**Recomendación:** Nunca loguear información sensible

```csharp
// ❌ MALO
_logger.LogInformation($"User login: {user.Email} with password: {password}");

// ✅ BIEN
_logger.LogInformation($"User login attempt for user ID: {user.Id}");
```

---

## 🌐 5. HTTPS/TLS

### 5.1 Implementado

Todos los servicios redirigen a HTTPS:

```csharp
app.UseHttpsRedirection();
```

**Configuración en appsettings.json:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "path/to/cert.pfx",
          "Password": "password"
        }
      }
    }
  }
}
```

### 5.2 Certificados de Producción

⚠️ **Usar certificados válidos (no auto-firmados)**

**Opciones:**
1. **Let's Encrypt** (gratuito)
2. **Azure Key Vault** (si está en Azure)
3. **DigiCert/Sectigo** (enterprise)

---

## 🚫 6. CORS (Cross-Origin Resource Sharing)

### 6.1 Configuración Recomendada

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins("https://example.com", "https://app.example.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors("AllowSpecificOrigins");
```

**⚠️ NO hacer esto en producción:**
```csharp
// ❌ INSEGURO
policy.AllowAnyOrigin().AllowAnyMethod();
```

---

## 📝 7. Validación de Schema/DTOs

### 7.1 Implementado

```csharp
public class UserResponseDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    
    [Url]
    public string Avatar { get; set; }
}
```

---

## 🔑 8. Gestión de Secretos

### 8.1 Desarrollo (Actual)

```json
// appsettings.Development.json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long"
  }
}
```

### 8.2 Producción (Recomendado)

⚠️ **NUNCA guardar secretos en código**

**Usar User Secrets (desarrollo local):**
```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-secret-key-32-chars"
```

**Usar variables de entorno (producción):**
```powershell
$env:JWT_KEY="your-secret-key-32-chars"
```

**Usar Azure Key Vault (en Azure):**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## 🆔 9. API Keys

### 9.1 Configuración ReqRes

```json
{
  "ReqRes": {
    "BaseUrl": "https://reqres.in/api",
    "ApiKey": "reqres_248067fc8c6d4b8ba119bfbf66f6b99e"
  }
}
```

⚠️ La API key está en el código fuente (no ideal).

**Mejor aproximación:**
```csharp
// Infrastructure/HttpHandlers/ApiKeyHandler.cs
public class ApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;
    
    public ApiKeyHandler(IConfiguration config)
    {
        _apiKey = config["ReqRes:ApiKey"];
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        request.Headers.Add("X-API-Key", _apiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

## 🔄 10. Inyección de Dependencias (DI) Segura

### 10.1 Ciclos de Vida

```csharp
// ✅ BIEN - Nuevo por request
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ✅ BIEN - Puede reutilizarse
builder.Services.AddTransient<IValidator, UserValidator>();

// ⚠️ CUIDADO - Compartido en toda la app
builder.Services.AddSingleton<IConfiguration>(config);
```

---

## ✅ Checklist de Seguridad

### Implementado ✅
- [x] Autenticación JWT con validación
- [x] Endpoints protegidos con [Authorize]
- [x] SSRF validation en URLs
- [x] Rate limiting en ImageService
- [x] Errores genéricos sin exposición de datos
- [x] HTTPS redirection
- [x] DTOs con validación

### Por Implementar ⚠️
- [ ] RS256 en lugar de HS256
- [ ] Refresh tokens
- [ ] Token blacklist con Redis
- [ ] CORS configurado específicamente
- [ ] Secretos en Azure Key Vault
- [ ] Logging secure (sin datos sensibles)
- [ ] WAF (Web Application Firewall) en producción
- [ ] Rate limiting por usuario/IP
- [ ] Audit trail para acciones críticas
- [ ] OWASP Top 10 compliance scan

---

## 🚀 Plan de Hardening para Producción

### Fase 1: Inmediato
1. Cambiar secretos a environment variables
2. Implementar certificados válidos
3. Configurar CORS específicamente
4. Agregar logging secure

### Fase 2: Corto Plazo
1. Migrar a RS256
2. Implementar refresh tokens
3. Agregar token revocation
4. Implementar audit trail

### Fase 3: Mediano Plazo
1. Migrar secretos a Azure Key Vault
2. Implementar WAF
3. Realizar security audit
4. Compliance scan (PCI-DSS, OWASP)

---

## 📚 Referencias

- **OWASP Top 10:** https://owasp.org/www-project-top-ten/
- **JWT Best Practices:** https://tools.ietf.org/html/rfc8725
- **SSRF Prevention:** https://owasp.org/www-community/attacks/Server-Side_Request_Forgery
- **.NET Security:** https://docs.microsoft.com/en-us/dotnet/architecture/microservices/secure-net-microservices-web-applications/

---

**Última actualización:** Marzo 2026
