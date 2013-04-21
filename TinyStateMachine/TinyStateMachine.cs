// Copyright (c) 2013 Mohammad Bahij Abdulfatah
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
//   1. The origin of this software must not be misrepresented; you must not
//      claim that you wrote the original software. If you use this software
//      in a product, an acknowledgment in the product documentation would be
//      appreciated but is not required.
//
//   2. Altered source versions must be plainly marked as such, and must not be
//      misrepresented as being the original software.
//
//   3. This notice may not be removed or altered from any source distribution.

using System;
using System.Collections.Generic;

/// <summary>
/// A simple
/// <a href="http://en.wikipedia.org/wiki/Finite-state_machine">finite-state
/// machine (FSM)</a> that uses state transition tables for configuration.
/// </summary>
/// <typeparam name="TState">
/// The type representing all possible states of the FSM.
/// </typeparam>
/// <typeparam name="TTrigger">
/// The type representing the triggers that cause state transitions.
/// </typeparam>
public class TinyStateMachine<TState, TTrigger>
{
    #region Nested types

    private class TransitionEntry
    {
        public Action<TState, TTrigger, TState> Action { get; set; }
        public Func<TState, TTrigger, TState, bool> Guard { get; set; }
        public TState Next { get; private set; }

        public TransitionEntry(TState next)
        {
            Next = next;
            Action = null;
            Guard = null;
        }
    }

    #endregion Nested types

    private readonly Dictionary<TState, Dictionary<TTrigger, TransitionEntry>> transitions;
    private bool canConfigure;
    private TState lastConfiguredState;
    private TTrigger lastConfiguredTrigger;
    private readonly TState startingState;
    private TState state;

    /// <summary>
    /// The current state of the FSM.
    /// </summary>
    public TState State
    {
        get
        {
            canConfigure = false;
            return state;
        }
    }

    /// <summary>
    /// Initializes a new instance with the starting state given in
    /// <paramref name="startingState"/>
    /// </summary>
    /// <param name="startingState">The starting state of the FSM</param>
    public TinyStateMachine(TState startingState)
    {
        this.canConfigure = true;
        this.state = startingState;
        this.startingState = startingState;
        this.transitions = new Dictionary<TState, Dictionary<TTrigger, TransitionEntry>>();
    }

    /// <summary>
    /// Transitions to a new state determined by <paramref name="trigger"/>
    /// and the configuration of the current state previously set by calls
    /// to one of the <see cref="Tr"/> methods.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">No transition was
    /// configured for <paramref name="trigger"/> and the current state.
    /// </exception>
    public void Fire(TTrigger trigger)
    {
        canConfigure = false;

        if (!transitions.ContainsKey(state))
        {
            var errorMessage = string.Format
                ("There are no transitions configured for state \"{0}\""
                , state);
            throw new InvalidOperationException(errorMessage);
        }

        if (!transitions[state].ContainsKey(trigger))
        {
            var errorMessage = string.Format
                ("There are no transitions configured for state \"{0}\" and trigger \"{1}\""
                , state
                , trigger);
            throw new InvalidOperationException(errorMessage);
        }

        var transition = transitions[state][trigger];

        var guardAllowsFiring
            = transition.Guard == null
            || transition.Guard(state, trigger, transition.Next);
        if (!guardAllowsFiring)
            return;

        if (transition.Action != null)
            transition.Action(state, trigger, transition.Next);
        state = transition.Next;
    }

    /// <summary>
    /// See <see cref="Guard(Func<TState,TTrigger,TState,bool>)"/>.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Guard(Func<bool> guard)
    {
        return Guard((f, tr, t) => guard());
    }

    /// <summary>
    /// Sets the method that will be called _before_ attempting to make the
    /// transition described by the last call to <see cref="Tr()">Tr</see>. The
    /// transition will be silently aborted without throwing any errors if
    /// <paramref name="guard"/> method returns <c>false</c>, and will continue
    /// normally if the method returns <c>true</c>
    /// </summary>
    /// <param name="guard">A delegate to the method that will be called
    /// before attempting the transition.</param>
    /// <returns><c>this</c></returns>
    /// <exception cref="System.InvalidOperationException">No transition was
    /// configured before calling this method or a guard was already set
    /// for the last transition.
    /// </exception>
    public TinyStateMachine<TState, TTrigger> Guard(Func<TState, TTrigger, TState, bool> guard)
    {
        if (!canConfigure)
        {
            throw new InvalidOperationException
                ("\"Guard\" cannot be called after \"Fire()\" or \"State\" are called.");
        }

        if (transitions.Count == 0)
        {
            throw new InvalidOperationException
                ("\"Action\" cannot be called before configuring a transition.");
        }

        var tr = transitions[lastConfiguredState][lastConfiguredTrigger];

        if (tr.Guard != null)
        {
            var errorMessage = string.Format
                ("A guard has already been configured for state {0} and trigger {1}."
                , lastConfiguredState
                , lastConfiguredTrigger);
            throw new InvalidOperationException(errorMessage);
        }

        tr.Guard = guard;

        return this;
    }

