using RestSharp;

namespace LinkCrawler.Models
{
    public class RequestModel
    {
        public readonly string Url;
        public readonly string ReferrerUrl;
        public bool IsInternalUrl { get; }
        public RestClient Client;

        public RequestModel(string url, string referrerUrl, string baseUrl)
        {
            Url = url;
            IsInternalUrl = url.StartsWith(baseUrl);
            ReferrerUrl = referrerUrl;
            Client = new RestClient(Url);
        }
    }
}
