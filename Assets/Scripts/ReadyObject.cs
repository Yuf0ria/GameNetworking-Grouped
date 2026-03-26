/*
 * When a player presses Ctrl+R while looking at this object, the prep phase is skipped.
 */

using UnityEngine;

public class ReadyObject : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask Ring;

    private Camera _playerCamera;

    private void Update()
    {
        // Only the local player can trigger this
        if (_playerCamera == null)
        {
            // Find the active local camera
            _playerCamera = Camera.main;
            if (_playerCamera == null) return;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            TryTriggerReady();
        }
    }

    private void TryTriggerReady()
    {
        if (GameTimerManager.Instance == null) return;
        if (GameTimerManager.Instance.CurrentPhase != GamePhase.PrepPhase)
        {
            Debug.Log("[ReadyObject] Not in prep phase, Ctrl+R ignored.");
            return;
        }

        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, Ring))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("[ReadyObject] Ctrl+R confirmed — starting game!");
                GameTimerManager.Instance.ForceStartGame();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
