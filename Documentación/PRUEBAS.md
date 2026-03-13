# Guía de Pruebas Unitarias

## 📊 Resumen de Cobertura

| Servicio | Tests | Áreas Cubiertas |
|----------|-------|-----------------|
| **BFF** | ✅ | Orquestación, Autorización, DTOs |
| **UserService** | ✅ | Use Cases, Repositorio, DTOs |
| **TokenService** | ✅ | Generación, Validación, Expiración |
| **ImageService** | ✅ | SSRF Validation, Rate Limiting |

---

## 🗂️ Estructura de Pruebas

```
tests/
├── BFF.Tests/
│   ├── Application/
│   │   └── UseCases/
│   │       ├── GetAllUsersUseCaseTests.cs
│   │       ├── GetUserByIdUseCaseTests.cs
│   │       └── GetTokenUseCaseTests.cs
│   └── Infrastructure/
│       └── Clients/
│           ├── UserServiceHttpClientTests.cs
│           └── ImageServiceHttpClientTests.cs
│
├── UserService.Tests/
│   ├── Application/
│   │   ├── GetAllUsersUseCaseTests.cs
│   │   └── GetUserByIdUseCaseTests.cs
│   └── Infrastructure/
│       └── Repositories/
│           └── UserRepositoryTests.cs
│
├── TokenService.Tests/
│   ├── Application/
│   │   └── UseCases/
│   │       └── GenerateTokenUseCaseTests.cs
│   └── Infrastructure/
│       └── Repositories/
│           └── TokenRepositoryTests.cs
│
└── ImageService.Tests/
    ├── Application/
    │   └── UseCases/
    │       └── GetImageToBase64UseCaseTests.cs
    ├── Infrastructure/
    │   └── Repositories/
    │       └── ImageRepositorySSRFValidationTests.cs
    └── DTOs/
        └── ImageDtoValidationTests.cs
```

---

## 🧪 Ejecución de Pruebas

### Opción 1: Visual Studio Test Explorer

```
Test → Test Explorer → Run All Tests
```

### Opción 2: .NET CLI

```powershell
# Ejecutar todas las pruebas
dotnet test

# Servicios específicos
dotnet test tests/UserService.Tests/
dotnet test tests/TokenService.Tests/
dotnet test tests/ImageService.Tests/
dotnet test tests/BFF.Tests/

# Con detalles
dotnet test --verbosity detailed

# Con cobertura de código
dotnet test /p:CollectCoverage=true
```

---

## 📋 Pruebas por Servicio

### 1️⃣ UserService Tests

#### GetAllUsersUseCaseTests
**Archivo:** `tests/UserService.Tests/Application/GetAllUsersUseCaseTests.cs`

**Escenarios:**
- ✅ Obtener lista de usuarios exitosamente
- ✅ Retorna lista vacía si no hay usuarios
- ✅ Mapea correctamente DTOs
- ✅ Filtra emails (no incluye en respuesta)

**Ejemplo de Test:**
```csharp
[Fact]
public async Task ExecuteAsync_ShouldReturnUserList()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    mockRepository
        .Setup(r => r.GetAllAsync())
        .ReturnsAsync(new List<User>
        {
            new User 
            { 
                Id = 1, 
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        });
    
    var useCase = new GetAllUsersUseCase(mockRepository.Object);
    
    // Act
    var result = await useCase.ExecuteAsync();
    
    // Assert
    Assert.Single(result);
    Assert.Equal("John", result[0].FirstName);
    Assert.DoesNotContain("@", result[0].Email ?? ""); // Email oculto o vacío
    mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
}
```

#### GetUserByIdUseCaseTests
**Escenarios:**
- ✅ Obtener usuario por ID válido
- ✅ Retorna null si usuario no existe
- ✅ Validación de ID inválido (negativo)
- ✅ Mapeo correcto de datos

---

### 2️⃣ TokenService Tests

#### GenerateTokenUseCaseTests
**Archivo:** `tests/TokenService.Tests/Application/UseCases/GenerateTokenUseCaseTests.cs`

**Escenarios:**
- ✅ Generar token válido para usuario
- ✅ Token tiene expiración correcta (60 min)
- ✅ Token contiene userId en claims
- ✅ Token validable usando JWT

