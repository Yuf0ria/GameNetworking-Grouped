using UnityEngine;
using Fusion;

public class NetworkDebug : NetworkBehaviour
{
    private float _logTimer = 0f;

    public override void FixedUpdateNetwork()
    {
        _logTimer += Runner.DeltaTime;

        if (_logTimer >= 1f) // Log every second
        {
            _logTimer = 0f;

            Debug.Log($"[{Object.InputAuthority}] " +
            $"HasInput={HasInputAuthority}, " +
            $"HasState={HasStateAuthority}, " +
            $"Pos={transform.position}, " +
            $"Rot={transform.eulerAngles.y:F1}°");
        }
    }
}
