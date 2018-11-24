using System;
using System.Collections.Generic;

namespace ServerClientStateMachine
{
    public enum TransitionMatching
    {
        ClientAndServerMatch,
        ClientAndServerCanMismatch,
        ClientAndServerMismatch
    }

    public class StateMachineBuilder<TState>
        where TState : Enum
    {
        List<StateTransitionRule<TState>> m_ServerRules = new List<StateTransitionRule<TState>>();
        List<StateTransitionRule<TState>> m_ClientRules = new List<StateTransitionRule<TState>>();

        List<StateTransitionPredicate<TState>> m_ServerPredicate = new List<StateTransitionPredicate<TState>>();
        List<StateTransitionPredicate<TState>> m_ClientPredicate = new List<StateTransitionPredicate<TState>>();

        TState m_InitialServer = default;
        TState m_InitialClient = default;

        public StateMachineBuilder<TState> ServerPermit(TState from, TState to, TransitionMatching matching = TransitionMatching.ClientAndServerMatch)
        {
            AddRule(m_ServerRules, from, to, matching);
            return this;
        }

        public StateMachineBuilder<TState> ClientPermit(TState from, TState to, TransitionMatching matching = TransitionMatching.ClientAndServerMatch)
        {
            AddRule(m_ClientRules, from, to, matching);
            return this;
        }

        public StateMachineBuilder<TState> TryServerMatch(TState from, TState to, Func<bool> predicate)
        {
            AddPredicate(m_ServerPredicate, from, to, predicate);
            return this;
        }

        public StateMachineBuilder<TState> TryClientMatch(TState from, TState to, Func<bool> predicate)
        {
            AddPredicate(m_ServerPredicate, from, to, predicate);
            return this;
        }

        public StateMachineBuilder<TState> ServerTimeout(TimeSpan span, TState errorState)
        {
            return this;
        }

        public StateMachineBuilder<TState> ClientTimeout(TimeSpan span, TState errorState)
        {
            return this;
        }

        public StateMachineBuilder<TState> InitialServer(TState state)
        {
            m_InitialServer = state;
            return this;
        }

        public StateMachineBuilder<TState> InitialClient(TState state)
        {
            m_InitialClient = state;
            return this;
        }

        public StateMachine<TState> BuildClient()
        {
            return new StateMachine<TState>(
                initialState: m_InitialClient, 
                isServer: false, 
                rules: m_ClientRules, 
                remoteRules: m_ServerRules,
                actions: m_ClientPredicate
            );
        }

        public StateMachine<TState> BuildServer()
        {
            return new StateMachine<TState>(
                initialState: m_InitialServer,
                isServer: true,
                rules: m_ServerRules,
                remoteRules: m_ClientRules,
                actions: m_ServerPredicate
            );
        }

        private void AddRule(List<StateTransitionRule<TState>> target, TState from, TState to, TransitionMatching matching)
        {
            // TODO: checks
            target.Add(new StateTransitionRule<TState>(from, to, matching));
        }

        private void AddPredicate(List<StateTransitionPredicate<TState>> target, TState from, TState to, Func<bool> predicate)
        {
            // TODO: checks
            target.Add(new StateTransitionPredicate<TState>(from, to, predicate));
        }

    }
}
