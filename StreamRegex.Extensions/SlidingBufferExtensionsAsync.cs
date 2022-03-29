namespace StreamRegex.Extensions;

public static class SlidingBufferExtensionsAsync
{
    /// <summary>
    /// Find the first index of a target string in the given Stream
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<bool> ContainsAsync(this Stream toMatch, string target, StringComparison? comparison = null, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(toMatch).ContainsAsync(target, comparison, options);
    }

    /// <summary>
    /// Find the first index of a target string in the given StreamReader
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<bool> ContainsAsync(this StreamReader toMatch, string target, StringComparison? comparison = null, SlidingBufferOptions? options = null)
    {
        return await (comparison is { } notNullComparison ?
            toMatch.IsMatchAsync(contentChunk => contentChunk.Contains(target, notNullComparison), options) :
            toMatch.IsMatchAsync(contentChunk => contentChunk.Contains(target), options));
    }

    /// <summary>
    /// Find the first index of a target string in the given Stream
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<long> IndexOfAsync(this Stream toMatch, string target, StringComparison comparison = StringComparison.CurrentCulture, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(toMatch).IndexOfAsync(target, comparison, options);
    }

    /// <summary>
    /// Find the first index of a target string in the given StreamReader
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="target"></param>
    /// <param name="comparison"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<long> IndexOfAsync(this StreamReader toMatch, string target, StringComparison comparison = StringComparison.CurrentCulture, SlidingBufferOptions? options = null)
    {
        var opts = options ?? new SlidingBufferOptions();
        // Ensure that we can find the target string
        if (opts.OverlapSize < target.Length)
        {
            opts.OverlapSize = target.Length;
        }
        var match = await toMatch.GetFirstMatchAsync(contentChunk =>
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
    /// <summary>
    /// Check if a Stream matches a Function
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<bool> IsMatchAsync(this Stream tomatch, Func<string, bool> action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(tomatch).IsMatchAsync(action, options);
    }
    
    /// <summary>
    /// Check if a StreamReader matches a Function
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<bool> IsMatchAsync(this StreamReader toMatch, Func<string, bool> action, SlidingBufferOptions? options = null)
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
            numChars = await toMatch.ReadAsync(buffer[opts.OverlapSize..]);
        }

        return false;
    }
    /// <summary>
    /// Check if a Stream matches a Function
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<SlidingBufferMatch> GetFirstMatchAsync(this Stream tomatch, Func<string, SlidingBufferMatch> action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(tomatch).GetFirstMatchAsync(action, options);
    }
    /// <summary>
    /// Check if a Stream matches a Function
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this Stream toMatch, Func<string, IEnumerable<SlidingBufferMatch>> action, SlidingBufferOptions? options = null)
    {
        return await new StreamReader(toMatch).GetMatchCollectionAsync(action, options);
    }
    /// <summary>
    /// Check if a StreamReader matches a Function
    /// </summary>
    /// <param name="toMatch"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this StreamReader toMatch, Func<string, IEnumerable<SlidingBufferMatch>> action, SlidingBufferOptions? options = null)
    {        
        SlidingBufferMatchCollection<SlidingBufferMatch> collection = new();

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
            numChars = await toMatch.ReadAsync(buffer[opts.OverlapSize..]);
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