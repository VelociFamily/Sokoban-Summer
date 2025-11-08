namespace Core
{
    /// <summary>
    /// Defines the types of scenes in the game for better organization than build index numbers
    /// </summary>
    public enum SceneType
    {
        /// <summary>
        /// Initial game scene with GameInitializer
        /// </summary>
        Game = 0,
    
        /// <summary>
        /// Main menu scene
        /// </summary>
        MainMenu = 1,
    
        /// <summary>
        /// Tutorial level scenes
        /// </summary>
        TutorialLevel = 2,
    
        /// <summary>
        /// Regular gameplay level scenes
        /// </summary>
        GameplayLevel = 3
    }
}
