using LinkCrawler.Models;
using LinkCrawler.Utils.Settings;
using System;
using System.IO;

namespace LinkCrawler.Utils.Outputs
{
    public class CsvOutput : IOutput, IDisposable
    {
        private readonly ISettings _settings;
        private TextWriter _writer;

        public CsvOutput(ISettings settings)
        {
            _settings = settings;
            Setup();
        }

        private void Setup()
        {
            var fileMode = _settings.CsvOverwrite ? FileMode.Create : FileMode.Append;
            var file = new FileStream(_settings.CsvFilePath, fileMode, FileAccess.Write);

            var streamWriter = new StreamWriter(file) { AutoFlush = true };
            _writer = TextWriter.Synchronized(streamWriter);

            if (fileMode == FileMode.Create)
            {
                _writer.WriteLine("Code{0}Status{0}Url{0}Referer{0}Location{0}Error", _settings.CsvDelimiter);
            }
        }

        public void WriteError(IResponseModel responseModel)
        {
            Write(responseModel);
        }

        public void WriteInfo(IResponseModel responseModel)
        {
            Write(responseModel);
        }

        public void WriteInfo(string[] info)
        {
            // Do nothing - string info is only for console
        }

        private void Write(IResponseModel responseModel)
        {
            var reqUrl = responseModel.RequestedUrl;
            //if (_settings.CsvDelimiter == ";" && reqUrl.Contains("&amp;"))
            //{
            //    reqUrl = $"ERROR: delimited replaced by #!#: {reqUrl.Replace("&amp;", "#!#")}";
            //}
            var refUrl = responseModel.ReferrerUrl;
            //if (_settings.CsvDelimiter == ";" && refUrl.Contains("&amp;"))
            //{
            //    refUrl = $"ERROR: delimited replaced by #!#: {refUrl.Replace("&amp;", "#!#")}";
            //}
            var lineOut = $@"{responseModel.StatusCodeNumber}{_settings.CsvDelimiter}""{responseModel.StatusCode}""{_settings.CsvDelimiter}""{reqUrl}""{_settings.CsvDelimiter}""{refUrl}""{_settings.CsvDelimiter}""{responseModel.Location}""{_settings.CsvDelimiter}""{responseModel.ErrorMessage}""";
            _writer?.WriteLine(lineOut);
        }

        public void Dispose()
        {
            _writer?.Close();
            _writer?.Dispose();
        }
    }
}
