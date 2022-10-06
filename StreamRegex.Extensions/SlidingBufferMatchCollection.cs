using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StreamRegex.Extensions;

/// <summary>
/// A collection holding unique <see cref="SlidingBufferMatch"/>. The backing collection is thread-safe and deduplicated.
/// </summary>
/// <typeparam name="T">The type must inherit from <see cref="SlidingBufferMatch"/></typeparam>
public class SlidingBufferMatchCollection<T> : ICollection<T>, IReadOnlyCollection<T> where T : SlidingBufferMatch
{
    private readonly ConcurrentDictionary<T, bool> _backingCollection = new();

    /// <summary>
    /// Remove the provided item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>True if the item was removed</returns>
    public bool Remove(T item)
    {
        return _backingCollection.Remove(item, out bool _);
    }

    /// <summary>
    /// The number of Matches in the collection
    /// </summary>
    public int Count => _backingCollection.Keys.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <summary>
    /// This collection is thread-safe.
    /// </summary>
    public bool IsSynchronized => true;

    /// <summary>
    /// Not supported.
    /// </summary>
    public object SyncRoot => throw new NotSupportedException();

    public SlidingBufferMatchCollection() { }

    /// <summary>
    /// Gets an <see cref="IEnumerable{SlidingBufferMatch}"/> over the <see cref="SlidingBufferMatch"/> in the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{SlidingBufferMatch}"/> over the <see cref="SlidingBufferMatch"/> in the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _backingCollection.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Add a <see cref="SlidingBufferMatch"/> to the collection.  If the same match has already been added no-op.
    /// </summary>
    /// <param name="item">The match to add.</param>
    public void Add(T item)
    {
        _backingCollection.TryAdd(item, true);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _backingCollection.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(T item) => _backingCollection.ContainsKey(item);

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => _backingCollection.Keys.CopyTo(array, arrayIndex);
}