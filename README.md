[![Nuget](https://img.shields.io/nuget/v/StreamRegex.Extensions)](https://www.nuget.org/packages/StreamRegex.Extensions/)
# StreamRegex
A .NET Library with Extension Methods for performing arbitrary checks on the string content of Streams and StreamReaders, including built-in extension methods for Regex.

The Extensions are available on Nuget: [https://www.nuget.org/packages/StreamRegex.Extensions/](https://www.nuget.org/packages/StreamRegex.Extensions/)

Auto-Generated API Documentation is hosted on [GitHub Pages](https://gfs.github.io/StreamRegex/api/StreamRegex.Extensions.html).

## Motivation

When dealing with large files it may be inconvenient or impractical to read the whole file into memory.

## To use Regex
Here is some simple sample code to get started
### StreamReader
```c#
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
You can also call the methods on a Stream directly. If you do so, a StreamReader will be created to read it.
```c#
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

## To use Custom Method
You can provide your own custom methods for both boolean matches and match metadata.
### For Boolean Matches
```c#
// Include this for the extension methods
using StreamRegex.Extensions;

// Create your stream reader
StreamReader reader = new StreamReader(stream);

bool YourMethod(string chunk)
{
    // Your logic here
}

if(reader.IsMatch(contentChunk => YourMethod(contentChunk))
{
    // Your method matched some chunk of the Stream
}
else
{
    // Your method did not match any chunk of the Stream
}
```
### For Value Data
```c#
// Include this for the extension methods
using StreamRegex.Extensions;

// Create your stream reader
StreamReader reader = new StreamReader(stream);

string target = "Something";

// Return the index of the target string relative to the chunk. 
// It will be adjusted to the correct relative position for the Stream automatically.
SlidingBufferMatch YourMethod(string chunk)
{
    var idx = contentChunk.IndexOf(target, comparison);
    if (idx != -1)
    {
        return new SlidingBufferMatch(true, idx, contentChunk[idx..(idx + target.Length)]);
    }

    return new SlidingBufferMatch();
}

var match = reader.GetFirstMatch(contentChunk => YourMethod(contentChunk);
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
```c#
// Include this for the extension methods
using StreamRegex.Extensions;

// Create your stream reader
StreamReader reader = new StreamReader(stream);
// Your arbitrary engine that can generate multiple matches
YourEngine engine = new MatchingEngine();
public IEnumerable<SlidingBufferMatch> YourMethod(string arg)
{
    foreach (Match match in engine.MakeMatches(arg))
    {
        yield return new SlidingBufferMatch(true, match.Index, match.Value);
    }
}

var collection = reader.GetMatchCollection(contentChunk => YourMethod(contentChunk));
```

## How it works
A sliding buffer is used across the stream. The `OverlapSize` parameter is the amount of overlap buffer to use to ensure no matches are missed across buffer boundaries. Always ensure that the Overlap is sufficient for the length of the matches you want to find.

https://github.com/gfs/StreamRegex/blob/fce9cdbbe5bdcf3629ece9547a4c5230b941d072/StreamRegex.Extensions/SlidingBufferExtensions.cs#L206-L245
## Benchmarks
The benchmark results below are a selection of the results from the Benchmarks project in the repository.

### Performance on Large Files
* A Stream is generated of length `paddingSegmentLength * numberPaddingSegmentsBefore` + `paddingSegmentLength * numberPaddingSegmentsAfter` + the length of a target string. There is only one match for the target operation in the Stream.
* The query used for both regex and string matching was `racecar` - no regex operators.
* The `CompiledRegex` benchmark uses the `IsMatch` method of a Regex which is compiled before the test. The cost of converting the Stream into a String before operation is included.

|         Method |      Job |  Runtime | paddingSegmentLength | numberPaddingSegmentsBefore | numberPaddingSegmentsAfter |           Mean |          Error |         StdDev |         Median | Ratio | RatioSD |        Gen0 |        Gen1 |      Gen2 |     Allocated | Alloc Ratio |
|--------------- |--------- |--------- |--------------------- |---------------------------- |--------------------------- |---------------:|---------------:|---------------:|---------------:|------:|--------:|------------:|------------:|----------:|--------------:|------------:|
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                           0 |                          0 |       3.042 us |      0.0623 us |      0.0692 us |       3.000 us |  1.00 |    0.00 |           - |           - |         - |       3.72 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                           0 |                          0 |       5.275 us |      0.1016 us |      0.2120 us |       5.200 us |  1.72 |    0.09 |           - |           - |         - |      13.01 KB |        3.50 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                           0 |                          0 |       4.869 us |      0.1014 us |      0.1929 us |       4.800 us |  1.00 |    0.00 |           - |           - |         - |       3.72 KB |        1.00 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                           0 |                          0 |       8.277 us |      0.0868 us |      0.0725 us |       8.300 us |  1.67 |    0.09 |           - |           - |         - |      12.97 KB |        3.49 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                           0 |                     100000 | 229,652.299 us | 13,387.8464 us | 39,264.2360 us | 237,164.200 us | 1.000 |    0.00 |  26000.0000 |  14000.0000 | 3000.0000 |  391532.95 KB |       1.000 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                           0 |                     100000 |       7.082 us |      0.1441 us |      0.2561 us |       7.000 us | 0.000 |    0.00 |           - |           - |         - |      20.99 KB |       0.000 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                           0 |                     100000 | 175,093.965 us |  4,755.8204 us | 13,947.9979 us | 179,085.000 us | 1.000 |    0.00 |  28000.0000 |  27000.0000 | 4000.0000 |  391541.21 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                           0 |                     100000 |       9.673 us |      0.1966 us |      0.2692 us |       9.550 us | 0.000 |    0.00 |           - |           - |         - |      12.97 KB |       0.000 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                           0 |                     200000 | 379,099.480 us |  5,632.3389 us |  5,268.4933 us | 380,197.000 us | 1.000 |    0.00 |  51000.0000 |  27000.0000 | 4000.0000 |  783045.12 KB |       1.000 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                           0 |                     200000 |       6.854 us |      0.1372 us |      0.2254 us |       6.800 us | 0.000 |    0.00 |           - |           - |         - |      20.99 KB |       0.000 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                           0 |                     200000 | 270,276.005 us |  4,126.3805 us |  4,586.4610 us | 269,684.000 us | 1.000 |    0.00 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.38 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                           0 |                     200000 |       9.623 us |      0.1776 us |      0.2181 us |       9.550 us | 0.000 |    0.00 |           - |           - |         - |      12.97 KB |       0.000 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                      100000 |                          0 | 215,746.261 us |  4,251.1636 us |  9,151.0782 us | 218,077.050 us |  1.00 |    0.00 |  26000.0000 |  14000.0000 | 3000.0000 |  391532.95 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                      100000 |                          0 | 100,541.920 us |    626.3890 us |    585.9247 us | 100,651.200 us |  0.47 |    0.02 |  25000.0000 |           - |         - |  209821.32 KB |        0.54 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                      100000 |                          0 | 162,793.479 us |  3,538.9523 us | 10,379.1345 us | 164,210.400 us |  1.00 |    0.00 |  28000.0000 |  27000.0000 | 4000.0000 |  391541.21 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                      100000 |                          0 |  18,836.267 us |    275.5123 us |    257.7143 us |  18,798.600 us |  0.11 |    0.01 |           - |           - |         - |    1729.58 KB |       0.004 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                      100000 |                     100000 | 443,359.960 us |  3,006.2274 us |  2,812.0270 us | 443,313.200 us |  1.00 |    0.00 |  51000.0000 |  27000.0000 | 4000.0000 |  783045.12 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                      100000 |                     100000 |  99,966.243 us |    695.0068 us |    616.1054 us |  99,911.750 us |  0.23 |    0.00 |  25000.0000 |           - |         - |   209828.8 KB |        0.27 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                      100000 |                     100000 | 281,541.238 us |  3,210.7039 us |  2,681.0832 us | 282,293.100 us |  1.00 |    0.00 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.38 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                      100000 |                     100000 |  17,791.175 us |    174.7172 us |    250.5742 us |  17,728.250 us |  0.06 |    0.00 |           - |           - |         - |    1729.58 KB |       0.002 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                      100000 |                     200000 | 613,439.146 us |  2,974.8773 us |  2,484.1573 us | 612,968.000 us |  1.00 |    0.00 |  76000.0000 |  41000.0000 | 5000.0000 | 1174557.28 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                      100000 |                     200000 |  99,295.113 us |    904.0865 us |    845.6830 us |  99,575.600 us |  0.16 |    0.00 |  25000.0000 |           - |         - |   209828.8 KB |        0.18 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                      100000 |                     200000 | 403,339.907 us |  3,835.0740 us |  3,587.3306 us | 403,236.500 us |  1.00 |    0.00 |  76000.0000 |  75000.0000 | 5000.0000 | 1174557.28 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                      100000 |                     200000 |  18,868.003 us |    345.2631 us |    527.2533 us |  19,066.400 us |  0.05 |    0.00 |           - |           - |         - |    1729.58 KB |       0.001 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                      200000 |                          0 | 539,301.326 us | 10,709.1633 us | 13,543.6626 us | 540,457.200 us |  1.00 |    0.00 |  51000.0000 |  27000.0000 | 4000.0000 |  783045.12 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                      200000 |                          0 | 198,403.929 us |  1,498.7502 us |  1,328.6031 us | 198,428.800 us |  0.37 |    0.01 |  51000.0000 |           - |         - |  419629.63 KB |        0.54 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                      200000 |                          0 | 327,254.492 us |  6,484.1770 us | 11,010.6247 us | 328,614.500 us |  1.00 |    0.00 |  52000.0000 |  51000.0000 | 5000.0000 |  783053.38 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                      200000 |                          0 |  36,291.386 us |    412.4609 us |    365.6359 us |  36,257.100 us |  0.11 |    0.00 |           - |           - |         - |    3446.19 KB |       0.004 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                      200000 |                     100000 | 704,108.592 us |  8,045.8337 us |  6,718.6356 us | 706,813.300 us |  1.00 |    0.00 |  76000.0000 |  41000.0000 | 5000.0000 | 1174557.33 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                      200000 |                     100000 | 207,031.880 us |  2,753.0846 us |  2,575.2371 us | 207,081.400 us |  0.29 |    0.00 |  51000.0000 |           - |         - |  419636.62 KB |        0.36 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                      200000 |                     100000 | 437,730.544 us |  8,659.5221 us |  9,265.5917 us | 438,264.300 us |  1.00 |    0.00 |  76000.0000 |  75000.0000 | 5000.0000 | 1174557.28 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                      200000 |                     100000 |  34,265.146 us |    423.3306 us |    353.5002 us |  34,332.000 us |  0.08 |    0.00 |           - |           - |         - |    3446.19 KB |       0.003 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 6.0 | .NET 6.0 |                 1000 |                      200000 |                     200000 | 893,739.307 us | 14,269.0380 us | 13,347.2670 us | 894,285.600 us |  1.00 |    0.00 | 100000.0000 |  54000.0000 | 5000.0000 | 1566061.23 KB |        1.00 |
| RegexExtension | .NET 6.0 | .NET 6.0 |                 1000 |                      200000 |                     200000 | 204,772.836 us |  3,838.1073 us |  3,402.3821 us | 204,041.500 us |  0.23 |    0.01 |  51000.0000 |           - |         - |  419636.62 KB |        0.27 |
|                |          |          |                      |                             | |                |                |                |                |       |         |             |             |           |               |             |
|  CompiledRegex | .NET 7.0 | .NET 7.0 |                 1000 |                      200000 |                     200000 | 583,033.010 us | 11,506.1290 us | 26,437.2449 us | 577,418.900 us |  1.00 |    0.00 | 101000.0000 | 100000.0000 | 6000.0000 | 1566069.45 KB |       1.000 |
| RegexExtension | .NET 7.0 | .NET 7.0 |                 1000 |                      200000 |                     200000 |  36,507.580 us |    592.8086 us |    554.5135 us |  36,490.900 us |  0.06 |    0.00 |           - |           - |         - |    3446.19 KB |       0.002 |

### Async vs Sync

Because Span is not supported in async contexts async is significantly slower.

|              Method | TestFileName |     Mean |   Error |  StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|-------------------- |------------- |---------:|--------:|--------:|------:|--------:|-----------:|----------:|----------:|
|      RegexExtension |    175MB.txt | 193.3 ms | 2.93 ms | 2.74 ms |  1.00 |    0.00 | 23000.0000 |         - |    370 MB |
| RegexExtensionAsync |    175MB.txt | 406.8 ms | 7.71 ms | 8.25 ms |  2.10 |    0.06 | 43000.0000 | 1000.0000 |    384 MB |

## License

MIT
