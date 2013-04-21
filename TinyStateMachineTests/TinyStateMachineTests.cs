using System;
using NUnit.Framework;

[TestFixture]
public class TinyStateMachineTests
{
    enum DoorState
    {
        Closed,
        Opened,
    }

    enum DoorEvents
    {
        Open,
        Close,
    }

    private static TinyStateMachine<DoorState, DoorEvents> GetFixture()
    {
        var machine = new TinyStateMachine<DoorState, DoorEvents>(DoorState.Closed);
        machine.Tr(DoorState.Closed, DoorEvents.Open, DoorState.Opened)
               .Tr(DoorState.Opened, DoorEvents.Close, DoorState.Closed);
        return machine;
    }

    [Test]
    public void Test_simple_machine_construction()
    {
        var machine = GetFixture();
        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
    }

    [Test]
    public void Test_simple_transition()
    {
        var machine = GetFixture();
        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
        machine.Fire(DoorEvents.Open);
        Assert.That(machine.State, Is.EqualTo(DoorState.Opened));
    }

    [Test]
    public void Appropritate_action_is_called_on_transition()
    {
        var wasDoorOpened = false;
        var wasDoorClosed = false;

        var machine = new TinyStateMachine<DoorState, DoorEvents>(DoorState.Closed);
        machine.Tr(DoorState.Closed, DoorEvents.Open,  DoorState.Opened).OnTransition(()=> wasDoorOpened = true)
               .Tr(DoorState.Opened, DoorEvents.Close, DoorState.Closed).OnTransition(()=> wasDoorClosed = true);

        Assert.That(wasDoorOpened, Is.False);
        Assert.That(wasDoorClosed, Is.False);

        machine.Fire(DoorEvents.Open);

        Assert.That(wasDoorOpened, Is.True);
        Assert.That(wasDoorClosed, Is.False);

        machine.Fire(DoorEvents.Close);

        Assert.That(wasDoorOpened, Is.True);
        Assert.That(wasDoorClosed, Is.True);
    }

    [Test]
    public void Firing_trigger_with_no_valid_transition_throws_exception()
    {
        var machine = GetFixture();
        Assert.Throws<InvalidOperationException>(() => machine.Fire(DoorEvents.Close));
    }

    [Test]
    public void Guard_can_stop_transition()
    {
        var machine = new TinyStateMachine<DoorState, DoorEvents>(DoorState.Closed);
        machine.Tr(DoorState.Closed, DoorEvents.Open,  DoorState.Opened).Guard(() => false)
               .Tr(DoorState.Opened, DoorEvents.Close, DoorState.Closed);

        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
        machine.Fire(DoorEvents.Open);
        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
    }

    [Test]
    public void Action_is_called_with_correct_parameters()
    {
        var machine = new TinyStateMachine<DoorState, DoorEvents>(DoorState.Closed);
        machine
            .Tr(DoorState.Closed, DoorEvents.Open, DoorState.Opened)
            .OnTransition((from, trigger, to) => 
            {
                Assert.That(from, Is.EqualTo(DoorState.Closed));
                Assert.That(trigger, Is.EqualTo(DoorEvents.Open));
                Assert.That(to, Is.EqualTo(DoorState.Opened));
            })
            .Tr(DoorState.Opened, DoorEvents.Close, DoorState.Closed)
            .OnTransition((from, trigger, to) =>
            {
                Assert.That(from, Is.EqualTo(DoorState.Opened));
                Assert.That(trigger, Is.EqualTo(DoorEvents.Close));
                Assert.That(to, Is.EqualTo(DoorState.Closed));
            });

        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
        machine.Fire(DoorEvents.Open);
        Assert.That(machine.State, Is.EqualTo(DoorState.Opened));
        machine.Fire(DoorEvents.Close);
        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
    }

    [Test]
    public void Guard_is_called_with_correct_parameters()
    {
        var machine = new TinyStateMachine<DoorState, DoorEvents>(DoorState.Closed);
        machine
            .Tr(DoorState.Closed, DoorEvents.Open, DoorState.Opened)
            .Guard((from, trigger, to) =>
            {
                Assert.That(from, Is.EqualTo(DoorState.Closed));
                Assert.That(trigger, Is.EqualTo(DoorEvents.Open));
                Assert.That(to, Is.EqualTo(DoorState.Opened));
                return true;
            })
            .Tr(DoorState.Opened, DoorEvents.Close, DoorState.Closed)
            .Guard((from, trigger, to) =>
            {
                Assert.That(from, Is.EqualTo(DoorState.Opened));
                Assert.That(trigger, Is.EqualTo(DoorEvents.Close));
                Assert.That(to, Is.EqualTo(DoorState.Closed));
                return true;
            });

        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
        machine.Fire(DoorEvents.Open);
        Assert.That(machine.State, Is.EqualTo(DoorState.Opened));
        machine.Fire(DoorEvents.Close);
        Assert.That(machine.State, Is.EqualTo(DoorState.Closed));
    }
}
