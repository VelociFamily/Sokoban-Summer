using System.Threading.Tasks;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SceneButton : MonoBehaviour
    {
        [Tooltip("Build index of the scene this button will load")]
        public int sceneIndex;

        [Tooltip("Optional: Drag a lock overlay GameObject (e.g., lock icon or panel) here")]
        public GameObject lockOverlay;
        private Button button;

        // This will be true once Scene 5 has been loaded at least once
        public static bool hatUnlocked;

        /// <summary>
        /// Initialize the SceneButton - can be called by UI management systems
        /// </summary>
        public void Awake()
        {
            button = GetComponent<Button>();

            // Load saved unlock state from PlayerPrefs
            hatUnlocked = PlayerPrefs.GetInt("HasPlayedScene5", 0) == 1;

            UpdateLockState();

            SceneManager.sceneLoaded += OnSceneLoaded;
        
            Debug.Log($"[SceneButton]: Initialized for scene {sceneIndex}");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public async void LoadScene()
        {
            if (SceneSelector.CanLoadScene(sceneIndex))
            {
                // 1️⃣ Load the level additively first
                var loadOp = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
                while (!loadOp.isDone)
                    await Task.Yield();

                // 2️⃣ Set the new scene active
                var newScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
                if (newScene.IsValid())
                {
                    SceneManager.SetActiveScene(newScene);

                    // Recursively search for the GameObject with the tag "background"
                    foreach (var rootObject in newScene.GetRootGameObjects())
                    {
                        var backgroundObj = FindWithTagRecursive(rootObject, "background");
                        if (backgroundObj != null)
                        {
                            backgroundObj.SetActive(false);
                            break;
                        }
                    }
                }

                // 3️⃣ Unload "Main Menu" only if it's loaded and NOT the only loaded scene
                var mainMenuScene = SceneManager.GetSceneByName("Main Menu");
                if (mainMenuScene.IsValid() && mainMenuScene.isLoaded && SceneManager.sceneCount > 1)
                {
                    var unloadOp = SceneManager.UnloadSceneAsync("Main Menu");
                    while (!unloadOp.isDone)
                        await Task.Yield();
                }
            }
            else
            {
                Debug.Log("[SceneButton]: Level locked - complete previous level first to unlock");
            }
        }

        // Helper method to recursively search for a tag
        private GameObject FindWithTagRecursive(GameObject obj, string tag)
        {
            if (obj.CompareTag(tag))
                return obj;

            foreach (Transform child in obj.transform)
            {
                var result = FindWithTagRecursive(child.gameObject, tag);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void UpdateLockState()
        {
            var canLoad = SceneSelector.CanLoadScene(sceneIndex);

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!canLoad);
            }

            if (button != null)
            {
                button.interactable = canLoad;
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check if this is a special scene that unlocks the hat feature
            if (SceneInfo.GetSceneType(scene) == SceneType.GameplayLevel)
            {
                hatUnlocked = true;
                PlayerPrefs.SetInt("HasPlayedScene5", 1);
                PlayerPrefs.Save();
            }
        }
    }
}
