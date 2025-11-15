namespace Gameplay
{
    /// <summary>
    /// Enumeration of all possible player states.
    /// Used for state machine transitions and state identification.
    /// </summary>
    public enum PlayerStateType
    {
        /// <summary>
        /// Player is idle, waiting for input.
        /// </summary>
        Idle,

        /// <summary>
        /// Player is actively moving in a direction.
        /// </summary>
        Move,

        /// <summary>
        /// Player is pushing a crate or pushable object.
        /// </summary>
        Pushing,

        /// <summary>
        /// Player is teleporting between portals.
        /// </summary>
        Teleporting
    }
}
