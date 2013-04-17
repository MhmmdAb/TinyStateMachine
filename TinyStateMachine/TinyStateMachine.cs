// Copyright (c) 2013, Mohammad Bahij Abdulfatah
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//   * Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Diagnostics;
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
        public TState Next { get; private set; }
        public Action<TState, TState, TTrigger> Action { get; private set; }
        public Func<TState, TState, TTrigger, bool> Guard { get; private set; }

        public TransitionEntry
            (TState next
            , Action<TState, TState, TTrigger> action
            , Func<TState, TState, TTrigger, bool> guard)
        {
            Next = next;
            Action = action;
            Guard = guard;
        }
    }
    #endregion

    private bool canConfigure;
    private TState state;
    private readonly Dictionary<TState, Dictionary<TTrigger, TransitionEntry>> transitions;

    /// <summary>
    /// Initializes a new instance with the starting state given in 
    /// <paramref name="startingState"/>
    /// </summary>
    /// <param name="startingState">The starting state of the FSM</param>
    public TinyStateMachine(TState startingState)
    {
        canConfigure = true;
        state = startingState;
        transitions = new Dictionary<TState, Dictionary<TTrigger, TransitionEntry>>();
    }

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
    /// Transitions to a new state determined by <paramref name="trigger"/>
    /// and the configuration of the current state previously set by calls
    /// to one of the <see cref="Tr"/> methods.
    /// </summary>
    /// 
    /// <param name="trigger">The trigger to fire.</param>
    /// 
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
            =  transition.Guard == null
            || transition.Guard(state, transition.Next, trigger);
        if (!guardAllowsFiring)
            return;

        if (transition.Action != null)
            transition.Action(state, transition.Next, trigger);
        state = transition.Next;
    }

    /// <summary>
    /// Adds a new entry to the state transition table. See
    /// <see cref="Tr(TState, TState, TTrigger, Action<TState, TState, TTrigger>,
    /// Func<TState, TState, TTrigger, bool>)"/> for details.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Tr
        (TState from
        , TState to
        , TTrigger trigger)
    {
        Action<TState, TState, TTrigger> action = null;
        Func<TState, TState, TTrigger, bool> guard = null;
        return Tr(from, to, trigger, action, guard);
    }

    /// <summary>
    /// Adds a new entry to the state transition table. See
    /// <see cref="Tr(TState, TState, TTrigger, Action<TState, TState, TTrigger>,
    /// Func<TState, TState, TTrigger, bool>)"/> for details.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Tr
        ( TState from
        , TState to
        , TTrigger trigger
        , Action action)
    {
        return Tr(from, to, trigger, (c, t, n) => action());
    }

    /// <summary>
    /// Adds a new entry to the state transition table. See
    /// <see cref="Tr(TState, TState, TTrigger, Action<TState, TState, TTrigger>,
    /// Func<TState, TState, TTrigger, bool>)"/> for details.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Tr
        (TState from
        , TState to
        , TTrigger trigger
        , Action<TState, TState, TTrigger> action)
    {
        return Tr(from, to, trigger, action, null);
    }

    /// <summary>
    /// Adds a new entry to the state transition table. See
    /// <see cref="Tr(TState, TState, TTrigger, Action<TState, TState, TTrigger>,
    /// Func<TState, TState, TTrigger, bool>)"/> for details.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Tr
        (TState from
        , TState to
        , TTrigger trigger
        , Func<bool> guard)
    {
        return Tr(from, to, trigger, (f, t, tr) => guard());
    }

    /// <summary>
    /// Adds a new entry to the state transition table. See
    /// <see cref="Tr(TState, TState, TTrigger, Action<TState, TState, TTrigger>,
    /// Func<TState, TState, TTrigger, bool>)"/> for details.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Tr
        ( TState from
        , TState to
        , TTrigger trigger
        , Func<TState, TState, TTrigger, bool> guard)
    {
        return Tr(from, to, trigger, null, guard);
    }

    /// <summary>
    /// Adds a new entry to the state transition table. See
    /// <see cref="Tr(TState, TState, TTrigger, Action<TState, TState, TTrigger>,
    /// Func<TState, TState, TTrigger, bool>)"/> for details.
    /// </summary>
    public TinyStateMachine<TState, TTrigger> Tr
        (TState from
        , TState to
        , TTrigger trigger
        , Action action
        , Func<bool> guard)
    {
        return Tr(from, to, trigger, (f1, t1, tr1) => action(), (f2, t2, tr2) => guard());
    }

    /// <summary>
    /// Short for "Transition." Adds a new entry to the state transition table.
    /// </summary>
    /// <param name="from">Current state</param>
    /// <param name="to">The state the FSM will transition to.</param>
    /// <param name="trigger">Trigger</param>
    /// <param name="action">A delegate to a method that will be called _after_
    /// making the transition described by <paramref name="from"/>,
    /// <paramref name="to"/>, and <paramref name="trigger"/>.</param>
    /// <param name="guard">A delegate to a method that will be called before 
    /// before attempting to make the transition described by 
    /// <paramref name="from"/>, <paramref name="to"/>,
    /// and <paramref name="trigger"/>. The transition will be aborted silently
    /// and without raising any errors if the method pointed to by
    /// <c>guard</c> returns <c>false</c>, and it will proceed normally if the
    /// method returns <c>true</c>.
    /// <returns><c>this</c></returns>
    /// <exception cref="System.InvalidOperationException">If called after
    /// calling <see cref="Fire()">Fire</see> or <see cref="State"/></exception>
    /// <remarks>
    /// <see cref="Tr"/> methods should be called after the
    /// <see cref="TinyStateMachine(TState)">constructor</see> and
    /// _before_ calling <see cref="Fire()">Fire</see> or <see cref="State"/>.
    /// Attempting to call any of the <see cref="Tr"/> methods 
    /// afterward will raise an
    /// <see cref="System.InvalidOperationException">InvalidOperationException</see>.
    /// </remarks>
    public TinyStateMachine<TState, TTrigger> Tr
        ( TState from
        , TState to
        , TTrigger trigger
        , Action<TState, TState, TTrigger> action
        , Func<TState, TState, TTrigger, bool> guard)
    {
        if (!canConfigure)
        {
            string errorMessage
                = "\"AddTransition\" cannot be called after  \"Fire()\" "
                + "or \"State\" are called.";
            throw new InvalidOperationException(errorMessage);
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

        var transtition = new TransitionEntry(to, action, guard);
        transitions[from].Add(trigger, transtition);

        return this;
    }
}
