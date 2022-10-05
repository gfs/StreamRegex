using System;
using System.Collections.Generic;
using System.IO;

namespace StreamRegex.Extensions.Core;

/// <summary>
/// Synchronous methods to perform arbitrary string check operations on <see cref="Stream"/> and <see cref="StreamReader"/>.
/// </summary>
public static class SlidingBufferExtensions
{
    /// <summary>
    /// Delegate for IsMatch methods.
    /// </summary>
    /// <param name="chunk">The section of the Stream currently being checked</param>
    /// <param name="delegateOptions">Options for delegate operation</param>
    public delegate bool IsMatchDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions);

    /// <summary>
    /// Check if a <see cref="Stream"/> matches the provided <paramref name="isMatchDelegate"/>
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="isMatchDelegate">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <paramref name="streamToMatch"/> matches the <paramref name="isMatchDelegate"/></returns>
    public static bool IsMatch(this Stream streamToMatch, IsMatchDelegate isMatchDelegate, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToMatch).IsMatch(isMatchDelegate, options);
    }

    /// <summary>
    /// Check if a <see cref="StreamReader"/> matches the provided <paramref name="isMatchDelegate"/>
    /// </summary>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to check for matches</param>
    /// <param name="isMatchDelegate">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <paramref name="streamReaderToMatch"/> matches the <paramref name="isMatchDelegate"/></returns>
    public static bool IsMatch(this StreamReader streamReaderToMatch, IsMatchDelegate isMatchDelegate, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new();
        var bufferSize = opts.BufferSize < opts.OverlapSize * 2 ? opts.OverlapSize * 2 : opts.BufferSize;
        // This is our string building buffer.
        Span<char> buffer = new(new char[bufferSize + opts.OverlapSize]);

        long offset = 0;
        var numChars = streamReaderToMatch.Read(buffer[..opts.BufferSize]);
        while (numChars > 0)
        {
            // The number of characters to read out of the builder
            // Characters after this are not valid for this read
            var numValidCharacters = offset > 0 ? numChars + opts.OverlapSize : numChars;
            if (isMatchDelegate.Invoke(buffer[..numValidCharacters], opts.DelegateOptions))
            {
                return true;
            }

            offset += numChars;
            // This is an indication there is no more to read.
            // It protects us from trying to take a negative sized slice below for small strings.
            if (numChars < opts.BufferSize)
            {
                break;
            }

            // Copy the overlap slice to the start of the buffer.
            buffer[(numValidCharacters - opts.OverlapSize)..numValidCharacters].CopyTo(buffer[..opts.OverlapSize]);
            // Read the new content after the overlap
            numChars = streamReaderToMatch.Read(buffer[opts.OverlapSize..]);
        }

        return false;
    }

    /// <summary>
    /// Delegate for GetFirstMatch methods.
    /// </summary>
    /// <param name="chunk">The section of the Stream currently being checked</param>
    /// <param name="delegateOptions">Options for delegate operation</param>
    public delegate SlidingBufferMatch GetFirstMatchDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions);

    /// <summary>
    /// Get the first match for a <see cref="Stream"/> from an Function.
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="getFirstMatchDelegate">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatch"/> object representing the match state of the first match.</returns>
    public static SlidingBufferMatch GetFirstMatch(this Stream streamToMatch, GetFirstMatchDelegate getFirstMatchDelegate, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToMatch).GetFirstMatch(getFirstMatchDelegate, options);
    }

    /// <summary>
    /// Get the first match for a <see cref="StreamReader"/> from an Function.
    /// </summary>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to check for matches</param>
    /// <param name="getFirstMatchDelegate">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatch"/> object representing the match state of the first match.</returns>
    public static SlidingBufferMatch GetFirstMatch(this StreamReader streamReaderToMatch, GetFirstMatchDelegate getFirstMatchDelegate, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new();
        var bufferSize = opts.BufferSize < opts.OverlapSize * 2 ? opts.OverlapSize * 2 : opts.BufferSize;
        // This is our string building buffer.
        Span<char> buffer = new(new char[bufferSize + opts.OverlapSize]);

        long offset = 0;
        var numChars = streamReaderToMatch.Read(buffer[..opts.BufferSize]);

        while (numChars > 0)
        {
            // The number of characters to read out of the builder
            // Characters after this are not valid for this read
            var numValidCharacters = offset > 0 ? numChars + opts.OverlapSize : numChars;
            var match = getFirstMatchDelegate.Invoke(buffer[..numValidCharacters], opts.DelegateOptions);
            if (match.Success)
            {
                match.Index += offset > 0 ? offset - opts.OverlapSize : 0;
                return match;
            }

            offset += numChars;
            // This is an indication there is no more to read.
            // It protects us from trying to take a negative sized slice below for small strings.
            if (numChars < opts.BufferSize)
            {
                break;
            }

            // Copy the overlap slice to the start of the buffer.
            buffer[(numValidCharacters - opts.OverlapSize)..numValidCharacters].CopyTo(buffer[..opts.OverlapSize]);
            // Read the new content after the overlap
            numChars = streamReaderToMatch.Read(buffer[opts.OverlapSize..]);
        }

        return new SlidingBufferMatch();
    }

    /// <summary>
    /// Delegate for GetMatchCollection methods.
    /// </summary>
    /// <param name="chunk">The section of the Stream currently being checked</param>
    /// <param name="delegateOptions">Options for delegate operation</param>
    public delegate SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollectionDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions);

    /// <summary>
    /// Get the all matches for a <see cref="Stream"/> from an Function.
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="getMatchCollectionDelegate">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <paramref name="streamToMatch"/>.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this Stream streamToMatch, GetMatchCollectionDelegate getMatchCollectionDelegate, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToMatch).GetMatchCollection(getMatchCollectionDelegate, options);
    }

    /// <summary>
    /// Get the all matches for a <see cref="StreamReader"/> from an Function.
    /// </summary>
    /// <param name="streamReaderToMatch"><see cref="StreamReader"/> to check for matches</param>
    /// <param name="getMatchCollectionDelegate">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <paramref name="streamReaderToMatch"/>.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this StreamReader streamReaderToMatch, GetMatchCollectionDelegate getMatchCollectionDelegate, SlidingBufferOptions? options = null)
    {
        SlidingBufferMatchCollection<SlidingBufferMatch> collection = new();

        var opts = options ?? new();

        var bufferSize = opts.BufferSize < opts.OverlapSize * 2 ? opts.OverlapSize * 2 : opts.BufferSize;
        // This is our string building buffer.
        Span<char> buffer = new(new char[bufferSize + opts.OverlapSize]);

        long offset = 0;
        var numChars = streamReaderToMatch.Read(buffer[..opts.BufferSize]);
        while (numChars > 0)
        {
            // The number of characters to read out of the builder
            // Characters after this are not valid for this read
            var numValidCharacters = offset > 0 ? numChars + opts.OverlapSize : numChars;
            var matches = getMatchCollectionDelegate.Invoke(buffer[..numValidCharacters], opts.DelegateOptions);
            foreach (SlidingBufferMatch match in matches)
            {
                // Adjust the match position
                match.Index += offset > 0 ? offset - opts.OverlapSize : 0;
                collection.Add(match);
            }
            offset += numChars;
            // This is an indication there is no more to read.
            // It protects us from trying to take a negative sized slice below for small strings.
            if (numChars < opts.BufferSize)
            {
                break;
            }

            // Copy the overlap slice to the start of the buffer.
            buffer[(numValidCharacters - opts.OverlapSize)..numValidCharacters].CopyTo(buffer[..opts.OverlapSize]);
            // Read the new content after the overlap
            numChars = streamReaderToMatch.Read(buffer[opts.OverlapSize..]);
        }

        return collection;
    }
}