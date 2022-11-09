using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StreamRegex.Extensions.Core;

/// <summary>
/// Asyncronous Extension methods for matching <see cref="Stream"/> and <see cref="StreamReader"/>.
/// </summary>
public static class SlidingBufferExtensionsAsync
{
    /// <summary>
    /// Check if a <see cref="Stream"/> matches the provided <paramref name="action"/>
    /// </summary>
    /// <param name="tomatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <paramref name="tomatch"/> matches the <paramref name="action"/>></returns>
    public static async Task<bool> IsMatchAsync(this Stream tomatch, SlidingBufferExtensions.IsMatchDelegate action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(tomatch, Encoding.Default, true, 4096, true).IsMatchAsync(action, options);
    }

    /// <summary>
    /// Check if a <see cref="StreamReader"/> matches the provided <paramref name="action"/>
    /// </summary>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <paramref name="streamReaderToMatch"/> matches the <paramref name="action"/></returns>
    public static async Task<bool> IsMatchAsync(this StreamReader streamReaderToMatch, SlidingBufferExtensions.IsMatchDelegate action, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new();
        var bufferSize = opts.BufferSize < opts.OverlapSize * 2 ? opts.OverlapSize * 2 : opts.BufferSize;
        // This is our string building buffer.
        Memory<char> buffer = new(new char[bufferSize + opts.OverlapSize]);

        long offset = 0;
        var numChars = await streamReaderToMatch.ReadAsync(buffer[..opts.BufferSize]);
        while (numChars > 0)
        {
            // The number of characters to read out of the builder
            // Characters after this are not valid for this read
            var numValidCharacters = offset > 0 ? numChars + opts.OverlapSize : numChars;
            if (action.Invoke(buffer.Span, opts.DelegateOptions))
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
            numChars = await streamReaderToMatch.ReadAsync(buffer[opts.OverlapSize..]);
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
    public static async Task<SlidingBufferMatch> GetFirstMatchAsync(this Stream streamToMatch, SlidingBufferExtensions.GetFirstMatchDelegate action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(streamToMatch, Encoding.Default, true, 4096, true).GetFirstMatchAsync(action, options);
    }

    /// <summary>
    /// Get the all matches for a <see cref="Stream"/> from an Function.
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <paramref name="streamToMatch"/>.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this Stream streamToMatch, SlidingBufferExtensions.GetMatchCollectionDelegate action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(streamToMatch, Encoding.Default, true, 4096, true).GetMatchCollectionAsync(action, options);
    }

    /// <summary>
    /// Get the all matches for a <see cref="StreamReader"/> from an Function.
    /// </summary>
    /// <param name="streamReaderToMatch"><see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <paramref name="streamReaderToMatch"/>.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this StreamReader streamReaderToMatch, SlidingBufferExtensions.GetMatchCollectionDelegate action, SlidingBufferOptions? options = null)
    {
        SlidingBufferMatchCollection<SlidingBufferMatch> collection = new();

        var opts = options ?? new();

        var bufferSize = opts.BufferSize < opts.OverlapSize * 2 ? opts.OverlapSize * 2 : opts.BufferSize;
        // This is our string building buffer.
        Memory<char> buffer = new(new char[bufferSize + opts.OverlapSize]);

        long offset = 0;
        var numChars = await streamReaderToMatch.ReadAsync(buffer[..opts.BufferSize]);
        while (numChars > 0)
        {
            // The number of characters to read out of the builder
            // Characters after this are not valid for this read
            var numValidCharacters = offset > 0 ? numChars + opts.OverlapSize : numChars;
            var matches = action.Invoke(buffer.Span, opts.DelegateOptions);
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
            numChars = await streamReaderToMatch.ReadAsync(buffer[opts.OverlapSize..]);
        }

        return collection;
    }

    /// <summary>
    /// Check if a StreamReader matches a Function
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<SlidingBufferMatch> GetFirstMatchAsync(this StreamReader toMatch, SlidingBufferExtensions.GetFirstMatchDelegate action, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new();
        var bufferSize = opts.BufferSize < opts.OverlapSize * 2 ? opts.OverlapSize * 2 : opts.BufferSize;
        // This is our string building buffer.
        Memory<char> buffer = new(new char[bufferSize + opts.OverlapSize]);

        long offset = 0;
        var numChars = await toMatch.ReadAsync(buffer[..opts.BufferSize]);

        while (numChars > 0)
        {
            // The number of characters to read out of the builder
            // Characters after this are not valid for this read
            var numValidCharacters = offset > 0 ? numChars + opts.OverlapSize : numChars;
            var match = action.Invoke(buffer[..numValidCharacters].Span, opts.DelegateOptions);
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
            numChars = await toMatch.ReadAsync(buffer[opts.OverlapSize..]);
        }

        return new SlidingBufferMatch();
    }
}