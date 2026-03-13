# Arquitectura Hexagonal - Guía Detallada

## 📐 Principios Fundamentales

La arquitectura hexagonal (también llamada "Puertos y Adaptadores") aísla la lógica de negocio del código técnico, permitiendo que el dominio sea completamente independiente de detalles de implementación.

```
┌──────────────────────────────────────────────────────┐
│  MUNDO EXTERNO (HTTP, BD, Librerías)                 │
└──────────────────────────────────────────────────────┘
      ▲                                        ▲
      │ (Adaptadores - Concreto)               │
      │                                        │
┌─────┴──────────────────────────────────────┴────────┐
│                  PUERTOS (Interfaces)                │
├──────────────────────────────────────────────────────┤
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │    USAR APLICACIÓN (Use Cases)              │   │
│  │  - Lógica de negocio                        │   │
│  │  - Orquestación de procesos                 │   │
│  │  - Independente de tecnología               │   │
│  └──────────┬────────────────────────────────┘   │
│             │                                     │
│             ▼                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │    DOMINIO (Entidades)                      │   │
│  │  - Modelos de negocio                       │   │
│  │  - Validaciones de negocio                  │   │
│  │  - Sin dependencias externas                │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
└──────────────────────────────────────────────────────┘
```

---

## 🏗️ Capas por Microservicio

### Capa 1: API (Controllers)
**Responsabilidad:** Exponer endpoints HTTP

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IGetAllUsersUseCase _getAllUsersUseCase;
    
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var result = await _getAllUsersUseCase.ExecuteAsync();
        return Ok(result);
    }
}
```

**Responsabilidades:**
- ✅ Recibir solicitudes HTTP
- ✅ Validar parámetros
- ✅ Llamar Use Cases
- ✅ Retornar respuestas

**Ventaja:** Separación entre transporte (HTTP) y lógica.

---

### Capa 2: Application (Use Cases)
**Responsabilidad:** Orquestar lógica de negocio

```csharp
public interface IGetAllUsersUseCase
{
    Task<List<UserResponseDto>> ExecuteAsync();
}

public class GetAllUsersUseCase : IGetAllUsersUseCase
{
    private readonly IUserRepository _userRepository;
    
    public GetAllUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<List<UserResponseDto>> ExecuteAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserResponseDto(
            u.Id, u.FirstName, u.LastName, u.Avatar
        )).ToList();
    }
}
```

**Responsabilidades:**
- ✅ Implementar lógica de negocio
- ✅ Orquestar múltiples repositorios
- ✅ Aplicar reglas de negocio
- ✅ Transformar datos (DTOs)

**Patrón:** Inyección de dependencias mediante puertos (interfaces)

---

### Capa 3: Domain (Entidades)
**Responsabilidad:** Modelos de negocio puros

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Avatar { get; set; }
    
    // Métodos de dominio (validaciones)
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Email) && 
               !string.IsNullOrEmpty(FirstName);
    }
}
```

**Responsabilidades:**
- ✅ Definir estructura de datos
- ✅ Validaciones de negocio
- ✅ Sin dependencias externas

**Ventaja:** Código portable, sin referencias a frameworks

---

### Capa 4: Infrastructure (Adaptadores)
**Responsabilidad:** Implementar puertos concretos

#### Ejemplo: ReqResHttpClient (Adaptador externo)
```csharp
public interface IReqResClient  // PUERTO
{
    Task<List<User>> GetUsersAsync();
}

public class ReqResHttpClient : IReqResClient  // ADAPTADOR
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<User>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync(
            "https://reqres.in/api/users"
        );
        
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ReqResResponse>(json);
        
        return data.Data.Select(u => new User
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Avatar = u.Avatar
        }).ToList();
    }
}
```

#### Ejemplo: UserRepository (Adaptador de persistencia)
```csharp
public interface IUserRepository  // PUERTO
{
    Task<List<User>> GetAllAsync();
    Task<User> GetByIdAsync(int id);
    Task SaveAsync(User user);
}

public class UserRepository : IUserRepository  // ADAPTADOR
{
    private static List<User> _users = new();  // Cache en memoria
    
    public async Task<List<User>> GetAllAsync()
    {
        return await Task.FromResult(_users);
    }
    
    public async Task<User> GetByIdAsync(int id)
    {
        return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }
}
```

**Responsabilidades:**
- ✅ Implementar interfaces (puertos)
- ✅ Llamar APIs externas
- ✅ Acceder a bases de datos
- ✅ Convertir datos externos

**Ventaja:** Fácil de cambiar (e.g., BD en memoria → SQL)

---

## 🔌 Patrones de Inyección de Dependencias

### Registrar en Program.cs

