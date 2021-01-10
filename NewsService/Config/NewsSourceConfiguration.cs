namespace NewsService.Config
{
    public class NewsSourceConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string LinkPage { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public XPaths XPaths { get; set; } = new XPaths();
        public string[] PublishedAtPattern { get; set; } = new string[0];
    }
}