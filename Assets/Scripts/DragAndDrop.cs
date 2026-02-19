/* this script controls the drag and drop of the obj being carried
 * Fusion 2 compatible with proper network synchronization
 * --fixed position syncing for remote players --dani
 */

using UnityEngine;
using Fusion;
using Network_Scripts;

public class DragAndDrop : NetworkBehaviour
{
    [Header("Gizmos Range")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask draggableLayer;
    [Header("Carry Settings")]
    [SerializeField] private Transform handPosition;
    [SerializeField] private Vector3 holdOffset = new Vector3(0, 1f, 1f);
    [SerializeField] private float objSpeed = 10f;
    
    [Networked] private NetworkObject CarriedNetworkObject { get; set; }
    
    private GameObject Object;
    private Rigidbody objRigidbody;
    
    void Start()
    {
        //this spawns gameobject that appears in front of the playerPrefab at the start of the game
        //It's just here in case I forgot to put the gameobject on the player prefab
        if (handPosition == null)
        {
            GameObject handPoint = new GameObject("HandPosition");
            handPoint.transform.SetParent(transform);
            handPoint.transform.localPosition = holdOffset;
            handPosition = handPoint.transform;
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        // Only local player can control input
        if (HasInputAuthority)
        {
            if (GetInput(out NetworkInputData input))
            {
                if (input.interact)
                {
                    if (Object == null)
                    {
                        RaycastPickupObj();
                    }
                    else
                    {
                        DropObj();
                    }
                    Debug.Log("E key pressed!");
                }
            }
        }
        
        // Update carried object position - THIS RUNS FOR ALL PLAYERS NOW
        if (Object != null)
        {
            Update_ObjectPos();
        }
    }
    
    void RaycastPickupObj()
    {
        // Raycast or sphere cast to find nearby draggable objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, draggableLayer);
        Debug.Log($"Found {colliders.Length} objects in range");
        
        if (colliders.Length > 0)
        {
            // Pick up the closest object
            GameObject closestObject = colliders[0].gameObject;
            float closestDistance = Vector3.Distance(transform.position, closestObject.transform.position);
            
            foreach (Collider col in colliders)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestObject = col.gameObject;
                    closestDistance = distance;
                }
            }
            
            // Check if object has NetworkObject component
            NetworkObject netObj = closestObject.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                RPC_PickupObject(netObj);
            }
            else
            {
                Debug.LogWarning("Object doesn't have NetworkObject component!");
            }
        }
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_PickupObject(NetworkObject netObj)
    {
        // Server handles the pickup
        if (netObj == null || CarriedNetworkObject != null) return;
        
        CarriedNetworkObject = netObj;
        RPC_SyncPickup(netObj);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SyncPickup(NetworkObject netObj)
    {
        // All clients sync the pickup
        if (netObj == null) return;
        
        Object = netObj.gameObject;
        objRigidbody = Object.GetComponent<Rigidbody>();
        
        if (objRigidbody != null)
        {
            objRigidbody.isKinematic = true;
            objRigidbody.useGravity = false;
        }
        
        // Disable collisions with player
        Collider objCollider = Object.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (objCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerCollider, true);
        }
        
        Debug.Log($"Object picked up and synced: {Object.name}");
    }
    
    void DropObj()
    {
        if (Object == null) return;
        RPC_DropObject();
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_DropObject()
    {
        // Server handles the drop
        if (CarriedNetworkObject == null) return;
        
        NetworkObject netObj = CarriedNetworkObject;
        CarriedNetworkObject = null;
        RPC_SyncDrop(netObj);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SyncDrop(NetworkObject netObj)
    {
        // All clients sync the drop
        if (Object == null) return;
        
        if (objRigidbody != null)
        {
            objRigidbody.isKinematic = false;
            objRigidbody.useGravity = true;
        }
        
        // Re-enable collisions
        Collider objCollider = Object.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (objCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objCollider, playerCollider, false);
        }
        
        Object = null;
        objRigidbody = null;
        
        Debug.Log("Object dropped and synced");
    }
    
    void Update_ObjectPos()
    {
        if (Object != null && handPosition != null)
        {
            // Smooth follow
            Object.transform.position = Vector3.Lerp(
                Object.transform.position, 
                handPosition.position, 
                objSpeed * Time.deltaTime
            );
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualize pickup range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        
        if (handPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(handPosition.position, 0.2f);
        }
    }
}