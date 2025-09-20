using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all power-up systems and their interactions with the player
/// </summary>
public class PowerUpManager
{
    private readonly List<IPowerUp> _activePowerUps;
    private readonly PlayerController _playerController;
    
    // Power-up implementations
    private static readonly ConfusionPowerUp _confusionPowerUp = new ConfusionPowerUp();
    private static readonly TeleportationPowerUp _teleportationPowerUp = new TeleportationPowerUp();
    
    public PowerUpManager(PlayerController playerController)
    {
        _playerController = playerController;
        _activePowerUps = new List<IPowerUp>
        {
            _confusionPowerUp,
            _teleportationPowerUp
        };
    }
    
    /// <summary>
    /// Initializes power-up effects and animations when a level starts (for level transitions)
    /// </summary>
    public void InitializeLevelStart()
    {
        foreach (var powerUp in _activePowerUps)
        {
            if (powerUp.IsActive)
            {
                powerUp.StartEffect(_playerController);
                Debug.Log($"[PowerUpManager]: {powerUp.PowerUpName} animation started on level start - {powerUp.RemainingUses} uses remaining");
            }
        }
        
        // Apply persistent effects that should be active immediately when level starts
        if (_teleportationPowerUp.IsActive)
        {
            _playerController.moveSpeed = _playerController.teleportSpeed;
            Debug.Log($"[PowerUpManager]: Teleport speed boost applied on level start");
        }
        else
        {
            _playerController.moveSpeed = _playerController.normalMoveSpeed;
        }
    }
    
    /// <summary>
    /// Processes power-ups during player movement
    /// </summary>
    /// <param name="inputDirection">Original input direction</param>
    /// <returns>Modified input direction after applying power-up effects</returns>
    public Vector2 ProcessMovement(Vector2 inputDirection)
    {
        var modifiedDirection = inputDirection;
        
        foreach (var powerUp in _activePowerUps)
        {
            if (powerUp.IsActive)
            {
                // Start animation if not already playing
                powerUp.StartEffect(_playerController);
                
                // Apply power-up effect
                modifiedDirection = powerUp.ApplyEffect(modifiedDirection, _playerController);
            }
            else
            {
                // Stop animation if power-up is not active
                powerUp.StopEffect(_playerController);
            }
        }
        
        return modifiedDirection;
    }
    
    /// <summary>
    /// Processes power-up consumption after a successful move
    /// </summary>
    public void ProcessPostMove()
    {
        foreach (var powerUp in _activePowerUps)
        {
            if (powerUp.IsActive)
            {
                // Play special effects for teleport before consuming
                if (powerUp is TeleportationPowerUp teleportPowerUp)
                {
                    teleportPowerUp.PlayTeleportSound(_playerController);
                }
                
                bool stillActive = powerUp.ConsumeUse();
                
                if (!stillActive)
                {
                    // Power-up just expired, stop its effects immediately
                    powerUp.StopEffect(_playerController);
                    Debug.Log($"[PowerUpManager]: {powerUp.PowerUpName} expired");
                }
            }
        }
        
        // Reset move speed to normal if no teleport is active
        if (!_teleportationPowerUp.IsActive)
        {
            _playerController.moveSpeed = _playerController.normalMoveSpeed;
        }
    }
    
    /// <summary>
    /// Gets whether any power-up that affects movement direction is active
    /// </summary>
    public bool IsDirectionModificationActive => _confusionPowerUp.IsActive;
    
    /// <summary>
    /// Gets whether any power-up that affects movement speed is active
    /// </summary>
    public bool IsSpeedModificationActive => _teleportationPowerUp.IsActive;
}