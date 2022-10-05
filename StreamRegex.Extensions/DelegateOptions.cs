namespace StreamRegex.Extensions;

/// <summary>
/// Options for the Sliding Buffer
/// </summary>
public class DelegateOptions
{
    /// <summary>
    /// If set, Match objects will contain the value which was matched. May increase memory allocations.
    /// </summary>
    public bool CaptureValues { get; set; } = false;
}