// About the Script:
// Used for sending input information to the APP Id
// To make this work you have to go to the Network Session Manager to make it work.
// Ps. Don't be afraid of the debug functions, it's just... there incase I need them
// --dani, March 6

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
    }
}