# Tiny State Machine

**Tiny State Machie** is a lightweight [Finite State Machine](http://t.co/)
library for the .NET framework written in C#. It's so lightweight
that it consists of a single source file. There are no assemlbies to reference
and no binaries to wrangle.

# Example
A good example is a Finite State Machine (FSM) for a door. To keep
things as simple as possible, let's assume that the door can be in one
of two states: *Closed* and *Opened*. We'll also need triggers (or events)
that will cause the door to change state. There are two of those as well:
*Close* and *Open*.

Here's the [state transition table](https://en.wikipedia.org/wiki/State_transition_table) for this FSM

| Current state | Trigger | Next state |
| --- | --- | --- |
| Closed | Open  | Opened |
| Opened | Close | Closed |

And here's how this table is represented in Tiny State Machine:

~~~c#
// Define the states of the door FSM
public enum DoorState { Opened, Closed }

// Define the triggers that would cause the
// FSM to switch between states.
public enum DoorTrigger { Open, Close }

public void WorkTheDoor()
{
    // Declare the FSM and specify the starting state.
    var doorFsm = new TinyStateMachine<DoorState, DoorTrigger>
    (DoorState.Closed);

    // Now configure the state transition table.
    doorFsm.Tr(DoorState.Closed, DoorTrigger.Open,  DoorState.Opened)
           .Tr(DoorState.Opened, DoorTrigger.Close, DoorState.Closed);

    // As specified in the constructor, the door starts closed.
    Debug.Assert(doorFsm.IsInState(DoorState.Closed));

    // Let's trigger a transition
    doorFsm.Trigger(DoorTrigger.Open);

    // Door is now open.
    Debug.Assert(doorFsm.IsInState(DoorState.Opened));

    // Let's trigger the other transition
    doorFsm.Trigger(DoorTrigger.Close);

    // Door is now closed.
    Debug.Assert(doorFsm.IsInState(DoorState.Closed));

    // According to the transition table, a closed door
    // cannot be closed. The following will throw an exception
    bool exceptionWasThrown = false;
    try
    {
        doorFsm.Trigger(DoorTrigger.Close);
    }
    catch
    {
        exceptionWasThrown = true;
    }
    Debug.Assert(exceptionWasThrown == true);
}
~~~

Note how `enum` types are used to define both the states and the triggers. It's
possible to use any other type for this purpose, but enums usually make the
most sense, especially when it comes to triggers.

A more ellaborate example will follow.

# Requirements
Tiny State Machine runs on:

*   The .NET Framework 3.5 and above.
*   Unity3D 4.6 and above.

It might work with other frameworks and/or versions, but these are
the ones that I have tested.

# Installation
Download the file [TinyStateMachine.cs](https://github.com/MhmmdAb/TinyStateMachine/blob/master/TinyStateMachine.cs)
and add it to your project.

# License
Tiny State Machine is released under the [MIT license](https://github.com/MhmmdAb/TinyStateMachine/blob/master/LICENSE).
Crediting the [author](http://m16h.com) is highly appreciated but not at all
required.

# Credits
  * The whole *tiny* philosophy of small, single file libraries was inpspired by
    [TinyMessenger](https://github.com/grumpydev/TinyMessenger).
  * Some of the terminology and concepts were borrowed from
    [Stateless](https://github.com/dotnet-state-machine/stateless), an
excellent and more elaborate FSM implementation for .NET.
  * The state transition table concept was inpsired by
    [Boost's Meta State Machine](http://www.boost.org/doc/libs/release/libs/msm/).
