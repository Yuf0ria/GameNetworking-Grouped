/*
 * GameTimerUI.cs
 * Attach to a Canvas GameObject.
 * Displays the current game phase and timer.
 */

using TMPro;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text phaseText;   // e.g. "PREP PHASE" / "GAME IN PROGRESS"
    [SerializeField] private TMP_Text timerText;   // e.g. "4:32"
    [SerializeField] private GameObject readyHint; // "Press Ctrl+R to start early" hint

    private void Start()
    {
        if (GameTimerManager.Instance == null)
        {
            Debug.LogError("[GameTimerUI] GameTimerManager not found!");
            return;
        }

        GameTimerManager.Instance.OnPhaseChanged += UpdatePhaseDisplay;
        GameTimerManager.Instance.OnTimerTick += UpdateTimerDisplay;

        // Set initial state
        UpdatePhaseDisplay(GameTimerManager.Instance.CurrentPhase);
    }

    private void UpdatePhaseDisplay(GamePhase phase)
    {
        if (readyHint != null)
            readyHint.SetActive(phase == GamePhase.PrepPhase);

        if (phaseText == null) return;

        switch (phase)
        {
            case GamePhase.WaitingForPlayers:
                phaseText.text = "Waiting for players...";
                break;
            case GamePhase.PrepPhase:
                phaseText.text = "Prep Phase";
                break;
            case GamePhase.GamePhase:
                phaseText.text = "Game in Progress";
                break;
            case GamePhase.GameOver:
                phaseText.text = "Game Over!";
                if (timerText != null) timerText.text = "0:00";
                break;
        }
    }

    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes}:{seconds:00}";
    }

    private void OnDestroy()
    {
        if (GameTimerManager.Instance == null) return;
        GameTimerManager.Instance.OnPhaseChanged -= UpdatePhaseDisplay;
        GameTimerManager.Instance.OnTimerTick -= UpdateTimerDisplay;
    }
}
