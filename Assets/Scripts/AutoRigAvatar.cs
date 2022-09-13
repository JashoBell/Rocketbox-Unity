using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Reflection;
using UnityEditor;
using System.IO;
using RootMotion.FinalIK;
using RootMotion.Demos;

[ExecuteInEditMode]
public class AutoRigAvatar : MonoBehaviour
{
    [SerializeField] bool bipedMapped = false;
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
        }
        

        
    }

    public GameObject FinalIKSetup(GameObject avatarBase)
    {
        var vrik = avatarBase.AddComponent<VRIK>();
        vrik.AutoDetectReferences();
        vrik.GuessHandOrientations();
        
        var rWrist = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Wrist");
        var lWrist = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Wrist");
        if(BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Wrist") != null)
        {
            var twistRelaxerRight = rWrist.gameObject.AddComponent<TwistRelaxer>();

            var twistSolverWristRight = new TwistSolver();
            twistSolverWristRight.transform = rWrist;
            var twistSolverForearmRight = new TwistSolver();
            twistSolverForearmRight.transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Forearm");
            var twistSolverUpperArmRight = new TwistSolver();
            twistSolverUpperArmRight.transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R UpperArm");

            twistRelaxerRight.twistSolvers = new TwistSolver[] {twistSolverWristRight, twistSolverForearmRight, twistSolverUpperArmRight};
            twistRelaxerRight.ik = vrik;

            var twistRelaxerLeft = lWrist.gameObject.AddComponent<TwistRelaxer>();
            var twistSolverWristLeft = new TwistSolver();
            twistSolverWristLeft.transform = lWrist;
            var twistSolverForearmLeft = new TwistSolver();
            twistSolverForearmLeft.transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Forearm");
            var twistSolverUpperArmLeft = new TwistSolver();
            twistSolverUpperArmLeft.transform = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L UpperArm");

            twistRelaxerLeft.twistSolvers = new TwistSolver[] {twistSolverWristLeft, twistSolverForearmLeft, twistSolverUpperArmLeft};
            twistRelaxerLeft.ik = vrik;
        }


        return avatarBase;
    }

    private GameObject AddIKConstraints(GameObject avatarBase){
        
        

        var rigBuilder = avatarBase.AddComponent<RigBuilder>();

        var head = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 Head");
        
        var upperarmL = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Upperarm");
        var upperarmR = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Upperarm");

        var forearmL = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Forearm");
        var forearmR = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Forearm");
        
        var handL = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Hand");
        var handR = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Hand");

        var thighL = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Thigh");
        var thighR = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Thigh");

        var calfL = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Calf");
        var calfR = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Calf");

        var footL = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 L Foot");
        var footR = BoneUtilities.SearchHierarchyForBone(avatarBase.transform, "Bip01 R Foot");

        var constraintsRoot = new GameObject("ikConstraints");
        var rig = constraintsRoot.AddComponent<Rig>();

        rigBuilder.layers.Add(new RigLayer(rig, true));
        
        var forearmConstraintLeft = ArmIK("left_forearm", upperarmL.gameObject, forearmL.gameObject, handL.gameObject);
        var forearmConstraintRight = ArmIK("right_forearm", upperarmR.gameObject, forearmR.gameObject, handR.gameObject);

        forearmConstraintLeft.transform.SetParent(constraintsRoot.transform);
        forearmConstraintRight.transform.SetParent(constraintsRoot.transform);

        var handConstraintLeft = SetupTwoBoneIK("left_hand", upperarmL.gameObject, forearmL.gameObject, handL.gameObject);
        var handConstraintRight = SetupTwoBoneIK("right_hand", upperarmR.gameObject, forearmR.gameObject, handR.gameObject);

        handConstraintLeft.transform.SetParent(constraintsRoot.transform);
        handConstraintRight.transform.SetParent(constraintsRoot.transform);

        var footConstraintLeft = SetupTwoBoneIK("left_foot", thighL.gameObject, calfL.gameObject, footL.gameObject);
        var footConstraintRight = SetupTwoBoneIK("right_foot", thighR.gameObject, calfR.gameObject, footR.gameObject);

        footConstraintLeft.transform.SetParent(constraintsRoot.transform);
        footConstraintRight.transform.SetParent(constraintsRoot.transform);

        var headConstraint = HeadIK(head.gameObject);

        headConstraint.transform.SetParent(constraintsRoot.transform);

        return constraintsRoot;
    }

    private GameObject SetupTwoBoneIK(string name, GameObject root, GameObject mid, GameObject tip)
    {
        
        GameObject hand = new GameObject(name);
        
        var twoboneIK = hand.AddComponent<TwoBoneIKConstraint>();

        twoboneIK.data.tip = tip.transform;
        twoboneIK.data.mid = mid.transform;
        twoboneIK.data.root = root.transform;

        //Twobone IK needs a target to aim for, and a hint to assist bending of the limb

        var target = new GameObject(name + "_target");
        var hint = new GameObject(name + "_hint");

        target.transform.SetParent(hand.transform);
        hint.transform.SetParent(hand.transform);

        twoboneIK.data.target = target.transform;
        twoboneIK.data.hint = hint.transform;

        target.transform.position = tip.transform.position;
        target.transform.rotation = tip.transform.rotation;

        //Hint should be placed where limb should bend

        if(name.ToLower().Contains("foot")){
            hint.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y+.5f, mid.transform.position.z + 1f);
        } else if(name.ToLower().Contains("hand"))
        {
            hint.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y, mid.transform.position.z - .1f);
        }
        

        return hand;
    }

    //Adds a multi-rotational constraint to the forearm such that it matches the x rotation of the hand, ensuring that the wrist does not deform during reaches.

    private GameObject ArmIK(string name, GameObject upperarm, GameObject forearm, GameObject hand)
    {
        GameObject _forearm = new GameObject(name);

        var armIK = forearm.AddComponent<TwistCorrection>();

        armIK.data.sourceObject = hand.transform;
        var transforms = new WeightedTransformArray
        {
            new WeightedTransform(forearm.transform, .75f),
            new WeightedTransform(upperarm.transform, .25f)
        };
        armIK.data.twistNodes = transforms;

        armIK.data.twistAxis = TwistCorrectionData.Axis.X;

        return forearm;
    }

    private GameObject HeadIK(GameObject headbone)
    {
        GameObject head = new GameObject("head");
        GameObject headTarget = new GameObject("head_target");
        headTarget.transform.SetParent(head.transform);
        headTarget.transform.position = headbone.transform.position;
        headTarget.transform.rotation = headbone.transform.rotation;
        

        var headIK = head.AddComponent<MultiParentConstraint>();
        headIK.data.constrainedObject = headbone.transform;
        
        var transforms = new WeightedTransformArray
        {
            new WeightedTransform(headTarget.transform, 1)
        };

        headIK.data.sourceObjects = transforms;

        return head;
    }

}
