namespace UserService.Application.DTOs;

/// <summary>
/// DTO con información completa del usuario (incluyendo Email)
/// Usado internamente entre servicios (BFF puede ver el email)
/// NO debe ser devuelto al cliente final
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Avatar { get; set; }
}
