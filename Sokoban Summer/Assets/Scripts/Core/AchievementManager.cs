using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    // Public properties to access the state without allowing direct modification
    [Header("Achievement States")]
    public bool ConfuseAndSpeed => _confuseAndSpeed;
    public bool CompleteTutorial => _completeTutorial;
    public bool CompleteLevelTwo => _completeLevelTwo;

    // Flags to prevent premature unlocking
    private bool _confuseAndSpeed = false;
    private bool _completeTutorial = false;
    private bool _completeLevelTwo = false;


    [Header("Selected Hat")]
    public string selectedHatName = ""; // stores chosen hat name

    private TextMeshProUGUI achievementText;

    private Coroutine hideAchievementCoroutine;
    private float displayDuration = 5f;

    private bool confuseAndSpeedUnlocked;

    // Tracks the number of scenes loaded
    private int sceneLoadCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        FindAchievementText();
        UpdateAchievementDisplay(true);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (hideAchievementCoroutine != null)
        {
            StopCoroutine(hideAchievementCoroutine);
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        // Increment the counter each time a scene is loaded
        sceneLoadCount++;

        FindAchievementText();
        UpdateAchievementDisplay(true);

        // Check if the loaded scene is the tutorial completion scene (index 6)
        // AND it is the second scene to be loaded
        if (scene.buildIndex == 6)
        {
            UnlockTutorial();
            _completeTutorial = true;
        }
    }

    private void Update()
     {
        // Only check achievements if the active scene is a gameplay scene (not menu)
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex == 0) return; // Assuming buildIndex 0 is your menu
        if (activeScene.buildIndex == 6)
        {
            UnlockTutorial();
            _completeTutorial = true;
        }
        CheckPowerUpAchievement();
        UpdateAchievementDisplay();
    }

    // ======================
    // CHECKS
    // ======================

    void CheckPowerUpAchievement()
    {
        // Ensure achievement is only unlocked when conditions are met during gameplay
        if (!confuseAndSpeedUnlocked &&
            ConfusePowerDown.confuseTurns > 0 &&
            TeleportPowerUp.teleportTimes > 0)
        {
            confuseAndSpeedUnlocked = true; // Prevents re-unlocking
            UnlockConfuseAndSpeed();
        }
    }

    // ======================
    // UNLOCK METHODS (call these from triggers/events)
    // ======================

    public void UnlockConfuseAndSpeed()
    {
        _confuseAndSpeed = true;
        Debug.Log("AchievementManager: Confuse and Speed unlocked!");
    }

    public void UnlockTutorial()
    {
        _completeTutorial = true;
        Debug.Log("AchievementManager: Tutorial completed!");
    }

    public void UnlockLevelTwo()
    {
        _completeLevelTwo = true;
        Debug.Log("AchievementManager: Level Two completed!");
    }

    // ======================
    // HAT SYSTEM
    // ======================

    public void SetSelectedHat(string hatName)
    {
        selectedHatName = hatName;
        Debug.Log("AchievementManager: Hat selected -> " + hatName);
    }

    // ======================
    // UI DISPLAY
    // ======================

    private void FindAchievementText()
    {
        achievementText = null;
        GameObject achievementObj = GameObject.FindGameObjectWithTag("achievement");
        if (achievementObj != null)
        {
            achievementText = achievementObj.GetComponent<TextMeshProUGUI>();
        }
    }

    void UpdateAchievementDisplay(bool forceUpdate = false)
    {
        if (achievementText == null) return;

        // Check against the private fields
        if (forceUpdate ||
            _confuseAndSpeed != _confuseAndSpeed ||
            _completeTutorial != _completeTutorial ||
            _completeLevelTwo != _completeLevelTwo)
        {
            string display = "Achievements:\n";
            if (_confuseAndSpeed) display += "- Confuse and Speed Combo\n";
            if (_completeTutorial) display += "- Completed Tutorial\n";
            if (_completeLevelTwo) display += "- Completed Level Two\n";

            achievementText.text = display;

            if (hideAchievementCoroutine != null)
            {
                StopCoroutine(hideAchievementCoroutine);
            }
            hideAchievementCoroutine = StartCoroutine(HideAchievementAfterDelay(displayDuration));
        }
    }

    private IEnumerator HideAchievementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (achievementText != null)
        {
            achievementText.text = "";
        }
        hideAchievementCoroutine = null;
    }
}