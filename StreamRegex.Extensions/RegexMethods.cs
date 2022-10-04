using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace StreamRegex.Extensions;

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

    /// <summary>
    /// Used for Async methods which cannot use the Span based delegates
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    internal IEnumerable<StreamRegexMatch> RegexGetMatchCollectionFunction(string arg)
    {
        foreach (var engine in _engines)
        {
            MatchCollection matches = engine.Matches(arg);
            foreach (Match match in matches)
            {
                yield return new StreamRegexMatch(engine, true, match.Index, match.Value);
            }
        }
    }

    /// <summary>
    /// Used for Async methods which cannot use the Span based delegates
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    internal StreamRegexMatch RegexGetFirstMatchFunction(string arg)
    {
        foreach (var engine in _engines)
        {
            var match = engine.Match(arg);
            if (match.Success)
            {
                return new StreamRegexMatch(engine, true, match.Index, match.Value);
            }
        }

        return new StreamRegexMatch(null);
    }

    /// <summary>
    /// Used for Async methods which cannot use the Span based delegates
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    internal bool RegexIsMatchFunction(string arg)
    {
        foreach (var engine in _engines)
        {
            var match = engine.Match(arg);
            if (match.Success)
            {
                return true;
            }
        }

        return false;
    }
    
    internal SlidingBufferValueMatchCollection<SlidingBufferValueMatch> RegexGetMatchCollectionDelegate(ReadOnlySpan<char> chunk)
    {
#if NET7_0_OR_GREATER
        SlidingBufferValueMatchCollection<SlidingBufferValueMatch> matchList = new();
        foreach (var engine in _engines)
        {
            Regex.ValueMatchEnumerator matches = engine.EnumerateMatches(chunk);
            foreach (ValueMatch match in matches)
            {
                matchList.Add(new StreamRegexValueMatch(engine, true, match.Index, match.Length));
            }
        }
        return matchList;
#else
        SlidingBufferValueMatchCollection<SlidingBufferValueMatch> matchList = new();
        foreach (var engine in _engines)
        {
            MatchCollection matches = engine.Matches(chunk.ToString());
            foreach (Match match in matches)
            {
                matchList.Add(new StreamRegexValueMatch(engine, true, match.Index, match.Length));
            }
        }
        return matchList;
#endif
    }

    internal bool RegexIsMatchDelegate(ReadOnlySpan<char> chunk)
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

    internal SlidingBufferValueMatch RegexGetFirstMatchDelegate(ReadOnlySpan<char> chunk)
    {
#if NET7_0_OR_GREATER
        foreach (var engine in _engines)
        {
            Regex.ValueMatchEnumerator matches = engine.EnumerateMatches(chunk);
            foreach (var match in matches)
            {
                return new StreamRegexValueMatch(engine, true, match.Index, match.Length);
            }
        }

        return new StreamRegexValueMatch();
#else
        foreach (var engine in _engines)
        {
            MatchCollection matches = engine.Matches(chunk.ToString());
            foreach (Match match in matches)
            {
                return new StreamRegexValueMatch(engine, true, match.Index, match.Length);
            }
        }

        return new StreamRegexValueMatch();
#endif
    }
}