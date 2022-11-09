[![Nuget](https://img.shields.io/nuget/v/StreamRegex.Extensions)](https://www.nuget.org/packages/StreamRegex.Extensions/)
# StreamRegex
A .NET Library with Extension Methods for performing arbitrary checks on the string content of Streams and StreamReaders, including built-in extension methods for Regex.

The Extensions are available on Nuget: [https://www.nuget.org/packages/StreamRegex.Extensions/](https://www.nuget.org/packages/StreamRegex.Extensions/)

Auto-Generated API Documentation is hosted on [GitHub Pages](https://gfs.github.io/StreamRegex/api/StreamRegex.Extensions.html).

## Motivation

Memory allocation is an expensive operation - in many cases it may be consuming more time than any other operation in your program. .NET introduces an excellent 0 allocation regex implementation for strings and Spans (under the covers the string path uses spans as well).

However, it may be the case that you want to check many arbitrarily large files without reading every file out into a string - an allocation expensive operation.  Using the extension methods here you can check your Stream or StreamReader directly with minimal allocations. For a 400MB file, on .NET 7 allocations can be reduced from 1.5GB to ~4MB - see [Benchmarks](#benchmarks)

## To use Regex
Here is some simple sample code to get started
### StreamReader
```c#
// Include this for the extension methods
using StreamRegex.Extensions.RegexExtensions;

// Construct your regex as normal
Regex myRegex = new Regex(expression);

// Create your stream reader
StreamReader reader = new StreamReader(stream);

// Get matches
SlidingBufferMatchCollection<StreamRegexMatch> matchCollection = myRegex.GetMatchCollection(reader);
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

Alternately check if there is only one match. Note that the position of the Stream or StreamReader is not reset by these methods. Ensure the position of your stream is where you want to start parsing.
```c#
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

Or you can just check if there is any match but not get details on the match
```c#
// Check if there is any match
if (myRegex.IsMatch(reader))
{
    // At least one match
}
else
{
    // No matches
}
```

### Stream
You can also call the methods on a Stream directly. If you do so, a StreamReader will be created to read it with `leaveOpen = true`, reading from the current position of the Stream. The Stream will not have its position reset after reading, and will not be closed or disposed.
```c#
// Include this for the extension methods
using StreamRegex.Extensions.RegexExtensions;

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

## Options
You can adjust the internal buffer and overlap sizes, and capture the value that was matched as well as the Index using `SlidingBufferOptions`.

* The `BufferSize` is the size of the internal `Span<char>` used for checking. 
* The `OverlapSize` is the number of characters from the previous buffer to include at the start of the next to guarantee matches across boundaries.
* If `CaptureValues` is set to true, `SlidingBufferMatch` objects (including `StreamRegexMatch` objects) will contain the value of the actual match in addition to the length. If false, match objects will only contain `Index` and `Length` of the match.

```c#
// Include this for the extension methods
using StreamRegex.Extensions.RegexExtensions;
// Include this for options objects
using StreamRegex;

// Construct your regex as normal
Regex myRegex = new Regex(expression);

var bufferOptions = new SlidingBufferOptions()
{
    BufferSize = 8192, // The number of characters to check at a time, default 4096
    OverlapSize = 512, // Must be as long as your longest desired match, default 256
    DelegateOptions = new DelegateOptions()
    {
        CaptureValues = true // If the actual value matched by the Regex should be included in the SlidingBufferMatch, default false
    }
};

StreamRegexMatch match = myRegex.GetFirstMatch(reader, bufferOptions);
```

## To use Custom Method
You can provide your own custom methods for both boolean matches and match metadata.
### For Boolean Matches
Implement the `IsMatch` delegate.
```c#
// Include this for the extension methods
using StreamRegex.Extensions.Core;

// Create your stream reader
StreamReader reader = new StreamReader(stream);

bool YourMethod(ReadOnlySpan<char> chunk)
{
    // Your logic here
}

if(reader.IsMatch(YourMethod)
{
    // Your method matched some chunk of the Stream
}
else
{
    // Your method did not match any chunk of the Stream
}
```
### For Value Data
Implement the `GetFirstMatch` delegate.
```c#
// Include this for the extension methods
using StreamRegex.Extensions.Core;

// Create your stream reader
StreamReader reader = new StreamReader(stream);

// Return the index of the target string relative to the chunk. 
// It will be adjusted to the correct relative position for the Stream automatically.
SlidingBufferMatch YourMethod(ReadOnlySpan<char> chunk)
{
    if (SomeCheckOf(chunk))
    {
        return new SlidingBufferMatch(true, idx, target.Length);
    }

    return new SlidingBufferMatch();
}

var match = reader.GetFirstMatch(YourMethod);
if(match.Success)
{
    // Your method matched some chunk of the Stream
}
else
{
    // Your method did not match any chunk of the Stream
}
```

### For a collection
Implement the `GetMatchCollection` delegate.
```c#
// Include this for the extension methods
using StreamRegex.Extensions.Core;

// Create your stream reader
StreamReader reader = new StreamReader(stream);
// Your arbitrary engine that can generate multiple matches
YourEngineHolder matchingEngine = new YourEngineHolder();

public class YourEngineHolder
{
    private YourMatchingEngine _internalEngine;
    
    public YourEngineHolder()
    {
        _internalEngine = new YourMatchingEngine();
    }
    
    public SlidingBufferMatchCollection<SlidingBufferMatch> YourMethod(ReadOnlySpan<char> arg)
    {
        SlidingBufferMatchCollection<SlidingBufferMatch> matchCollection = new SlidingBufferMatchCollection<SlidingBufferMatch>();
        foreach(var match in _internalEngine.MakeMatches(arg))
        {
            matchCollection.Add(match);
        }
        return matchCollection;
    }
}

var collection = reader.GetMatchCollection(matchingEngine.YourMethod);
```

## How it works
A sliding buffer is used across the stream. The `OverlapSize` parameter is the amount of overlap buffer to use to ensure no matches are missed across buffer boundaries. Always ensure that the Overlap is sufficient for the length of the matches you want to find.

https://github.com/gfs/StreamRegex/blob/fce9cdbbe5bdcf3629ece9547a4c5230b941d072/StreamRegex.Extensions/SlidingBufferExtensions.cs#L206-L245
## Benchmarks
The benchmark results below are a selection of the results from the Benchmarks project in the repository.

### Performance on Large Files
* A Stream is generated of length `paddingSegmentLength * numberPaddingSegmentsBefore` + `paddingSegmentLength * numberPaddingSegmentsAfter` + the length of a target string. There is only one match for the target operation in the Stream.
* The query used for both regex and string matching was `racecar` - no regex operators.
* The `JustReadTheStreamToString` reads the full contents of the Stream into a string.
* The `Enumerate` benchmark uses the `EnumerateMatches` method of a Regex on a `Span<char>` of the Bytes of the Stream stopping after the first match is found. The cost of converting the Stream into a String before operation is included.
* The `RegexExtension` benchmark uses the `IsMatch` extension method of a Regex on a `StreamReader` stopping after the first match is found.

This benchmark iteration finds the only instance of `racecar` located 200MB into a 400MB Stream. Using the extension method is 12 times faster and allocates .2% of the memory. Memory usage is [configurable](#options).

We find that the majority of the operation time is spent on reading full Stream to a string before operation, by comparison with the `JustReadTheStreamToString` benchmark.

|         Method            |           Mean |          Error |         StdDev |         Median |  Ratio |    Allocated  | Alloc Ratio |
|--------------------------:|---------------:|---------------:|---------------:|---------------:|-------:|--------------:|------------:|
| JustReadTheStreamToString | 464,216.947 us | 9,237.0810 us | 19,684.9728 us | 462,662.900 us |  0.95   | 1566069.56 KB |       1.000 |
|     CompiledRegexWithSpan | 487,368.232 us | 9,735.8353 us | 22,757.2012 us | 483,413.500 us |  1.00   | 1566069.56 KB |       1.000 |
|            RegexExtension | 39,002.165 us  |   709.5091 us |  1,261.1516 us |  38,862.950 us |  0.08   |    3446.34 KB |       0.002 |

### Complete run details

|                    Method | paddingSegmentLength | numberPaddingSegmentsBefore | numberPaddingSegmentsAfter |           Mean |         Error |         StdDev |         Median | Ratio | RatioSD |        Gen0 |        Gen1 |      Gen2 |     Allocated | Alloc Ratio |
| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:| -----:|
| JustReadTheStreamToString |                 1000 |                           0 |                          0 |       4.270 us |     0.2109 us |      0.5878 us |       4.100 us |  0.75 |    0.13 |           - |           - |         - |       3.84 KB |        1.00 |
|     CompiledRegexWithSpan |                 1000 |                           0 |                          0 |       5.707 us |     0.1859 us |      0.5213 us |       5.400 us |  1.00 |    0.00 |           - |           - |         - |       3.84 KB |        1.00 |
|            RegexExtension |                 1000 |                           0 |                          0 |       8.241 us |     0.1653 us |      0.1698 us |       8.200 us |  1.37 |    0.12 |           - |           - |         - |      12.79 KB |        3.33 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                           0 |                     100000 | 166,482.284 us | 5,653.6156 us | 16,669.8132 us | 165,875.900 us | 1.026 |    0.15 |  28000.0000 |  27000.0000 | 4000.0000 |  391541.33 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                           0 |                     100000 | 164,031.268 us | 5,622.5283 us | 16,578.1517 us | 162,907.300 us | 1.000 |    0.00 |  28000.0000 |  27000.0000 | 4000.0000 |  391541.33 KB |       1.000 |
|            RegexExtension |                 1000 |                           0 |                     100000 |       9.992 us |     0.5957 us |      1.6205 us |       9.300 us | 0.000 |    0.00 |           - |           - |         - |      12.79 KB |       0.000 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                           0 |                     200000 | 266,022.758 us | 5,348.0675 us | 15,600.5638 us | 266,423.450 us | 0.997 |    0.08 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.49 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                           0 |                     200000 | 268,004.598 us | 5,473.0687 us | 16,137.4665 us | 267,348.750 us | 1.000 |    0.00 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.49 KB |       1.000 |
|            RegexExtension |                 1000 |                           0 |                     200000 |      10.042 us |     0.5961 us |      1.5911 us |       9.300 us | 0.000 |    0.00 |           - |           - |         - |      12.79 KB |       0.000 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                      100000 |                          0 | 165,313.388 us | 5,565.6649 us | 16,410.4884 us | 165,561.600 us |  0.93 |    0.14 |  28000.0000 |  27000.0000 | 4000.0000 |  391541.33 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                      100000 |                          0 | 180,195.883 us | 5,885.1690 us | 17,260.1820 us | 181,039.600 us |  1.00 |    0.00 |  28000.0000 |  27000.0000 | 4000.0000 |  391541.33 KB |       1.000 |
|            RegexExtension |                 1000 |                      100000 |                          0 |  20,467.507 us |   963.3668 us |  2,764.0800 us |  19,296.000 us |  0.11 |    0.02 |           - |           - |         - |    1729.73 KB |       0.004 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                      100000 |                     100000 | 268,263.768 us | 6,484.9961 us | 19,121.1573 us | 267,571.450 us |  0.98 |    0.10 |  53000.0000 |  52000.0000 | 5000.0000 |  783053.49 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                      100000 |                     100000 | 275,622.627 us | 7,163.8514 us | 20,897.2909 us | 274,244.450 us |  1.00 |    0.00 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.49 KB |       1.000 |
|            RegexExtension |                 1000 |                      100000 |                     100000 |  20,240.201 us |   413.5572 us |  1,206.3659 us |  20,002.150 us |  0.07 |    0.01 |           - |           - |         - |    1729.73 KB |       0.002 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                      100000 |                     200000 | 363,243.707 us | 7,170.1571 us | 18,250.3392 us | 365,225.550 us |  0.97 |    0.06 |  76000.0000 |  75000.0000 | 5000.0000 |  1174557.4 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                      100000 |                     200000 | 375,206.747 us | 7,493.8196 us | 19,209.5245 us | 372,731.200 us |  1.00 |    0.00 |  76000.0000 |  75000.0000 | 5000.0000 |  1174557.4 KB |       1.000 |
|            RegexExtension |                 1000 |                      100000 |                     200000 |  20,547.844 us |   409.8397 us |  1,101.0077 us |  20,452.300 us |  0.05 |    0.00 |           - |           - |         - |    1729.73 KB |       0.001 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                      200000 |                          0 | 266,579.896 us | 6,540.2770 us | 19,181.5003 us | 265,581.200 us |  0.92 |    0.10 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.49 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                      200000 |                          0 | 290,947.307 us | 6,644.5908 us | 19,487.4347 us | 291,815.100 us |  1.00 |    0.00 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.49 KB |       1.000 |
|            RegexExtension |                 1000 |                      200000 |                          0 |  39,771.622 us |   793.8631 us |  1,529.5046 us |  39,761.500 us |  0.14 |    0.01 |           - |           - |         - |    3446.34 KB |       0.004 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                      200000 |                     100000 | 372,794.817 us | 7,444.6621 us | 19,612.2176 us | 372,598.250 us |  0.97 |    0.07 |  76000.0000 |  75000.0000 | 5000.0000 |  1174557.4 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                      200000 |                     100000 | 384,893.258 us | 7,671.8121 us | 18,528.2884 us | 384,279.200 us |  1.00 |    0.00 |  76000.0000 |  75000.0000 | 5000.0000 |  1174557.4 KB |       1.000 |
|            RegexExtension |                 1000 |                      200000 |                     100000 |  37,077.547 us |   738.9003 us |  1,542.3602 us |  37,021.400 us |  0.10 |    0.01 |           - |           - |         - |    3446.34 KB |       0.003 |
|                           |                      |                             |                            |                |               |                |                | |         |             |             |           |               |             |
| JustReadTheStreamToString |                 1000 |                      200000 |                     200000 | 464,216.947 us | 9,237.0810 us | 19,684.9728 us | 462,662.900 us |  0.95 |    0.06 | 101000.0000 | 100000.0000 | 6000.0000 | 1566069.56 KB |       1.000 |
|     CompiledRegexWithSpan |                 1000 |                      200000 |                     200000 | 487,368.232 us | 9,735.8353 us | 22,757.2012 us | 483,413.500 us |  1.00 |    0.00 | 101000.0000 | 100000.0000 | 6000.0000 | 1566069.56 KB |       1.000 |
|            RegexExtension |                 1000 |                      200000 |                     200000 |  39,002.165 us |   709.5091 us |  1,261.1516 us |  38,862.950 us |  0.08 |    0.00 |           - |           - |         - |    3446.34 KB |       0.002 |

### Async vs Sync

Because Span is not supported in async contexts async is significantly slower.

|              Method | TestFileName |     Mean |   Error |  StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|-------------------- |------------- |---------:|--------:|--------:|------:|--------:|-----------:|----------:|----------:|
|      RegexExtension |    175MB.txt | 193.3 ms | 2.93 ms | 2.74 ms |  1.00 |    0.00 | 23000.0000 |         - |    370 MB |
| RegexExtensionAsync |    175MB.txt | 406.8 ms | 7.71 ms | 8.25 ms |  2.10 |    0.06 | 43000.0000 | 1000.0000 |    384 MB |

## License

MIT
