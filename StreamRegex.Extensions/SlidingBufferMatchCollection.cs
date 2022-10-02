using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// A collection holding unique <see cref="SlidingBufferMatch"/> for a single resource. The matches are Records which are deduplicated automatically.
/// </summary>
public class SlidingBufferMatchCollection<T> : IEnumerable<T> where T : SlidingBufferMatch
{
    private readonly ConcurrentQueue<T> _collection = new();
    private readonly ConcurrentDictionary<T, bool> _deduper = new();
    internal SlidingBufferMatchCollection(){}
    
    internal SlidingBufferMatchCollection(IEnumerable<T> matches)
    {
        foreach (var match in matches)
        {
            _collection.Enqueue(match);
        }
    }

    /// <summary>
    /// Add a <see cref="SlidingBufferMatch"/> to the collection.  If the same match has already been added no-op.
    /// </summary>
    /// <param name="match">The match to add.</param>
    public void AddMatch(T match)
    {
        if (_deduper.TryAdd(match, true))
        {
            _collection.Enqueue(match);
        }
    }
    
    /// <summary>
    /// Add the matches in the provided <paramref name="matchCollection"/> to this collection. The added matches will be deduplicated.
    /// </summary>
    /// <param name="matchCollection">The matches to add</param>
    public void AddMatches(IEnumerable<T> matchCollection)
    {
        foreach (var match in matchCollection)
        {
            AddMatch(match);
        }
    }

    /// <summary>
    /// Update the index position of the matches in this collection by a specific offset and return the modified collection. Does not make a copy.
    /// </summary>
    /// <param name="offset">The offset to apply</param>
    /// <returns>This collection with the matches modified</returns>
    public SlidingBufferMatchCollection<T> WithOffset(long offset)
    {
        foreach (var slidingBufferMatch in _collection)
        {
            slidingBufferMatch.Index += offset;
        }

        return this;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}