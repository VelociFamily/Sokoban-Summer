using UnityEngine;

/// <summary>
/// Implementation of the teleport power-up that provides speed boost and special effects
/// </summary>
public class TeleportationPowerUp : IPowerUp
{
    private static int _teleportTimes;
    
    public int RemainingUses => _teleportTimes;
    public bool IsActive => _teleportTimes > 0;
    public string PowerUpName => "TeleportationPowerUp";
    
    // Static accessor for backward compatibility
    public static int TeleportTimes
    {
        get => _teleportTimes;
        set => _teleportTimes = value;
    }
    
    public void Activate(int uses)
    {
        _teleportTimes = uses;
    }
    
    public bool ConsumeUse()
    {
        if (_teleportTimes > 0)
        {
            _teleportTimes--;
            return _teleportTimes > 0;
        }
        return false;
    }
    
    public void StartEffect(PlayerController playerController)
    {
        if (playerController.teleportEffect != null && !playerController.teleportEffect.isPlaying)
        {
            playerController.teleportEffect.Play();
        }
    }
    
    public void StopEffect(PlayerController playerController)
    {
        if (playerController.teleportEffect != null && playerController.teleportEffect.isPlaying)
        {
            playerController.teleportEffect.Stop();
        }
    }
    
    public Vector2 ApplyEffect(Vector2 inputDirection, PlayerController playerController)
    {
        // Apply speed boost by setting the move speed
        playerController.moveSpeed = playerController.teleportSpeed;
        
        return inputDirection; // Direction is not modified by teleport
    }
    
    /// <summary>
    /// Plays the teleport sound effect
    /// </summary>
    /// <param name="playerController">The player controller to play sound on</param>
    public void PlayTeleportSound(PlayerController playerController)
    {
        if (playerController.audioSource != null && playerController.teleportSound != null)
        {
            playerController.audioSource.PlayOneShot(playerController.teleportSound);
        }
        else if (playerController.teleportSound == null)
        {
            Debug.LogWarning("[TeleportationPowerUp]: Teleport sound not assigned - cannot play audio feedback");
        }
    }
}