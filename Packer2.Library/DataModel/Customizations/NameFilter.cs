using System.Text;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Customizations
{
    public class NameFilter
    {
        Regex regex;

        public NameFilter(string pattern)
        {
            var regexPattern = ConvertToRegexPattern(pattern);
            regex = new Regex(regexPattern, RegexOptions.Compiled);
        }

        public bool IsMatch(string str)
            => regex.IsMatch(str);

        private string ConvertToRegexPattern(string pattern)
        {
            // currently only supporting '*' wildcard (not supporting '?' at the moment)
            var sections = Regex.Split(pattern, @"(?<!\\)\*");

            var regexSections = sections.Select(sect =>
            {
                sect = Regex.Replace(sect, @"\\(.)", "$1");

                StringBuilder sb = new StringBuilder();
                // convert each char a [lU] set, e.g. a => [aA]
                foreach (var c in sect)
                {
                    if (c == '\\')
                        sb.Append(@"\\");
                    else if(char.IsLetter(c))
                        sb.AppendFormat("[{0}{1}]", char.ToUpper(c), char.ToLower(c));
                    else
                        sb.AppendFormat("[{0}]", c);
                }
                return sb.ToString();
            });

            return $"^{string.Join(".*", regexSections)}$";
        }
    }
}
