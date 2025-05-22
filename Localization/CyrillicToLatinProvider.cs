using System.Text;
using Newtonsoft.Json.Linq;

namespace DeploymentManagementSystem.Localization
{
    public static class CyrillicToLatinProvider
    {
        private static Dictionary<char, string> cyrillicToLatinMap = new()
        {
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
            {'е', "e"}, {'ё', "e"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
            {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
            {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
            {'у', "u"}, {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"},
            {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
            {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        public static string ToLatin(string input)
        {
            input = input.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (cyrillicToLatinMap.TryGetValue(c, out var latin))
                    sb.Append(latin);
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
