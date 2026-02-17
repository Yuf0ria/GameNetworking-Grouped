/*
 * This scripts allows the game to get and check the following before spawning the player:
 * - Get the player's name
 * - Get their desired mesh/material
 * - Get their prefab using Network Object
 * - Check if the Session works before the player is spawned
 *
 * (Imported from an previous Network Project)
 * if you have any questions leave a comment or ping me on discord
 * --dani
 */
//Built-in namespaces
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/*
 * this the NetworkInputData Script, I namespaced it as another way to call a scripts that in a folder
 * this also allows it to be easily found, unlike getting an error that it's not found.
 * ("=_=)
 * --dani
 */
//Custom namespace
using Network_Scripts;

public class PlayerTeamSpawnManager : MonoBehaviour
{
    //canvas
    [Header("Components for the Canvas")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button button;
    //player
    [Header("For Instantiation & Network call")]
    [SerializeField] private NetworkObject playerPrefab;
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
        /* most of the statements here are null checks, DON'T COMMENT IT OUT.
         * 
         * -- dani, Feb 17
         */
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
                Debug.LogWarning("Local player spawned");
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
                Debug.LogError($"AYAW TUMAKBO, BAKIT ALL FALSE State: IsServer={runner.IsServer}, IsClient={runner.IsClient}");
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("no prefab");
                return;
            }
        #endregion
        
        try
        {
            NetworkObject player = runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                runner.LocalPlayer
            );

            #region NULLCHECKS (PLAYER)
                if (player == null)
                {
                    Debug.LogError("NO SPAWN?!?!");
                    return;
                }
            #endregion
            
            //CUSTOMIZATION SETUP -- PLAYER CUSTOMIZATION SCRIPT
            PlayerCustomization customization = player.GetComponent<PlayerCustomization>();
            if (customization != null)
            {
                customization.InsPlayerInfo(playerName, selectedMatIndex);
                Debug.Log($"Customization applied: Name='{playerName}', MatIndex={selectedMatIndex}");
            }
            else
            {
                Debug.LogWarning("Spawned player has no PlayerCustomization component – skipping customization.");
            }
            
            //SWITCH
            hasSpawned = true;
            panel.SetActive(false);
        }
        catch (System.Exception e)
        {
            //Error if there is a prob
            Debug.LogError($"Spawn error: {e.Message}");
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
