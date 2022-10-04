using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manus.Hand;

public class AvatarManusHandSetup : MonoBehaviour
{
    public void ImportSetup()
    {

        var g = this.gameObject;
        var rHand = FindHand(g, "right");
        var lHand = FindHand(g, "left");


        if(lHand.GetComponent(typeof(HandAnimator)) == null)
        {
            AttachManusHandAnimator(lHand, "left");
        } 
        if(rHand.GetComponent(typeof(HandAnimator)) == null)
        {
            AttachManusHandAnimator(rHand, "right");
        }
    }


    public void AttachHands()
    {
        if (!(GameObject.Find("right_hand_tracker") != null & GameObject.Find("left_hand_tracker") != null)) return;
        var rHandTarget = GameObject.Find("right_hand_tracker");
        var lHandTarget = GameObject.Find("left_hand_tracker");

        if(lHandTarget.GetComponent(typeof(Hand)) == null)
        {
            AttachManusHand(lHandTarget, "left");
        }
        if(rHandTarget.GetComponent(typeof(Hand)) == null)
        {
            AttachManusHand(rHandTarget, "right");
        }
    }


    public GameObject FindHand(GameObject root, string side)
    {
        
        var handL = BoneUtilities.SearchHierarchyForBone(root.transform, "Bip01 L Hand");
        var handR = BoneUtilities.SearchHierarchyForBone(root.transform, "Bip01 R Hand");

        return side == "right" ? handR.gameObject : handL.gameObject;
 
    }


    public GameObject AttachManusHandAnimator(GameObject hand_, string side)
    {
        GameObject hand = hand_;
        
        var handAnimator = hand.AddComponent<HandAnimator>();

        handAnimator.handModelType = side switch
        {
            "right" => Manus.Utility.HandType.RightHand,
            "left" => Manus.Utility.HandType.LeftHand,
            _ => handAnimator.handModelType
        };

        handAnimator.FindFingers();
        handAnimator.CalculateAxes();
        handAnimator.SetDefaultLimits();

        return hand;
    }
    

    public GameObject AttachManusHand(GameObject handTarget, string side)
    {
        
        var handComponent = handTarget.AddComponent<Hand>();

        switch (side)
        {
            case "right":
                handComponent.type = Manus.Utility.HandType.RightHand;
                handComponent.rotationOffset = new Vector3(0, 90, 90);
                break;
            case "left":
                handComponent.type = Manus.Utility.HandType.LeftHand;
                handComponent.rotationOffset = new Vector3(0, 90, 90);
                break;
        }

        return handTarget;
    }
}
