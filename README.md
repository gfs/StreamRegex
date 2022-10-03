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
* If the finding is located at the start of the Stream we see a 20,000x speedup and an 18,644x reduction in amount of memory used.
* If the finding is located in the middle of the Stream we see a ~4x speedup and a ~4x reduction in memory usage.
* If the fingind is lcoated at the end of the Stream we see a ~2.5x speedup and a ~2x reduction in memory usage.


| Method                 | paddingSegmentLength | numberPaddingSegmentsBefore | numberPaddingSegmentsAfter |           Mean |         Error |         StdDev |         Median |      Gen 0 |      Gen 1 |     Gen 2 |  Allocated |
|----------------------- |--------------------- |---------------------------- |--------------------------- |---------------:|--------------:|---------------:|---------------:|-----------:|-----------:|----------:|-----------:|
|          CompiledRegex |                 1000 |                           0 |                     100000 | 162,222.236 us | 2,526.0027 us |  2,239.2356 us | 161,818.200 us | 26000.0000 | 14000.0000 | 3000.0000 | 391,533 KB |
|         RegexExtension |                 1000 |                           0 |                     100000 |       8.078 us |     0.3774 us |      1.0948 us |       7.700 us | - |          - |         - |      21 KB |
|          CompiledRegex |                 1000 |                      100000 |                     100000 | 485,832.839 us | 9,497.0205 us | 10,161.7055 us | 483,620.250 us | 51000.0000 | 27000.0000 | 4000.0000 | 783,045 KB |
|         RegexExtension |                 1000 |                      100000 |                     100000 | 111,593.716 us | 2,215.5921 us |  3,383.4432 us | 112,188.200 us | 25000.0000 |          - |         - | 209,829 KB |
|          CompiledRegex |                 1000 |                      100000 |                          0 | 224,392.006 us | 4,217.4968 us |  4,142.1458 us | 223,900.000 us | 26000.0000 | 14000.0000 | 3000.0000 | 391,525 KB |
|         RegexExtension |                 1000 |                      100000 |                          0 |  98,293.693 us |   748.6467 us |    700.2846 us |  98,222.300 us | 25000.0000 |          - |         - | 209,822 KB |
### Async vs Sync

Because Span is not supported in async contexts async is significantly slower.

|              Method | TestFileName |     Mean |   Error |  StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|-------------------- |------------- |---------:|--------:|--------:|------:|--------:|-----------:|----------:|----------:|
|      RegexExtension |    175MB.txt | 193.3 ms | 2.93 ms | 2.74 ms |  1.00 |    0.00 | 23000.0000 |         - |    370 MB |
| RegexExtensionAsync |    175MB.txt | 406.8 ms | 7.71 ms | 8.25 ms |  2.10 |    0.06 | 43000.0000 | 1000.0000 |    384 MB |

## License

MIT
