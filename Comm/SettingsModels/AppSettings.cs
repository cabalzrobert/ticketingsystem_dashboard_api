namespace Comm.SettingsModels
{
    public class AppSettings
    {
        public string ImageDirectory { get; set; }
        public TokenSettings TokenSettings { get; set; }
    }

    public class TokenSettings
    {
        public string Issuer { get; set; }
        public string Key { get; set; }
    }
}
