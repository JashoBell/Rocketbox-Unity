using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[ExecuteInEditMode]
public class AutoRigAvatar : MonoBehaviour
{
    private void Awake() {

        var g = this.gameObject;
        if(g.GetComponent(typeof(RigBuilder)) == null)
        {
            var obj = AddIKConstraints(g);
            obj.transform.SetParent(g.transform);
        } 
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
        
        var forearmConstraintLeft = ArmIK("left forearm", upperarm_l.gameObject, forearm_l.gameObject, hand_l.gameObject);
        var forearmConstraintRight = ArmIK("right forearm", upperarm_r.gameObject, forearm_r.gameObject, hand_r.gameObject);

        forearmConstraintLeft.transform.SetParent(constraintsRoot.transform);
        forearmConstraintRight.transform.SetParent(constraintsRoot.transform);

        var handConstraintLeft = SetupTwoBoneIK("left hand", upperarm_l.gameObject, forearm_l.gameObject, hand_l.gameObject);
        var handConstraintRight = SetupTwoBoneIK("right hand", upperarm_r.gameObject, forearm_r.gameObject, hand_r.gameObject);

        handConstraintLeft.transform.SetParent(constraintsRoot.transform);
        handConstraintRight.transform.SetParent(constraintsRoot.transform);

        var footConstraintLeft = SetupTwoBoneIK("left foot", thigh_l.gameObject, calf_l.gameObject, foot_l.gameObject);
        var footConstraintRight = SetupTwoBoneIK("right foot", thigh_r.gameObject, calf_r.gameObject, foot_r.gameObject);

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

    private GameObject ArmIK(string name, GameObject source_a, GameObject constrained, GameObject source_b)
    {
        GameObject forearm = new GameObject(name);

        var armIK = forearm.AddComponent<MultiRotationConstraint>();

        armIK.data.constrainedObject = constrained.transform;
        var transforms = new WeightedTransformArray();
        transforms.Add(new WeightedTransform(source_a.transform, 1));
        transforms.Add(new WeightedTransform(source_b.transform, 1));
        armIK.data.sourceObjects = transforms;

        armIK.data.constrainedXAxis = true;
        armIK.data.constrainedYAxis = false;
        armIK.data.constrainedZAxis = false;

        return forearm;
    }

    private GameObject HeadIK(GameObject headbone)
    {
        GameObject head_ = new GameObject("head");
        GameObject head_target = new GameObject("head_target");
        head_target.transform.SetParent(head_.transform);
        head_target.transform.position = headbone.transform.position;
        

        var headIK = head_.AddComponent<MultiParentConstraint>();
        headIK.data.constrainedObject = headbone.transform;
        
        var transforms = new WeightedTransformArray();
        transforms.Add(new WeightedTransform(head_target.transform, 1));

        headIK.data.sourceObjects = transforms;

        return head_;
    }

    private HumanDescription generateAvatarBoneMappings(GameObject g)
    {
        Dictionary<string, string> boneName = new System.Collections.Generic.Dictionary<string, string>();
        boneName["Chest"] = "Bip01 Spine1";
        boneName["UpperChest"] = "Bip01 Spine2";
        boneName["Head"] = "Bip01 Head";
        boneName["Neck"] = "Bip01 Neck";
        boneName["Hips"] = "Bip01 Pelvis";
        boneName["LeftToes"] = "Bip01 L Toe0";
        boneName["LeftFoot"] = "Bip01 L Foot";
        boneName["LeftHand"] = "Bip01 L Hand";
        boneName["LeftLowerArm"] = "Bip01 L Forearm";
        boneName["LeftLowerLeg"] = "Bip01 L Calf";
        boneName["LeftShoulder"] = "Bip01 L Clavicle";
        boneName["LeftUpperArm"] = "Bip01 L UpperArm";
        boneName["LeftUpperLeg"] = "Bip01 L Thigh";
        boneName["RightToes"] = "Bip01 R Toe0";
        boneName["RightFoot"] = "Bip01 R Foot";
        boneName["RightHand"] = "Bip01 R Hand";
        boneName["RightLowerArm"] = "Bip01 R Forearm";
        boneName["RightLowerLeg"] = "Bip01 R Calf";
        boneName["RightShoulder"] = "Bip01 R Clavicle";
        boneName["RightUpperArm"] = "Bip01 R UpperArm";
        boneName["RightUpperLeg"] = "Bip01 R Thigh";
        boneName["Spine"] = "Bip01 Spine";

        boneName["LeftThumbProximal"] = "Bip01 L Finger0";
        boneName["LeftThumbIntermediate"] = "Bip01 L Finger01";
        boneName["LeftThumbDistal"] = "Bip01 L Finger02";
        boneName["LeftIndexProximal"] = "Bip01 L Finger1";
        boneName["LeftIndexIntermediate"] = "Bip01 L Finger11";
        boneName["LeftIndexDistal"] = "Bip01 L Finger12";
        boneName["LeftMiddleProximal"] = "Bip01 L Finger2";
        boneName["LeftMiddleIntermediate"] = "Bip01 L Finger21";
        boneName["LeftMiddleDistal"] = "Bip01 L Finger22";
        boneName["LeftRingProximal"] = "Bip01 L Finger3";
        boneName["LeftRingIntermediate"] = "Bip01 L Finger31";
        boneName["LeftRingDistal"] = "Bip01 L Finger32";
        boneName["LeftLittleProximal"] = "Bip01 L Finger4";
        boneName["LeftLittleIntermediate"] = "Bip01 L Finger41";
        boneName["LeftLittleDistal"] = "Bip01 L Finger42";

        boneName["RightThumbProximal"] = "Bip01 R Finger0";
        boneName["RightThumbIntermediate"] = "Bip01 R Finger01";
        boneName["RightThumbDistal"] = "Bip01 R Finger02";
        boneName["RightIndexProximal"] = "Bip01 R Finger1";
        boneName["RightIndexIntermediate"] = "Bip01 R Finger11";
        boneName["RightIndexDistal"] = "Bip01 R Finger12";
        boneName["RightMiddleProximal"] = "Bip01 R Finger2";
        boneName["RightMiddleIntermediate"] = "Bip01 R Finger21";
        boneName["RightMiddleDistal"] = "Bip01 R Finger22";
        boneName["RightRingProximal"] = "Bip01 R Finger3";
        boneName["RightRingIntermediate"] = "Bip01 R Finger31";
        boneName["RightRingDistal"] = "Bip01 R Finger32";
        boneName["RightLittleProximal"] = "Bip01 R Finger4";
        boneName["RightLittleIntermediate"] = "Bip01 R Finger41";
        boneName["RightLittleDistal"] = "Bip01 R Finger42";

        string[] humanName = HumanTrait.BoneName;
        HumanBone[] humanBones = new HumanBone[boneName.Count];


        var skeletonBones = new List<SkeletonBone>();
        var rootBone = new SkeletonBone();

        rootBone.name = "Bip01";
        var rootBoneTransform = g.transform.Find("Bip01");
        rootBone.position = rootBoneTransform.position;
        rootBone.rotation = rootBoneTransform.rotation;
        rootBone.scale = rootBoneTransform.lossyScale;
        
        skeletonBones.Add(rootBone);
       
        int j = 0;
        int i = 0;
        while (i < humanName.Length)
        {
            if (boneName.ContainsKey(humanName[i]))
            {
                HumanBone humanBone = new HumanBone();
                humanBone.humanName = humanName[i];
                humanBone.boneName = boneName[humanName[i]];
                var skeletonBone = new SkeletonBone();
                var currentBoneName = boneName[humanName[i]];
                skeletonBone.name = currentBoneName;
                var currentBone = SearchHierarchyForBone(g.transform, currentBoneName);
                if(currentBoneName.ToLower().Contains("l clavicle"))
                {
                    skeletonBone.position = new Vector3();
                }
                skeletonBone.position = currentBone.position;
                skeletonBone.rotation = currentBone.rotation;
                skeletonBone.scale = currentBone.lossyScale;
                skeletonBones.Add(skeletonBone);
                humanBone.limit.useDefaultValues = true;
                humanBones[j++] = humanBone;
            }
            i++;
        }
        var humanDescription = new HumanDescription();
        humanDescription.human = humanBones;
        humanDescription.skeleton = skeletonBones.ToArray();
        return humanDescription;
    }

    public Transform SearchHierarchyForBone(Transform current, string name)   
    {
        // check if the current bone is the bone we're looking for, if so return it
        if (current.name == name)
            return current;
        // search through child bones for the bone we're looking for
        for (int i = 0; i < current.childCount; ++i)
        {
            // the recursive step; repeat the search one step deeper in the hierarchy
            Transform found = SearchHierarchyForBone(current.GetChild(i), name);
            // a transform was returned by the search above that is not null,
            // it must be the bone we're looking for
            if (found != null)
                return found;
        }
    
        // bone with name was not found
        return null;
    }

}
