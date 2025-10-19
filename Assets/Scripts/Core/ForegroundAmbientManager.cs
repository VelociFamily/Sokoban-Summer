using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Manages foreground ambience effects (e.g., wind trails and falling leaves) for gameplay scenes only.
    /// The manager persists across scene loads and spawns configured effect prefabs while gameplay scenes are active.
    /// </summary>
    public class ForegroundAmbientManager : MonoBehaviour
    {
        [Header("Effect Prefabs")]
        [Tooltip("Looping wind effect instantiated once while gameplay scenes are active.")]
        [SerializeField] private GameObject windEffectPrefab;

        [Tooltip("Burst effect spawned periodically to simulate drifting leaves. Should be a self-cleaning particle effect.")]
        [SerializeField] private GameObject leavesBurstPrefab;

    [Header("Placement")]
        [SerializeField] private Vector3 windEffectOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 leavesSpawnPivot = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector2 leavesSpawnArea = new Vector2(14f, 6f);

        [Header("Timing (seconds)")]
    [SerializeField] private Vector2 leavesSpawnIntervalRange = new Vector2(1f, 2f);
        [SerializeField] private float leavesLifetime = 10f;

    [Header("Rendering Order")]
    [Tooltip("Sorting layer applied to all spawned foreground effects.")]
    [SerializeField] private string targetSortingLayer = "Foreground";

    [Tooltip("Sorting order assigned to all renderers in the foreground effects.")]
    [SerializeField] private int targetSortingOrder = 500;

        private GameObject windInstance;
        private Coroutine leavesRoutine;
        private bool isInitialized;
    private bool sortingLayerIsValid;

        /// <summary>
        /// Configure and activate the ambient manager. Subsequent calls update the configuration.
        /// </summary>
        public void Initialize(
            GameObject windPrefab,
            GameObject leavesPrefab,
            Vector3 windOffset,
            Vector3 leavesPivot,
            Vector2 spawnArea,
            Vector2 spawnIntervalRange,
            float burstLifetime = 10f,
            string sortingLayer = "Foreground",
            int sortingOrder = 500)
        {
            ApplyConfiguration(
                windPrefab,
                leavesPrefab,
                windOffset,
                leavesPivot,
                spawnArea,
                spawnIntervalRange,
                burstLifetime,
                sortingLayer,
                sortingOrder);

            if (isInitialized)
            {
                RefreshForScene(SceneManager.GetActiveScene());
                return;
            }

            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            SceneManager.sceneLoaded += HandleSceneLoaded;

            isInitialized = true;
            RefreshForScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Scene loads can be additive; rely on the currently active scene for context.
            RefreshForScene(SceneManager.GetActiveScene());
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            RefreshForScene(newScene);
        }

        private void RefreshForScene(Scene scene)
        {
            if (!isInitialized)
                return;

            if (!scene.IsValid() || !scene.isLoaded)
                return;

            if (SceneInfo.IsGameplayScene(scene))
            {
                EnableEffects();
            }
            else
            {
                DisableEffects();
            }
        }

        private void EnableEffects()
        {
            if (windEffectPrefab != null && windInstance == null)
            {
                windInstance = Instantiate(windEffectPrefab, transform);
                windInstance.transform.localPosition = windEffectOffset;
                windInstance.transform.localRotation = Quaternion.identity;
                windInstance.transform.localScale = Vector3.one;
                ApplySorting(windInstance);
            }

            if (leavesBurstPrefab != null && leavesRoutine == null)
            {
                leavesRoutine = StartCoroutine(SpawnLeavesLoop());
            }
        }

        private void DisableEffects()
        {
            if (windInstance != null)
            {
                Destroy(windInstance);
                windInstance = null;
            }

            if (leavesRoutine != null)
            {
                StopCoroutine(leavesRoutine);
                leavesRoutine = null;
            }

            // Clean up any residual leaf bursts.
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child == null)
                    continue;

                if (child.gameObject == windInstance)
                    continue;

                Destroy(child.gameObject);
            }
        }

        private IEnumerator SpawnLeavesLoop()
        {
            while (true)
            {
                SpawnLeafBurst();
                var delay = Random.Range(leavesSpawnIntervalRange.x, leavesSpawnIntervalRange.y);
                yield return new WaitForSeconds(Mathf.Max(0.1f, delay));
            }
        }

        private void SpawnLeafBurst()
        {
            if (leavesBurstPrefab == null)
                return;

            var position = leavesSpawnPivot;
            position.x += Random.Range(-leavesSpawnArea.x * 0.5f, leavesSpawnArea.x * 0.5f);
            position.y += Random.Range(-leavesSpawnArea.y * 0.5f, leavesSpawnArea.y * 0.5f);

            var burst = Instantiate(leavesBurstPrefab, transform);
            burst.transform.localPosition = position;
            burst.transform.localRotation = Quaternion.identity;
            burst.transform.localScale = Vector3.one;
            ApplySorting(burst);

            if (leavesLifetime > 0f)
            {
                Destroy(burst, leavesLifetime);
            }
        }

        private void ApplyConfiguration(
            GameObject windPrefab,
            GameObject leavesPrefab,
            Vector3 windOffset,
            Vector3 leavesPivot,
            Vector2 spawnArea,
            Vector2 spawnIntervalRange,
            float burstLifetime,
            string sortingLayer,
            int sortingOrder)
        {
            if (windPrefab != null)
            {
                windEffectPrefab = windPrefab;
            }

            if (leavesPrefab != null)
            {
                leavesBurstPrefab = leavesPrefab;
            }

            windEffectOffset = windOffset;
            leavesSpawnPivot = leavesPivot;
            leavesSpawnArea = new Vector2(Mathf.Max(0f, spawnArea.x), Mathf.Max(0f, spawnArea.y));

            var normalizedInterval = spawnIntervalRange;
            if (normalizedInterval.x > normalizedInterval.y)
            {
                (normalizedInterval.x, normalizedInterval.y) = (normalizedInterval.y, normalizedInterval.x);
            }

            normalizedInterval.x = Mathf.Max(0.5f, normalizedInterval.x);
            normalizedInterval.y = Mathf.Max(normalizedInterval.x, normalizedInterval.y);
            leavesSpawnIntervalRange = normalizedInterval;
            leavesLifetime = Mathf.Max(0f, burstLifetime);

            targetSortingLayer = sortingLayer;
            targetSortingOrder = sortingOrder;
            sortingLayerIsValid = CheckSortingLayerValidity(targetSortingLayer);
        }

        private void ApplySorting(GameObject root)
        {
            if (root == null)
                return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (sortingLayerIsValid && !string.IsNullOrWhiteSpace(targetSortingLayer))
                {
                    renderer.sortingLayerName = targetSortingLayer;
                }

                renderer.sortingOrder = Mathf.Max(targetSortingOrder, renderer.sortingOrder);
            }
        }

        private static bool CheckSortingLayerValidity(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return false;

            foreach (var layer in SortingLayer.layers)
            {
                if (layer.name == layerName)
                    return true;
            }

            return false;
        }
    }
}
