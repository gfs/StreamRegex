[![Nuget](https://img.shields.io/nuget/v/StreamRegex.Extensions)](https://www.nuget.org/packages/StreamRegex.Extensions/)
# StreamRegex
A .NET Library with Extension Methods for performing arbitrary checks on the string content of Streams and StreamReaders, including built-in extension methods for Regex.

The Extensions are available on Nuget: [https://www.nuget.org/packages/StreamRegex.Extensions/](https://www.nuget.org/packages/StreamRegex.Extensions/)
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
A sliding buffer is used across the stream. The `OverlapSize` parameter is the amount of overlap buffer to use to ensure no matches are missed across buffer boundaries.

https://github.com/gfs/StreamRegex/blob/fce9cdbbe5bdcf3629ece9547a4c5230b941d072/StreamRegex.Extensions/SlidingBufferExtensions.cs#L206-L245
## Benchmarks
These benchmarks were run with a pre-release version of the library. 

### Large File Test
* This is a worst case scenario. A very large file (175MB) that contains what we want to find once at the very end.
* The query used for both regex and string matching was `racecar` - no regex operators.
* The benchmarks for CompiledRegex and SimpleString operations emulate existing behavior of reading the contents of the Stream into a string to be queried.
* This library allocates significantly less memory and for Regular Expressions is faster.

|          Method | TestFileName |       Mean |    Error |   StdDev | Ratio |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|---------------- |------------- |-----------:|---------:|---------:|------:|-----------:|-----------:|----------:|----------:|
|   CompiledRegex |    175MB.txt |   438.3 ms |  8.72 ms | 10.71 ms |  0.10 | 24000.0000 | 13000.0000 | 3000.0000 |    686 MB |
|  RegexExtension |    175MB.txt |   214.4 ms |  4.30 ms |  7.86 ms |  0.05 | 23000.0000 |          - |         - |    370 MB |
|    SimpleString |    175MB.txt | 4,361.8 ms | 18.09 ms | 14.12 ms |  1.00 | 24000.0000 | 13000.0000 | 3000.0000 |    686 MB |
| StringExtension |    175MB.txt | 4,379.5 ms | 16.09 ms | 12.56 ms |  1.00 | 22000.0000 |          - |         - |    366 MB |

### Async vs Sync

Because Span is not supported in async contexts async is significantly slower.

|              Method | TestFileName |     Mean |   Error |  StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Allocated |
|-------------------- |------------- |---------:|--------:|--------:|------:|--------:|-----------:|----------:|----------:|
|      RegexExtension |    175MB.txt | 193.3 ms | 2.93 ms | 2.74 ms |  1.00 |    0.00 | 23000.0000 |         - |    370 MB |
| RegexExtensionAsync |    175MB.txt | 406.8 ms | 7.71 ms | 8.25 ms |  2.10 |    0.06 | 43000.0000 | 1000.0000 |    384 MB |
