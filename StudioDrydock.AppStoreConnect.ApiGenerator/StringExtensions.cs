using System.Text;

namespace StudioDrydock.AppStoreConnect.ApiGenerator
{
    public static class StringExtensions
    {
        public static string TitleCase(this string @this)
        {
            return char.ToUpper(@this[0]) + @this.Substring(1);
        }

        // Sanitize an OpenAPI parameter like "fields[appClipHeaderImages]" into "fieldsAppClipHeaderImages"
        public static string MakeValidIdentifier(this string @this)
        {
            bool upper = false;
            var sb = new StringBuilder();
            foreach (var c in @this)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(upper ? char.ToUpper(c) : c);
                    upper = false;
                }
                else
                {
                    upper = true;
                }
            }
            return sb.ToString();
        }

        public static string MakeValidEnumIdentifier(this string @this)
        {
            if (@this.StartsWith("-"))
                @this = $"{@this.Substring(1)}Descending";
            return @this.MakeValidIdentifier();
        }
    }
}