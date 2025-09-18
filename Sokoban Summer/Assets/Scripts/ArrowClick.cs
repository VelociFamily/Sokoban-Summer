using UnityEngine;

public class ArrowClick : MonoBehaviour
{
    public HatSelectionManager manager; // Drag your HatSelectionManager here
    public bool isRightArrow;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // This object was clicked!
                if (isRightArrow)
                {
                    manager.NextHat();
                }
                else
                {
                    manager.PreviousHat();
                }
            }
        }
    }
}
