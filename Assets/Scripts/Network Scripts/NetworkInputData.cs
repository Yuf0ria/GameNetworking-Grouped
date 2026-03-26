using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Network_Scripts
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 movementInput; //WASD
        public Vector2 mouseInput; //CAMERA MOVEMENT
        public NetworkBool isSprinting; //Runs
        public NetworkBool isJumping; //Jumps
        public NetworkBool interact; //E to interact
        public NetworkBool drop;
    }
}