    /// <summary>
    /// See <see cref="OnTransition(Action<TState,TTrigger,TState>)"/>.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> OnTransition(Action action)
    {
        return OnTransition((f, tr, t) => action());
    }

    /// <summary>
    /// Sets the action that will be called _after_ the transition described
    /// by the last call to <see cref="Tr()">Tr</see> takes place.
    /// </summary>
    /// <param name="action">A delegate to a method that will be called on state
    /// change.</param>
    /// <returns><c>this</c></returns>
    /// <exception cref="System.InvalidOperationException">No transition was
    /// configured before calling this method or an action was already set
    /// for the last transition.
    /// </exception>
    public TinyStateMachine<TState, TTrigger> OnTransition(Action<TState, TTrigger, TState> action)
    {
        if (!canConfigure)
        {
            throw new InvalidOperationException
                ("\"Action\" cannot be called after \"Fire()\" or \"State\" are called.");
        }

        if (transitions.Count == 0)
        {
            throw new InvalidOperationException
                ("\"Action\" cannot be called before configuring a transition.");
        }

        var tr = transitions[lastConfiguredState][lastConfiguredTrigger];
        if (tr.Action != null)
        {
            var errorMessage = string.Format
                ("An action has already been configured for state {0} and trigger {1}."
                , lastConfiguredState
                , lastConfiguredTrigger);
            throw new InvalidOperationException(errorMessage);
        }

        tr.Action = action;

        return this;
    }

    /// <summary>
    /// Sets the state of the machine to the starting state specified in
    /// the <see cref="TinyStateMachine(TState)">constructor</see>, but does
    /// _not_ fire any <see cref="OnTransition">transition events</see>
    /// and does _not_ check any of the <see cref="Guard">guard methods.</see>
    /// </summary>
    public void Reset()
    {
        this.state = startingState;
    }

    /// <summary>
    /// Sets the state of the machine to <paramref name="state"/>, but does
    /// _not_ fire any <see cref="OnTransition">transition events</see>
    /// and does _not_ check any of the <see cref="Guard">guard methods.</see>
    /// </summary>
    /// <param name="state">The state to which the machine will be set.</param>
    public void Reset(TState state)
    {
        this.state = state;
    }

    /// <summary>
    /// Short for "Transition." Adds a new entry to the state transition table.
    /// </summary>
    /// <param name="from">Current state</param>
    /// <param name="trigger">Trigger</param>
    /// <param name="to">The state the FSM will transition to.</param>
    /// <returns><c>this</c></returns>
    /// <exception cref="System.InvalidOperationException">If called after
    /// calling <see cref="Fire()">Fire</see> or <see cref="State"/></exception>
    /// <remarks>
    /// <see cref="Tr"/> methods should be called after the
    /// <see cref="TinyStateMachine(TState)">constructor</see> and
    /// _before_ calling <see cref="Fire()">Fire</see> or <see cref="State"/>.
    /// Attempting to call any of the <see cref="Tr"/> methods
    /// afterward will throw an
    /// <see cref="System.InvalidOperationException">InvalidOperationException</see>.
    /// </remarks>
    /// </param>
    public TinyStateMachine<TState, TTrigger> Tr(TState from, TTrigger trigger, TState to)
    {
        if (!canConfigure)
        {
            throw new InvalidOperationException
                ("\"Tr\" cannot be called after \"Fire()\" or \"State\" are called.");
        }

        if (!transitions.ContainsKey(from))
        {
            transitions.Add(from, new Dictionary<TTrigger, TransitionEntry>());
        }
        else if (transitions[from].ContainsKey(trigger))
        {
            string errorMessage = string.Format
                ("A transition is already defined for state {0} and trigger {1}"
                , from
                , trigger);
            throw new InvalidOperationException(errorMessage);
        }

        var transtition = new TransitionEntry(to);
        transitions[from].Add(trigger, transtition);

        lastConfiguredState = from;
        lastConfiguredTrigger = trigger;

        return this;
    }
}