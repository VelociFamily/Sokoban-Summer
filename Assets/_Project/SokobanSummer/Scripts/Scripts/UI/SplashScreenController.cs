using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    [DisallowMultipleComponent]
    public class SplashScreenController : MonoBehaviour
    {
        private const string DefaultTitle = "Title";

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private Image backgroundImage;

        private UIService _uiService;

        /// <summary>
        /// Sets the displayed title text. Falls back to a default when the provided value is null or whitespace.
        /// </summary>
        /// <param name="title">The text to display.</param>
        public void SetTitle(string title)
        {
            if (!EnsureComponents())
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                titleLabel.text = title;
            }
            else
            {
                titleLabel.text = DefaultTitle;
            }
        }

        /// <summary>
        /// Plays the splash sequence: hold the splash for a duration, then fade it out.
        /// The controller destroys its GameObject once the sequence finishes.
        /// </summary>
        /// <param name="holdSeconds">Seconds to keep the splash fully visible before fading.</param>
        /// <param name="fadeSeconds">Seconds the fade-out should take.</param>
        public async Task PlaySequenceAsync(float holdSeconds, float fadeSeconds)
        {
            if (!EnsureComponents())
            {
                Destroy(gameObject);
                return;
            }

            // Bind canvas to UIService camera if available
            BindCanvasToUIService();

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var remainingHoldTime = Mathf.Max(0f, holdSeconds);
            while (remainingHoldTime > 0f)
            {
                await Task.Yield();
                remainingHoldTime -= Time.unscaledDeltaTime;
            }

            var fadeDuration = Mathf.Max(0f, fadeSeconds);
            if (fadeDuration <= Mathf.Epsilon)
            {
                canvasGroup.alpha = 0f;
                Destroy(gameObject);
                return;
            }

            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                await Task.Yield();
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            canvasGroup.alpha = 0f;
            Destroy(gameObject);
        }

        private void Awake()
        {
            EnsureComponents();
        }

        private void Reset()
        {
            EnsureComponents();
        }

        private bool EnsureComponents()
        {
            if (targetCanvas == null)
            {
                targetCanvas = GetComponent<Canvas>();
            }

            if (targetCanvas == null)
            {
                targetCanvas = gameObject.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 999;
            }

            cameraResolved = AssignCameraIfAvailable();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                Debug.LogError("[SplashScreenController]: Unable to ensure CanvasGroup component.");
                return false;
            }

            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            if (backgroundImage == null)
            {
                backgroundImage = GetComponentInChildren<Image>();
                if (backgroundImage == null)
                {
                    var backgroundObject = new GameObject("Background", typeof(RectTransform));
                    backgroundObject.transform.SetParent(transform, false);
                    backgroundObject.transform.SetAsFirstSibling();

                    var rectTransform = backgroundObject.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;

                    backgroundImage = backgroundObject.AddComponent<Image>();
                    backgroundImage.color = Color.black;
                }
            }

            if (titleLabel == null)
            {
                titleLabel = GetComponentInChildren<TMP_Text>();
                if (titleLabel == null)
                {
                    var textObject = new GameObject("TitleText", typeof(RectTransform));
                    textObject.transform.SetParent(transform, false);

                    var rectTransform = textObject.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;

                    titleLabel = textObject.AddComponent<TextMeshProUGUI>();
                    titleLabel.alignment = TextAlignmentOptions.Center;
                    titleLabel.fontSize = 96f;
                    titleLabel.text = DefaultTitle;
                    titleLabel.color = Color.white;
                }
            }
            return true;
        }

        /// <summary>
        /// Binds the target canvas to the UIService primary camera.
        /// Replaces the old LateUpdate/AssignCameraIfAvailable pattern.
        /// </summary>
        private void BindCanvasToUIService()
        {
            if (targetCanvas == null)
            {
                return;
            }

            // Skip if canvas is overlay mode or already has a camera
            if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay || targetCanvas.worldCamera != null)
            {
                return;
            }

            // Get UIService and bind canvas
            if (ServiceLocator.TryGet(out _uiService) && _uiService.IsReady())
            {
                _uiService.BindCanvas(targetCanvas);
                Debug.Log($"[SplashScreenController]: Bound splash canvas to UIService primary camera");
            }
            else
            {
                Debug.LogWarning($"[SplashScreenController]: UIService not ready. Splash canvas may not render correctly.");
            }
        }
    }
}
