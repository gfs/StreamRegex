﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StreamRegex.Extensions;

/// <summary>
/// A collection holding unique <see cref="SlidingBufferValueMatch"/>. The backing collection is threadsafe.
/// </summary>
/// <typeparam name="T">The type must inherit from <see cref="SlidingBufferValueMatch"/></typeparam>
public class SlidingBufferValueMatchCollection<T> : IEnumerable<T>, ICollection, IReadOnlyCollection<T> where T : SlidingBufferValueMatch
{
    private readonly ConcurrentQueue<T> _collection = new();
    private readonly ConcurrentDictionary<T, bool> _deduper = new();

    /// <inheritdoc/>
    public int Count => _collection.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public bool IsSynchronized => true;

    /// <summary>
    /// Not supported.
    /// </summary>
    public object SyncRoot => throw new NotSupportedException();

    internal SlidingBufferValueMatchCollection(){}

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
    /// <returns>This <see cref="SlidingBufferMatchCollection{T}"/> with the match indices modified.</returns>
    public SlidingBufferValueMatchCollection<T> WithOffset(long offset)
    {
        foreach (var slidingBufferMatch in _collection)
        {
            slidingBufferMatch.Index += offset;
        }

        return this;
    }

    /// <summary>
    /// Gets an <see cref="IEnumerable{SlidingBufferMatch}"/> over the <see cref="SlidingBufferMatch"/> in the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{SlidingBufferMatch}"/> over the <see cref="SlidingBufferMatch"/> in the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        if (_deduper.TryAdd(item, true))
        {
            _collection.Enqueue(item);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _collection.Clear();
        _deduper.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(T item) => _deduper.ContainsKey(item);

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public void CopyTo(Array array, int index) => _collection.CopyTo((T[])array, index);
}