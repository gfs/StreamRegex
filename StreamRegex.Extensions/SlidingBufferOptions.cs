namespace StreamRegex.Extensions;
public class SlidingBufferOptions
{
    /// <summary>
    /// Size in bytes of the read buffer for processing.
    /// If it is not at least twice as large as <see cref="OverlapSize"/>, twice the <see cref="OverlapSize"/> will be used instead.
    /// </summary>
    public int BufferSize { get; set; } = 4096;

    /// <summary>
    /// Size in bytes to use for overlap to ensure that matches that span buffer slices are found.
    /// Potential matches longer than this parameter may be missed.
    /// Increasing this parameter may impact performance and result in extra duplicate matches when enumerating from <see cref="StreamRegexExtensions"/>
    /// </summary>
    public int OverlapSize { get; set; } = 256;
}