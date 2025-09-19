using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraBinder : MonoBehaviour
{
    void Start()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main; // Finds the active camera in the scene
        }
    }
}