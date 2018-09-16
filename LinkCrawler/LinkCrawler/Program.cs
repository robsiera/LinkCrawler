using LinkCrawler.Utils;
using StructureMap;
using System;
using LinkCrawler.Utils.Parsers;
using LinkCrawler.Utils.Settings;
using LinkCrawler.Utils.Extensions;

namespace LinkCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            
            using (var container = Container.For<StructureMapRegistry>())
            {
                var linkCrawler = container.GetInstance<LinkCrawler>();
                if (args.Length > 0)
                {
                    var validUrlParser = new ValidUrlParser(new Settings());
                    var result = validUrlParser.Parse(args[0], out string parsed);
                    if (result)
                    {
                        // make sure the base URL is just a domain
                        linkCrawler.BaseUrl = validUrlParser.BaseUrl = parsed.GetUrlBase();
                        linkCrawler.ValidUrlParser = validUrlParser;
                    }
                }
                linkCrawler.Start();
                // this line *has* to be here, because otherwise the app finishes before the asynchronous HTTP requests have returned
                Console.Read();
            }
        }
    }
}
