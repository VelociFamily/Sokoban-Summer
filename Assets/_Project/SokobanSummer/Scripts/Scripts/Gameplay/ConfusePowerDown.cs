namespace Gameplay
{
    /// <summary>
    /// MonoBehaviour for confusion power-up collection
    /// </summary>
    public class ConfusePowerDown : PowerUpBase
    {
        protected override IPowerUp PowerUpImplementation => PowerUpManager.ConfusionPowerUp;
    
        private void Awake()
        {
            // Set default uses for confusion power-up
            if (usesGranted == 1) // Only set if not changed in inspector
                usesGranted = 5;
        }
    
        // Static accessors for backward compatibility with existing code
        public static int confuseTurns
        {
            get => PowerUpManager.ConfusionPowerUp.RemainingUses;
#pragma warning disable CS0618
            set => ConfusionPowerUp.ConfuseTurns = value;
#pragma warning restore CS0618
        }
    }
}
