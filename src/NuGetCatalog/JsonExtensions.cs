using System;
using Newtonsoft.Json.Linq;

namespace NuGetHelpers
{
    public static class JTokenExtensions
    {
        public static DateTimeOffset? GetDateTimeOffset(this JToken token, string key)
        {
            var child = token[key];
            if (child == null)
            {
                return null;
            }

            string value;
            try
            {
                value = (string) child;
            }
            catch
            {
                return null;
            }

            DateTimeOffset result;
            if (!DateTimeOffset.TryParse(value, out result))
                return null;
            return result;
        }
    }
}