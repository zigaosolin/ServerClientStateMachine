using System;
using System.Collections.Generic;
using System.Text;

namespace ServerClientStateMachine
{
    public enum StateTransitionFailReason
    {
        Success,
        NoRule,
        RuleServerClientStateMismatch,
        RuleServerClientStateMatch
    }

    public class StateMachine<TEnum>
        where TEnum : Enum
    {
        List<StateTransitionRule<TEnum>> m_Rules;
        List<StateTransitionRule<TEnum>> m_RemoteRules;
        List<StateTransitionPredicate<TEnum>> m_Actions;

        public bool IsServer { get; }
        public bool IsClient => !IsServer;
        public TEnum State { get; private set; }
        public TEnum RemoteState { get; private set; }

        internal StateMachine(TEnum initialState, bool isServer, 
            List<StateTransitionRule<TEnum>> rules, List<StateTransitionRule<TEnum>> remoteRules,
            List<StateTransitionPredicate<TEnum>> actions)
        {
            m_Rules = rules;
            m_RemoteRules = remoteRules;
            m_Actions = actions;

            IsServer = isServer;
            State = initialState;
            RemoteState = initialState;
        }

        public void ReportRemoteState(TEnum newServerState)
        {
            RemoteState = newServerState;

            if (State.Equals(newServerState))
                return;

            var transition = m_RemoteRules.Find(x => x.From.Equals(State) && x.To.Equals(newServerState));
            if (transition == null)
                return;

            var action = m_Actions.Find(x => x.From.Equals(State) && x.To.Equals(newServerState));
            if(action == null || action.Predicate())
            {
                State = newServerState;
            }
        }

        public void FollowServerState()
        {
            State = RemoteState;
        }

        public void SetState(TEnum newState)
        {
            if (TrySetState(newState, out StateTransitionFailReason failReason))
                return;

            switch(failReason)
            {
                case StateTransitionFailReason.NoRule:
                    throw new StateMachineException($"No rule for transition {State} -> {newState}");
                case StateTransitionFailReason.RuleServerClientStateMatch:
                    throw new StateMachineException($"State and remote state do not match, {State} != {RemoteState}, match is required by rule");
                case StateTransitionFailReason.RuleServerClientStateMismatch:
                    throw new StateMachineException($"State and remote state match, {State} == {RemoteState}, mismatch is required by rule");
            }

        }

        public bool TrySetState(TEnum newState, out StateTransitionFailReason failReason)
        {
            var rule = m_Rules.Find(x => x.From.Equals(State) && x.To.Equals(newState));
            if(rule == null)
            {
                failReason = StateTransitionFailReason.NoRule;
                return false;
            }

            switch(rule.Matching)
            {
                case TransitionMatching.ClientAndServerCanMismatch:
                    break;
                case TransitionMatching.ClientAndServerMatch:
                    if(!RemoteState.Equals(State))
                    {
                        failReason = StateTransitionFailReason.RuleServerClientStateMismatch;
                        return false;
                    }
                    break;
                case TransitionMatching.ClientAndServerMismatch:
                    if (RemoteState.Equals(State))
                    {
                        failReason = StateTransitionFailReason.RuleServerClientStateMatch;
                        return false;
                    }
                    break;
            }

            State = newState;

            failReason = StateTransitionFailReason.Success;        
            return true;
        }

        public static void Sync(StateMachine<TEnum> server, StateMachine<TEnum> client)
        {
            if(!server.IsServer)
                throw new StateMachineException("Server state machine not server");
            
            if(!client.IsClient)
                throw new StateMachineException("Client state machine not server");

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);
        }
    }
}
