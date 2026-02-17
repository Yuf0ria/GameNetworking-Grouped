using Fusion;
using UnityEngine;

namespace Network_Scripts
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 movementInput;
        public Vector2 mouseInput;
        public NetworkBool isSprinting;
        public NetworkBool isJumping;
    }
}