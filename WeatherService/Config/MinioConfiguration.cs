namespace NewsService.Config
{
    public class MinioConfiguration
    {
        public string MinioEndpoint { get; set; }
        public string StaticHostEndpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string FilePrefix { get; set; }
        public string BucketDirectory { get; set; }
    }
}