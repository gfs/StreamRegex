using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// This class can be used with the extension methods to check multiple regexes at once against the same StreamReader.
/// </summary>
public class RegexCache : IEnumerable<Regex>
{
    /// <summary>
    /// Construct a cache from existing regexes.
    /// </summary>
    /// <param name="regexes">The regexes to use</param>
    public RegexCache(IEnumerable<Regex> regexes)
    {
        foreach (var regex in regexes)
        {
            _collection.Add(regex);
        }
    }

    /// <summary>
    /// Construct an empty cache
    /// </summary>
    public RegexCache()
    {
    }
    
    private readonly ConcurrentBag<Regex> _collection = new();
    
    /// <inheritdoc/>
    public IEnumerator<Regex> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }
    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Get the first <see cref="Match"/> for a string.
    /// </summary>
    /// <param name="toCheck">The string to check</param>
    /// <returns>A <see cref="Match"/> object representing the first Match.</returns>
    public Match Match(string toCheck)
    {
        foreach (Regex regex in _collection)
        {
            var match = regex.Match(toCheck);
            if (match.Success)
            {
                return match;
            }
        }
        return System.Text.RegularExpressions.Match.Empty;
    }
    
    /// <summary>
    /// Get all the <see cref="RegexStreamMatch"/> for a string.
    /// </summary>
    /// <param name="toCheck">The string to check</param>
    /// <returns>A <see cref="RegexStreamMatchCollection"/> containing all the matches. This will be empty if there are no matches.</returns>
    public RegexStreamMatchCollection GetMatchCollection(string toCheck)
    {
        RegexStreamMatchCollection collection = new();
        foreach (Regex engine in _collection)
        {
            var matches = engine.Matches(toCheck);
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