using UnityEngine;

/// <summary>
/// MonoBehaviour for teleport power-up collection
/// </summary>
public class TeleportPowerUp : PowerUpBase
{
    private static readonly TeleportationPowerUp _teleportImplementation = new TeleportationPowerUp();
    
    protected override IPowerUp PowerUpImplementation => _teleportImplementation;
    
    // For backward compatibility, keep the teleportItem field
    [Header("Teleport Specific")]
    [Tooltip("Specific GameObject to disable for teleport (overrides base itemToDisable)")]
    public GameObject teleportItem;
    
    private void Awake()
    {
        // Set default uses for teleport power-up
        if (usesGranted == 1) // Only set if not changed in inspector
            usesGranted = 3;
    }
    
    protected override void CollectPowerUp(Collider2D playerCollider)
    {
        // Use teleportItem if specified, otherwise use base logic
        if (teleportItem != null)
        {
            var originalItemToDisable = itemToDisable;
            itemToDisable = teleportItem;
            base.CollectPowerUp(playerCollider);
            itemToDisable = originalItemToDisable;
        }
        else
        {
            base.CollectPowerUp(playerCollider);
        }
    }
    
    // Static accessors for backward compatibility with existing code
    public static int teleportTimes
    {
        get => _teleportImplementation.RemainingUses;
        set => TeleportationPowerUp.TeleportTimes = value;
    }
}
