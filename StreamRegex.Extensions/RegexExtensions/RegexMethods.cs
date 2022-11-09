using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StreamRegex.Extensions.Core;

namespace StreamRegex.Extensions.RegexExtensions;

/// <summary>
/// This class has the methods for the <see cref="StreamRegexExtensions"/> to use with the <see cref="SlidingBufferExtensions"/>
/// </summary>
internal class RegexMethods
{
    private readonly RegexCache _engines;

    internal RegexMethods(IEnumerable<Regex> engines)
    {
        _engines = engines is RegexCache cache ? cache : new RegexCache(engines);
    }

    internal SlidingBufferMatchCollection<SlidingBufferMatch> RegexGetMatchCollectionDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions)
    {
#if NET7_0_OR_GREATER
        SlidingBufferMatchCollection<SlidingBufferMatch> matchList = new();
        foreach (var engine in _engines)
        {
            Regex.ValueMatchEnumerator matches = engine.EnumerateMatches(chunk);
            foreach (ValueMatch match in matches)
            {
                matchList.Add(new StreamRegexMatch(engine, true, match.Index, match.Length, delegateOptions.CaptureValues ? chunk[match.Index..(match.Index + match.Length)].ToString() : null));
            }
        }
        return matchList;
#else
        SlidingBufferMatchCollection<SlidingBufferMatch> matchList = new();
        foreach (var engine in _engines)
        {
            MatchCollection matches = engine.Matches(chunk.ToString());
            foreach (Match match in matches)
            {
                matchList.Add(new StreamRegexMatch(engine, true, match.Index, match.Length, delegateOptions.CaptureValues ? chunk[match.Index..(match.Index + match.Length)].ToString() : null));
            }
        }
        return matchList;
#endif
    }

    internal bool RegexIsMatchDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions)
    {
        foreach (var engine in _engines)
        {
#if NET7_0_OR_GREATER
            if (engine.IsMatch(chunk))
#else
            if (engine.IsMatch(chunk.ToString()))
#endif
            {
                return true;
            }
        }

        return false;
    }

    internal SlidingBufferMatch RegexGetFirstMatchDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions)
    {
#if NET7_0_OR_GREATER
        foreach (var engine in _engines)
        {
            Regex.ValueMatchEnumerator matches = engine.EnumerateMatches(chunk);
            foreach (var match in matches)
            {
                return new StreamRegexMatch(engine, true, match.Index, match.Length, delegateOptions.CaptureValues ? chunk[match.Index..(match.Index + match.Length)].ToString() : null);
            }
        }

        return new StreamRegexMatch();
#else
        foreach (var engine in _engines)
        {
            MatchCollection matches = engine.Matches(chunk.ToString());
            foreach (Match match in matches)
            {
                return new StreamRegexMatch(engine, true, match.Index, match.Length, delegateOptions.CaptureValues ? chunk[match.Index..(match.Index + match.Length)].ToString() : null);
            }
        }

        return new StreamRegexMatch();
#endif
    }
}