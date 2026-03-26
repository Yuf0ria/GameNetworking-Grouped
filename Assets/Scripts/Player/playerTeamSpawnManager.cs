/*
 * This scripts allows the game to get and check the following before spawning the player:
 * - Get the player's name
 * - Get their desired mesh/material
 * - Get their prefab using Network Object
 * - Check if the Session works before the player is spawned
 *
 */
//Built-in namespaces
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//Custom namespace
using Network_Scripts;

public class PlayerTeamSpawnManager : MonoBehaviour
{
    //canvas
    [Header("Components for the Canvas")]
    [SerializeField] private GameObject RegisterPanel, InGamePanel;
    [SerializeField] private Button button;
    //Spawning not on the same spot
    [Header("Spawn Area")]
    [SerializeField] private Vector3 spawnAreaCenter = new Vector3(0, 1, 0);
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeight = 1f; 
    //player name and materials
    [Header("Name & Outfit")]
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private TMP_Dropdown materialDropdown;
    [SerializeField] private Material[] materials;
    //checking if spawned()
    private bool hasSpawned = false;

    private void Start()
    {
        InGamePanel.SetActive(false);
        //IMPORTANT NULL CHECKS
        #region Null Checks
            if (button != null)
            {
                button.interactable = false;
                button.onClick.AddListener(OnButtonClick); 
            }else Debug.LogError("No button assigned!");
                
            if (materialDropdown != null)
            {
                MaterialsInDropdown();
            }
            else Debug.LogError("Dropdown is empty, check inspector");

            if (playerNameInputField == null)
                Debug.LogError("Input field is empty, check inspector");
                
            if (NetworkSessionManager.Instance != null)
            {
                if (NetworkSessionManager.Instance.IsSessionReady)
                    SessionOK();
                else NetworkSessionManager.Instance.OnSessionStarted += SessionOK;
            }
            
            if (SpawnRequestHandler.Instance != null)
                SpawnRequestHandler.Instance.OnHandlerReady += SessionOK;
            else Debug.LogError("NetworkSessionManager.cs not found"); //Don't delete this script 💀
        #endregion
    }
    
    private void MaterialsInDropdown()
    {
        materialDropdown.ClearOptions();
        #region NULLCHECKS (MATERIALS)
            if (materials == null || materials.Length == 0)
            {
                Debug.LogWarning("No mats here. Add your Materials to the dropdown list!");
                return;
            }
        #endregion
        
        var options = new System.Collections.Generic.List<string>();
        foreach (Material mat in materials)
            options.Add(mat != null ? mat.name : "--Empty--");

        materialDropdown.AddOptions(options);
    }

    private void SessionOK()
    {
        button.interactable = true;
    }
    private void OnButtonClick()
    {
        var position = RandomSpawnPosition();
        SpawnPlayer(position);
    }

    private Vector3 RandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return new Vector3(
            spawnAreaCenter.x + randomCircle.x,
            spawnHeight,
            spawnAreaCenter.z + randomCircle.y
        );
    }

    private void SpawnPlayer(Vector3 spawnPosition)
    {
        #region NULLCHECKS (SPAWNING)
            if (hasSpawned)
            {
                Debug.LogWarning("Local player already spawned");
                return;
            }

            string playerName = playerNameInputField != null ? playerNameInputField.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("name is null. name = 'player'");
                playerName = "Player";
            }

            int selectedMatIndex = materialDropdown != null ? materialDropdown.value : 0;

            if (NetworkSessionManager.Instance == null)
            {
                Debug.LogError("SESSION IS NULL!!");
                return;
            }

            NetworkRunner runner = NetworkSessionManager.Instance.Runner;
            if (runner == null)
            {
                Debug.LogError("NO RUNNER!! AAAA");
                return;
            }

            if (!runner.IsRunning)
            {
                Debug.LogError($"State: IsServer={runner.IsServer}, IsClient={runner.IsClient}");
                return;
            }

            if (SpawnRequestHandler.Instance == null || !SpawnRequestHandler.Instance.IsReady)
            {
                Debug.LogError("SpawnRequestHandler not ready yet! Is it a NetworkObject in the scene?");
                return;
            }
        #endregion

        try
        {
            SpawnRequestHandler.Instance.RPC_RequestSpawn(playerName, selectedMatIndex, spawnPosition);

            // Close the UI immediately — the spawn happens async on the host
            hasSpawned = true;
            RegisterPanel.SetActive(false);
            InGamePanel.SetActive(true);

            Debug.Log($"[PlayerTeamSpawnManager] Spawn request sent — Name='{playerName}', Mat={selectedMatIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Spawn request error: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
            NetworkSessionManager.Instance.OnSessionStarted -= SessionOK;

        if (button != null) button.onClick.RemoveListener(OnButtonClick);
    }
}
