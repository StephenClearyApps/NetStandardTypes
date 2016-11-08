using System.IO;
using Mono.Cecil;

namespace NetStandardTypes.PackageIndexer
{
    public sealed class NullAssemblyResolver : IAssemblyResolver
    {
        private readonly TextWriter _log;

        public NullAssemblyResolver(TextWriter log)
        {
            _log = log;
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return Resolve(AssemblyNameReference.Parse(fullName), new ReaderParameters());
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, new ReaderParameters());
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            _log.WriteLine("Unable to resolve assembly " + name.FullName + ".");
            return null;
        }
    }
}
