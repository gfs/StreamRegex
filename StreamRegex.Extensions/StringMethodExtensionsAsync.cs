namespace StreamRegex.Extensions;

public static class StringMethodExtensionsAsync
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
    public static async Task<bool> ContainsAsync(this Stream streamToCheck, string value, StringComparison? comparisonType = null, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(streamToCheck).ContainsAsync(value, comparisonType, options);
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
    public static async Task<bool> ContainsAsync(this StreamReader streamReaderToCheck, string value, StringComparison? comparisonType = null, SlidingBufferOptions? options = null)
    {
        return await (comparisonType is { } notNullComparison ?
            streamReaderToCheck.IsMatchAsync(contentChunk => contentChunk.Contains(value, notNullComparison), options) :
            streamReaderToCheck.IsMatchAsync(contentChunk => contentChunk.Contains(value), options));
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

    public static async Task<long> IndexOfAsync(this Stream streamToCheck, string value, StringComparison? comparisonType = null, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(streamToCheck).IndexOfAsync(value, comparisonType, options);
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

    public static async Task<long> IndexOfAsync(this StreamReader streamReaderToCheck, string value, StringComparison? comparisonType = null, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new SlidingBufferOptions();
        // Ensure that we can find the target string
        if (opts.OverlapSize < value.Length)
        {
            opts.OverlapSize = value.Length;
        }
        var match = await streamReaderToCheck.GetFirstMatchAsync(contentChunk =>
        {
            var idx = comparisonType is {} notNullComparison ? contentChunk.IndexOf(value, notNullComparison) : contentChunk.IndexOf(value);
            if (idx != -1)
            {
                return new SlidingBufferMatch(true, idx, contentChunk[idx..(idx + value.Length)]);
            }

            return new SlidingBufferMatch();
        }, options);
        return match.Index;
    }
}