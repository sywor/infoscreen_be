using System.Collections.Generic;

using Newtonsoft.Json;

namespace NewsService.Data
{
    [JsonObject]
    public class JOrigin
    {
        [JsonProperty("streamId")]
        public string? StreamId { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("htmlUrl")]
        public string? HtmlUrl { get; set; }
    }

    [JsonObject]
    public class JSummary
    {
        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("direction")]
        public string? Direction { get; set; }
    }

    [JsonObject]
    public class JAlternate
    {
        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }
    }

    [JsonObject]
    public class JVisual
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("expirationDate")]
        public long ExpirationDate { get; set; }

        [JsonProperty("edgeCacheUrl")]
        public string? EdgeCacheUrl { get; set; }

        [JsonProperty("processor")]
        public string? Processor { get; set; }

        [JsonProperty("contentType")]
        public string? ContentType { get; set; }
    }

    [JsonObject]
    public class JCategory
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("label")]
        public string? Label { get; set; }
    }

    [JsonObject]
    public class JEnclosure
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    [JsonObject]
    public class JContent
    {
        [JsonProperty("content")]
        public string?Content { get; set; }

        [JsonProperty("direction")]
        public string? Direction { get; set; }
    }

    [JsonObject]
    public class JMeme
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("label")]
        public string? Label { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }
    }

    [JsonObject]
    public class JWebfeeds
    {
        [JsonProperty("relatedLayout")]
        public string? RelatedLayout { get; set; }

        [JsonProperty("relatedTarget")]
        public string? RelatedTarget { get; set; }

        [JsonProperty("logo")]
        public string?Logo { get; set; }

        [JsonProperty("analyticsEngine")]
        public string? AnalyticsEngine { get; set; }

        [JsonProperty("analyticsId")]
        public string? AnalyticsId { get; set; }

        [JsonProperty("accentColor")]
        public string? AccentColor { get; set; }

        [JsonProperty("wordmark")]
        public string? Wordmark { get; set; }
    }

    [JsonObject]
    public class JThumbnail
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    [JsonObject]
    public class JCanonical
    {
        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }
    }

    [JsonObject]
    public class JItem
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("originId")]
        public string? OriginId { get; set; }

        [JsonProperty("fingerprint")]
        public string? Fingerprint { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("crawled")]
        public long Crawled { get; set; }

        [JsonProperty("origin")]
        public JOrigin? Origin { get; set; }

        [JsonProperty("published")]
        public long Published { get; set; }

        [JsonProperty("summary")]
        public JSummary? Summary { get; set; }

        [JsonProperty("alternate")]
        public List<JAlternate>? Alternate { get; set; }

        [JsonProperty("visual")]
        public JVisual? Visual { get; set; }

        [JsonProperty("canonicalUrl")]
        public string? CanonicalUrl { get; set; }

        [JsonProperty("unread")]
        public bool Unread { get; set; }

        [JsonProperty("categories")]
        public List<JCategory>? Categories { get; set; }

        [JsonProperty("engagement")]
        public int Engagement { get; set; }

        [JsonProperty("engagementRate")]
        public double EngagementRate { get; set; }

        [JsonProperty("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("enclosure")]
        public List<JEnclosure>? Enclosure { get; set; }

        [JsonProperty("ampUrl")]
        public string? AmpUrl { get; set; }

        [JsonProperty("cdnAmpUrl")]
        public string? CdnAmpUrl { get; set; }

        [JsonProperty("content")]
        public JContent? Content { get; set; }

        [JsonProperty("recrawled")]
        public long Recrawled { get; set; }

        [JsonProperty("updateCount")]
        public int UpdateCount { get; set; }

        [JsonProperty("memes")]
        public List<JMeme>? Memes { get; set; }

        [JsonProperty("webfeeds")]
        public JWebfeeds? Webfeeds { get; set; }

        [JsonProperty("thumbnail")]
        public List<JThumbnail>? Thumbnail { get; set; }

        [JsonProperty("canonical")]
        public List<JCanonical>? Canonical { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }
    }

    [JsonObject]
    public class JFeedlyArticleStream
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonProperty("continuation")]
        public string? Continuation { get; set; }

        [JsonProperty("items")]
        public List<JItem>? Items { get; set; }
    }
}