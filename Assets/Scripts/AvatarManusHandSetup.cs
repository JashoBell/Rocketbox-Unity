using System.Collections;
using System.Collections.Generic;
using AvatarUtilities;
using UnityEngine;
using Manus.Hand;

[ExecuteInEditMode]
public class AvatarManusHandSetup : MonoBehaviour
{
    private void Awake()
    {
        
        if (!(GameObject.Find("right_hand_tracker") != null & GameObject.Find("left_hand_tracker") != null)) return;
        
        var g = this.gameObject;
        var rHand = FindHand(g, "right");
        var lHand = FindHand(g, "left");
        var rHandTarget = GameObject.Find("right_hand_tracker");
        var lHandTarget = GameObject.Find("left_hand_tracker");

        if(lHandTarget.GetComponent(typeof(Hand)) == null)
        {
            lHandTarget = AttachManusHand(lHandTarget, "left");
        }
        if(rHandTarget.GetComponent(typeof(Hand)) == null)
        {
            rHandTarget = AttachManusHand(rHandTarget, "right");
        }

        if(lHand.GetComponent(typeof(HandAnimator)) == null)
        {
            lHand = AttachManusHandAnimator(lHand, "left");
        } 
        if(rHand.GetComponent(typeof(HandAnimator)) == null)
        {
            rHand = AttachManusHandAnimator(rHand, "right");
        }



    }

    private static GameObject FindHand(GameObject root, string side)
    {
        
        var handL = BoneUtilities.SearchHierarchyForBone(root.transform, "Bip01 L Hand");
        var handR = BoneUtilities.SearchHierarchyForBone(root.transform, "Bip01 R Hand");

        return side == "right" ? handR.gameObject : handL.gameObject;
 
    }

    private static GameObject AttachManusHandAnimator(GameObject hand, string side)
    {
        var handAnimator = hand.AddComponent<HandAnimator>();

        switch (side)
        {
            case "right":
                handAnimator.handModelType = Manus.Utility.HandType.RightHand;
                break;
            case "left":
                handAnimator.handModelType = Manus.Utility.HandType.LeftHand;
                break;
        }

        handAnimator.FindFingers();
        handAnimator.CalculateAxes();
        handAnimator.SetDefaultLimits();

        return hand;
    }

    private static GameObject AttachManusHand(GameObject hand, string side)
    {
        var handComponent = hand.AddComponent<Hand>();

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

        return hand;
    }
}
