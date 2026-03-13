namespace TokenService.Domain.Entities;

public class Token
{
    public string Value { get; set; }
    public DateTime ExpiresAt { get; set; }
}
