using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manus.Hand;

[ExecuteInEditMode]
public class AvatarManusHandSetup : MonoBehaviour
{
    private void Awake() {
        var g = this.gameObject;
        var r_hand = findHand(g, "right");
        var l_hand = findHand(g, "left");
        var r_hand_target = GameObject.Find("right_hand_tracker");
        var l_hand_target = GameObject.Find("left_hand_tracker");

        if(l_hand_target.GetComponent(typeof(Hand)) == null)
        {
            l_hand_target = AttachManusHand(l_hand_target, "left");
        }
        if(r_hand_target.GetComponent(typeof(Hand)) == null)
        {
            r_hand_target = AttachManusHand(r_hand_target, "right");
        }

        if(l_hand.GetComponent(typeof(HandAnimator)) == null)
        {
            l_hand = AttachManusHandAnimator(l_hand, "left");
        } 
        if(r_hand.GetComponent(typeof(HandAnimator)) == null)
        {
            r_hand = AttachManusHandAnimator(r_hand, "right");
        }


    }

    public GameObject findHand(GameObject root, string side)
    {
        var spine = root.transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2");
        var head = spine.transform.Find("Bip01 Neck").Find("Bip01 Head");
        
        var upperarm_l = spine.transform.Find("Bip01 L Clavicle").Find("Bip01 L UpperArm");
        var upperarm_r = spine.transform.Find("Bip01 R Clavicle").Find("Bip01 R UpperArm");

        var forearm_l = upperarm_l.transform.Find("Bip01 L Forearm");
        var forearm_r = upperarm_r.transform.Find("Bip01 R Forearm");
        
        var hand_l = forearm_l.transform.Find("Bip01 L Hand");
        var hand_r = forearm_r.transform.Find("Bip01 R Hand");

        if(side == "right")
        {
            return hand_r.gameObject;
        } else
        {
            return hand_l.gameObject;
        }
 
    }

    public GameObject AttachManusHandAnimator(GameObject hand_, string side)
    {
        GameObject hand = hand_;
        
        var hand_animator = hand.AddComponent<HandAnimator>();

        if (side == "right")
        {
            hand_animator.handModelType = Manus.Utility.HandType.RightHand;
        }
        else if(side == "left")
        {
            hand_animator.handModelType = Manus.Utility.HandType.LeftHand;
        }

        hand_animator.FindFingers();
        hand_animator.CalculateAxes();
        hand_animator.SetDefaultLimits();

        return hand;
    }
    
    public GameObject AttachManusHand(GameObject hand_target_, string side)
    {
        GameObject hand_target = hand_target_;

        var hand_component = hand_target.AddComponent<Hand>();

        if (side == "right")
        {
            hand_component.type = Manus.Utility.HandType.RightHand;
            hand_component.rotationOffset = new Vector3(180, -90, -90);
        }
        else if(side == "left")
        {
            hand_component.type = Manus.Utility.HandType.LeftHand;
            hand_component.rotationOffset = new Vector3(0, -90, -90);
        }

        return hand_target;
    }
}
