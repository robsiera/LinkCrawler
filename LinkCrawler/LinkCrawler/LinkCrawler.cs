using LinkCrawler.Models;
using LinkCrawler.Utils.Extensions;
using LinkCrawler.Utils.Helpers;
using LinkCrawler.Utils.Outputs;
using LinkCrawler.Utils.Parsers;
using LinkCrawler.Utils.Settings;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LinkCrawler
{
    public class LinkCrawler
    {
        public string BaseUrl { get; set; }
        private bool CheckImages { get; }
        private bool FollowRedirects { get; }
        private RestRequest GetRequest { get; }
        private RestClient Client { get; }
        private IEnumerable<IOutput> Outputs { get; }
        public IValidUrlParser ValidUrlParser { get; set; }
        private bool ReportOnRedirect { get; }
        private bool ReportOnSuccess { get; }

        public readonly List<LinkModel> UrlList;
        private readonly ISettings _settings;
        private readonly Stopwatch timer;

        public LinkCrawler(IEnumerable<IOutput> outputs, IValidUrlParser validUrlParser, ISettings settings)
        {
            BaseUrl = settings.BaseUrl;
            Outputs = outputs;
            ValidUrlParser = validUrlParser;
            CheckImages = settings.CheckImages;
            FollowRedirects = settings.FollowRedirects;
            UrlList = new List<LinkModel>();
            GetRequest = new RestRequest(Method.GET).SetHeader("Accept", "*/*");
            Client = new RestClient() { FollowRedirects = false }; // we don't want RestSharp following the redirects, otherwise we won't see them
            // https://stackoverflow.com/questions/8823349/how-do-i-use-the-cookie-container-with-restsharp-and-asp-net-sessions - set cookies up according to this link?

            ReportOnRedirect = settings.ReportOnRedirect;
            ReportOnSuccess = settings.ReportOnSuccess;

            _settings = settings;
            this.timer = new Stopwatch();
        }

        public void Start()
        {
            this.timer.Start();
            lock (UrlList)
            {
                UrlList.Add(new LinkModel(BaseUrl));
            }
            SendRequest(BaseUrl);
        }

        private void SendRequest(string crawlUrl, string referrerUrl = "")
        {
            var requestModel = new RequestModel(crawlUrl, referrerUrl, BaseUrl);
            Client.BaseUrl = new Uri(crawlUrl);

            var resp = Client.ExecuteAsync(GetRequest, response =>
           {
               if (response == null)
                   return;

               var responseModel = new ResponseModel(response, requestModel, _settings);
               ProcessResponse(responseModel);
           });
        }

        private void ProcessResponse(IResponseModel responseModel)
        {
            WriteOutput(responseModel);

            // follow 3xx redirects
            if (FollowRedirects && responseModel.IsRedirect)
                FollowRedirect(responseModel);

            // follow internal links in response
            if (responseModel.ShouldCrawl)
                CrawlLinksInResponse(responseModel);
        }

        private void FollowRedirect(IResponseModel responseModel)
        {
            string redirectUrl;
            if (responseModel.Location.StartsWith("/"))
                redirectUrl = responseModel.RequestedUrl.GetUrlBase() + responseModel.Location; // add base URL to relative links
            else
                redirectUrl = responseModel.Location;

            SendRequest(redirectUrl, responseModel.RequestedUrl);
        }

        public void CrawlLinksInResponse(IResponseModel responseModel)
        {
            var linksFoundInMarkup = MarkupHelpers.GetValidUrlListFromMarkup(responseModel.Markup, ValidUrlParser, CheckImages);

            SendRequestsToLinks(linksFoundInMarkup, responseModel.RequestedUrl);
        }

        private void SendRequestsToLinks(List<string> urls, string referrerUrl)
        {
            foreach (string url in urls)
            {
                lock (UrlList)
                {
                    if (UrlList.Count(l => l.Address == url) > 0)
                        continue;

                    UrlList.Add(new LinkModel(url));
                }
                SendRequest(url, referrerUrl);
            }
        }

        public void WriteOutput(IResponseModel responseModel)
        {
            if (responseModel.IsInteresting)
            {
                if (!responseModel.IsSuccess)
                {
                    foreach (var output in Outputs)
                    {
                        output.WriteError(responseModel);
                    }
                }
                else if (responseModel.IsRedirect && ReportOnRedirect)
                {
                    foreach (var output in Outputs)
                    {
                        output.WriteInfo(responseModel);
                    }
                }
                else
                {
                    if (ReportOnSuccess)
                    {
                        foreach (var output in Outputs)
                        {
                            output.WriteInfo(responseModel);
                        }
                    }
                    else
                    {
                        Console.Write("\r{0}%            ", responseModel);
                    }
                }
            }

            CheckIfFinal(responseModel);
        }

        private void CheckIfFinal(IResponseModel responseModel)
        {
            lock (UrlList)
            {

                // First set the status code for the completed link (this will set "CheckingFinished" to true)
                foreach (LinkModel lm in UrlList.Where(l => l.Address == responseModel.RequestedUrl))
                {
                    lm.StatusCode = responseModel.StatusCodeNumber;
                }

                // Then check to see whether there are any pending links left to check
                if (UrlList.Count > 1 && UrlList.Count(l => l.CheckingFinished == false) == 0)
                {
                    FinaliseSession();
                }
            }
        }

        private void FinaliseSession()
        {
            this.timer.Stop();
            if (this._settings.PrintSummary)
            {
                var messages = new List<string>();
                messages.Add(""); // add blank line to differentiate summary from main output

                messages.Add("Processing complete. Checked " + UrlList.Count() + " links in " + this.timer.ElapsedMilliseconds + "ms");

                messages.Add("");
                messages.Add(" Status | # Links");
                messages.Add(" -------+--------");

                IEnumerable<IGrouping<int, string>> statusSummary = UrlList.GroupBy(link => link.StatusCode, link => link.Address);
                foreach (IGrouping<int, string> statusGroup in statusSummary)
                {
                    messages.Add($"   {statusGroup.Key}  | {statusGroup.Count(),5}");
                }

                foreach (var output in Outputs)
                {
                    output.WriteInfo(messages.ToArray());
                }
            }
        }
    }
}