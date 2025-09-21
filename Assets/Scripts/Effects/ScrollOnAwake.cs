using UnityEngine;

namespace Effects
{
    public class ScrollOnAwake : MonoBehaviour
    {
        public float speed = 50f; // Units per second

        void Update()
        {
            // Move upward each frame
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
    }
}
