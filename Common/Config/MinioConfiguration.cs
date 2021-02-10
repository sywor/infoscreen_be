namespace Common.Config
{
    public class MinioConfiguration
    {
        public string? MinioEndpoint { get; set; }
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
        public string? EphemeralBucketName { get; set; }
        
        public string? StaticBucketName { get; set; }
        public string? BucketDirectory { get; set; }
    }
}