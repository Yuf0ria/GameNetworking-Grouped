using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Network_Scripts
{
    public class NetworkSessionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkSessionManager Instance { get; private set; }
    
        [SerializeField] private GameMode gameMode = GameMode.Shared; // Changed to Shared explicitly
        [SerializeField] private string sessionName = "TestRoom";

        public NetworkRunner Runner { get; set; }
        public bool IsSessionReady { get; private set; }
    
        public System.Action OnSessionStarted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("✅ NetworkSessionManager Instance Created");
            }
            else
            {
                Debug.Log("⚠️ Duplicate NetworkSessionManager - Destroying");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(StartSession());
        }

        private IEnumerator StartSession()
        {
            Debug.Log("Starting Fusion session...");
            Debug.Log($"GameMode: {gameMode}");
            Debug.Log($"Session Name: {sessionName}");
        
            Runner = gameObject.AddComponent<NetworkRunner>();
            Runner.ProvideInput = true;
            Runner.AddCallbacks(this);
        
            var startTask = Runner.StartGame(new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        
            while (!startTask.IsCompleted)
            {
                yield return null;
            }
        
            var result = startTask.Result;
        
            if (result.Ok)
            {
                Debug.Log("fusion started");
                // Debug.Log($"   IsRunning: {Runner.IsRunning}");
                // Debug.Log($"   IsServer: {Runner.IsServer}");
                // Debug.Log($"   IsClient: {Runner.IsClient}");
                // Debug.Log($"   LocalPlayer ID: {Runner.LocalPlayer}");
                // Debug.Log($"   Session Name: {Runner.SessionInfo.Name}");
            
                IsSessionReady = true;
                OnSessionStarted?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to start session: {result.ShutdownReason}");
            }
        }
    
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // CRITICAL: Only collect input if this is the local player
            if (!runner.IsRunning) return;

            NetworkInputData inputData = new NetworkInputData();

            // Collect movement input
            inputData.movementInput = new Vector3(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical")
            );

            // Collect mouse input
            inputData.mouseInput = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );

            // Collect sprint input
            inputData.isSprinting = Input.GetKey(KeyCode.LeftShift);

            // Send input to network
            input.Set(inputData);

            // Debug to verify input is being collected
            Debug.Log($"Input collected: Move={inputData.movementInput}, Mouse={inputData.mouseInput}");
        }
    
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            Debug.Log($"🎮 PLAYER JOINED!");
            Debug.Log($"   Player ID: {player.PlayerId}");
            Debug.Log($"   Is Local Player: {player == runner.LocalPlayer}");
            Debug.Log($"   Total Players: {runner.ActivePlayers.Count()}");
            
            // List all players in session
            foreach (var p in runner.ActivePlayers)
            {
                Debug.Log($"- Player {p.PlayerId} {(p == runner.LocalPlayer ? "(LOCAL)" : "(REMOTE)")}");
            }
        }
    
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
        {
            Debug.Log($"Player left: {player.PlayerId}");
        }
        
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) 
        {
            Debug.LogWarning($"Input missing for player {player.PlayerId}");
        }
        
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
        {
            Runner.RemoveCallbacks(this);
            Debug.LogWarning($"Session shutdown: {shutdownReason}");
        }
        
        public void OnConnectedToServer(NetworkRunner runner) 
        {
            Debug.Log("✅ Connected to server!");
        }
        
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
        {
            Debug.LogWarning($"Disconnected from server: {reason}");
        }
        
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
        {
            Debug.LogError($"Connection failed: {reason}");
        }
        
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) 
        {
            Debug.Log($"Object exited AOI: {obj.name} for player {player.PlayerId}");
        }
        
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) 
        {
            Debug.Log($"Object entered AOI: {obj.name} for player {player.PlayerId}");
        }
    }
}
