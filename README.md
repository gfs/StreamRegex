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
RegexStreamMatchCollection matchCollection = myRegex.GetMatchCollection(reader);
if (matchCollection.Any())
{
    foreach(RegexStreamMatch match in matchCollection)
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
RegexStreamMatch match = myRegex.GetFirstMatch(reader);
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
RegexStreamMatchCollection matchCollection = myRegex.GetMatchCollection(stream);
if (matchCollection.Any())
{
    foreach(RegexStreamMatch match in matchCollection)
    {
        // Do something with matches.
    }
}
else
{
    // No match
}
```
