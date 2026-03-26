/*
 * CustomerBehaviour.cs
 * Attached to each customer NetworkObject.
 * - 3 minute leave timer
 * - Detects when a player delivers an item (either by walking up with item, or dropping in zone)
 * - Despawns on served or timeout
 */

using Fusion;
using UnityEngine;

public class CustomerBehaviour : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float leaveTimer = 180f; // 3 minutes

    [Networked] private float _timeLeft { get; set; }
    [Networked] private NetworkBool _isServed { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
            _timeLeft = leaveTimer;

        Debug.Log($"[Customer] Spawned. Has {leaveTimer}s before leaving.");
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (_isServed) return;

        _timeLeft -= Runner.DeltaTime;

        if (_timeLeft <= 0f)
        {
            Debug.Log("[Customer] Time's up — customer is leaving!");
            Leave();
        }
    }

    /// <summary>
    /// Triggered when a player enters the customer's trigger zone.
    /// Checks if they have an item in either hand (via playerGrabbing).
    /// Works for both "walk up with item" and "drop item in zone".
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_isServed) return;
        if (!HasStateAuthority) return;

        // Check if the collider is a player holding an item
        playerGrabbing grabbing = other.GetComponent<playerGrabbing>();
        if (grabbing == null) return;

        if (grabbing.isLeftHandGrabbing || grabbing.isRightHandGrabbing)
        {
            Serve();
        }
    }

    private void Serve()
    {
        _isServed = true;
        Debug.Log("[Customer] Served! Despawning.");
        // TODO: add score/cash here — e.g. GameTimerManager.Instance.AddScore()
        Runner.Despawn(Object);
    }

    private void Leave()
    {
        Debug.Log("[Customer] Left without being served.");
        Runner.Despawn(Object);
    }

    /// <summary>
    /// Returns 0-1 for UI progress bars if needed.
    /// </summary>
    public float GetTimeRatio() => _timeLeft / leaveTimer;
    public float GetTimeLeft() => _timeLeft;
}
