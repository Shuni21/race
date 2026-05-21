using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    [Header("Checkpoints")]
    public Collider[] checkpoints;

    [Header("Race Settings")]
    public int totalLaps = 3;

    private int playerLap = 1;
    private int playerNextCheckpoint = 0;

    private class AIState
    {
        public int lap = 1;
        public int nextCheckpoint = 0;
        public bool finished = false;
    }

    private Dictionary<int, AIState> aiStates = new Dictionary<int, AIState>();

    public bool isRaceStarted = false;
    private bool isRaceEnded = false;

    private float raceTime = 0f;
    private UIController uiController;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        uiController = Object.FindAnyObjectByType<UIController>();
        StartCoroutine(StartCountdown());
    }

    void Update()
    {
        if (isRaceStarted && !isRaceEnded)
        {
            raceTime += Time.deltaTime;
            uiController?.UpdateTimerText(raceTime);
        }
    }

    IEnumerator StartCountdown()
    {
        uiController?.UpdateLapText("READY");

        for (int i = 3; i > 0; i--)
        {
            uiController?.UpdateTimerTextCustom(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        uiController?.UpdateTimerTextCustom("GO!");
        isRaceStarted = true;

        yield return new WaitForSeconds(0.5f);

        UpdateUI();
    }

    public void PlayerTriggered(Collider cp, GameObject car)
    {
        if (!isRaceStarted || isRaceEnded)
            return;

        if (car.CompareTag("Player"))
        {
            HandlePlayer(cp);
        }
        else
        {
            AICarController ai = car.GetComponent<AICarController>();

            if (ai != null)
            {
                HandleAI(cp, ai.aiId);
            }
        }
    }

    // ================= PLAYER =================

    void HandlePlayer(Collider cp)
    {
        // защита от пустого массива
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogError("Checkpoints array is empty!");
            return;
        }

        // защита от выхода за границы
        if (playerNextCheckpoint >= checkpoints.Length)
        {
            playerNextCheckpoint = 0;
        }

        // если это НЕ нужный чекпоинт
        if (cp != checkpoints[playerNextCheckpoint])
            return;

        playerNextCheckpoint++;

        // круг завершён
        if (playerNextCheckpoint >= checkpoints.Length)
        {
            playerNextCheckpoint = 0;

            if (playerLap < totalLaps)
            {
                playerLap++;
                UpdateUI();
            }
            else
            {
                EndRace();
            }
        }
    }

    // ================= AI =================

    void HandleAI(Collider cp, int id)
    {
        if (!aiStates.ContainsKey(id))
        {
            aiStates[id] = new AIState();
        }

        AIState ai = aiStates[id];

        if (ai.finished)
            return;

        if (checkpoints == null || checkpoints.Length == 0)
            return;

        if (ai.nextCheckpoint >= checkpoints.Length)
        {
            ai.nextCheckpoint = 0;
        }

        if (cp != checkpoints[ai.nextCheckpoint])
            return;

        ai.nextCheckpoint++;

        if (ai.nextCheckpoint >= checkpoints.Length)
        {
            ai.nextCheckpoint = 0;

            if (ai.lap < totalLaps)
            {
                ai.lap++;
            }
            else
            {
                ai.finished = true;
            }
        }
    }

    void EndRace()
    {
        isRaceEnded = true;

        bool aiWon = false;

        foreach (var ai in aiStates.Values)
        {
            if (ai.finished)
            {
                aiWon = true;
                break;
            }
        }

        uiController?.ShowEndGameScreen(aiWon ? "YOU LOSE!" : "YOU WIN!");
    }

    void UpdateUI()
    {
        uiController?.UpdateLapText($"LAP: {playerLap} / {totalLaps}");
    }
}