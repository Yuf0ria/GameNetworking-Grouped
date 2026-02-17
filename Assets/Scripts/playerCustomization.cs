//<summary>
    //renders the information taken from the panel to the player
    //Show the name of the player above the capsule
    //Show the material color that the player took
//</summary>
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerCustomization : NetworkBehaviour
{
    #region Serialized Fields
        [SerializeField] public Material[] materials;//this calls the script which handles the materials in the dropdown option
        [SerializeField] private Renderer _playerRenderer;
        [SerializeField] private TMP_Text _name;
    #endregion
    #region Networked
        [Networked] private NetworkString<_32>  _playerName { get; set; }
        [Networked] private int _matIndex { get; set; }
    #endregion
    
    //for checking if materials are duplicating
    private int _lastMaterialIndex = -1;
    
    //instantiate player info
    public void InsPlayerInfo(string name, int matIndex)
    {
        if (HasStateAuthority)
        {
            _playerName = name;
            _matIndex = matIndex;
            return;
        }
        Debug.Log("return works");
    }
    
    public override void Render()
    {
        // Update name,,still fixing this but no materials found
        if (_name != null)
        {
            _name.text = _playerName.ToString();
        } else Debug.LogError("Name is null");
        // Update material
        if (_matIndex == _lastMaterialIndex && materials != null && _matIndex >= materials.Length)
        {
            Debug.Log("MaterialsHere");
            _playerRenderer.material = materials[_matIndex];
            _lastMaterialIndex = _matIndex;
        } else Debug.LogWarning("No materials found");
    }
}