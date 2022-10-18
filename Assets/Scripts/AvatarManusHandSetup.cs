using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using UnityEngine;
using Manus;
using Manus.Skeletons;

public class AvatarManusHandSetup : MonoBehaviour
{
    public void ImportSetup()
    {

        var g = this.gameObject;
        var rHand = FindHand(g, "right");
        var lHand = FindHand(g, "left");

        
        if(lHand.GetComponent(typeof(Skeleton)) == null)
        {
            AttachManusHandAnimator(lHand, "left");
        } 
        if(rHand.GetComponent(typeof(Skeleton)) == null)
        {
            AttachManusHandAnimator(rHand, "right");
        } 
        
    }


    public void AttachHands()
    {
        /*if (!(GameObject.Find("right_hand_tracker") != null & GameObject.Find("left_hand_tracker") != null)) return;
        var rHandTarget = GameObject.Find("right_hand_tracker");
        var lHandTarget = GameObject.Find("left_hand_tracker");

        if(lHandTarget.GetComponent(typeof(Hand)) == null)
        {
            AttachManusHand(lHandTarget, "left");
        }
        if(rHandTarget.GetComponent(typeof(Hand)) == null)
        {
            AttachManusHand(rHandTarget, "right");
        }*/
    }


    public GameObject FindHand(GameObject root, string side)
    {
        
        var handL = BoneUtilities.SearchHierarchyForBone(root.transform, "Bip01 L Hand");
        var handR = BoneUtilities.SearchHierarchyForBone(root.transform, "Bip01 R Hand");

        return side == "right" ? handR.gameObject : handL.gameObject;
 
    }

    private Chain CreateChain(CoreSDK.ChainType type, CoreSDK.Side side)
    {
        
        
        List<uint> nodes = type switch
        {
            CoreSDK.ChainType.Hand => new List<uint>() {0},
            CoreSDK.ChainType.FingerThumb => new List<uint> { 1, 2, 3 },
            CoreSDK.ChainType.FingerIndex => new List<uint> { 4, 5, 6 },
            CoreSDK.ChainType.FingerMiddle => new List<uint> { 7, 8, 9 },
            CoreSDK.ChainType.FingerRing => new List<uint> { 10, 11, 12 },
            CoreSDK.ChainType.FingerPinky => new List<uint> { 13, 14, 15 },
            _ => new List<uint>()
        };
        uint id = type switch
        {
            CoreSDK.ChainType.Hand => 1,
            CoreSDK.ChainType.FingerThumb => 2,
            CoreSDK.ChainType.FingerIndex => 3,
            CoreSDK.ChainType.FingerMiddle => 4,
            CoreSDK.ChainType.FingerRing => 5,
            CoreSDK.ChainType.FingerPinky => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var chainSettings = FillChainSettings(new CoreSDK.ChainSettings(), type);
        
        var chain = new Chain
        {
            type = type,
            dataSide = side,
            appliedDataType = type,
            nodeIds = nodes,
            settings = chainSettings,
            dataIndex = 0,
            id = id,
        };
        
        chain.UpdateName();
        
        return chain;
    }
    
    private static CoreSDK.ChainSettings FillChainSettings(CoreSDK.ChainSettings settings, CoreSDK.ChainType type)
    {
        settings.usedSettings = type;
        switch (type)
        {
            case CoreSDK.ChainType.FingerThumb 
                 or CoreSDK.ChainType.FingerIndex 
                 or CoreSDK.ChainType.FingerMiddle
                 or CoreSDK.ChainType.FingerRing
                 or CoreSDK.ChainType.FingerPinky:
                settings.finger.handChainId = 1;
                break;
            case CoreSDK.ChainType.Hand:
                settings.hand.fingerChainIdsUsed = 5;
                settings.hand.fingerChainIds = new int[5] { 2, 3, 4, 5, 6 };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return settings;
    }
    
    
    public GameObject AttachManusHandAnimator(GameObject hand_, string side)
    {
        GameObject hand = hand_;
        
        
        var handAnimator = hand.AddComponent<Skeleton>();
        
        // Create chains for the hand and fingers, and set them up with sensible defaults.
        var chains = new List<Chain>();
        
        var handChains = new List<CoreSDK.ChainType>()
        {
            CoreSDK.ChainType.Hand,
            CoreSDK.ChainType.FingerThumb,
            CoreSDK.ChainType.FingerIndex,
            CoreSDK.ChainType.FingerMiddle,
            CoreSDK.ChainType.FingerRing,
            CoreSDK.ChainType.FingerPinky
        };

        foreach (var chainType in handChains)
        {
            var chain = CreateChain(chainType, side == "right" ? CoreSDK.Side.Right : CoreSDK.Side.Left);
            chains.Add(chain);
        }
        
        // Set up skeleton settings, create the skeletonData object to add to the Skeleton component, and set up nodes.
        var skeletonSettings = new CoreSDK.SkeletonSettings();
        skeletonSettings.targetType = CoreSDK.SkeletonTargetType.GloveData;
        skeletonSettings.skeletonTargetGloveData.id = (uint)(side == "right" ? 0 : 1);
        skeletonSettings.scaleToTarget = true;

        handAnimator.skeletonData = new SkeletonData
        {
            type = CoreSDK.SkeletonType.Hand,
            chains = chains,
            id = (uint)(side == "right" ? 1 : 2),
            settings = skeletonSettings
        };
        handAnimator.SetupNodes();

        return hand;
    }
    

    public GameObject AttachManusHand(GameObject handTarget, string side)
    {
        
        /*var handComponent = handTarget.AddComponent<Hand>();

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
        }*/

        return handTarget;
    }
}
