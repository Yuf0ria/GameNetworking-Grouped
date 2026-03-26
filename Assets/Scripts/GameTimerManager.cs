/*
 * GameTimerManager.cs
 * Handles the full game flow:
 * 1. Wait for all session players to spawn
 * 2. 5-minute prep phase (skip with Ctrl+R on ReadyObject)
 * 3. 30-minute game phase — customers spawn every 5 minutes (1-5 random)
 * 4. Game over
 */

using Fusion;
using UnityEngine;
using System.Linq;

public enum GamePhase
{
    WaitingForPlayers,
    PrepPhase,
    GamePhase,
    GameOver
}

public class GameTimerManager : NetworkBehaviour
{
    public static GameTimerManager Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float prepDuration = 300f;  // 5 minutes
    [SerializeField] private float gameDuration = 1800f; // 30 minutes
    [SerializeField] private float customerSpawnInterval = 300f; // every 5 minutes

    [Header("Customer Settings")]
    [SerializeField] private NetworkObject customerPrefab;
    [SerializeField] private Transform[] customerSpawnPoints; // assign in inspector


    [Networked] public GamePhase CurrentPhase { get; private set; } = GamePhase.WaitingForPlayers;
    [Networked] public float TimeRemaining { get; private set; }
    [Networked] private int _spawnedPlayerCount { get; set; }
    [Networked] private float _nextCustomerSpawnTime { get; set; }
    
    
    public System.Action<GamePhase> OnPhaseChanged;
    public System.Action<float> OnTimerTick;

    private GamePhase _lastPhase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void Spawned()
    {
        _lastPhase = CurrentPhase;
    }

    /// <summary>
    /// Called by SpawnRequestHandler (or PlayerMovement.Spawned) when a player spawns.
    /// Only the host increments the count.
    /// </summary>
    public void RegisterPlayerSpawn()
    {
        if (!HasStateAuthority) return;

        _spawnedPlayerCount++;
        Debug.Log($"[GameTimerManager] Players spawned: {_spawnedPlayerCount}/{Runner.ActivePlayers.Count()}");

        if (_spawnedPlayerCount >= Runner.ActivePlayers.Count())
            StartPrepPhase();
    }

    /// <summary>
    /// Called by ReadyObject when Ctrl+R is pressed.
    /// Skips the prep timer and goes straight to game phase.
    /// </summary>
    public void ForceStartGame()
    {
        if (!HasStateAuthority) return;
        if (CurrentPhase != GamePhase.PrepPhase) return;

        Debug.Log("[GameTimerManager] Prep skipped by Ctrl+R");
        StartGamePhase();
    }

    private void StartPrepPhase()
    {
        CurrentPhase = GamePhase.PrepPhase;
        TimeRemaining = prepDuration;
        Debug.Log("[GameTimerManager] Prep phase started");
    }

    private void StartGamePhase()
    {
        CurrentPhase = GamePhase.GamePhase;
        TimeRemaining = gameDuration;
        _nextCustomerSpawnTime = customerSpawnInterval;
        Debug.Log("[GameTimerManager] Game phase started");
    }

    private void EndGame()
    {
        CurrentPhase = GamePhase.GameOver;
        TimeRemaining = 0f;
        Debug.Log("[GameTimerManager] Game over");
    }

    public override void FixedUpdateNetwork()
    {
        // Fire event on all clients when phase changes
        if (CurrentPhase != _lastPhase)
        {
            OnPhaseChanged?.Invoke(CurrentPhase);
            _lastPhase = CurrentPhase;
        }

        OnTimerTick?.Invoke(TimeRemaining);

        // Only the host ticks the timers
        if (!HasStateAuthority) return;

        switch (CurrentPhase)
        {
            case GamePhase.PrepPhase:
                TimeRemaining -= Runner.DeltaTime;
                if (TimeRemaining <= 0f)
                    StartGamePhase();
                break;

            case GamePhase.GamePhase:
                TimeRemaining -= Runner.DeltaTime;

                _nextCustomerSpawnTime -= Runner.DeltaTime;
                if (_nextCustomerSpawnTime <= 0f)
                {
                    SpawnCustomerWave();
                    _nextCustomerSpawnTime = customerSpawnInterval;
                }

                if (TimeRemaining <= 0f)
                    EndGame();
                break;
        }
    }

    private void SpawnCustomerWave()
    {
        if (customerPrefab == null || customerSpawnPoints == null || customerSpawnPoints.Length == 0)
        {
            Debug.LogError("[GameTimerManager] Customer prefab or spawn points not assigned!");
            return;
        }

        int count = Random.Range(1, 6); // 1 to 5
        Debug.Log($"[GameTimerManager] Spawning {count} customer(s)");

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = customerSpawnPoints[Random.Range(0, customerSpawnPoints.Length)];
            Runner.Spawn(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
