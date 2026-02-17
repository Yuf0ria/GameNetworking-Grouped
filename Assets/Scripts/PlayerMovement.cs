using UnityEngine;
using Fusion;
using Network_Scripts; //This is where the network input data is located, in the editor > assets > scritps > network

public class PlayerMovement : NetworkBehaviour
{
    //Changeable
    private float _speed = 3; 
    //DO NOT CHANGE
    private Vector3 _velocity;
    private const float Sensitivity = 1;
    private const float Gravity = 9.81f;
    private const float JumpForce = 5f;

    [Header("Components Needed")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController controller;

    #region Network Properties
        /* <summary>
            this sets up the local positions to the fusion 2 network, do not change the public variable to private,
            I think I called these variables in other scripts as well - dani, 19:21 | Feb 17
         </summary>
         */
        [Networked] public Vector3 n_PlayerPos { get; set; }
        [Networked] public Quaternion n_PlayerRotate { get; set; }
        [Networked] private float _xrotation { get; set; }
    #endregion

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            //when the player controls the camera, the cursor is not active
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            /* This checks for the player's camera, the camera should only follow the local player, but since
             * but we're only using ONE player prefab so if the remote player's camera is also in the world this makes
             * sure their camera and audio listener is turned off. --dani
             */
            if (playerCamera != null) //LocalPlayer
            {
                playerCamera.gameObject.SetActive(true);
                Camera yourCam = playerCamera.GetComponent<Camera>();
                if (yourCam != null) yourCam.enabled = true;
                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }
            Debug.Log($"Your player has joined the room: {Object.InputAuthority}");
        }
        else //RemotePlayer
        {
            if (playerCamera != null)
            {
                Camera notYourCam = playerCamera.GetComponent<Camera>();
                if (notYourCam != null) notYourCam.enabled = false;
                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }

            Debug.Log($"another player has spawned: {Object.InputAuthority}");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            //uncomment if player is not moving
            // Debug.Log($"Input received: move={input.movementInput}, sprint={input.isSprinting}"); // remove once confirmed
            MovePlayer(input);
            RotatePlayer(input);
            RotateCamera(input);

            n_PlayerPos = transform.position;
            n_PlayerRotate = transform.rotation;
            _xrotation = playerCamera.localEulerAngles.x;
        }
        else
        {
            Debug.LogWarning("GetInput returned false"); // this firing every tick = the real problem
        }

        IsGrounded();
    }

    private void Update()
    {
        // Only interpolate remote players
        if (HasInputAuthority) return;

        // Smoothly move remote players to their networked position
        transform.position = Vector3.Lerp(
            transform.position,
            n_PlayerPos,
            Time.deltaTime * 15f
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            n_PlayerRotate,
            Time.deltaTime * 15f
        );

        // Sync camera rotation for remote players
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Lerp(
                playerCamera.localRotation,
                Quaternion.Euler(_xrotation, 0f, 0f),
                Time.deltaTime * 15f
            );
        }
    }

    private void IsGrounded()
    {
        if (!HasInputAuthority) return;
    }

    private void MovePlayer(NetworkInputData input)
    {
        Vector3 moveDirection = transform.right * input.movementInput.x
            + transform.forward * input.movementInput.z;

        _speed = input.isSprinting ? 9f : 3f;

        if (controller.isGrounded)
        {
            _velocity.y = -2f;
            if (input.isJumping)
                _velocity.y = JumpForce;
        }
        else
        {
            _velocity.y -= Gravity * 2f * Runner.DeltaTime;
        }

        Vector3 finalMove = (moveDirection * _speed) + new Vector3(0, _velocity.y, 0);
        controller.Move(finalMove * Runner.DeltaTime);
    }

    private void RotatePlayer(NetworkInputData input)
    {
        transform.Rotate(0f, input.mouseInput.x * Sensitivity, 0f);
    }

    private void RotateCamera(NetworkInputData input)
    {
        if (playerCamera == null) return;

        float currentXRotation = playerCamera.localEulerAngles.x;

        if (currentXRotation > 180f)
            currentXRotation -= 360f;

        currentXRotation -= input.mouseInput.y * Sensitivity;
        currentXRotation = Mathf.Clamp(currentXRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
    }

    #region AudioSource
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_PlayFootstep(Vector3 position)
        {
            // AudioSource.PlayClipAtPoint(footstepSound, position);
        }
    #endregion
}