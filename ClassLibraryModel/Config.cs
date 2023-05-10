namespace ClassLibraryModel
{
    public class DbConfig : IDbConfig
    {
        public string ConnectionString { get; set; }
        public string UserCollectionName { get; set; }
        public string DatabaseName { get; set; }
        public string UsersCN { get; set; }
        public string VerificationCodesCN { get; set; }
        public string FormsCN { get; set; }
    }
    public class JwtConfig : IJWTConfig
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
    public class BackendConfig : IBackendConfig
    {
        public string Scheme { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
    }
}
