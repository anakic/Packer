using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Customizations
{
    public enum TargetAction
    {
        Ignore,
        Keep
    }

    public record IgnoreRule(string TablePattern, string? ObjectPattern, TargetAction Action);

    public class IgnoreFileParser
    {
        Regex regex = new Regex(@"^(?'invert'!?)('(?'tableNamePattern'[^]]*)'|(?'tableNamePattern'[^]]*))(\[(?'objectNamePattern'[^]]*)\])?$", RegexOptions.Compiled);

        public IEnumerable<IgnoreRule> Parse(string inputText)
        {
            var lines = inputText.Split(Environment.NewLine);

            int lineNr = 0;
            foreach (var line in lines)
            {
                lineNr++;

                if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    continue;

                var m = regex.Match(line);
                if (!m.Success)
                    throw new FormatException($"Failed to parse ignore file, line {lineNr}");

                var tableNamePattern = m.Groups["tableNamePattern"].Value;
                string? objectNamePattern = null;
                var objectNamePatternGroup = m.Groups["objectNamePattern"];
                if (objectNamePatternGroup.Success)
                    objectNamePattern = objectNamePatternGroup.Value;

                TargetAction action = m.Groups["invert"].Value switch 
                {
                    "!" => TargetAction.Ignore,
                    _ => TargetAction.Keep
                };

                yield return new IgnoreRule(tableNamePattern, objectNamePattern, action);
            }
        }
    }

}
