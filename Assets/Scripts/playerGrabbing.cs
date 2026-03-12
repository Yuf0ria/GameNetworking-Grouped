using Fusion;
using Network_Scripts;
using UnityEngine;

/// <summary>
/// What I added:
/// - if left arm has an item, E - interact should be grabbed by the right
/// </summary>
public class playerGrabbing : NetworkBehaviour
{
    [Header("Player Arms")] 
    public GameObject leftArm;
    public GameObject rightArm;

    [Header("Player Arms Transforms")] 
    [SerializeField] private Transform leftArmTransform;
    [SerializeField] private Transform rightArmTransform;

    [Header("Hand Slots")] 
    [SerializeField] private GameObject _leftHandFull;
    [SerializeField] private GameObject _rightHandFull;
    [Header("Grab Settings")]
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private LayerMask GrabLayerMask;
    private NetworkBool isLeftHandGrabbing { get; set; }
    private NetworkBool isRightHandGrabbing { get; set; }

    private void Start()
    {
        leftArm.SetActive(false);
        rightArm.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        leftArm.SetActive(isLeftHandGrabbing);
        rightArm.SetActive(isRightHandGrabbing);

        if (GetInput(out NetworkInputData input))
        {
            if (input.interact)
            {
                PickupObjsCollider();
            }
        }
    }

    void PickupObjsCollider()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange, GrabLayerMask);
        if (hits.Length == 0) return;

        GameObject closestObj = ClosestObj(hits);
        if (closestObj == null) return;

        NetworkObject netObj = closestObj.GetComponent<NetworkObject>();
        if (netObj != null)
            RPC_RequestGrab(netObj.Id);
        else
            Debug.LogWarning($"[Grabbing] {closestObj.name} has no NetworkObject — can't grab over network");
    }

    private GameObject ClosestObj(Collider[] hits)
    {
        GameObject closestObj = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
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
            isLeftHandGrabbing = true; // ✅ Host sets this — safe
            Debug.Log($"[Grabbing] Left hand picked up: {obj.name}");
        }
        else if (!isRightHandGrabbing)
        {
            _rightHandFull = obj;
            AttachToSlot(obj, rightArmTransform);
            isRightHandGrabbing = true; // ✅ Host sets this — safe
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
        DropSingleObject(ref _leftHandFull);
        isLeftHandGrabbing = false; // ✅ Host sets this — safe

        DropSingleObject(ref _rightHandFull);
        isRightHandGrabbing = false; // ✅ Host sets this — safe
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
