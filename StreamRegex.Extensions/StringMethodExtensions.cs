namespace StreamRegex.Extensions;

public static class StringMethodExtensions
{
    
    /// <summary>
    /// Find the first index of a target string in the given Stream
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static bool Contains(this Stream toMatch, string target, StringComparison? comparison = null, SlidingBufferOptions? options = null)
    {
        return new StreamReader(toMatch).Contains(target, comparison, options);
    }

    /// <summary>
    /// Find the first index of a target string in the given StreamReader
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static bool Contains(this StreamReader toMatch, string target, StringComparison? comparison = null, SlidingBufferOptions? options = null)
    {
        return comparison is { } notNullComparison ?
            toMatch.IsMatch(contentChunk => contentChunk.Contains(target, notNullComparison), options) :
            toMatch.IsMatch(contentChunk => contentChunk.Contains(target), options);
    }

    /// <summary>
    /// Find the first index of a target string in the given Stream
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static long IndexOf(this Stream toMatch, string target, StringComparison comparison = StringComparison.CurrentCulture, SlidingBufferOptions? options = null)
    {
        return new StreamReader(toMatch).IndexOf(target, comparison, options);
    }

    /// <summary>
    /// Find the first index of a target string in the given StreamReader
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static long IndexOf(this StreamReader toMatch, string target, StringComparison comparison = StringComparison.CurrentCulture, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new SlidingBufferOptions();
        // Ensure that we can find the target string
        if (opts.OverlapSize < target.Length)
        {
            opts.OverlapSize = target.Length;
        }
        var match = toMatch.GetFirstMatch(contentChunk =>
        {
            var idx = contentChunk.IndexOf(target, comparison);
            if (idx != -1)
            {
                return new SlidingBufferMatch(true, idx, contentChunk[idx..(idx + target.Length)]);
            }

            return new SlidingBufferMatch();
        }, options);
        return match.Index;
    }
}