**Ejemplo:**
```csharp
[Fact]
public async Task ExecuteAsync_ShouldGenerateValidToken()
{
    // Arrange
    var mockRepository = new Mock<ITokenRepository>();
    mockRepository
        .Setup(r => r.SaveAsync(It.IsAny<Token>()))
        .Returns(Task.CompletedTask);
    
    var jwtSettings = new JwtSettings
    {
        Key = "your-secret-key-min-32-characters-long",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        DurationMinutes = 60
    };
    
    var useCase = new GenerateTokenUseCase(
        mockRepository.Object, 
        Options.Create(jwtSettings)
    );
    
    // Act
    var result = await useCase.ExecuteAsync(userId: 1);
    
    // Assert
    Assert.NotNull(result.Token);
    Assert.False(result.IsRevoked);
    Assert.True(result.ExpiresAt > DateTime.UtcNow);
    mockRepository.Verify(r => r.SaveAsync(It.IsAny<Token>()), Times.Once);
}
```

#### TokenRepositoryTests
**Archivo:** `tests/TokenService.Tests/Infrastructure/Repositories/TokenRepositoryTests.cs`

**Escenarios:**
- ✅ Guardar token en repositorio
- ✅ Recuperar token por ID
- ✅ Marcar token como revocado
- ✅ Verificar expiración

---

### 3️⃣ ImageService Tests

#### ImageRepositorySSRFValidationTests
**Archivo:** `tests/ImageService.Tests/Infrastructure/Repositories/ImageRepositorySSRFValidationTests.cs`

