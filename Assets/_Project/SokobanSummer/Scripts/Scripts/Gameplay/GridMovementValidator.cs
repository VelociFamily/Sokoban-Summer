using Core;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Runtime validator for grid-step movement configuration.
    /// Logs warnings if expected layers or alignment are missing.
    /// Lightweight: safe to leave in production; does nothing expensive.
    /// </summary>
    public class GridMovementValidator : MonoBehaviour
    {
        [SerializeField] private bool validateOnStart = true;
        [SerializeField] private int sampleCrateChecks = 6; // limit logs

        private void Start()
        {
            if (!validateOnStart) return;
            RunValidation();
        }

        [ContextMenu("Run Grid Movement Validation")] public void RunValidation()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                Debug.LogWarning("[GridMovementValidator]: No PlayerController found.");
                return;
            }

            int pushableLayerIndex = LayerMask.NameToLayer("Pushable");
            if (pushableLayerIndex == -1)
            {
                Debug.LogWarning("[GridMovementValidator]: 'Pushable' layer not defined; crates will not push.");
            }
            else if ((player.gameObject.layer == pushableLayerIndex))
            {
                Debug.LogWarning("[GridMovementValidator]: Player is on Pushable layer; should usually be separate.");
            }

            // Check step duration
            var stepDurationField = typeof(PlayerController).GetField("stepDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stepDurationField != null)
            {
                float stepDuration = (float)stepDurationField.GetValue(player);
                if (stepDuration <= 0f)
                    Debug.LogWarning("[GridMovementValidator]: stepDuration <= 0; movement will snap instantly.");
                else if (stepDuration > 0.5f)
                    Debug.LogWarning("[GridMovementValidator]: stepDuration > 0.5s; movement may feel sluggish.");
            }

            // Sample a few pushable objects for grid alignment
            if (pushableLayerIndex != -1)
            {
                int mask = 1 << pushableLayerIndex;
                var pushables = FindObjectsOfType<Transform>();
                int inspected = 0;
                foreach (var t in pushables)
                {
                    if (t.gameObject.layer != pushableLayerIndex) continue;
                    Vector3 p = t.position;
                    if (!IsNearlyInteger(p.x) || !IsNearlyInteger(p.y))
                        Debug.LogWarning($"[GridMovementValidator]: Pushable '{t.name}' not grid-aligned: {p}");
                    inspected++;
                    if (inspected >= sampleCrateChecks) break;
                }
            }

            Debug.Log("[GridMovementValidator]: Validation complete.");
        }

        private bool IsNearlyInteger(float v) => Mathf.Abs(v - Mathf.Round(v)) < 0.01f;
    }
}
