namespace ECommerceAPI.Options
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = "E-Commerce Application";
        public string FromEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string AppPassword { get; set; } = string.Empty;
    }
}
