using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runs the whole tutorial: Menu -> Stage 1 (keys) -> Stage 2 (mirror)
/// -> Stage 3 (sequences) -> Results.
/// Also validates the player's answers (using Move.Mirror) and manages
/// lives, streak and score. Emy is scripted: she is the "Observer".
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum State { Menu, Keys, Mirror, Sequences, Results }

    [Header("Scene references")]
    public InputReader input;
    public HandController playerLeft;
    public HandController playerRight;
    public HandController emyLeft;
    public HandController emyRight;
    public UIManager ui;
    public SessionLogger logger;

    [Header("Audio (optional)")]
    public AudioSource sfx;
    public AudioClip correctClip;
    public AudioClip wrongClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    [Header("Rules")]
    public int startingLives = 3;
    public int pointsPerStep = 10;
    public float pauseBetweenSteps = 0.6f;

    [Header("Colors")]
    public Color correctColor = new Color(0.2f, 0.8f, 0.3f);
    public Color wrongColor = new Color(0.9f, 0.2f, 0.2f);

    const int TotalKeys = 8;
    enum Result { Pending, Correct, Wrong }

    State state = State.Menu;
    Coroutine gameRoutine;

    int lives, score, streak, bestStreak, errors;
    bool dead;
    float sessionStart;

    // Step tracking
    bool awaiting;
    Result result;
    Move expected;
    Move lastPressed;
    float stepStartTime;
    string stageName = "";
    int sequenceIndex;
    int currentStep;

    readonly HashSet<string> pressedKeys = new HashSet<string>();

    static bool keysTutorialSeen;
    static bool mirrorTutorialSeen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        keysTutorialSeen = false;
        mirrorTutorialSeen = false;
    }
    void Start()
    {
        input.OnMove += HandleMove;
        input.OnConfirm += HandleConfirm;
        input.OnRetry += HandleRetry;
        ShowMenu();
    }

    void OnDestroy()
    {
        if (input == null) return;
        input.OnMove -= HandleMove;
        input.OnConfirm -= HandleConfirm;
        input.OnRetry -= HandleRetry;
    }

    // ---------- Menu / start / end ----------

    void ShowMenu()
    {
        state = State.Menu;
        ui.ClearGameplayTexts();
        ui.UpdateStats(0, 0, startingLives);
        ui.ShowCenter("TALK TO THE HAND\n\nPress SPACE or R to start");
    }

    void HandleConfirm()
    {
        if (state == State.Menu || state == State.Results) StartGame();
    }

    void StartGame()
    {
        if (gameRoutine != null) StopCoroutine(gameRoutine);
        lives = startingLives;
        score = 0;
        streak = 0;
        bestStreak = 0;
        errors = 0;
        dead = false;
        awaiting = false;
        sessionStart = Time.time;
        logger.NewSession();
        ui.ShowCenter("");
        ui.UpdateStats(score, streak, lives);
        result = Result.Pending;
        ui.ClearGameplayTexts();
        ui.SetInstructionColor(Color.white);
        gameRoutine = StartCoroutine(RunGame());
    }

    IEnumerator RunGame()
    {
    bool showIntro = !keysTutorialSeen || !mirrorTutorialSeen;

    if (!keysTutorialSeen)
    {
        yield return StageKeys();
        keysTutorialSeen = true; // se marca apenas termina, pase lo que pase después
    }

    if (!mirrorTutorialSeen)
    {
        yield return StageMirror();
        if (!dead) mirrorTutorialSeen = true;
    }

    if (!dead) yield return StageSequences(showIntro);
    EndGame(!dead);
    }

    void EndGame(bool completed)
    {
        state = State.Results;
        awaiting = false;
        float total = Time.time - sessionStart;
        logger.LogSummary(completed, score, bestStreak, errors, Mathf.Max(lives, 0), total);
        Play(completed ? winClip : loseClip);
        ui.ClearGameplayTexts();
        string title = completed ? "TUTORIAL COMPLETE!" : "GAME OVER";
        ui.ShowCenter($"{title}\n\nScore: {score}\nBest streak: {bestStreak}\nMistakes: {errors}\nTime: {total:0}s\n\nPress SPACE or R to play again");
    }

    // ---------- Stages ----------

    IEnumerator StageKeys()
    {
        state = State.Keys;
        stageName = "1-Keys";
        pressedKeys.Clear();
        ui.SetInstructionColor(Color.white);
        ui.SetInstruction("Stage 1: try every key");
        ui.SetHint("Left hand: W A S D     Right hand: Arrow keys");
        ui.SetKeysProgress(0, TotalKeys);

        yield return new WaitUntil(() => pressedKeys.Count >= TotalKeys);

        Play(winClip);
        ui.SetKeysProgress(-1, TotalKeys);
        ui.SetInstruction("Great! You know the keys.");
        ui.SetHint("");
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator StageMirror()
    {
        state = State.Mirror;
        stageName = "2-Mirror";
        ui.SetInstruction("Stage 2: be Emy's mirror");
        ui.SetHint("Emy faces you: her LEFT is your RIGHT. Copy her with the opposite hand.");
        yield return new WaitForSeconds(3f);

        for (int r = 1; r <= 4 && !dead; r++)
        {
            sequenceIndex = r;
            yield return PlaySequence(SequenceGenerator.Generate(1, true));
        }
    }

    IEnumerator StageSequences(bool showIntro)
    {
            state = State.Sequences;
            stageName = "3-Sequences";

        if (showIntro)
        {
            ui.SetInstruction("Stage 3: sequences");
            ui.SetHint("Now left and right sides flip too. A mistake restarts the sequence.");
            yield return new WaitForSeconds(3f);
            ui.SetHint("");
        }
        else
        {
            ui.SetInstruction("Get ready!");
            yield return new WaitForSeconds(1f);
        }

        int[] lengths = { 3, 3, 4, 4, 5, 5 };
        for (int i = 0; i < lengths.Length && !dead; i++)
        {
            sequenceIndex = i + 1;
            yield return PlaySequence(SequenceGenerator.Generate(lengths[i], false));
        }
    }

    // ---------- Core loop for one sequence ----------

    IEnumerator PlaySequence(List<Move> seq)
    {
        int step = 0;
        while (step < seq.Count && !dead)
        {
            currentStep = step + 1;
            Move emyMove = seq[step];

            ui.SetInstructionColor(Color.white);
            ui.SetInstruction(emyMove.ToInstruction());
            ui.SetProgress($"Step {step + 1} / {seq.Count}");
            EmyHand(emyMove.hand).Perform(emyMove.direction);

            expected = emyMove.Mirror();
            result = Result.Pending;
            stepStartTime = Time.time;
            awaiting = true;

            yield return new WaitUntil(() => result != Result.Pending);
            awaiting = false;

            if (result == Result.Correct)
            {
                step++;
                streak++;
                if (streak > bestStreak) bestStreak = streak;
                score += pointsPerStep;
                ui.SetInstructionColor(correctColor);
                PlayerHand(lastPressed.hand).Flash(correctColor);
                Play(correctClip);
            }
            else
            {
                errors++;
                streak = 0;
                lives--;
                ui.SetInstructionColor(wrongColor);
                PlayerHand(lastPressed.hand).Flash(wrongColor);
                Play(wrongClip);
                ui.UpdateStats(score, streak, lives);

                if (lives <= 0)
                {
                    dead = true;
                    break;
                }

                ui.SetInstruction("Wrong! Remember \n Emy's LEFT = your RIGHT");
                step = 0; // restart the same sequence
                yield return new WaitForSeconds(1.4f);
            }

            ui.UpdateStats(score, streak, lives);
            yield return new WaitForSeconds(pauseBetweenSteps);
        }

        if (!dead && seq.Count > 1)
        {
            score += seq.Count * 5; // small bonus for finishing a full sequence
            ui.UpdateStats(score, streak, lives);
            ui.SetInstructionColor(correctColor);
            ui.SetInstruction("Sequence complete!");
            Play(winClip);
            yield return new WaitForSeconds(1.2f);
        }
    }

    // ---------- Player input ----------

    void HandleMove(Move m)
    {
        // The player's hands always move, whatever the state.
        PlayerHand(m.hand).Perform(m.direction);

        switch (state)
        {
            case State.Keys:
                if (pressedKeys.Add(m.ToString()))
                {
                    ui.SetKeysProgress(pressedKeys.Count, TotalKeys);
                    Play(correctClip);
                }
                break;

            case State.Mirror:
            case State.Sequences:
                if (!awaiting || result != Result.Pending) break;
                lastPressed = m;
                bool ok = m.Matches(expected);
                float reaction = Time.time - stepStartTime;
                logger.LogStep(stageName, sequenceIndex, currentStep, expected, m, ok, reaction, lives);
                result = ok ? Result.Correct : Result.Wrong;
                break;
        }
    }

    // ---------- Helpers ----------

    HandController PlayerHand(Hand h) { return h == Hand.Left ? playerLeft : playerRight; }
    HandController EmyHand(Hand h) { return h == Hand.Left ? emyLeft : emyRight; }

    void Play(AudioClip clip)
    {
        if (sfx != null && clip != null) sfx.PlayOneShot(clip);
    }
    void HandleRetry()
{
    if (state == State.Keys || state == State.Mirror || state == State.Sequences)
        logger.LogSummary(false, score, bestStreak, errors, Mathf.Max(lives, 0), Time.time - sessionStart);
    StartGame();
}
}
