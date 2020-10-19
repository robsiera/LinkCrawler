using LinkCrawler.Utils.Settings;
using RestSharp;

namespace LinkCrawler.Utils.Extensions
{
    public static class RestResponseExtensions
    {
        public static bool IsHtmlDocument(this IRestResponse restResponse)
        {
            return restResponse.ContentType.StartsWith(Constants.Response.ContentTypeTextHtml);
        }

        public static string GetHeaderByName(this IRestResponse restResponse, string headerName)
        {
            foreach (Parameter header in restResponse.Headers)
            {
                if (header.Name.ToLower() == headerName.ToLower()) return header.Value.ToString();
            }
            return null;
        }
    }
}
