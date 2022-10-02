namespace StreamRegex.Extensions;

/// <summary>
///  A match on the sliding buffer.
/// </summary>
/// <param name="Success">If the match was successful</param>
/// <param name="Index">The position offset in the stream relative to the start when provided, or -1 if <paramref name="Success"/> is false</param>
/// <param name="Value">The content that was matched, or null if <paramref name="Success"/> is false</param>
public record SlidingBufferMatch(bool Success = false, long Index = -1, string? Value = null)
{
    /// <summary>If the match was successful</summary>
    public bool Success { get; } = Success;
    
    /// <summary>
    /// The index in the Stream where the match was found
    /// </summary>
    public long Index { get; internal set; } = Index;

    /// <summary>The content that was matched, or null if <paramref name="Success"/> is false</summary>
    public string? Value { get; } = Value;
}