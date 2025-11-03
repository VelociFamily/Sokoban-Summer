using UnityEngine;

namespace Core
{
    /// <summary>
    /// Ensures the active Cameras have the appropriate "Additional Camera Data" component
    /// for the current render pipeline (URP/HDRP). This prevents runtime warnings from
    /// packages that expect the component to exist. No-op for the Built-in pipeline.
    /// </summary>
    internal static class EnsureAdditionalCameraData
    {
        // Run after each scene load to cover additive loads as well.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureForAllCameras()
        {
#if UNITY_RENDER_PIPELINE_UNIVERSAL
            foreach (var cam in Camera.allCameras)
            {
                if (cam == null) continue;
                var data = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (data == null)
                {
                    cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
#if UNITY_EDITOR
                    Debug.Log($"[EnsureAdditionalCameraData] Added URP 'Universal Additional Camera Data' to camera: {cam.name}");
#endif
                }
            }
#elif UNITY_RENDER_PIPELINE_HDRP
            foreach (var cam in Camera.allCameras)
            {
                if (cam == null) continue;
                var data = cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                if (data == null)
                {
                    cam.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#if UNITY_EDITOR
                    Debug.Log($"[EnsureAdditionalCameraData] Added HDRP 'HD Additional Camera Data' to camera: {cam.name}");
#endif
                }
            }
#else
            // Built-in render pipeline: nothing to do.
#endif
        }
    }
}
