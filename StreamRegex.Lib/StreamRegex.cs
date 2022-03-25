using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StreamRegex.Lib;

public class StreamRegex
{
    private List<IState> _states;
    private int _bufferSize = 4096;
    private readonly ILogger _logger;

    // TODO: Add method to validate state machine can finish
    internal StreamRegex(List<IState> states, ILoggerFactory? loggerFactory = null)
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
    
    public long GetFirstMatchPosition(Stream toMatch)
    {
        var curState = _states[0]; 
        byte[] buffer = new byte[_bufferSize];
        long resultPosition = toMatch.Position;
        long counter = 0;
        var numBytes = toMatch.Read(buffer, 0, _bufferSize);
        while (numBytes > 0)
        {
            for (int byteNum = 0; byteNum < numBytes; byteNum++)
            {
                counter++;
                var nextState = curState.Transition((char)buffer[byteNum]);
                if (nextState == _states[0] && !curState.Accepts((char)buffer[byteNum]))
                {
                    resultPosition += counter;
                    counter = 0;
                }
                curState = nextState;
                if (curState.IsFinal)
                {
                    return resultPosition;
                }
            }

            numBytes = toMatch.Read(buffer, 0, _bufferSize);
        }

        return -1;
    }
}