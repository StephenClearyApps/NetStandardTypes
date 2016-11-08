using System.Linq;
using Mono.Cecil;

namespace NetStandardTypes.PackageIndexer
{
    public static class CecilExtensions
    {
        public static string TypeName(this TypeDefinition type)
        {
            var name = type.Name.StripBacktickSuffix();
            var result = name.Name;
            if (name.Value > 0)
                result += "<" + string.Join(",", type.GenericParameters.Take(name.Value).Select(x => x.Name)) + ">";
            return result;
        }
    }
}
