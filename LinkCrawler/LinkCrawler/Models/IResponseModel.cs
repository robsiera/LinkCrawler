using System.Net;

namespace LinkCrawler.Models
{
    public interface IResponseModel
    {
        string Markup { get; }
        string RequestedUrl { get; }
        string ReferrerUrl { get; }
        string Location { get; }
        HttpStatusCode StatusCode { get; }
        string ErrorMessage { get; }
        int StatusCodeNumber { get; }
        bool IsSuccess { get; }
        bool IsInteresting { get; }
        bool IsRedirect{ get; }
        bool ShouldCrawl { get; }
        string ToString();
    }
}
