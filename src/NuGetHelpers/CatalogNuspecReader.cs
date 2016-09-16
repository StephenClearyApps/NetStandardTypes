using System;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;

namespace NuGetHelpers
{
    // Copied from https://github.com/NuGet/NuGet.Services.Metadata/blob/f21c218de5cc1d06bc47a7ae53632570329424c2/src/NuGet.Indexing/Extraction/CatalogNuspecReader.cs
    public class CatalogNuspecReader
        : NuspecReader, IDisposable
    {
        public MemoryStream NuspecStream { get; }

        public CatalogNuspecReader(JObject catalogItem)
            : base(CreateXDocument(catalogItem))
        {
            NuspecStream = new MemoryStream();
            CreateXDocument(catalogItem).Save(NuspecStream, SaveOptions.DisableFormatting);
            NuspecStream.Position = 0;
        }

        private static XDocument CreateXDocument(JObject catalogItem)
        {
            var document = new XDocument();
            var package = new XElement("metadata");
            var metadata = new XElement("metadata");

            // ID
            var id = new XElement("id", (string)catalogItem["id"]);

            // dependencies
            var dependencies = new XElement("dependencies");
            foreach (var group in catalogItem.GetJArray("dependencyGroups"))
            {
                var groupElement = new XElement("group");
                groupElement.SetAttributeValue("targetFramework", (string)group["targetFramework"]);
                foreach (var dependency in group.GetJArray("dependencies"))
                {
                    var dependencyElement = new XElement("dependency");
                    dependencyElement.SetAttributeValue("id", (string)dependency["id"]);
                    dependencyElement.SetAttributeValue("version", (string)dependency["range"]);
                    groupElement.Add(dependencyElement);
                }

                dependencies.Add(groupElement);
            }

            // framework assemblies
            var frameworkAssemblies = new XElement("frameworkAssemblies");
            foreach (var group in catalogItem.GetJArray("frameworkAssemblyGroup"))
            {
                var element = new XElement("frameworkAssembly");
                element.SetAttributeValue("targetFramework", (string)group["targetFramework"]);
                frameworkAssemblies.Add(element);
            }

            metadata.Add(id);
            metadata.Add(dependencies);
            metadata.Add(frameworkAssemblies);
            package.Add(metadata);
            document.Add(package);

            return document;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                NuspecStream?.Dispose();
            }
        }
    }
}
