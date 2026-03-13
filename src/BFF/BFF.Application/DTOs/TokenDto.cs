using System;

namespace BFF.Application.DTOs
{
    public class TokenDto
    {
        public string Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
