﻿using LinkCrawler.Utils.Extensions;
using RestSharp;
using System;
using System.Net;
using LinkCrawler.Utils.Settings;

namespace LinkCrawler.Models
{
    public class ResponseModel : IResponseModel
    {
        public string Markup { get; }
        public string RequestedUrl { get; }
        public string ReferrerUrl { get; }
        public string Location { get; }

        public HttpStatusCode StatusCode { get; }
        public int StatusCodeNumber { get { return (int)StatusCode; } }
        public bool IsSuccess { get; }
        public bool ShouldCrawl { get; }
        public string ErrorMessage { get; }

        public ResponseModel(IRestResponse restResponse, RequestModel requestModel, ISettings settings)
        {
            ReferrerUrl = requestModel.ReferrerUrl;
            StatusCode = restResponse.StatusCode;
            RequestedUrl = requestModel.Url;
            Location = restResponse.GetHeaderByName("Location"); // returns null if no Location header present in the response
            ErrorMessage = restResponse.ErrorMessage;

            IsSuccess = settings.IsSuccess(StatusCode);
            if (!IsSuccess)
                return;
            Markup = restResponse.Content;
            ShouldCrawl = IsSuccess && requestModel.IsInternalUrl && restResponse.IsHtmlDocument();
        }

        public override string ToString()
        {
            if (!IsSuccess)
            {
                if (!String.IsNullOrEmpty(ErrorMessage))
                {
                    return $"{StatusCodeNumber}\t{StatusCode}\t{RequestedUrl}{Environment.NewLine}\tError:\t{ErrorMessage}{Environment.NewLine}\tReferer:\t{ReferrerUrl}";
                }
                else
                {
                    return $"{StatusCodeNumber}\t{StatusCode}\t{RequestedUrl}{Environment.NewLine}\tReferer:\t{ReferrerUrl}";
                }
            }
            else
            {
                if (StatusCodeNumber == 301 || StatusCodeNumber == 302)
                {
                    return $"{StatusCodeNumber}\t{StatusCode}\t{RequestedUrl}{Environment.NewLine}\t->\t{Location}";
                }
                else
                {
                    return $"{StatusCodeNumber}\t{StatusCode}\t{RequestedUrl}";
                }
            }
        }
    }
}