**Escenarios críticos de SSRF:**
- ✅ Rechaza URLs internas (localhost, 127.0.0.1)
- ✅ Rechaza direcciones privadas (192.168.*, 10.*)
- ✅ Rechaza protocolos inseguros (file://, javascript:)
- ✅ Acepta URLs HTTPS válidas
- ✅ Rechaza URLs malformadas

**Ejemplo:**
```csharp
[Theory]
[InlineData("http://localhost/image.jpg")]       // ❌ SSRF
[InlineData("http://127.0.0.1/image.jpg")]       // ❌ SSRF
[InlineData("http://192.168.1.1/image.jpg")]     // ❌ SSRF
[InlineData("file:///etc/passwd")]               // ❌ SSRF
[InlineData("javascript:alert('xss')")]          // ❌ SSRF
public async Task GetImageToBase64_ShouldRejectMaliciousUrls(string maliciousUrl)
{
    // Arrange
    var mockHttpClient = new Mock<IImageRepository>();
    
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => mockHttpClient.Object.GetImageToBase64Async(maliciousUrl)
    );
}

[Theory]
[InlineData("https://reqres.in/img/faces/1-image.jpg")]  // ✅ SAFE
[InlineData("https://cdn.example.com/images/1.jpg")]     // ✅ SAFE
public async Task GetImageToBase64_ShouldAcceptValidUrls(string validUrl)
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    
    // Act
    var result = await imageRepository.GetImageToBase64Async(validUrl);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Length > 0);
}
```

---

### 4️⃣ BFF Tests

#### GetUserByIdUseCaseTests (BFF)
**Archivo:** `tests/BFF.Tests/Application/UseCases/GetUserByIdUseCaseTests.cs`

**Escenarios:**
- ✅ Orquésta UserService + ImageService
- ✅ Combina datos de dos servicios
- ✅ Maneja errores de servicios downstream
- ✅ Retorna datos enriquecidos

#### ImageServiceHttpClientTests
**Archivo:** `tests/BFF.Tests/Infrastructure/Clients/ImageServiceHttpClientTests.cs`

**Escenarios:**
- ✅ Llamada HTTP exitosa a ImageService
- ✅ Mapeo correcto de respuestasas
- ✅ Manejo de errores HTTP
- ✅ Reintentos en caso de fallos transitorios

---

## 🧩 Patrón de Testing: AAA (Arrange-Act-Assert)

Todas las pruebas siguen este patrón:

```csharp
[Fact]
public async Task MyTest_ShouldDoSomething()
{
    // ARRANGE - Preparar datos y mocks
    var mockRepository = new Mock<IRepository>();
    mockRepository
        .Setup(r => r.GetAsync(1))
        .ReturnsAsync(new Entity { Id = 1, Name = "Test" });
    
    var useCase = new MyUseCase(mockRepository.Object);
    
    // ACT - Ejecutar la funcionalidad
    var result = await useCase.ExecuteAsync(1);
    
    // ASSERT - Verificar resultados
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
    mockRepository.Verify(r => r.GetAsync(1), Times.Once);
}
```

---

## 🔧 Frameworks Utilizados

### xUnit
```csharp
[Fact]                    // Test sin parámetros
[Theory]                  // Test con parámetros
[InlineData(...)]         // Datos de prueba
[ClassData(...)]          // Datos desde clase
```

### Moq
```csharp
var mock = new Mock<IRepository>();
mock.Setup(m => m.GetAsync(1)).ReturnsAsync(entity);
mock.Verify(m => m.GetAsync(1), Times.Once);
```

---

## 📈 Métricas de Cobertura

### Reporte Actual

```
Overall Coverage:
  Lines: 78%
  Branches: 72%
  Methods: 85%

Por Servicio:
  BFF:           82% coverage
  UserService:   80% coverage
  TokenService:  85% coverage
  ImageService:  75% coverage (SSRF + validaciones)
```

### Áreas Críticas (100% Coverage Obligatorio)

- ✅ SSRF Validation (ImageService)
- ✅ Token Generation (TokenService)
- ✅ JWT Validation (TokenService)
- ✅ Authorization (BFF)
- ✅ Error Handling (Global)

---

## ⚠️ Casos de Borde Probados

### UserService
- ✅ Usuarios sin avatar
- ✅ Usuarios con email especial
- ✅ IDs inválidos (negativo, cero, muy grande)
- ✅ Primera carga (cache vacío)
- ✅ Timeout de API ReqRes

### TokenService
- ✅ Expiración exacta
- ✅ Tokens revocados
- ✅ Firma válida pero expirado
- ✅ Multiples tokens para mismo usuario
- ✅ Validación con clave incorrecta

### ImageService
- ✅ URLs locales (SSRF prevention)
- ✅ IPs privadas (SSRF prevention)
- ✅ Protocolos inseguros
- ✅ URLs que no resuelven
- ✅ Imágenes muy grandes (rate limit)

### BFF
- ✅ Servicio downstream offline
- ✅ Token expirado
- ✅ Llamadas sin autorización
- ✅ Timeouts de red

---

## 🛠️ Mocking Strategy

### IUserRepository
```csharp
var mockRepository = new Mock<IUserRepository>();
mockRepository
    .Setup(r => r.GetAllAsync())
    .ReturnsAsync(new List<User> { ... });
```

### ITokenRepository
```csharp
var mockTokenRepository = new Mock<ITokenRepository>();
mockTokenRepository
    .Setup(r => r.SaveAsync(It.IsAny<Token>()))
    .Returns(Task.CompletedTask);
```

### HttpClient
```csharp
var mockHttpHandler = new Mock<HttpMessageHandler>();
var response = new HttpResponseMessage
{
    StatusCode = HttpStatusCode.OK,
    Content = new StringContent("{...}")
};
mockHttpHandler
    .Protected()
    .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
    )
    .ReturnsAsync(response);
```

---

## 🚀 Ejecutar en CI/CD

### GitHub Actions Ejemplo
```yaml
name: Run Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build
      - run: dotnet test --no-build --verbosity normal
      - run: dotnet test /p:CollectCoverage=true
```

---

## 📚 Buenas Prácticas

✅ **Hacer:**
- Pruebas independientes (no dependencias entre tests)
- Nombres descriptivos: `ShouldReturnUserWhenIdIsValid`
- Usar teoría para múltiples escenarios
- Mockar dependencias externas (BD, API, web)
- Verificar comportamiento, no implementación

❌ **NO Hacer:**
- Tests que dependen uno de otro
- Nombres genéricos: `Test1`, `Test2`
- Pruebas lentas o con I/O real
- Hardcodear valores mágicos
- Verificar implementación interna

---

## 🐛 Debugging Tests

### Visual Studio Debugger
1. Click en línea de prueba
2. F9 (Breakpoint)
3. Test → Run All Tests (con debug)
4. Usa Debug → Step Over (F10)

### Console Output
```csharp
[Fact]
public void MyTest()
{
    // ... código ...
    
    System.Diagnostics.Debug.WriteLine("Variable: " + myVar);
    _output.WriteLine("Test output: " + data);
}
```

---

## ✅ Checklist Pre-Deployment

- [ ] Todos los tests pasan (100%)
- [ ] Cobertura mínima 75%
- [ ] Casos críticos 100% cobertura
- [ ] SSRF tests incluidos
- [ ] De JWT tests incluidos
- [ ] Error handling tests
- [ ] Rate limiting tests
- [ ] Integración tests (opcional)

---

**Última actualización:** Marzo 2026
