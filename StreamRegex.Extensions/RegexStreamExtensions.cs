using System.Collections;
using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// Extends the <see cref="Regex"/> class with functionality to check against <see cref="StreamReader"/>.
/// </summary>
public static class RegexStreamExtensions
{
    private static int _minimumBufferSize = 4096;

    /// <summary>
    /// Find if a StreamReader matches a regular expression.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A <see cref="RegexStreamMatch"/> object representing the first match. If there is no match, the <see cref="RegexStreamMatch.Matches"/> will be false, the <see cref="RegexStreamMatch.MatchContent"/> will be null.</returns>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this Regex engine, StreamReader toMatch, int maxMatchLength = 256)
    {
        var bufferSize = maxMatchLength * 2;
        bufferSize = bufferSize < _minimumBufferSize ? _minimumBufferSize : bufferSize;
        Span<char> buffer = new(new char[bufferSize]);
        Span<char> builder = new(new char[bufferSize + maxMatchLength]);
        var numChars = toMatch.Read(buffer);
        if (numChars > 0)
        {
            buffer.CopyTo(builder);
            if (engine.IsMatch(builder[..numChars].ToString()))
            {
                return true;
            }
            numChars = toMatch.Read(buffer);
            builder.Slice(numChars-maxMatchLength,maxMatchLength).CopyTo(builder[..maxMatchLength]);
            while (numChars > 0)
            {
                buffer.CopyTo(builder[maxMatchLength..]);
                if (engine.IsMatch(builder[..(maxMatchLength + numChars)].ToString()))
                {
                    return true;
                }
                builder.Slice(numChars-maxMatchLength,maxMatchLength).CopyTo(builder[..maxMatchLength]);
                numChars = toMatch.Read(buffer);
            }
        }
        return false;
    }
    
    /// <summary>
    /// Find the first match for a <see cref="Regex"/> in a <see cref="Stream"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A <see cref="RegexStreamMatch"/> object representing the first match. If there is no match, the <see cref="RegexStreamMatch.Matches"/> will be false, the <see cref="RegexStreamMatch.MatchContent"/> will be null.</returns>
    public static RegexStreamMatch GetFirstMatch(this Regex engine, Stream toMatch, int maxMatchLength = 256)
    {
        using var reader = new StreamReader(toMatch);
        return engine.GetFirstMatch(toMatch, maxMatchLength);
    }
    
    /// <summary>
    /// Find if a StreamReader matches a regular expression.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A <see cref="RegexStreamMatch"/> object representing the first match. If there is no match, the <see cref="RegexStreamMatch.Matches"/> will be false, the <see cref="RegexStreamMatch.MatchContent"/> will be null.</returns>
    public static RegexStreamMatch GetFirstMatch(this Regex engine, StreamReader toMatch, int maxMatchLength = 256)
    {
        return EnumerateMatches(engine, toMatch, maxMatchLength).FirstOrDefault() ?? new RegexStreamMatch(engine);
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A <see cref="RegexStreamMatchCollection"/> object representing all matches. This object will be empty if there are no matches.</returns>
    public static RegexStreamMatchCollection GetMatchCollection(this Regex engine, StreamReader toMatch, int maxMatchLength = 256)
    {
        var collection = new RegexStreamMatchCollection();
        collection.AddMatches(engine.EnumerateMatches(toMatch, maxMatchLength));

        return collection;
    }
    
    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A true forward only enumeration of the matches.</returns>
    public static IEnumerable<RegexStreamMatch> EnumerateMatches(this Regex engine, StreamReader toMatch, int maxMatchLength = 256)
    {
        var collection = new RegexStreamMatchCollection();
        var bufferSize = maxMatchLength * 2;
        bufferSize = bufferSize < _minimumBufferSize ? _minimumBufferSize : bufferSize;
        Span<char> buffer = new(new char[bufferSize]);
        Span<char> builder = new(new char[bufferSize + maxMatchLength]);
        var numChars = toMatch.Read(buffer);
        long offset = 0;
        if (numChars <= 0)  yield break;
        buffer.CopyTo(builder);

        while (numChars > 0)
        {
            var numToUse = numChars;
            if (offset > 0)
            {
                buffer.CopyTo(builder[maxMatchLength..]);
                numToUse += maxMatchLength;
            }
            var match = engine.Match(builder[..numToUse].ToString());
            if (match.Success)
            {
                yield return new RegexStreamMatch(engine, true, match.Index, match.Value);
            }

            int sliceStart = offset == 0 ? numChars - maxMatchLength : numChars;
            offset += numChars;
            builder.Slice(sliceStart, maxMatchLength).CopyTo(builder[..maxMatchLength]);
            numChars = toMatch.Read(buffer);
        }
    }
    
    /// <summary>
    /// Find all matches for the engines in the given <see cref="RegexCache"/>
    /// </summary>
    /// <param name="engines">The Regexes to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A <see cref="RegexStreamMatchCollection"/> object representing all matches. This object will be empty if there are no matches.</returns>
    public static RegexStreamMatchCollection GetMatchCollection(this RegexCache engines, StreamReader toMatch, int maxMatchLength = 256)
    {
        var collection = new RegexStreamMatchCollection();
        collection.AddMatches(EnumerateMatches(engines, toMatch, maxMatchLength));

        return collection;
    }

    /// <summary>
    /// Find all matches for a given <see cref="RegexCache"/>
    /// </summary>
    /// <param name="engines">The Regexes to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">Matches longer than this value may not be matched. This parameter changes the size of the sliding buffer used. The larger this parameter is the more double checks will be done.</param>
    /// <returns>A true forward only enumeration of the matches.</returns>
    public static IEnumerable<RegexStreamMatch> EnumerateMatches(this RegexCache engines, StreamReader toMatch, int maxMatchLength = 256)
    {
        var collection = new RegexStreamMatchCollection();
        var bufferSize = maxMatchLength * 2;
        bufferSize = bufferSize < _minimumBufferSize ? _minimumBufferSize : bufferSize;
        Span<char> buffer = new(new char[bufferSize]);
        Span<char> builder = new(new char[bufferSize + maxMatchLength]);
        var numChars = toMatch.Read(buffer);
        long offset = 0;
        if (numChars <= 0)  yield break;
        buffer.CopyTo(builder);

        while (numChars > 0)
        {
            var numToUse = numChars;
            if (offset > 0)
            {
                buffer.CopyTo(builder[maxMatchLength..]);
                numToUse += maxMatchLength;
            }
            foreach (var engine in engines)
            {
                var match = engines.Match(builder[..numToUse].ToString());
                if (match.Success)
                {
                    yield return new RegexStreamMatch(engine, true, match.Index, match.Value);
                }
            }

            int sliceStart = offset == 0 ? numChars - maxMatchLength : numChars;
            offset += numChars;
            builder.Slice(sliceStart, maxMatchLength).CopyTo(builder[..maxMatchLength]);
            numChars = toMatch.Read(buffer);
        }
    }
}