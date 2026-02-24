using Fusion;
using Network_Scripts;
using UnityEngine;

public class playerGrabbing : NetworkBehaviour
{
    public GameObject leftArm, rightArm;

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData input))
        {
            RaiseLeftArm(input);
            RaiseRightArm(input);
        }
    }

    void RaiseLeftArm(NetworkInputData input)
    {
        Debug.Log("RaiseLeftArm1");
        if (input.leftArmRaise)
        {
            Debug.Log("RaiseLeftArm2");
            leftArm.SetActive(true);

            if (input.leftGrab)
            {
                Debug.Log("RaiseLeftArm3");
                //left hand grab (detect grabbable object via layers and triggers near hand) 
            }
        }
        else
        {
            leftArm.SetActive(false);
        }
    }

    void RaiseRightArm(NetworkInputData input)
    {
        if (input.rightArmRaise)
        {
            rightArm.SetActive(true);

            if (input.rightGrab)
            {
                //right hand grab
            }
        }
        else
        {
            rightArm.SetActive(false);
        }
    }
}
