using System;
using System.Collections.Generic;
using System.IO;

namespace StreamRegex.Extensions;

/// <summary>
/// Synchronous methods to perform arbitrary string check operations on <see cref="Stream"/> and <see cref="StreamReader"/>.
/// </summary>
public static class SlidingBufferExtensions
{
    /// <summary>
    /// Check if a <see cref="Stream"/> matches the provided <paramref name="action"/>
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <paramref name="streamToMatch"/> matches the <paramref name="action"/></returns>
    public static bool IsMatch(this Stream streamToMatch, Func<string, bool> action, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToMatch).IsMatch(action, options);
    }

    /// <summary>
    /// Check if a <see cref="StreamReader"/> matches the provided <paramref name="action"/>
    /// </summary>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <paramref name="streamReaderToMatch"/> matches the <paramref name="action"/></returns>
    public static bool IsMatch(this StreamReader streamReaderToMatch, Func<string, bool> action, SlidingBufferOptions? options = null)
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
            if (action.Invoke(buffer[..numValidCharacters].ToString()))
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
    /// Get the first match for a <see cref="Stream"/> from an Function.
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatch"/> object representing the match state of the first match.</returns>
    public static SlidingBufferMatch GetFirstMatch(this Stream streamToMatch, Func<string, SlidingBufferMatch> action, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToMatch).GetFirstMatch(action, options);
    }
    
    /// <summary>
    /// Get the first match for a <see cref="StreamReader"/> from an Function.
    /// </summary>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatch"/> object representing the match state of the first match.</returns>
    public static SlidingBufferMatch GetFirstMatch(this StreamReader streamReaderToMatch, Func<string, SlidingBufferMatch> action, SlidingBufferOptions? options = null)
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
            var match = action.Invoke(buffer[..numValidCharacters].ToString());
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
    /// Get the all matches for a <see cref="Stream"/> from an Function.
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <paramref name="streamToMatch"/>.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this Stream streamToMatch, Func<string, IEnumerable<SlidingBufferMatch>> action, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToMatch).GetMatchCollection(action, options);
    }

    /// <summary>
    /// Get the all matches for a <see cref="StreamReader"/> from an Function.
    /// </summary>
    /// <param name="streamReaderToMatch"><see cref="StreamReader"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <paramref name="streamReaderToMatch"/>.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this StreamReader streamReaderToMatch, Func<string, IEnumerable<SlidingBufferMatch>> action, SlidingBufferOptions? options = null)
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
            var matches = action.Invoke(buffer[..numValidCharacters].ToString());
            foreach (SlidingBufferMatch match in matches)
            {
                // Adjust the match position
                match.Index += offset > 0 ? offset - opts.OverlapSize : 0;
                collection.AddMatch(match);
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