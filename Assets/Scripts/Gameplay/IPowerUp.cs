using UnityEngine;

/// <summary>
/// Interface defining the contract for power-up implementations
/// </summary>
public interface IPowerUp
{
    /// <summary>
    /// Gets the current count/uses remaining for this power-up
    /// </summary>
    int RemainingUses { get; }
    
    /// <summary>
    /// Gets whether this power-up is currently active
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Gets the name of this power-up for logging purposes
    /// </summary>
    string PowerUpName { get; }
    
    /// <summary>
    /// Activates the power-up with the specified number of uses
    /// </summary>
    /// <param name="uses">Number of uses to grant</param>
    void Activate(int uses);
    
    /// <summary>
    /// Consumes one use of the power-up
    /// </summary>
    /// <returns>True if there are still uses remaining after consuming</returns>
    bool ConsumeUse();
    
    /// <summary>
    /// Starts the visual effect for this power-up
    /// </summary>
    /// <param name="playerController">The player controller to start effects on</param>
    void StartEffect(PlayerController playerController);
    
    /// <summary>
    /// Stops the visual effect for this power-up
    /// </summary>
    /// <param name="playerController">The player controller to stop effects on</param>
    void StopEffect(PlayerController playerController);
    
    /// <summary>
    /// Applies the power-up's game logic effect (e.g., direction confusion, speed boost)
    /// </summary>
    /// <param name="inputDirection">The original input direction</param>
    /// <param name="playerController">The player controller to apply effects to</param>
    /// <returns>The modified input direction (if applicable)</returns>
    Vector2 ApplyEffect(Vector2 inputDirection, PlayerController playerController);
}