using System;
using System.IO;
using System.Text;
using StreamRegex.Extensions.Core;

namespace StreamRegex.Extensions.StringMethods;

/// <summary>
/// Synchronous methods to emulate <see cref="string.Contains(string)"/> and <see cref="string.IndexOf(string)"/> operations on Streams.
/// </summary>
public static class StringMethodExtensions
{
    /// <summary>
    /// Check if a given string is contained in the given <see cref="Stream"/>.
    /// Using a <see cref="System.Text.RegularExpressions.Regex"/> may be faster.
    /// </summary>
    /// <param name="streamToCheck">The <see cref="Stream"/> to check for <paramref name="value"/></param>
    /// <param name="value">The string to check for in the <paramref name="streamToCheck"/></param>
    /// <param name="comparisonType"><see cref="StringComparison"/> type to use</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> specifying the sizes of the buffer and the overlap.</param>
    /// <returns>True if <paramref name="value"/> is contained in <paramref name="streamToCheck"/></returns>
    public static bool Contains(this Stream streamToCheck, string value, StringComparison comparisonType = StringComparison.Ordinal, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToCheck, Encoding.Default, true, 4096, true).Contains(value, comparisonType, options);
    }

    /// <summary>
    /// Check if a given string is contained in the given <see cref="StreamReader"/>.
    /// Using a <see cref="System.Text.RegularExpressions.Regex"/> may be faster.
    /// </summary>
    /// <param name="streamReaderToCheck">The <see cref="StreamReader"/> to check for <paramref name="value"/></param>
    /// <param name="value">The string to check for in the <paramref name="streamReaderToCheck"/></param>
    /// <param name="comparisonType"><see cref="StringComparison"/> type to use</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> specifying the sizes of the buffer and the overlap.</param>
    /// <returns>True if <paramref name="value"/> is contained in <paramref name="streamReaderToCheck"/></returns>
    public static bool Contains(this StreamReader streamReaderToCheck, string value, StringComparison comparisonType = StringComparison.Ordinal, SlidingBufferOptions? options = null)
    {
        var matcher = new StringMatcher(value, comparisonType);
        return streamReaderToCheck.IsMatch(matcher.IsMatchDelegate, options);
    }

    /// <summary>
    /// Find the first index of a given string is contained in the given <see cref="Stream"/>.
    /// Using a <see cref="System.Text.RegularExpressions.Regex"/> may be faster.
    /// </summary>
    /// <param name="streamToCheck">The <see cref="Stream"/> to check for <paramref name="value"/></param>
    /// <param name="value">The string to check for in the <paramref name="streamToCheck"/></param>
    /// <param name="comparisonType"><see cref="StringComparison"/> type to use</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> specifying the sizes of the buffer and the overlap.</param>
    /// <returns>The index offset relative to the position when <paramref name="streamToCheck"/> was provided or -1 if not found.</returns>
    public static long IndexOf(this Stream streamToCheck, string value, StringComparison comparisonType = StringComparison.CurrentCulture, SlidingBufferOptions? options = null)
    {
        return new StreamReader(streamToCheck, Encoding.Default, true, 4096, true).IndexOf(value, comparisonType, options);
    }

    /// <summary>
    /// Find the first index of a given string is contained in the given <see cref="StreamReader"/>.
    /// Using a <see cref="System.Text.RegularExpressions.Regex"/> may be faster.
    /// </summary>
    /// <param name="streamReaderToCheck">The <see cref="StreamReader"/> to check for <paramref name="value"/></param>
    /// <param name="value">The string to check for in the <paramref name="streamReaderToCheck"/></param>
    /// <param name="comparisonType"><see cref="StringComparison"/> type to use</param>
    /// <param name="options">The <see cref="SlidingBufferOptions"/> specifying the sizes of the buffer and the overlap.</param>
    /// <returns>The index offset relative to the position when <paramref name="streamReaderToCheck"/> was provided or -1 if not found.</returns>
    public static long IndexOf(this StreamReader streamReaderToCheck, string value, StringComparison comparisonType = StringComparison.CurrentCulture, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new SlidingBufferOptions();
        // Ensure that we can find the target string
        if (opts.OverlapSize < value.Length)
        {
            opts.OverlapSize = value.Length;
        }
        var matcher = new StringMatcher(value, comparisonType);
        var match = streamReaderToCheck.GetFirstMatch(matcher.GetFirstMatchDelegate, options);
        return match.Index;
    }
}