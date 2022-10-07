using System;
using System.IO;

namespace StreamRegex.Extensions.Indexing;

/// <summary>
/// Extensions for getting a slice of a <see cref="Stream"/> or <see cref="StreamReader"/> relative to the current Position of the StreamReader.
/// </summary>
public static class IndexingExtensions
{
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
        Span<char> theSpan = new Span<char>(new char[endIndex-startIndex]);
        if (target.BaseStream.CanSeek)
        {
            target.BaseStream.Position = startIndex;
        }
        else
        {
            Span<char> overhead = new Span<char>(new char[startIndex]);
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
        Span<byte> theSpan = new Span<byte>(new byte[endIndex-startIndex]);
        if (target.CanSeek)
        {
            target.Position = startIndex;
        }
        else
        {
            Span<byte> overhead = new Span<byte>(new byte[startIndex]);
            _ = target.Read(overhead);
        }
        var readBytes = target.Read(theSpan);
        return theSpan[..readBytes];
    }
}