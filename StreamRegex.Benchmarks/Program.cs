using BenchmarkDotNet.Running;
using StreamRegex.Benchmarks;
using StreamRegex.Lib;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

var summary = BenchmarkRunner.Run<PerformanceVsStandard>();
// NFATest();
void ExtensionsTest()
{
    var text = File.OpenRead("175MB");
    var stateMachine = StateMachineFactory.CreateStateMachine("ra[ce]*car");
    var match = stateMachine.Match(text);
    if (match is null)
    {
        Console.WriteLine("No match.");
    }
    else
    {
        Console.WriteLine("Found match {Match} at {Position}", match.matched, match.position);
    } 
}
void DFATest()
{
    var text = File.OpenRead("Tiny.txt");
    var stateMachine = StateMachineFactory.CreateStateMachine("ra[ce]*car");
    var match = stateMachine.Match(text);
    if (match is null)
    {
        Console.WriteLine("No match.");
    }
    else
    {
        Console.WriteLine("Found match {Match} at {Position}", match.matched, match.position);
    } 
}

void NFATest()
{
    var text = File.OpenRead("TargetEnd.txt");
    var stateMachine = NfaStateMachineFactory.CreateStateMachine("racecar");
    var match = stateMachine.Match(text);
    if (match is null)
    {
        Console.WriteLine("No match.");
    }
    else
    {
        Console.WriteLine("Found match {0} at {1}", match.matched, match.position);
    }
}

