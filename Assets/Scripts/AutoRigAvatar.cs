using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Reflection;
using UnityEditor;
using System.IO;
using AvatarUtilities;
using RootMotion.FinalIK;
using RootMotion.Demos;

[ExecuteInEditMode]
public class AutoRigAvatar : MonoBehaviour
{
    [SerializeField] private bool bipedMapped = false;
    public enum IKSolver {FinalIK, UnityXR};

    public void IKSetupChooser(IKSolver ikSolver, GameObject g)
    {
        switch (ikSolver){
        case IKSolver.FinalIK:
        if(g.GetComponent(typeof(VRIK)) == null)
        {
            var obj = FinalIKSetup(g.gameObject);
            if(g.GetComponent(typeof(VRIKCalibrationController)) == null)
            {
                var cal = g.AddComponent<VRIKCalibrationController>();
                cal.ik = g.GetComponent<VRIK>();
            }

        }
        
        break;
        case IKSolver.UnityXR: 
        if(g.gameObject.GetComponent(typeof(RigBuilder)) == null)
        {
            var obj = AddIKConstraints(g);
            obj.transform.SetParent(g.transform);
        } 
        break;
        default:
            throw new ArgumentOutOfRangeException(nameof(ikSolver), ikSolver, null);
        }
        

        
    }

