using Fusion;
using UnityEngine;

public class SpawnRequestHandler : NetworkBehaviour
{
    public static SpawnRequestHandler Instance { get; private set; }
    public bool IsReady { get; private set; }
    public System.Action OnHandlerReady;
    [Header("Serialize Fields")]
    [SerializeField] private NetworkObject playerPrefab;

    private void Awake()
    {
        //if instance null, make instantiate else naur, bye bye, prevent duplicates
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[SpawnRequestHandler] Ready");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void Spawned()
    {
        IsReady = true;
        OnHandlerReady?.Invoke();
        Debug.Log("[SpawnRequestHandler] OK");
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestSpawn(string playerName, int matIndex, Vector3 spawnPosition, RpcInfo info = default)
    {
        Debug.Log($"Player is:  {info.Source.PlayerId}");

        if (playerPrefab == null)
        {
            Debug.LogError("wala sa inspector");
            return;
        }

        PlayerRef inputAuthority = info.Source.IsNone ? Runner.LocalPlayer : info.Source;

        NetworkObject player = Runner.Spawn(
            playerPrefab,
            spawnPosition,
            Quaternion.identity,
            inputAuthority
        );

        if (player == null)
        {
            Debug.LogError($"Player is not spawned: {info.Source.PlayerId}");
            return;
        }
        
        PlayerCustomization customization = player.GetComponent<PlayerCustomization>();
        if (customization != null)
        {
            customization.InsPlayerInfo(playerName, matIndex);
            GameTimerManager.Instance.RegisterPlayerSpawn();
            Debug.Log($"Player Here: {info.Source.PlayerId} — Name='{playerName}', Mat={matIndex}");
        }
        else
        {
            Debug.LogWarning(" not custom mat component.");
        }
    }
}
