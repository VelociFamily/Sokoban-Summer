using Core;
using UnityEngine;

namespace UI
{
    public class CharacterHatLoader : MonoBehaviour
    {
        [Tooltip("The Hats folder inside this character prefab.")]
        public Transform hatsFolder;
        void Start()
        {
            if (AchievementManager.Instance == null || string.IsNullOrEmpty(AchievementManager.Instance.selectedHatName))
                return;

            var hatToActivate = AchievementManager.Instance.selectedHatName;

            // Disable all hats first
            foreach (Transform hat in hatsFolder)
            {
                hat.gameObject.SetActive(false);
            }

            // Find the hat with the matching name and activate it
            var selectedHat = hatsFolder.Find(hatToActivate);
            if (selectedHat != null)
            {
                selectedHat.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[CharacterHatLoader] No hat named '{hatToActivate}' found under {hatsFolder.name}");
            }
        }
    }
}
