using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// A bag of <see cref="Regex"/> to use with the extension methods.
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

    /// <summary>
    /// Gets an <see cref="IEnumerable{Regex}"/> over the <see cref="Regex"/> in the collection.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Regex> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}