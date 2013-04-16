using System;
using NUnit.Framework;

[TestFixture]
public class TinyStateMachineTests
{
    public enum TurnstileState
    {
        Locked,
        Unlocked,
    }

    public enum TurnstileTrigger
    {
        InsertCoin,
        Push,
    }

    private static TinyStateMachine<TurnstileState, TurnstileTrigger> ConfigureFixture()
    {
        var machine = new TinyStateMachine<TurnstileState, TurnstileTrigger>(TurnstileState.Locked);
        machine.Tr(TurnstileState.Locked,   TurnstileState.Locked,   TurnstileTrigger.Push)
               .Tr(TurnstileState.Locked,   TurnstileState.Unlocked, TurnstileTrigger.InsertCoin)
               .Tr(TurnstileState.Unlocked, TurnstileState.Unlocked, TurnstileTrigger.InsertCoin)
               .Tr(TurnstileState.Unlocked, TurnstileState.Locked,   TurnstileTrigger.Push);
        return machine;
    }

    [Test]
    public void Test_simple_machine_construction()
    {
        var m = ConfigureFixture();
        Assert.That(m.State, Is.EqualTo(TurnstileState.Locked));
    }

    [Test]
    public void Test_simple_transition()
    {
        var m = ConfigureFixture();
        m.Fire(TurnstileTrigger.InsertCoin);
        Assert.That(m.State, Is.EqualTo(TurnstileState.Unlocked));
    }
}
