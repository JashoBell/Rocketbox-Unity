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
    public enum ikSolver {FinalIK, UnityXR};

    public void IKSetupChooser(ikSolver ikSolver, GameObject g)
    {
        switch (ikSolver){
        case ikSolver.FinalIK:
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
        case ikSolver.UnityXR: 
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
        return avatarBase;
    }

    private GameObject AddIKConstraints(GameObject avatarBase){
        
        

        var rigBuilder = avatarBase.AddComponent<RigBuilder>();

        var spine = avatarBase.transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2");
        var head = spine.transform.Find("Bip01 Neck").Find("Bip01 Head");
        
        var upperarm_l = spine.transform.Find("Bip01 L Clavicle").Find("Bip01 L UpperArm");
        var upperarm_r = spine.transform.Find("Bip01 R Clavicle").Find("Bip01 R UpperArm");

        var forearm_l = upperarm_l.transform.Find("Bip01 L Forearm");
        var forearm_r = upperarm_r.transform.Find("Bip01 R Forearm");
        
        var hand_l = forearm_l.transform.Find("Bip01 L Hand");
        var hand_r = forearm_r.transform.Find("Bip01 R Hand");

        var thigh_l = avatarBase.transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 L Thigh");
        var thigh_r = avatarBase.transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 R Thigh");
        var calf_l = thigh_l.Find("Bip01 L Calf");
        var calf_r = thigh_r.Find("Bip01 R Calf");
        var foot_l = calf_l.Find("Bip01 L Foot");
        var foot_r = calf_r.Find("Bip01 R Foot");

        var constraintsRoot = new GameObject("ikConstraints");
        var rig = constraintsRoot.AddComponent<Rig>();

        rigBuilder.layers.Add(new RigLayer(rig, true));
        
        var forearmConstraintLeft = ArmIK("left_forearm", upperarm_l.gameObject, forearm_l.gameObject, hand_l.gameObject);
        var forearmConstraintRight = ArmIK("right_forearm", upperarm_r.gameObject, forearm_r.gameObject, hand_r.gameObject);

        forearmConstraintLeft.transform.SetParent(constraintsRoot.transform);
        forearmConstraintRight.transform.SetParent(constraintsRoot.transform);

        var handConstraintLeft = SetupTwoBoneIK("left_hand", upperarm_l.gameObject, forearm_l.gameObject, hand_l.gameObject);
        var handConstraintRight = SetupTwoBoneIK("right_hand", upperarm_r.gameObject, forearm_r.gameObject, hand_r.gameObject);

        handConstraintLeft.transform.SetParent(constraintsRoot.transform);
        handConstraintRight.transform.SetParent(constraintsRoot.transform);

        var footConstraintLeft = SetupTwoBoneIK("left_foot", thigh_l.gameObject, calf_l.gameObject, foot_l.gameObject);
        var footConstraintRight = SetupTwoBoneIK("right_foot", thigh_r.gameObject, calf_r.gameObject, foot_r.gameObject);

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

        var target = new GameObject(name + "_target");
        var hint = new GameObject(name + "_hint");

        target.transform.SetParent(hand.transform);
        hint.transform.SetParent(hand.transform);

        twoboneIK.data.target = target.transform;
        twoboneIK.data.hint = hint.transform;

        target.transform.position = tip.transform.position;
        target.transform.rotation = tip.transform.rotation;

        if(name.ToLower().Contains("foot")){
            hint.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y+.5f, mid.transform.position.z+1f);
        } else if(name.ToLower().Contains("hand"))
        {
            hint.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y, mid.transform.position.z-.1f);
        }
        

        return hand;
    }

    //Adds a multi-rotational constraint to the forearm such that it matches the x rotation of the hand, ensuring that the wrist does not deform during reaches.

    private GameObject ArmIK(string name, GameObject upperarm_, GameObject forearm_, GameObject hand_)
    {
        GameObject forearm = new GameObject(name);

        var armIK = forearm.AddComponent<TwistCorrection>();

        armIK.data.sourceObject = hand_.transform;
        var transforms = new WeightedTransformArray();
        transforms.Add(new WeightedTransform(forearm_.transform, .75f));
        transforms.Add(new WeightedTransform(upperarm_.transform, .25f));
        armIK.data.twistNodes = transforms;

        armIK.data.twistAxis = TwistCorrectionData.Axis.X;

        return forearm;
    }

    private GameObject HeadIK(GameObject headbone)
    {
        GameObject head_ = new GameObject("head");
        GameObject head_target = new GameObject("head_target");
        head_target.transform.SetParent(head_.transform);
        head_target.transform.position = headbone.transform.position;
        head_target.transform.rotation = headbone.transform.rotation;
        

        var headIK = head_.AddComponent<MultiParentConstraint>();
        headIK.data.constrainedObject = headbone.transform;
        
        var transforms = new WeightedTransformArray();
        transforms.Add(new WeightedTransform(head_target.transform, 1));

        headIK.data.sourceObjects = transforms;

        return head_;
    }

}
