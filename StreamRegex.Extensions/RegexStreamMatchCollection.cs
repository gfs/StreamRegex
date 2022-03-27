using System.Collections;
using System.Collections.Concurrent;

namespace StreamRegex.Extensions;

/// <summary>
/// A collection holding <see cref="RegexStreamMatch"/>.
/// </summary>
public class RegexStreamMatchCollection : IEnumerable<RegexStreamMatch>
{
    private readonly ConcurrentQueue<RegexStreamMatch> _collection = new();

    internal RegexStreamMatchCollection(){}
    
    internal RegexStreamMatchCollection(IEnumerable<RegexStreamMatch> matches)
    {
        foreach (var match in matches)
        {
            _collection.Enqueue(match);
        }
    }

    /// <summary>
    /// Add a <see cref="RegexStreamMatch"/> to the collection.
    /// </summary>
    /// <param name="match">The match to add.</param>
    public void AddMatch(RegexStreamMatch match)
    {
        _collection.Enqueue(match);
    }
    
    /// <inheritdoc/>
    public IEnumerator<RegexStreamMatch> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }
    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    /// <summary>
    /// Add the matches in the provided <paramref name="matchCollection"/> to this collection.
    /// </summary>
    /// <param name="matchCollection">The matches to add</param>
    public void AddMatches(IEnumerable<RegexStreamMatch> matchCollection)
    {
        foreach (var match in matchCollection)
        {
            _collection.Enqueue(match);
        }
    }

    /// <summary>
    /// Update the index position of the matches in this collection by a specific offset and return the collection.
    /// </summary>
    /// <param name="offset">The offset to apply</param>
    /// <returns>This collection with the matches modified</returns>
    public RegexStreamMatchCollection WithOffset(long offset)
    {
        foreach (var regexStreamMatch in _collection)
        {
            regexStreamMatch.MatchPosition += offset;
        }

        return this;
    }
}