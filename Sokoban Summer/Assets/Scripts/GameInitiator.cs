using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitiator : MonoBehaviour
{
    public GameObject Background;
    private GameObject backgroundClone;
    public SfxVolumeControl SFXVolumeControl;
    public VolumeControl VolumeControl;
    public AudioSource AudioSource;
    public LevelLogger LevelLogger;

    private AchievementManager achievementManager;

    private async void Start()
    {
        try
        {
            backgroundClone = Instantiate(Background);
            if (!backgroundClone.CompareTag("background"))
                backgroundClone.tag = "background";
            Instantiate(VolumeControl);
            Instantiate(AudioSource);
            Instantiate(LevelLogger);
            Instantiate(SFXVolumeControl);
            achievementManager = AchievementManager.Instance;
            await SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    private void Update()
    {
        if (backgroundClone == null)
            return;
        for (var index = 0; index < SceneManager.sceneCount; index++)
            switch (SceneManager.GetSceneAt(index).buildIndex)
            {
                case >= 2 and <= 7:
                    backgroundClone.SetActive(false);
                    break;
                case 1:
                    backgroundClone.SetActive(true);
                    break;
            }
    }
}
