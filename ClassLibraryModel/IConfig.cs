namespace ClassLibraryModel
{
    public interface IJWTConfig
    {
        string SecretKey { get; set; }
        string Issuer { get; set; }
        string Audience { get; set; }
    }
    public interface IDbConfig
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string UsersCN { get; set; }
        string VerificationCodesCN { get; set; }
        string FormsCN { get; set; }
    }
    public interface IBackendConfig
    {
        string Scheme { get; set; }
        string HostName { get; set; }
        int Port { get; set; }
    }
}
