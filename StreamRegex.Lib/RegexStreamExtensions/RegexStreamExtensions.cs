using System.Text;
using System.Text.RegularExpressions;

namespace StreamRegex.Lib.RegexStreamExtensions;

/// <summary>
/// Match regexes in a stream up maximum length
/// </summary>
public static class RegexStreamExtensions
{
    private static int _minimumBufferSize = 4096;

    /// <summary>
    /// Find if a Stream matches a regular expression.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">The maximum length of string which will be matched. Has a large impact on performance.</param>
    /// <returns></returns>
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
            while (numChars > 0)
            {
                buffer.CopyTo(builder[maxMatchLength..]);
                if (engine.IsMatch(builder[..(maxMatchLength + numChars)].ToString()))
                {
                    return true;
                }

                numChars = toMatch.Read(buffer);
            }
        }
        return false;
    }
    
    /// <summary>
    /// Find if a Stream matches a regular expression.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">The maximum length of string which will be matched. Has a large impact on performance.</param>
    /// <returns></returns>
    public static RegexStreamMatch GetFirstMatch(this Regex engine, StreamReader toMatch, int maxMatchLength = 256)
    {
        var bufferSize = maxMatchLength * 2;
        bufferSize = bufferSize < _minimumBufferSize ? _minimumBufferSize : bufferSize;
        Span<char> buffer = new(new char[bufferSize]);
        Span<char> builder = new(new char[bufferSize + maxMatchLength]);
        var numChars = toMatch.Read(buffer);
        long offset = 0;
        if (numChars > 0)
        {
            buffer.CopyTo(builder);
            var match = engine.Match(builder[..numChars].ToString());
            if (match.Success)
            {
                return new RegexStreamMatch(engine, true, match.Index, match.Value);
            }
            offset += numChars;
            builder.Slice(numChars-maxMatchLength,maxMatchLength).CopyTo(builder[..maxMatchLength]);
            numChars = toMatch.Read(buffer);
            while (numChars > 0)
            {
                buffer.CopyTo(builder[maxMatchLength..]);
                match = engine.Match(builder[..(numChars + maxMatchLength)].ToString());
                if (match.Success)
                {
                    return new RegexStreamMatch(engine, true, match.Index + offset - maxMatchLength, match.Value);
                }
                offset += numChars;
                numChars = toMatch.Read(buffer);
            }
        }

        return new RegexStreamMatch(engine);
    }

    /// <summary>
    /// Find if a Stream matches a regular expression.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="maxMatchLength">The maximum length of string which will be matched. Has a large impact on performance.</param>
    /// <returns></returns>
    public static RegexStreamMatchCollection GetMatchCollection(this Regex engine, StreamReader toMatch,
        int maxMatchLength = 256)
    {
        var collection = new RegexStreamMatchCollection();
        var bufferSize = maxMatchLength * 2;
        bufferSize = bufferSize < _minimumBufferSize ? _minimumBufferSize : bufferSize;
        Span<char> buffer = new(new char[bufferSize]);
        Span<char> builder = new(new char[bufferSize + maxMatchLength]);
        var numChars = toMatch.Read(buffer);
        long offset = 0;
        if (numChars <= 0) return collection;
        buffer.CopyTo(builder);
        var match = engine.Match(builder[..numChars].ToString());
        if (match.Success)
        {
            collection.AddMatch(new RegexStreamMatch(engine, true, match.Index, match.Value));
        }

        offset += numChars;
        builder.Slice(numChars - maxMatchLength, maxMatchLength).CopyTo(builder[..maxMatchLength]);
        numChars = toMatch.Read(buffer);
        while (numChars > 0)
        {
            buffer.CopyTo(builder[maxMatchLength..]);
            match = engine.Match(builder[..(numChars + maxMatchLength)].ToString());
            if (match.Success)
            {
                collection.AddMatch(new RegexStreamMatch(engine, true, match.Index + offset - maxMatchLength, match.Value));
            }

            offset += numChars;
            numChars = toMatch.Read(buffer);
        }

        return collection;
    }

    public static RegexStreamMatchCollection GetMatchCollection(this RegexCache engines, StreamReader toMatch, int maxMatchLength = 256)
    {
        var collection = new RegexStreamMatchCollection();
        var bufferSize = maxMatchLength * 2;
        bufferSize = bufferSize < _minimumBufferSize ? _minimumBufferSize : bufferSize;
        Span<char> buffer = new(new char[bufferSize]);
        Span<char> builder = new(new char[bufferSize + maxMatchLength]);
        var numChars = toMatch.Read(buffer);
        long offset = 0;
        if (numChars <= 0) return collection;
        
        buffer.CopyTo(builder);
        collection.AddMatches(engines.GetMatchCollection(builder[..numChars].ToString()));
        offset += numChars;
        builder.Slice(numChars-maxMatchLength,maxMatchLength).CopyTo(builder[..maxMatchLength]);
        numChars = toMatch.Read(buffer);
        while (numChars > 0)
        {
            buffer.CopyTo(builder[maxMatchLength..]);
            collection.AddMatches(engines.GetMatchCollection(builder[..(numChars + maxMatchLength)].ToString()).WithOffset(offset - maxMatchLength));
            offset += numChars;
            numChars = toMatch.Read(buffer);
        }

        return collection;
    }
}