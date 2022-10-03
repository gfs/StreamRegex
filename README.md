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

|         Method | paddingSegmentLength | numberPaddingSegmentsBefore | numberPaddingSegmentsAfter |           Mean |          Error |         StdDev |         Median | Ratio | RatioSD |       Gen 0 |      Gen 1 |     Gen 2 |    Allocated |
|--------------- |--------------------- |---------------------------- |--------------------------- |---------------:|---------------:|---------------:|---------------:|------:|--------:|------------:|-----------:|----------:|-------------:|
|  CompiledRegex |                 1000 |                           0 |                          0 |       3.381 us |      0.1587 us |      0.4655 us |       3.200 us |  1.00 |    0.00 |           - |          - |         - |         4 KB |
| RegexExtension |                 1000 |                           0 |                          0 |       6.151 us |      0.2991 us |      0.8772 us |       6.000 us |  1.85 |    0.36 |           - |          - |         - |        13 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                           0 |                     100000 | 145,016.025 us |  2,863.8565 us |  2,812.6900 us | 146,144.850 us | 1.000 |    0.00 |  26000.0000 | 14000.0000 | 3000.0000 |   391,533 KB |
| RegexExtension |                 1000 |                           0 |                     100000 |       7.999 us |      0.3498 us |      1.0315 us |       7.750 us | 0.000 |    0.00 |           - |          - |         - |        21 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                           0 |                     200000 | 380,037.800 us |  3,195.3889 us |  2,668.2945 us | 380,053.900 us | 1.000 |    0.00 |  51000.0000 | 27000.0000 | 4000.0000 |   783,045 KB |
| RegexExtension |                 1000 |                           0 |                     200000 |       8.481 us |      0.3589 us |      1.0470 us |       8.400 us | 0.000 |    0.00 |           - |          - |         - |        21 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                      100000 |                          0 | 206,456.623 us |  1,783.4354 us |  1,489.2494 us | 206,538.800 us |  1.00 |    0.00 |  26000.0000 | 14000.0000 | 3000.0000 |   391,533 KB |
| RegexExtension |                 1000 |                      100000 |                          0 |  99,272.350 us |    956.2306 us |    847.6735 us |  99,097.150 us |  0.48 |    0.00 |  25000.0000 |          - |         - |   209,821 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                      100000 |                     100000 | 450,871.033 us |  6,973.2095 us |  6,522.7445 us | 452,891.400 us |  1.00 |    0.00 |  51000.0000 | 27000.0000 | 4000.0000 |   783,047 KB |
| RegexExtension |                 1000 |                      100000 |                     100000 | 111,826.786 us |    577.9171 us |    512.3084 us | 111,795.200 us |  0.25 |    0.00 |  25000.0000 |          - |         - |   209,829 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                      100000 |                     200000 | 641,967.681 us |  4,229.5275 us |  3,531.8470 us | 641,693.150 us |  1.00 |    0.00 |  76000.0000 | 41000.0000 | 5000.0000 | 1,174,559 KB |
| RegexExtension |                 1000 |                      100000 |                     200000 | 111,957.180 us |    788.6008 us |    737.6577 us | 111,781.900 us |  0.17 |    0.00 |  25000.0000 |          - |         - |   209,829 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                      200000 |                          0 | 517,166.436 us |  3,392.9147 us |  3,007.7305 us | 516,462.300 us |  1.00 |    0.00 |  51000.0000 | 27000.0000 | 4000.0000 |   783,045 KB |
| RegexExtension |                 1000 |                      200000 |                          0 | 224,283.813 us |  1,959.6282 us |  1,833.0374 us | 224,403.000 us |  0.43 |    0.00 |  51000.0000 |          - |         - |   419,629 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                      200000 |                     100000 | 713,163.153 us | 12,084.0404 us | 11,303.4189 us | 708,513.500 us |  1.00 |    0.00 |  76000.0000 | 41000.0000 | 5000.0000 | 1,174,557 KB |
| RegexExtension |                 1000 |                      200000 |                     100000 | 221,747.320 us |  1,009.4813 us |    944.2694 us | 221,783.700 us |  0.31 |    0.01 |  51000.0000 |          - |         - |   419,637 KB |
|                |                      |                             |                            |                |                |                |                |       | |             |            |           |              |
|  CompiledRegex |                 1000 |                      200000 |                     200000 | 888,283.467 us | 17,136.2270 us | 16,029.2374 us | 881,548.800 us |  1.00 |    0.00 | 100000.0000 | 54000.0000 | 5000.0000 | 1,566,061 KB |
| RegexExtension |                 1000 |                      200000 |                     200000 | 225,162.413 us |  1,318.3762 us |  1,233.2100 us | 225,180.000 us |  0.25 |    0.00 |  51000.0000 |          - |         - |   419,636 KB |
### Async vs Sync

Because Span is not supported in async contexts async is significantly slower.

|              Method | TestFileName |     Mean |   Error |  StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|-------------------- |------------- |---------:|--------:|--------:|------:|--------:|-----------:|----------:|----------:|
|      RegexExtension |    175MB.txt | 193.3 ms | 2.93 ms | 2.74 ms |  1.00 |    0.00 | 23000.0000 |         - |    370 MB |
| RegexExtensionAsync |    175MB.txt | 406.8 ms | 7.71 ms | 8.25 ms |  2.10 |    0.06 | 43000.0000 | 1000.0000 |    384 MB |

## License

MIT
