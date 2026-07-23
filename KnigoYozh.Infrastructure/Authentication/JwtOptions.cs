namespace KnigoYozh.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "KnigoYozh";
    public string Audience { get; set; } = "KnigoYozh";
    public int ExpirationMinutes { get; set; } = 60;
}