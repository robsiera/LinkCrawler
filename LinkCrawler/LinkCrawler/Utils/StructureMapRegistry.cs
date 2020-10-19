using LinkCrawler.Utils.Outputs;
using LinkCrawler.Utils.Settings;
using StructureMap;
using StructureMap.Graph;
using System;
using System.Collections;
using System.Configuration;
using System.Linq;

namespace LinkCrawler.Utils
{
    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            var providers = (ConfigurationManager.GetSection(Constants.AppSettings.OutputProviders) as Hashtable)?
                .Cast<DictionaryEntry>()
                .ToDictionary(d => d.Key.ToString(), d => d.Value.ToString());

            if (providers != null)
            {
                var pluginType = typeof(IOutput);

                foreach (var provider in providers)
                {
                    var concreteType = Type.GetType(provider.Value);

                    if (concreteType == null)
                    {
                        throw new ConfigurationErrorsException($"Output provider '{provider.Key}' not found: {provider.Value}"); }

                    if (!concreteType.GetInterfaces().Contains(pluginType))
                    {
                        throw new ConfigurationErrorsException($"Output provider '{provider.Key}' does not implement IOutput: {provider.Value}");
                    }

                    For(pluginType).Add(concreteType).Named(provider.Key);
                }
            }
        }
    }
}
