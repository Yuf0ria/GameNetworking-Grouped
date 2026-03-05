using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Network_Scripts
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 movementInput;
        public Vector2 mouseInput;
        public NetworkBool isSprinting;
        public NetworkBool isJumping;
        public NetworkBool interact;
        
        public NetworkBool leftArmRaise;
        public NetworkBool rightArmRaise;
        public NetworkBool leftGrab;
        public NetworkBool rightGrab;
    }
}