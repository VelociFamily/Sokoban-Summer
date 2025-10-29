namespace Gameplay
{
    /// <summary>
    /// MonoBehaviour for confusion power-up collection
    /// </summary>
    public class ConfusePowerDown : PowerUpBase
    {
        private static readonly ConfusionPowerUp _confusionImplementation = new ConfusionPowerUp();
    
        protected override IPowerUp PowerUpImplementation => _confusionImplementation;
    
        private void Awake()
        {
            // Set default uses for confusion power-up
            if (usesGranted == 1) // Only set if not changed in inspector
                usesGranted = 5;
        }
    
        // Static accessors for backward compatibility with existing code
        public static int confuseTurns
        {
            get => _confusionImplementation.RemainingUses;
            set => ConfusionPowerUp.ConfuseTurns = value;
        }
    }
}