```csharp
// Infrastructure
builder.Services.AddHttpClient<IReqResClient, ReqResHttpClient>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Application
builder.Services.AddScoped<IGetAllUsersUseCase, GetAllUsersUseCase>();
builder.Services.AddScoped<IGetUserByIdUseCase, GetUserByIdUseCase>();
```

**Ciclos de vida:**
- 🔄 **Transient:** Nueva instancia cada vez (stateless)
- 🔄 **Scoped:** Una instancia por request HTTP
- 🔄 **Singleton:** Una única instancia para toda la aplicación

**Recomendación:**
- Use `Scoped` para servicios con estado por request
- Use `Transient` para servicios sin estado
- Use `Singleton` para configuración/cache compartida

---

## 🧪 Testabilidad

### Ventaja: Mocking de puertos

```csharp
[Fact]
public async Task GetAllUsers_ShouldReturnUserList()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    mockRepository
        .Setup(r => r.GetAllAsync())
        .ReturnsAsync(new List<User>
        {
            new User { Id = 1, Email = "user@example.com", ... }
        });
    
    var useCase = new GetAllUsersUseCase(mockRepository.Object);
    
    // Act
    var result = await useCase.ExecuteAsync();
    
    // Assert
    Assert.Single(result);
    mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
}
```

**Beneficios:**
- ✅ No necesita BD real para tests
- ✅ Control total de comportamiento
- ✅ Tests rápidos y aislados
- ✅ Fácil reproducir escenarios

---

## 📊 Flujo de Datos - Ejemplo: GetUserById

```
HTTP Request
    ↓
[GatewayController.GetUserById]
    ↓ Inyecta
[GetUserByIdUseCase.ExecuteAsync]
    ↓ Usa
[IUserRepository.GetByIdAsync] ← PUERTO 1
[IImageRepository.GetImagesByUserIdAsync] ← PUERTO 2
    ↓ Implementados por
[UserRepository] (HTTP Client → ReqRes API)
[ImageRepository] (HTTP Client → ImageService)
    ↓ Retorna
[User Domain Entity]
    ↓ Transforma
[UserWithImageResponseDto]
    ↓
HTTP Response 200 OK
```

---

## 🎯 Mapeo de Microservicios

### UserService
```
API Layer:
  └─ UserController
       ├─ Get /api/users
       └─ Get /api/users/{id}

Application Layer:
  ├─ GetAllUsersUseCase
  └─ GetUserByIdUseCase

Domain Layer:
  └─ User (Entity)
  └─ IUserRepository (Puerto)

Infrastructure Layer:
  ├─ IReqResClient (Puerto externo)
  ├─ ReqResHttpClient (Adaptador HTTP)
  ├─ UserRepository (Adaptador - Persistencia)
  └─ ReqResResponse (DTO externo)
```

### TokenService
```
Controllers:
  └─ TokenController
       ├─ POST /api/tokens
       └─ POST /api/tokens/validate

Application:
  ├─ GenerateTokenUseCase
  └─ ValidateTokenUseCase

Domain:
  └─ Token (Entity)
  └─ ITokenRepository (Puerto)

Infrastructure:
  └─ TokenRepository (Adaptador - In-Memory)
```

### ImageService
```
Controllers:
  └─ ImageController
       ├─ GET /api/images/user/{userId}
       └─ POST /api/images

Application:
  └─ GetImageToBase64UseCase

Domain:
  └─ Image (Entity)
  └─ IImageRepository (Puerto)

Infrastructure:
  └─ ImageRepository (Adaptador - HTTP + SSRF Validation)
```

### BFF
```
Controllers:
  └─ GatewayController
       ├─ GET /api/gateway/users
       ├─ GET /api/gateway/users/{id}
       └─ GET /api/gateway/token

Application:
  ├─ GetAllUsersUseCase
  ├─ GetUserByIdUseCase
  └─ GetTokenUseCase

Infrastructure (Clientes HTTP):
  ├─ IUserServiceClient → UserServiceHttpClient
  ├─ IImageServiceClient → ImageServiceHttpClient
  └─ ITokenServiceClient → TokenServiceHttpClient
```

---

## 🔒 Principios de Diseño Implementados

| Principio | Implementación |
|-----------|-----------------|
| **Single Responsibility** | Cada capa tiene una responsabilidad clara |
| **Open/Closed** | Abierto a extensión (nuevos adaptadores), cerrado a modificación |
| **Liskov Substitution** | Los adaptadores pueden reemplazarse sin cambiar el código |
| **Interface Segregation** | Puertos pequeños y específicos (IUserRepository, ITokenRepository) |
| **Dependency Inversion** | Las capas dependen de abstractos (interfaces), no de concretos |

---

**Última actualización:** Marzo 2026
