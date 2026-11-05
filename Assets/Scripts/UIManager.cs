using TMPro;
using UnityEngine;

/// <summary>Only displays things. All game logic lives in GameManager.</summary>
public class UIManager : MonoBehaviour
{
    public TMP_Text instructionText;  // Emy's line, big, top center
    public TMP_Text hintText;         // small help line under it
    public TMP_Text progressText;     // "Step 2 / 4" or key checklist
    public TMP_Text statsText;        // score, streak, lives (corner)
    public TMP_Text centerText;       // menu and results screens

    public void SetInstruction(string s) { instructionText.text = s; }
    public void SetHint(string s) { hintText.text = s; }
    public void SetProgress(string s) { progressText.text = s; }

    public void SetKeysProgress(int pressed, int total)
    {
        progressText.text = pressed < 0 ? "" : $"Keys tried: {pressed} / {total}";
    }

    public void UpdateStats(int score, int streak, int lives)
    {
        statsText.text = $"Score: {score}\nStreak: {streak}\nLives: {lives}\n[R] Retry";
    }

    public void ShowCenter(string s) { centerText.text = s; }

    public void ClearGameplayTexts()
    {
        instructionText.text = "";
        hintText.text = "";
        progressText.text = "";
    }

    public void SetInstructionColor(Color c) { instructionText.color = c; }
}
