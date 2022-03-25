using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StreamRegex.Lib.NFA;

public class NFAStreamRegex
{
    private List<INFAState> _states;
    private int _bufferSize = 4096;
    private readonly ILogger _logger;

    internal NFAStreamRegex(List<INFAState> states, ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<StreamRegex>() ?? NullLogger<StreamRegex>.Instance;
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
        HashSet<INFAState> currentStates = new() { _states[0] };
        HashSet<INFAState> nextStates = new() { _states[0] };
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
                foreach (var currentState in currentStates)
                {
                    foreach (var nextState in currentState.Transition((char) buffer[byteNum]))
                    {
                        if (nextState.IsFinal)
                        {
                            final = true;
                            any = true;
                            goto LoopDone;
                        }
                        
                        if (nextState is not InitialNfaState)
                        {
                            any = true;
                        }

                        nextStates.Add(nextState);
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
                
                (currentStates, nextStates) = (nextStates, currentStates);
                nextStates.Clear();
                nextStates.Add(_states[0]);
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