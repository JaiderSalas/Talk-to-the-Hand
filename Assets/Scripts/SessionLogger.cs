using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Writes two CSV files in Application.persistentDataPath:
///  - talk_to_hand_steps.csv    one row per key press that answers Emy
///  - talk_to_hand_sessions.csv one row per finished game
/// "session" is a counter saved in PlayerPrefs, +1 each time a game starts.
/// Write down which tester played which session number.
/// </summary>
public class SessionLogger : MonoBehaviour
{
    public string gameVersion = "1.0";

    string stepsPath;
    string sessionsPath;
    int session;

    void Awake()
    {
        stepsPath = Path.Combine(Application.persistentDataPath, "talk_to_hand_steps.csv");
        sessionsPath = Path.Combine(Application.persistentDataPath, "talk_to_hand_sessions.csv");
        EnsureHeader(stepsPath, "version,session,stage,sequence,step,expected,pressed,correct,reaction_seconds,lives_before");
        EnsureHeader(sessionsPath, "version,session,completed,score,best_streak,errors,lives_left,total_seconds");
        Debug.Log("Logs are saved in: " + Application.persistentDataPath);
    }

    public void NewSession()
    {
        session = PlayerPrefs.GetInt("th_session", 0) + 1;
        PlayerPrefs.SetInt("th_session", session);
        PlayerPrefs.Save();
    }

    public void LogStep(string stage, int sequence, int step, Move expected, Move pressed,
                        bool correct, float reaction, int livesBefore)
    {
        Append(stepsPath, string.Join(",",
            gameVersion, session, stage, sequence, step,
            expected, pressed, correct ? 1 : 0, F(reaction), livesBefore));
    }

    public void LogSummary(bool completed, int score, int bestStreak, int errors, int livesLeft, float totalSeconds)
    {
        Append(sessionsPath, string.Join(",",
            gameVersion, session, completed ? 1 : 0, score, bestStreak, errors, livesLeft, F(totalSeconds)));
    }

    static string F(float v)
    {
        return v.ToString("0.00", CultureInfo.InvariantCulture);
    }

    static void EnsureHeader(string path, string header)
    {
        if (!File.Exists(path)) File.WriteAllText(path, header + Environment.NewLine);
    }

    static void Append(string path, string line)
    {
        try { File.AppendAllText(path, line + Environment.NewLine); }
        catch (Exception e) { Debug.LogWarning("Could not write log: " + e.Message); }
    }

    [ContextMenu("Reset session counter")]
    void ResetCounter()
    {
        PlayerPrefs.DeleteKey("th_session");
    }
}
