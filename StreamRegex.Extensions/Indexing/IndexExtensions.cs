using System;
using System.IO;

namespace StreamRegex.Extensions.Indexing;

/// <summary>
/// Extensions for getting a slice of a <see cref="Stream"/> or <see cref="StreamReader"/> relative to the current Position of the StreamReader.
/// </summary>
public static class IndexingExtensions
{

    /// <summary>
    /// Gets a Span of chars from the target StreamReader
    /// </summary>
    /// <param name="target"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">If the BaseStream of the StreamReader is not seekable</exception>
    public static Span<char> GetRange(this StreamReader target, SlidingBufferMatch match)
    {
        if (!match.Success || match.Index == -1 || match.Length == -1)
        {
            throw new ArgumentException("Match must be a success and have an Index and Length set.");
        }
        target.BaseStream.Position = 0;
        return target.GetRange(match.Index, match.Index + match.Length);
    }

    /// <summary>
    /// Gets a Span of chars from the specified range in the <paramref name="target"/> <see cref="StreamReader"/>
    ///
    /// If <paramref name="end"/> is longer than the actual length of <paramref name="target"/> the resulting Span will be the length of the actual characters from the stream without padding.
    /// If <paramref name="start"/> is Greater than <paramref name="end"/> the resulting Span will be 0 length.
    /// </summary>
    /// <param name="target">The StreamReader to select from</param>
    /// <param name="start">The 0 indexed start to read from</param>
    /// <param name="end">The 0 indexed end to read to</param>
    /// <returns>A span with the actual characters from the range.</returns>
    public static Span<char> GetRange(this StreamReader target, long start, long end)
    {
        if (start >= end)
        {
            return new Span<char>(Array.Empty<char>());
        }
        Span<char> theSpan = new Span<char>(new char[end-start]);
        if (target.BaseStream.CanSeek)
        {
            target.BaseStream.Position = start;
        }
        else
        {
            Span<char> overhead = new Span<char>(new char[start]);
            _ = target.Read(overhead);
        }
        var readBytes = target.Read(theSpan);
        return theSpan[..readBytes];
    }
    
    /// <summary>
    /// Get a <see cref="Span{T}"/> of <see cref="char"/> from the provided <paramref name="target"/> <see cref="StreamReader"/>
    /// </summary>
    /// <param name="target">The target <see cref="StreamReader"/></param>
    /// <param name="rangeToGet">The <see cref="Range"/> to get from the <paramref name="target"/></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException">If the End of the Range is outside the length of the underlying Stream from the StreamReader.</exception>
    public static Span<char> GetRange(this StreamReader target, Range rangeToGet)
    {
        var effectiveEnd = target.BaseStream.Length - target.BaseStream.Position;
        var endIndex = rangeToGet.End.IsFromEnd ? effectiveEnd - rangeToGet.End.Value : rangeToGet.End.Value;
        var startIndex = rangeToGet.Start.IsFromEnd ? effectiveEnd - rangeToGet.Start.Value : rangeToGet.Start.Value;
        if (endIndex > effectiveEnd)
        {
            throw new IndexOutOfRangeException();
        }

        return GetRange(target, startIndex, endIndex);
    }
    
    /// <summary>
    /// Gets a Span of bytes based on the match
    /// </summary>
    /// <param name="target"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">If the Stream is not seekable</exception>
    public static Span<byte> GetRange(this Stream target, SlidingBufferMatch match)
    {
        if (!match.Success || match.Index == -1 || match.Length == -1)
        {
            throw new ArgumentException("Match must be a success and have an Index and Length set.");
        }
        target.Position = 0;
        return target.GetRange(match.Index, match.Index + match.Length);
    }

    /// <summary>
    /// Gets a Span of bytes from the specified range in the <paramref name="target"/> <see cref="Stream"/>
    ///
    /// If <paramref name="end"/> is longer than the actual length of <paramref name="target"/> the resulting Span will be the length of the actual characters from the stream without padding.
    /// If <paramref name="start"/> is Greater than <paramref name="end"/> the resulting Span will be 0 length.
    /// </summary>
    /// <param name="target">The Stream to select from</param>
    /// <param name="start">The 0 indexed start to read from</param>
    /// <param name="end">The 0 indexed end to read to</param>
    /// <returns>A span with the actual bytes from the range.</returns>
    public static Span<byte> GetRange(this Stream target, long start, long end)
    {
        if (start >= end)
        {
            return new Span<byte>(Array.Empty<byte>());
        }
        Span<byte> theSpan = new Span<byte>(new byte[end-start]);
        if (target.CanSeek)
        {
            target.Position = start;
        }
        else
        {
            Span<byte> overhead = new Span<byte>(new byte[start]);
            _ = target.Read(overhead);
        }
        var readBytes = target.Read(theSpan);
        return theSpan[..readBytes];
    }
    
    /// <summary>
    /// Get a <see cref="Span{T}"/> of <see cref="byte"/> from the provided <paramref name="target"/> <see cref="Stream"/> relative to the current Position of the Stream.
    /// </summary>
    /// <param name="target">The target <see cref="Stream"/></param>
    /// <param name="rangeToGet">The <see cref="Range"/> to get from the <paramref name="target"/></param>
    /// <returns>A Span of bytes from the Stream</returns>
    /// <exception cref="IndexOutOfRangeException">If the End of the Range is outside the length of the Stream.</exception>
    public static Span<byte> GetRange(this Stream target, Range rangeToGet)
    {
        var effectiveEnd = target.Length - target.Position;
        var endIndex = rangeToGet.End.IsFromEnd ? effectiveEnd - rangeToGet.End.Value : rangeToGet.End.Value;
        var startIndex = rangeToGet.Start.IsFromEnd ? effectiveEnd - rangeToGet.Start.Value : rangeToGet.Start.Value;
        if (endIndex > effectiveEnd)
        {
            throw new IndexOutOfRangeException();
        }

        return GetRange(target, startIndex, endIndex);
    }
}