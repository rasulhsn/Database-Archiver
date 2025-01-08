using DbArchiver.Core.Factories;
using DbArchiver.Core.Helper;
using DbArchiver.Provider.Common.Config;
using Microsoft.Extensions.Configuration;

namespace DbArchiver.Core
{
    public class ArchiverConfigurationFactory : IArchiverConfigurationFactory
    {
        const string PROVIDER_ASSEMBLY_PREFIX = "DbArchiver.Provider.";

        private readonly IConfiguration _configuration;

        public ArchiverConfigurationFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ArchiverConfiguration Create()
        {
            var archiverConfiguration = new ArchiverConfiguration();

            var sourceSection = _configuration.GetSection($"{nameof(ArchiverConfiguration)}:Source");
            archiverConfiguration.Source = new SourceConfiguration
            {
                Provider = sourceSection.GetValue<string>(nameof(SourceConfiguration.Provider)),
                Host = sourceSection.GetValue<string>(nameof(SourceConfiguration.Host)),
                TransferQuantity = sourceSection.GetValue<int>(nameof(SourceConfiguration.TransferQuantity))
            };

            var targetSection = _configuration.GetSection($"{nameof(ArchiverConfiguration)}:Target");
            archiverConfiguration.Target = new TargetConfiguration
            {
                Provider = targetSection.GetValue<string>(nameof(TargetConfiguration.Provider)),
                Host = targetSection.GetValue<string>(nameof(TargetConfiguration.Host)),
                PreScript = targetSection.GetValue<string>(nameof(TargetConfiguration.PreScript))
            };

            archiverConfiguration.Source.Settings = BindSourceSettings(archiverConfiguration.Source.Provider, 
                                                                        sourceSection.GetSection(nameof(SourceConfiguration.Settings)));
            archiverConfiguration.Target.Settings = BindTargetSettings(archiverConfiguration.Target.Provider,
                                                                        targetSection.GetSection(nameof(TargetConfiguration.Settings)));

            return archiverConfiguration;
        }

        private ISourceSettings BindSourceSettings(string sourceProviderName, IConfigurationSection section)
        {
            var sourceType = AssemblyTypeResolver.ResolveByInterface<ISourceSettings>($"{PROVIDER_ASSEMBLY_PREFIX}{sourceProviderName}");

            object settingsInstance = Activator.CreateInstance(sourceType);

            section.Bind(settingsInstance);
            return settingsInstance as ISourceSettings;
        }

        private ITargetSettings BindTargetSettings(string targetProviderName, IConfigurationSection section)
        {
            var targetType = AssemblyTypeResolver.ResolveByInterface<ITargetSettings>($"{PROVIDER_ASSEMBLY_PREFIX}{targetProviderName}");

            object settingsInstance = Activator.CreateInstance(targetType);

            section.Bind(settingsInstance);
            return settingsInstance as ITargetSettings;
        }
    }
}
