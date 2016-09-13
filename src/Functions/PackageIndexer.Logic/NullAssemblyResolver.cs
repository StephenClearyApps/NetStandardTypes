using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace PackageIndexer.Logic
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
