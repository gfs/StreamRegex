using System.Collections;
using System.Collections.Concurrent;

namespace StreamRegex.Extensions;

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

    public void AddMatch(RegexStreamMatch match)
    {
        _collection.Enqueue(match);
    }
    
    public IEnumerator<RegexStreamMatch> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddMatches(RegexStreamMatchCollection matchCollection)
    {
        foreach (var match in matchCollection)
        {
            _collection.Enqueue(match);
        }
    }

    /// <summary>
    /// Update the index position of the matches in this collection by a specific offset
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public RegexStreamMatchCollection WithOffset(long offset)
    {
        foreach (var regexStreamMatch in _collection)
        {
            regexStreamMatch.MatchPosition += offset;
        }

        return this;
    }
}