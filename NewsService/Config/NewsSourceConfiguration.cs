namespace NewsService.Config
{
    public class NewsSourceConfiguration
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string LinkPage { get; set; }
        public string Type { get; set; }
        public XPaths XPaths { get; set; }
        public string[] PublishedAtPattern { get; set; }
    }
}