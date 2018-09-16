using LinkCrawler.Utils;
using StructureMap;
using System;
using LinkCrawler.Utils.Parsers;
using LinkCrawler.Utils.Settings;

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
                        int prefixLength = parsed.IndexOf("//") + 2;
                        parsed = parsed.Substring(0, parsed.Substring(prefixLength).IndexOf("/") + prefixLength);
                        linkCrawler.BaseUrl = parsed;
                        validUrlParser.BaseUrl = parsed;
                        linkCrawler.ValidUrlParser = validUrlParser;
                    }
                }
                linkCrawler.Start();
                // this line *has* to be here in order to flush the output to the console, otherwise we don't see anything - why?
                Console.Read();
            }
        }
    }
}
