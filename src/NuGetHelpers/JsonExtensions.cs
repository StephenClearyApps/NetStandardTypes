using Newtonsoft.Json.Linq;

namespace NuGetHelpers
{
    // https://github.com/NuGet/NuGet.Services.Metadata/blob/f21c218de5cc1d06bc47a7ae53632570329424c2/src/NuGet.Indexing/Extraction/JTokenExtensions.cs
    internal static class JTokenExtensions
    {
        public static JArray GetJArray(this JToken token, string key)
        {
            var array = token[key];
            if (array == null)
            {
                return new JArray();
            }

            if (!(array is JArray))
            {
                array = new JArray(array);
            }

            return (JArray) array;
        }
    }
}