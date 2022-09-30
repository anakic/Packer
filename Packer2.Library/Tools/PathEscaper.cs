using System.Text;
using System.Text.RegularExpressions;

namespace Packer2.Library.Tools;

class PathEscaper
{
    private readonly char quoteStartChar;
    private readonly char quoteEndChar;
    private readonly char[] charsToQuote;

    public PathEscaper(char quoteStartChar = '%', char quoteEndChar = ';')
    {
        this.quoteStartChar = quoteStartChar;
        this.quoteEndChar = quoteEndChar;
        charsToQuote = Path.GetInvalidFileNameChars().Prepend(quoteStartChar).Prepend(quoteEndChar).ToArray();
    }

    public string UnescapeName(string name)
    {
        return Regex.Replace(name, $"{quoteStartChar}(?<code>\\d{{1,3}}){quoteEndChar}", m => ((char)int.Parse(m.Groups["code"].Value)).ToString());
    }

    public string EscapeName(string name)
    {
        var sb = new StringBuilder(name);
        foreach (var c in charsToQuote)
            sb.Replace(c.ToString(), $"{quoteStartChar}{(int)c}{quoteEndChar}");

        return sb.ToString();
    }
}
