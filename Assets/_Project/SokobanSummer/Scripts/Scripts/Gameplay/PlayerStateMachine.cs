using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Manages player state transitions and delegates behavior to active state.
    /// Embedded in PlayerController to centralize state logic without scattering references.
    /// </summary>
    public class PlayerStateMachine
    {
        private IPlayerState _currentState;
        private readonly Dictionary<PlayerStateType, IPlayerState> _states;
        private readonly PlayerController _controller;

        public PlayerStateType CurrentStateType { get; private set; }

        public PlayerStateMachine(PlayerController controller)
        {
            _controller = controller;
            _states = new Dictionary<PlayerStateType, IPlayerState>();
        }

        /// <summary>
        /// Register a state with the state machine.
        /// </summary>
        public void RegisterState(PlayerStateType type, IPlayerState state)
        {
            if (_states.ContainsKey(type))
            {
                Debug.LogWarning($"[PlayerStateMachine] State {type} already registered. Overwriting.");
            }
            _states[type] = state;
        }

        /// <summary>
        /// Change to a new state, calling Exit on the current state and Enter on the new state.
        /// </summary>
        public void ChangeState(PlayerStateType newStateType)
        {
            if (!_states.TryGetValue(newStateType, out var newState))
            {
                Debug.LogError($"[PlayerStateMachine] State {newStateType} not registered!");
                return;
            }

            if (_currentState == newState)
            {
                return; // Already in this state
            }

            _currentState?.Exit();
            _currentState = newState;
            CurrentStateType = newStateType;
            _currentState.Enter();
        }

        /// <summary>
        /// Initialize the state machine with a starting state.
        /// </summary>
        public void Initialize(PlayerStateType startingState)
        {
            if (!_states.ContainsKey(startingState))
            {
                Debug.LogError($"[PlayerStateMachine] Cannot initialize with unregistered state {startingState}!");
                return;
            }

            CurrentStateType = startingState;
            _currentState = _states[startingState];
            _currentState.Enter();
        }

        /// <summary>
        /// Update the current state's input handling.
        /// </summary>
        public void HandleInput()
        {
            _currentState?.HandleInput();
        }

        /// <summary>
        /// Update the current state's logic.
        /// </summary>
        public void Tick()
        {
            _currentState?.Tick();
        }

        /// <summary>
        /// Get a specific state by type.
        /// Used to access state-specific methods (e.g., TeleportingState.StartTeleport).
        /// </summary>
        public IPlayerState GetState(PlayerStateType type)
        {
            _states.TryGetValue(type, out var state);
            return state;
        }
    }
}
