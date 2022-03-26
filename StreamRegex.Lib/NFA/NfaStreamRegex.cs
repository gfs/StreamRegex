using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib.NFA;

public class NfaStreamRegex : IStreamRegex
{
    private List<INfaState> _states;
    private int _bufferSize = 4096;
    private readonly ILogger _logger;

    internal NfaStreamRegex(List<INfaState> states, ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<DfaStreamRegex>() ?? NullLogger<DfaStreamRegex>.Instance;
        _states = states;
    }

    public bool ValidateStateMachine()
    {
        if (!_states.Any(x => x.IsFinal))
        {
            _logger.LogError("State machine does not contain a final state.");
            return false;
        }
        
        var states = _states.Where(x => !x.IsFinal).Select(x => x.Success);;
        bool tooLoopy = true;
        // The minimum viable match should be at most N state transitions
        for (int i = 0; i < _states.Count; i++)
        {
            if (states.Any(x => x.IsFinal))
            {
                tooLoopy = false;
                break;
            }
            states = states.Select(x => x.Success);
        }

        if (tooLoopy)
        {
            _logger.LogError("The state machine does not appear to terminate.");
            return false;
        }

        return true;
    }
    public bool IsMatch(Stream toMatch)
    {
        return GetFirstMatchPosition(toMatch) != -1;
    }
    
    public StreamRegexMatch? Match(Stream toMatch)
    {
        Dictionary<INfaState, int> indexesOfStates = new Dictionary<INfaState, int>();
        for (int i = 0; i < _states.Count; i++)
        {
            indexesOfStates[_states[i]] = i;
        }
        
        // List<INfaState> currentStates = new();
        // List<INfaState> nextStates = new();
        // List<INfaState> seenNfaStates = new();
        // currentStates.Add(_states[0]);

        Span<bool> currentStatesBools = new bool[_states.Count];
        Span<bool> nextStatesBools = new bool[_states.Count];
        Span<bool> defaultBools = new bool[_states.Count];
        currentStatesBools[0] = true;

        byte[] buffer = new byte[_bufferSize];
        long resultPosition = toMatch.Position;
        long counter = 0;
        var numBytes = toMatch.Read(buffer, 0, _bufferSize);
        StringBuilder match = new();
        while (numBytes > 0)
        {
            bool any = false;
            bool final = false;
            for (int byteNum = 0; byteNum < numBytes; byteNum++)
            {
                counter++;
                
                any = false;
                final = false;
                for (int i = 0; i < _states.Count; i++)
                {
                    if (!currentStatesBools[i])
                    {
                        continue;
                    }
                    // if (seenNfaStates.Contains(currentStates[i]))
                    // {
                    //     continue;
                    // }
                    // seenNfaStates.Add(currentStates[i]);
                    foreach (var nextState in _states[i].Transition((char) buffer[byteNum]))
                    {
                        if (nextState.IsFinal)
                        {
                            final = true;
                            any = true;
                            goto LoopDone;
                        }
                    
                        if (!nextState.IsInitial)
                        {
                            any = true;
                        }

                        nextStatesBools[nextState.Index] = true;
                    }
                }

                LoopDone:
                if (any)
                {
                    match.Append((char) buffer[byteNum]);
                }
                else
                {
                    resultPosition += counter;
                    counter = 0;
                    match.Clear();
                }
                
                if (final)
                {
                    return new StreamRegexMatch(match.ToString(), resultPosition);
                }
                
                nextStatesBools.CopyTo(currentStatesBools);
                defaultBools.CopyTo(nextStatesBools);
            }

            numBytes = toMatch.Read(buffer, 0, _bufferSize);
        }

        return null;
    }
    
    public long GetFirstMatchPosition(Stream toMatch)
    {
        return Match(toMatch)?.position ?? -1;
    }
}