    private static GameObject FinalIKSetup(GameObject avatarBase)
    {
        var vrik = avatarBase.AddComponent<VRIK>();
        vrik.AutoDetectReferences();
        vrik.GuessHandOrientations();
        
        var rWrist = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Wrist");
        var lWrist = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Wrist");
        if (BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Wrist") == null) return avatarBase;
        var twistRelaxerRight = rWrist.gameObject.AddComponent<TwistRelaxer>();

        var twistSolverWristRight = new TwistSolver
        {
            transform = rWrist
        };
        var twistSolverForearmRight = new TwistSolver
        {
            transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Forearm")
        };
        var twistSolverUpperArmRight = new TwistSolver
        {
            transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R UpperArm")
        };

        twistRelaxerRight.twistSolvers = new TwistSolver[] {twistSolverWristRight, twistSolverForearmRight, twistSolverUpperArmRight};
        twistRelaxerRight.ik = vrik;

        var twistRelaxerLeft = lWrist.gameObject.AddComponent<TwistRelaxer>();
        var twistSolverWristLeft = new TwistSolver
        {
            transform = lWrist
        };
        var twistSolverForearmLeft = new TwistSolver
        {
            transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Forearm")
        };
        var twistSolverUpperArmLeft = new TwistSolver
        {
            transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L UpperArm")
        };

        twistRelaxerLeft.twistSolvers = new TwistSolver[] {twistSolverWristLeft, twistSolverForearmLeft, twistSolverUpperArmLeft};
        twistRelaxerLeft.ik = vrik;


        return avatarBase;
    }

    private GameObject AddIKConstraints(GameObject avatarBase){
        
        
        var baseTransform = avatarBase.transform;
        var rigBuilder = avatarBase.AddComponent<RigBuilder>();

        var head = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 Head");
        
        var upperarmL = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 L Upperarm");
        var upperarmR = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 R Upperarm");

        var forearmL = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 L Forearm");
        var forearmR = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 R Forearm");
        
        var handL = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 L Hand");
        var handR = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 R Hand");

        var thighL = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 L Thigh");
        var thighR = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 R Thigh");

        var calfL = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 L Calf");
        var calfR = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 R Calf");

        var footL = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 L Foot");
        var footR = BoneUtilities.SearchHierarchyForBone(baseTransform, "Bip01 R Foot");

        var constraintsRoot = new GameObject("ikConstraints");
        var rig = constraintsRoot.AddComponent<Rig>();
        var constraintsRootTransform = constraintsRoot.transform;
        
        rigBuilder.layers.Add(new RigLayer(rig, true));
        
        var forearmConstraintLeft = ArmIK("left_forearm", upperarmL.gameObject, forearmL.gameObject, handL.gameObject);
        var forearmConstraintRight = ArmIK("right_forearm", upperarmR.gameObject, forearmR.gameObject, handR.gameObject);

        forearmConstraintLeft.transform.SetParent(constraintsRootTransform);
        forearmConstraintRight.transform.SetParent(constraintsRootTransform);

        var handConstraintLeft = SetupTwoBoneIK("left_hand", upperarmL.gameObject, forearmL.gameObject, handL.gameObject);
        var handConstraintRight = SetupTwoBoneIK("right_hand", upperarmR.gameObject, forearmR.gameObject, handR.gameObject);

        handConstraintLeft.transform.SetParent(constraintsRootTransform);
        handConstraintRight.transform.SetParent(constraintsRootTransform);

        var footConstraintLeft = SetupTwoBoneIK("left_foot", thighL.gameObject, calfL.gameObject, footL.gameObject);
        var footConstraintRight = SetupTwoBoneIK("right_foot", thighR.gameObject, calfR.gameObject, footR.gameObject);

        footConstraintLeft.transform.SetParent(constraintsRootTransform);
        footConstraintRight.transform.SetParent(constraintsRootTransform);

        var headConstraint = HeadIK(head.gameObject);

        headConstraint.transform.SetParent(constraintsRootTransform);

        return constraintsRoot;
    }

    private GameObject SetupTwoBoneIK(string targetname, GameObject root, GameObject mid, GameObject tip)
    {
        
        GameObject hand = new (targetname);


        //Twobone IK needs a target to aim for, and a hint to assist bending of the limb
        GameObject target = new (targetname + "_target");
        GameObject hint = new (targetname + "_hint");

        target.transform.SetParent(hand.transform);
        hint.transform.SetParent(hand.transform);
        

        
        var twoboneIK = hand.AddComponent<TwoBoneIKConstraint>();
        
        var midtransform = mid.transform;
        var midposition = midtransform.position;
        
        twoboneIK.data.tip = tip.transform;
        twoboneIK.data.mid = mid.transform;
        twoboneIK.data.root = root.transform;
        twoboneIK.data.target = target.transform;
        twoboneIK.data.hint = hint.transform;
        


        target.transform.position = tip.transform.position;
        target.transform.rotation = tip.transform.rotation;

        //Hint should be placed where limb should bend

        if(targetname.ToLower().Contains("foot")){
            hint.transform.position = new Vector3(midposition.x, midposition.y+.5f, midposition.z + 1f);
        } else if(targetname.ToLower().Contains("hand"))
        {
            hint.transform.position = new Vector3(midposition.x, midposition.y, midposition.z - .1f);
        }
        

        return hand;
    }

    //Adds a multi-rotational constraint to the forearm such that it matches the x rotation of the hand, ensuring that the wrist does not deform during reaches.

    private static GameObject ArmIK(string objname, GameObject upperarm, GameObject forearm, GameObject hand)
    {
        GameObject forearmIK = new (objname);

        var armIK = forearmIK.AddComponent<TwistCorrection>();

        armIK.data.sourceObject = hand.transform;
        var transforms = new WeightedTransformArray
        {
            new (forearm.transform, .75f),
            new (upperarm.transform, .25f)
        };
        armIK.data.twistNodes = transforms;

        armIK.data.twistAxis = TwistCorrectionData.Axis.X;

        return forearmIK;
    }

    private static GameObject HeadIK(GameObject headbone)
    {
        GameObject head = new ("head");
        GameObject headTarget = new ("head_target");
        headTarget.transform.SetParent(head.transform);
        headTarget.transform.position = headbone.transform.position;
        headTarget.transform.rotation = headbone.transform.rotation;
        

        var headIK = head.AddComponent<MultiParentConstraint>();
        headIK.data.constrainedObject = headbone.transform;
        
        var transforms = new WeightedTransformArray { new (headTarget.transform, 1) };

        headIK.data.sourceObjects = transforms;

        return head;
    }

}
