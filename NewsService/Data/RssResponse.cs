using System;
using NodaTime;

namespace NewsService.Data
{
    public struct RssResponse
    {
        public string Uri { get; set; }
        public string Title { get; set; }
        public ZonedDateTime PublishedAt { get; set; }
    }
}