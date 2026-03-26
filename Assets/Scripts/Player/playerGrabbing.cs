using Fusion;
using Network_Scripts;
using UnityEngine;

public class playerGrabbing : NetworkBehaviour
{
    [Header("Player Arms")] 
    public GameObject leftArm;
    public GameObject rightArm;

    [Header("Player Arms Transforms")] 
    [SerializeField] private Transform leftArmTransform;
    [SerializeField] private Transform rightArmTransform;

    //Not for Serialization
    private GameObject _leftHandFull;
    private GameObject _rightHandFull;
    
    [Header("Grab Settings")]
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private LayerMask GrabLayerMask;
    
    //network bools
    [Networked] public NetworkBool isLeftHandGrabbing { get; set; }
    [Networked] public NetworkBool isRightHandGrabbing { get; set; }

    private void Start()
    {
        leftArm.SetActive(false);
        rightArm.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        //arm appears when E is pressed
        leftArm.SetActive(isLeftHandGrabbing);
        rightArm.SetActive(isRightHandGrabbing);

        if (!HasInputAuthority) return;
        
        if (GetInput(out NetworkInputData input))
        {
            if (input.interact)
                PickupObjsCollider(); // E = always grab

            if (input.drop && (isLeftHandGrabbing || isRightHandGrabbing))
                RPC_RequestDrop();    // F = always drop
        }
    }

    void PickupObjsCollider()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange, GrabLayerMask);
        if (hits.Length == 0) return;

        GameObject closestObj = ClosestObj(hits, _leftHandFull,  _rightHandFull);
        if (closestObj == null) return;

        NetworkObject netObj = closestObj.GetComponent<NetworkObject>();
        if (netObj != null)
            RPC_RequestGrab(netObj.Id);
        else
            Debug.LogWarning($"[Grabbing] {closestObj.name} has no NetworkObject — can't grab over network");
    }

    private GameObject ClosestObj(Collider[] hits, GameObject excludeL, GameObject excludeR)
    {
        GameObject closestObj = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if(hit.gameObject == excludeL ||  hit.gameObject == excludeR) continue;
            
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestObj = hit.gameObject;
            }
        }

        return closestObj;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestGrab(NetworkId objectId)
    {
        // Resolve the NetworkObject from its ID on the host
        if (!Runner.TryFindObject(objectId, out NetworkObject netObj))
        {
            Debug.LogWarning("[Grabbing] Could not find object to grab on host");
            return;
        }

        GameObject obj = netObj.gameObject;

        if (!isLeftHandGrabbing)
        {
            _leftHandFull = obj;
            AttachToSlot(obj, leftArmTransform);
            isLeftHandGrabbing = true; 
            Debug.Log($"[Grabbing] Left hand picked up: {obj.name}");
        }
        else if (!isRightHandGrabbing)
        {
            _rightHandFull = obj;
            AttachToSlot(obj, rightArmTransform);
            isRightHandGrabbing = true; 
            Debug.Log($"[Grabbing] Right hand picked up: {obj.name}");
        }
        else
        {
            Debug.Log("[Grabbing] Both hands full!");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestDrop()
    {
        if (isLeftHandGrabbing)
        {
            DropSingleObject(ref _leftHandFull);
            isLeftHandGrabbing = false;
        }
        if (isRightHandGrabbing)
        {
            DropSingleObject(ref _rightHandFull);
            isRightHandGrabbing = false;
        }
    }

    private void AttachToSlot(GameObject obj, Transform slot)
    {
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    private void DropSingleObject(ref GameObject obj)
    {
        if (obj == null) return;

        obj.transform.SetParent(null);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }

        Debug.Log($"[Grabbing] Dropped: {obj.name}");
        obj = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRange);
    }
}
