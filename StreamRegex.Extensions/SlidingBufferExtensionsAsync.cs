using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StreamRegex.Extensions;

public static class SlidingBufferExtensionsAsync
{
    /// <summary>
    /// Check if a <see cref="Stream"/> matches the provided <paramref name="action"/>
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <see cref="streamToMatch"/> matches the <see cref="action"/></returns>
    public static async Task<bool> IsMatchAsync(this Stream tomatch, Func<string, bool> action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(tomatch).IsMatchAsync(action, options);
    }
    
    /// <summary>
    /// Check if a <see cref="StreamReader"/> matches the provided <paramref name="action"/>
    /// </summary>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>True if the <see cref="streamReaderToMatch"/> matches the <see cref="action"/></returns>
    public static async Task<bool> IsMatchAsync(this StreamReader streamReaderToMatch, Func<string, bool> action, SlidingBufferOptions? options = null)
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
    public static async Task<SlidingBufferMatch> GetFirstMatchAsync(this Stream streamToMatch, Func<string, SlidingBufferMatch> action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(streamToMatch).GetFirstMatchAsync(action, options);
    }
    
    /// <summary>
    /// Get the all matches for a <see cref="Stream"/> from an Function.
    /// </summary>
    /// <param name="streamToMatch">The <see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <see cref="streamToMatch"/>.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this Stream streamToMatch, Func<string, IEnumerable<SlidingBufferMatch>> action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(streamToMatch).GetMatchCollectionAsync(action, options);
    }
    
    /// <summary>
    /// Get the all matches for a <see cref="StreamReader"/> from an Function.
    /// </summary>
    /// <param name="streamReaderToMatch"><see cref="Stream"/> to check for matches</param>
    /// <param name="action">The Function to run</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{SlidingBufferMatch}"/> object with all the matches for the <see cref="streamReaderToMatch"/>.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this StreamReader streamReaderToMatch, Func<string, IEnumerable<SlidingBufferMatch>> action, SlidingBufferOptions? options = null)
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
    public static async Task<SlidingBufferMatch> GetFirstMatchAsync(this StreamReader toMatch, Func<string, SlidingBufferMatch> action, SlidingBufferOptions? options = null)
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
            numChars = await toMatch.ReadAsync(buffer[opts.OverlapSize..]);
        }

        return new SlidingBufferMatch();
    }
}