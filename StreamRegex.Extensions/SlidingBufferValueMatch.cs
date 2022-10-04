namespace StreamRegex.Extensions;

/// <summary>
/// A match that includes the Index and Length.
/// </summary>
/// <param name="Success">If the match was successful</param>
/// <param name="Index">The position offset in the stream relative to the start when provided, or -1 if <paramref name="Success"/> is false</param>
/// <param name="Length">The length of the match in the stream, or -1 if <paramref name="Success"/> is false</param>
public record SlidingBufferValueMatch(bool Success = false, long Index = -1, long Length = -1)
{
    /// <summary>
    /// The index in the Stream where the match was found
    /// </summary>
    public long Index { get; internal set; } = Index;
}