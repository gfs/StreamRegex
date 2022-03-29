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
}