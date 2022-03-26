using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace StreamRegex.Lib.RegexStreamExtensions;

public class RegexCache : IEnumerable<Regex>
{
    public RegexCache(IEnumerable<Regex> regexes)
    {
        foreach (var regex in regexes)
        {
            _collection.Add(regex);
        }
    }

    public RegexCache()
    {
    }
    
    private readonly ConcurrentBag<Regex> _collection = new();
    public IEnumerator<Regex> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Match Match(string toString)
    {
        foreach (Regex regex in _collection)
        {
            var match = regex.Match(toString);
            if (match.Success)
            {
                return match;
            }
        }
        return System.Text.RegularExpressions.Match.Empty;
    }
    
    public RegexStreamMatchCollection GetMatchCollection(string toString)
    {
        RegexStreamMatchCollection collection = new();
        foreach (Regex engine in _collection)
        {
            var matches = engine.Matches(toString);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    collection.AddMatch(new RegexStreamMatch(engine, match.Success, match.Index, match.Value));
                }
            }
        }

        return collection;
    }
}