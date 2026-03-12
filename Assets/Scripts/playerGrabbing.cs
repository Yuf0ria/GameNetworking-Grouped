using System;
using Fusion;
using Network_Scripts;
using UnityEngine;
/// <summary>
/// What I added:
/// - if left arm has an item, E - interact should be grabbed by the right
/// - 
/// </summary>
public class playerGrabbing : NetworkBehaviour
{
    public GameObject leftArm, rightArm;


    private void Start()
    {
        leftArm.SetActive(false);
        rightArm.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        // if(GetInput(out NetworkInputData input))
        // {
        //     RaiseLeftArm(input);
        //     RaiseRightArm(input);
        // }
        
        //show arm when E key is interacted
    }
    
    void PicupObject(NetworkInputData input)
    {
        //priority, left hand grabs then right hand
        if (leftArm == null || rightArm == null)
        {
            
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
