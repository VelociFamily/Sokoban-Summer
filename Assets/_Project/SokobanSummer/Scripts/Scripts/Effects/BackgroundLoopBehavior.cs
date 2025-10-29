using UnityEngine;
using UnityEngine.UI;

namespace Effects
{
    public class BackgroundLoopBehavior : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private float _speedX, _speedY;

        private void Update()
        {
            _image.uvRect = new Rect(_image.uvRect.position + new Vector2(_speedX, _speedY) * Time.deltaTime,
                _image.uvRect.size);
        }
    }
}
