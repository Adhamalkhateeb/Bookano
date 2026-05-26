namespace Bookano.Domain.Common
{
    public sealed class MailSettings
    {
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string TemplatesPath { get; set; } = null!;

        public string? DevelopmentOverrideRecipient { get; set; }
        public bool IsDevelopment { get; set; }
    }
}
