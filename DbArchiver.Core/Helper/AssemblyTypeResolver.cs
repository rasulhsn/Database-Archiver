using System.Reflection;

namespace DbArchiver.Core.Helper
{
    public static class AssemblyTypeResolver
    {
        public static Type ResolveByInterface<IInterface>(string assemblyName)
        {
            AssemblyName? assemblyNameInstance = Assembly.GetEntryAssembly().GetReferencedAssemblies()
                         .Single(x => x.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

            Assembly providerAssembly = Assembly.Load(assemblyNameInstance);

            Type? resolvedType = providerAssembly.GetTypes()
                         .Single(x => x.IsClass && !x.IsAbstract
                                    && x.IsAssignableTo(typeof(IInterface)));

            return resolvedType!;
        }
    }
}
