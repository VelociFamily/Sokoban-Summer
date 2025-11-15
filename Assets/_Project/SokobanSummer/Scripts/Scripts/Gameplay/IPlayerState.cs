namespace Gameplay
{
    /// <summary>
    /// Interface for player behavior states in the State Pattern.
    /// Each state encapsulates specific player behavior (Idle, Move, Pushing, Teleporting).
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// Called once when entering this state.
        /// Use for initialization, animation triggers, or one-time setup.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called once when exiting this state.
        /// Use for cleanup, stopping animations, or resetting state-specific data.
        /// </summary>
        void Exit();

        /// <summary>
        /// Called every frame to handle input for this state.
        /// Input processing may trigger state transitions.
        /// </summary>
        void HandleInput();

        /// <summary>
        /// Called every frame to update state-specific logic.
        /// Use for movement, animations, or time-based state transitions.
        /// </summary>
        void Tick();
    }
}
