namespace LinkCrawler.Models
{
    public class RequestModel
    {
        public readonly string Url;
        public readonly string ReferrerUrl;
        public bool IsInternalUrl { get; }

        public RequestModel(string url, string referrerUrl, string baseUrl)
        {
            Url = url;
            IsInternalUrl = url.StartsWith(baseUrl);
            ReferrerUrl = referrerUrl;
        }
    }
}
