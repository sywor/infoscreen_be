using System;
using NodaTime;

namespace NewsService.Data
{
    public struct ArticleLinkResponse
    {
        public string Uri { get; set; }
        public string Title { get; set; }
        public ZonedDateTime PublishedAt { get; set; }
    }
}