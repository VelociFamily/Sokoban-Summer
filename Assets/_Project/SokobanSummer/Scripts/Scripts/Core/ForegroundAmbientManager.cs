using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private List<GameObject> leavesBurstPrefabs = new();

    [Header("Placement")]
        [SerializeField] private Vector3 windEffectOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 leavesSpawnPivot = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector2 leavesSpawnArea = new Vector2(14f, 6f);

        [Header("Timing (seconds)")]
            [SerializeField] private Vector2 leavesSpawnIntervalRange = new Vector2(1f, 2f);
            [SerializeField] private float leavesLifetime = 10f;
        [Tooltip("Seconds between wind effect re-spawns (min/max).")]
        [SerializeField] private Vector2 windRepositionIntervalRange = new Vector2(0.5f, 2f);
            [Tooltip("Offset from the camera near clip plane used when positioning the wind effect.")]
            [SerializeField] private float windCameraDepthOffset = 0.5f;
            [Tooltip("Minimum distance (world units) the wind effect should move between reposition attempts.")]
            [SerializeField] private float windMinRepositionDistance = 3f;
            [Tooltip("Viewport padding (0-0.5) to keep the wind effect away from the very edge of the screen.")]
            [SerializeField] [Range(0f, 0.49f)] private float windViewportPadding = 0.1f;

    [Header("Rendering Order")]
    [Tooltip("Sorting layer applied to all spawned foreground effects.")]
    [SerializeField] private string targetSortingLayer = "Foreground";

    [Tooltip("Sorting order assigned to all renderers in the foreground effects.")]
    [SerializeField] private int targetSortingOrder = 500;

        private Coroutine leavesRoutine;
        private Coroutine windSpawnLoopRoutine;
        private GameObject activeWindEffect;
        private Vector3 lastWindPosition;
        private bool hasWindPosition;
        private bool isInitialized;
        private bool sortingLayerIsValid;
        private float cachedWindClipDuration = -1f;

        /// <summary>
        /// Configure and activate the ambient manager. Subsequent calls update the configuration.
        /// </summary>
        public void Initialize(
            GameObject windPrefab,
            IReadOnlyList<GameObject> leavesPrefabs,
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
                leavesPrefabs,
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

            // Check if ANY loaded scene is a gameplay scene
            bool isGameplayActive = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.isLoaded && SceneInfo.IsGameplayScene(loadedScene))
                {
                    isGameplayActive = true;
                    break;
                }
            }

            if (isGameplayActive)
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
            if (windEffectPrefab != null && windSpawnLoopRoutine == null)
            {
                windSpawnLoopRoutine = StartCoroutine(HandleWindLoop());
            }

            if (leavesBurstPrefabs.Count > 0 && leavesRoutine == null)
            {
                leavesRoutine = StartCoroutine(SpawnLeavesLoop());
            }
        }

        private void DisableEffects()
        {
            if (windSpawnLoopRoutine != null)
            {
                StopCoroutine(windSpawnLoopRoutine);
                windSpawnLoopRoutine = null;
            }

            if (activeWindEffect != null)
            {
                Destroy(activeWindEffect);
                activeWindEffect = null;
            }

            if (leavesRoutine != null)
            {
                StopCoroutine(leavesRoutine);
                leavesRoutine = null;
            }

            hasWindPosition = false;
            cachedWindClipDuration = -1f;

            // Clean up any residual leaf bursts or spawned VFX.
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child == null)
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
                yield return new WaitForSeconds(Mathf.Max(0.05f, delay));
            }
        }

        private void SpawnLeafBurst()
        {
            if (leavesBurstPrefabs.Count == 0)
                return;

            var sourcePrefab = leavesBurstPrefabs[Random.Range(0, leavesBurstPrefabs.Count)];
            if (sourcePrefab == null)
                return;

            var position = leavesSpawnPivot;
            position.x += Random.Range(-leavesSpawnArea.x * 0.5f, leavesSpawnArea.x * 0.5f);
            position.y += Random.Range(-leavesSpawnArea.y * 0.5f, leavesSpawnArea.y * 0.5f);

            var burst = Instantiate(sourcePrefab, transform);
            burst.transform.localPosition = position;
            var randomZ = Random.Range(-20f, 20f);
            burst.transform.localRotation = Quaternion.Euler(0f, 0f, randomZ);
            var baseScale = Random.Range(0.8f, 1.25f);
            burst.transform.localScale = new Vector3(baseScale, baseScale, 1f);
            ApplySorting(burst);

            var animator = burst.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = Random.Range(1.05f, 1.35f);
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(stateInfo.fullPathHash, 0, Random.value);
            }

            var spriteRenderer = burst.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = Random.Range(0.75f, 1f);
                spriteRenderer.color = color;
            }

            if (leavesLifetime > 0f)
            {
                Destroy(burst, leavesLifetime);
            }
        }

        private void ApplyConfiguration(
            GameObject windPrefab,
            IReadOnlyList<GameObject> leavesPrefabs,
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
                cachedWindClipDuration = -1f;
            }

            if (leavesPrefabs != null)
            {
                leavesBurstPrefabs.Clear();
                foreach (var prefab in leavesPrefabs)
                {
                    if (prefab == null)
                        continue;

                    if (!leavesBurstPrefabs.Contains(prefab))
                    {
                        leavesBurstPrefabs.Add(prefab);
                    }
                }
            }

            windEffectOffset = windOffset;
            leavesSpawnPivot = leavesPivot;
            leavesSpawnArea = new Vector2(Mathf.Max(0f, spawnArea.x), Mathf.Max(0f, spawnArea.y));

            var normalizedInterval = spawnIntervalRange;
            if (normalizedInterval.x > normalizedInterval.y)
            {
                (normalizedInterval.x, normalizedInterval.y) = (normalizedInterval.y, normalizedInterval.x);
            }

            const float minLeafInterval = 0.2f;
            normalizedInterval.x = Mathf.Max(minLeafInterval, normalizedInterval.x);
            normalizedInterval.y = Mathf.Max(normalizedInterval.x, normalizedInterval.y);
            leavesSpawnIntervalRange = normalizedInterval;
            leavesLifetime = Mathf.Max(0f, burstLifetime);

            var windInterval = windRepositionIntervalRange;
            if (windInterval.x > windInterval.y)
            {
                (windInterval.x, windInterval.y) = (windInterval.y, windInterval.x);
            }

            windInterval.x = Mathf.Max(0.5f, windInterval.x);
            windInterval.y = Mathf.Max(windInterval.x, windInterval.y);
            windRepositionIntervalRange = windInterval;

            windCameraDepthOffset = Mathf.Max(0f, windCameraDepthOffset);
            windMinRepositionDistance = Mathf.Max(0f, windMinRepositionDistance);

            targetSortingLayer = sortingLayer;
            targetSortingOrder = sortingOrder;
            sortingLayerIsValid = CheckSortingLayerValidity(targetSortingLayer);
        }

        private IEnumerator HandleWindLoop()
        {
            hasWindPosition = false;

            while (isActiveAndEnabled)
            {
                if (windEffectPrefab == null)
                {
                    yield return null;
                    continue;
                }

                var camera = Camera.main ?? FindFirstObjectByType<Camera>();
                if (camera == null)
                {
                    yield return null;
                    continue;
                }

                var spawnPosition = PickWindWorldPosition(camera);
                SpawnWindEffect(spawnPosition);

                var playbackDuration = GetWindClipDuration();
                var animator = activeWindEffect != null ? activeWindEffect.GetComponent<Animator>() : null;

                if (animator != null)
                {
                    animator.speed = 1f;
                    var elapsed = 0f;
                    while (elapsed < playbackDuration && activeWindEffect != null)
                    {
                        yield return null;
                        elapsed += Time.deltaTime;
                    }

                    if (animator != null)
                    {
                        animator.speed = 0f;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(Mathf.Max(0.1f, playbackDuration));
                }

                if (activeWindEffect != null)
                {
                    Destroy(activeWindEffect);
                    activeWindEffect = null;
                }

                var extraDelay = Random.Range(windRepositionIntervalRange.x, windRepositionIntervalRange.y);
                if (extraDelay > 0.01f)
                {
                    yield return new WaitForSeconds(extraDelay);
                }
                else
                {
                    yield return null;
                }
            }

            if (activeWindEffect != null)
            {
                Destroy(activeWindEffect);
                activeWindEffect = null;
            }

            windSpawnLoopRoutine = null;
        }

        private void SpawnWindEffect(Vector3 position)
        {
            if (windEffectPrefab == null)
                return;

            if (activeWindEffect != null)
            {
                Destroy(activeWindEffect);
            }

            activeWindEffect = Instantiate(windEffectPrefab, position, Quaternion.identity, transform);
            ApplySorting(activeWindEffect);

            var animator = activeWindEffect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 1f;
                animator.Rebind();
                animator.Update(0f);
            }
        }

        private float GetWindClipDuration()
        {
            if (cachedWindClipDuration > 0f)
                return cachedWindClipDuration;

            float duration = 0f;

            if (windEffectPrefab != null)
            {
                var animator = windEffectPrefab.GetComponent<Animator>();
                if (animator != null)
                {
                    var controller = animator.runtimeAnimatorController;
                    if (controller != null)
                    {
                        foreach (var clip in controller.animationClips)
                        {
                            if (clip == null)
                                continue;

                            duration = Mathf.Max(duration, clip.length);
                            if (duration > 0f)
                                break;
                        }
                    }
                }
            }

            cachedWindClipDuration = duration > 0f ? duration : 1f;
            return cachedWindClipDuration;
        }

        private Vector3 PickWindWorldPosition(Camera camera)
        {
            var attempts = 0;
            var sqrMinDistance = windMinRepositionDistance * windMinRepositionDistance;

            while (attempts < 6)
            {
                var candidate = GenerateWindWorldPosition(camera);
                if (!hasWindPosition || (candidate - lastWindPosition).sqrMagnitude >= sqrMinDistance)
                {
                    lastWindPosition = candidate;
                    hasWindPosition = true;
                    return candidate;
                }

                attempts++;
            }

            lastWindPosition = GenerateWindWorldPosition(camera);
            hasWindPosition = true;
            return lastWindPosition;
        }

        private Vector3 GenerateWindWorldPosition(Camera camera)
        {
            var forward = camera.transform.forward.normalized;
            var right = camera.transform.right;
            var up = camera.transform.up;
            var baseDistance = camera.nearClipPlane + windCameraDepthOffset;
            var padding = Mathf.Clamp01(windViewportPadding);

            Vector3 worldPosition;

            if (camera.orthographic)
            {
                var halfHeight = camera.orthographicSize;
                var halfWidth = halfHeight * camera.aspect;

                var paddedWidth = Mathf.Max(0f, halfWidth * (1f - padding * 2f));
                var paddedHeight = Mathf.Max(0f, halfHeight * (1f - padding * 2f));

                var offsetX = Mathf.Approximately(paddedWidth, 0f) ? 0f : Random.Range(-paddedWidth, paddedWidth);
                var offsetY = Mathf.Approximately(paddedHeight, 0f) ? 0f : Random.Range(-paddedHeight, paddedHeight);

                var planeCenter = camera.transform.position + forward * baseDistance;
                worldPosition = planeCenter + right * offsetX + up * offsetY;
            }
            else
            {
                var min = padding;
                var max = 1f - padding;
                var viewport = new Vector3(Random.Range(min, max), Random.Range(min, max), baseDistance);
                worldPosition = camera.ViewportToWorldPoint(viewport);
            }

            worldPosition += new Vector3(windEffectOffset.x, windEffectOffset.y, 0f);
            worldPosition += forward * windEffectOffset.z;

            return worldPosition;
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
