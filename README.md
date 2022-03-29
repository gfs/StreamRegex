# StreamRegex
A .NET Library with Extension Methods for performing Regex operations on Streams and StreamReaders.

## Motivation

When dealing with large files it may be inconvenient or impractical to read the whole file into memory.

## To use

### StreamReader
```csharp
// Include this for the extension methods
using StreamRegex.Extensions;

// Construct your regex as normal
Regex myRegex = new Regex(expression);

// Create your stream reader
StreamReader reader = new StreamReader(stream);

// Get matches
StreamRegexMatchCollection matchCollection = myRegex.GetMatchCollection(reader);
if (matchCollection.Any())
{
    foreach(StreamRegexMatch match in matchCollection)
    {
        // Do something with matches.
    }
}
else
{
    // No match
}

// Check if there is any match
if (myRegex.IsMatch(reader))
{
    // At least one match
}
else
{
    // No matches
}

// Get only the first match
StreamRegexMatch match = myRegex.GetFirstMatch(reader);
if (match.Matches)
{
    // A match was found
}
else
{
    // No matches
}

```

### Stream
```csharp
// Include this for the extension methods
using StreamRegex.Extensions;

// This stream contains the content you want to check
Stream stream;

// Construct your regex as normal
Regex myRegex = new Regex(expression);

// Get matches
SlidingBufferMatchCollection<StreamRegexMatch> matchCollection = myRegex.GetMatchCollection(stream);
if (matchCollection.Any())
{
    foreach(StreamRegexMatch match in matchCollection)
    {
        // Do something with matches.
    }
}
else
{
    // No match
}
```

## How it works
A sliding buffer is used across the stream. The `OverlapSize` parameter is the amount of overlap buffer to use to ensure no matches are missed across buffer boundaries.

https://github.com/gfs/StreamRegex/blob/fce9cdbbe5bdcf3629ece9547a4c5230b941d072/StreamRegex.Extensions/SlidingBufferExtensions.cs#L206-L245
## Benchmarks
These benchmarks were run with a pre-release version of the library. 

### Large File Test
This is a worst case scenario. A very large file (175MB) that contains what we want to find once at the very end.
The query used for both regex and string matching was `racecar` - no regex operators.
Compared to a workflow of reading the entire string first into a stream, this library allocates significantly less memory and for Regular Expressions is faster.
Note that using a Regex is significantly faster than string.IndexOf. If do you do not absolutely need to specify StringComparison you should consider using Regex over IndexOf.

|          Method | TestFileName |       Mean |    Error |   StdDev | Ratio |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|---------------- |------------- |-----------:|---------:|---------:|------:|-----------:|-----------:|----------:|----------:|
|   CompiledRegex |    175MB.txt |   438.3 ms |  8.72 ms | 10.71 ms |  0.10 | 24000.0000 | 13000.0000 | 3000.0000 |    686 MB |
|  RegexExtension |    175MB.txt |   214.4 ms |  4.30 ms |  7.86 ms |  0.05 | 23000.0000 |          - |         - |    370 MB |
|    SimpleString |    175MB.txt | 4,361.8 ms | 18.09 ms | 14.12 ms |  1.00 | 24000.0000 | 13000.0000 | 3000.0000 |    686 MB |
| StringExtension |    175MB.txt | 4,379.5 ms | 16.09 ms | 12.56 ms |  1.00 | 22000.0000 |          - |         - |    366 MB |
