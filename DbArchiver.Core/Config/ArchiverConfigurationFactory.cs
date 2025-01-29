using DbArchiver.Core.Contract;
using DbArchiver.Core.Helper;
using DbArchiver.Provider.Common.Config;
using Microsoft.Extensions.Configuration;

namespace DbArchiver.Core.Config
{
    public class ArchiverConfigurationFactory : IArchiverConfigurationFactory
    {
        private readonly IConfiguration _configuration;

        public ArchiverConfigurationFactory(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public ArchiverConfiguration Create()
        {
            ArchiverConfiguration instance = new ArchiverConfiguration();

            var section = _configuration.GetSection($"{nameof(ArchiverConfiguration)}");

            var items = new List<ArchiverConfigurationItem>();

            foreach (var child in section.GetChildren())
            {
                var item = new ArchiverConfigurationItem
                {
                    JobSchedulerSettings = new JobSchedulerSettings
                    {
                        JobName = child.GetSection($"{nameof(ArchiverConfigurationItem.JobSchedulerSettings)}:{nameof(JobSchedulerSettings.JobName)}")
                            .Value,
                        Cron = child.GetSection($"{nameof(ArchiverConfigurationItem.JobSchedulerSettings)}:{nameof(JobSchedulerSettings.Cron)}")
                            .Value
                    },
                    TransferSettings = new TransferSettings
                    {
                        Source = new SourceProviderSettings
                        {
                            Provider = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Source)}:{nameof(SourceProviderSettings.Provider)}")
                                .Value,
                            Host = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Source)}:{nameof(SourceProviderSettings.Host)}")
                                .Value,
                            TransferQuantity = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Source)}:{nameof(SourceProviderSettings.TransferQuantity)}")
                                .Get<int>(),
                            DeleteAfterArchived = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Source)}:{nameof(SourceProviderSettings.DeleteAfterArchived)}")
                                .Get<bool>(),
                        },
                        Target = new TargetProviderSettings
                        {
                            Provider = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Target)}:{nameof(TargetProviderSettings.Provider)}")
                                .Value,
                            Host = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Target)}:{nameof(TargetProviderSettings.Host)}")
                                .Value,
                            PreScript = child.GetSection($"{nameof(TransferSettings)}:{nameof(TransferSettings.Target)}:{nameof(TargetProviderSettings.PreScript)}")
                                .Value
                        }
                    }
                };

                item.TransferSettings.Source.Settings = CreateSettings<ISourceSettings>(item.TransferSettings.Source.Provider,
                    child.GetSection($"{nameof(TransferSettings)}:Source:Settings")
                );

                item.TransferSettings.Target.Settings = CreateSettings<ITargetSettings>(item.TransferSettings.Target.Provider,
                    child.GetSection($"{nameof(TransferSettings)}:Target:Settings")
                );

                items.Add(item);
            }

            instance.Items = items;

            return instance;
        }

        private TInterface CreateSettings<TInterface>(string providerName, IConfigurationSection section) where TInterface : class
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Source provider name cannot be null or empty.");
            }

            var sourceType = ResolveType<TInterface>(providerName);
            return BindSettings<TInterface>(sourceType, section);
        }

        private static Type ResolveType<T>(string providerName)
        {
            var assemblyName = $"{Constants.PROVIDER_ASSEMBLY_PREFIX}{providerName}";
            var resolvedType = AssemblyTypeResolver.ResolveByInterface<T>(assemblyName);

            if (resolvedType == null)
            {
                throw new InvalidOperationException($"Could not resolve type for provider '{providerName}'.");
            }

            return resolvedType;
        }

        private static T BindSettings<T>(Type type, IConfigurationSection section) where T : class
        {
            var instance = Activator.CreateInstance(type);
            
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not create instance of type '{type.FullName}'.");
            }

            section.Bind(instance);
            return instance as T ?? throw new InvalidCastException($"Instance of type '{type.FullName}' could not be cast to '{typeof(T).FullName}'.");
        }
    }
}
