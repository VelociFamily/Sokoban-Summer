using UnityEngine;

namespace Core
{
    /// <summary>
    /// Scriptable object to store level metadata and configuration
    /// This can be attached to level scenes or stored as assets to define level properties
    /// </summary>
    [CreateAssetMenu(fileName = "New Level Data", menuName = "Sokoban/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Information")]
        [Tooltip("Display name for the level")]
        public string levelTitle = "New Level";
        
        [Tooltip("Brief description of the level")]
        [TextArea(3, 5)]
        public string description = "";
        
        [Tooltip("Order priority for level selection (lower numbers appear first)")]
        public int sortOrder = 0;
        
        [Header("Level Goals")]
        [Tooltip("Target number of moves to complete the level (0 = no goal)")]
        public int parMoves = 0;
        
        [Tooltip("Target time to complete the level in seconds (0 = no goal)")]
        public float parTime = 0f;
        
        [Header("Visual")]
        [Tooltip("Preview image to show in level selection")]
        public Sprite previewImage;
        
        [Header("Progression")]
        [Tooltip("Whether this level must be unlocked by completing previous levels")]
        public bool requiresUnlock = true;
        
        [Tooltip("Scene path or name for this level")]
        public string scenePath = "";
        
        /// <summary>
        /// Get a formatted time string for the par time
        /// </summary>
        public string GetFormattedParTime()
        {
            if (parTime <= 0) return "No Time Goal";
            
            var minutes = Mathf.FloorToInt(parTime / 60f);
            var seconds = Mathf.FloorToInt(parTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Get a formatted moves string for the par moves
        /// </summary>
        public string GetFormattedParMoves()
        {
            if (parMoves <= 0) return "No Move Goal";
            return $"{parMoves} moves";
        }
    }
}