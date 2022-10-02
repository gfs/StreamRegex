using System.Collections.Generic;
using System.Text.RegularExpressions;

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
}