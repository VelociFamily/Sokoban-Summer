using UnityEngine;
using TMPro;

public class BestScoresDisplay : MonoBehaviour
{
    private TextMeshProUGUI textBox;

    private void Start()
    {
        textBox = GetComponent<TextMeshProUGUI>();

        if (LevelLogger.Instance == null)
        {
            textBox.text = "No scores to display.";
            return;
        }

        textBox.text = GetFormattedScores();
    }

    private string GetFormattedScores()
    {
        var logger = LevelLogger.Instance;
        var results = logger.GetAllResults(); // We need to add this method to LevelLogger

        if (results.Count == 0)
            return "No levels completed yet.";

        var output = "<b>Best Scores:</b>\n";

        foreach (var kvp in results)
        {
            var sceneIndex = kvp.Key;
            var result = kvp.Value;
            var levelName = logger.GetLevelName(sceneIndex);
            var time = logger.FormatTime(result.bestTime);
            output += $"{levelName} - Moves: {result.bestMoves}, Time: {time}\n";
        }

        return output;
    }
}
