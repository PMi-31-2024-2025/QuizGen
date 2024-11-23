public class StoredCredentials
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
} 