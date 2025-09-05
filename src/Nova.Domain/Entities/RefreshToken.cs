namespace Nova.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public virtual User User { get; set; } = null!;

    public RefreshToken()
    {
        Token = string.Empty;
        UserId = string.Empty;
    }
}
