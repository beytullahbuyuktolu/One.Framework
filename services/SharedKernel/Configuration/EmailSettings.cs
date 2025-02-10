namespace SharedKernel.Configuration;

public class EmailSettings
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
