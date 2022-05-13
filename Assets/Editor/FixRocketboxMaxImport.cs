using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
public class FixRocketboxMaxImport : AssetPostprocessor
{
    bool usingMixamoAnimations = true; 
    void OnPostprocessMaterial(Material material)
    {
        // This fixes two problems with importing 3DSMax materials. The first is that the Max materials
        // assumed that diffuse material was set by the texture, whereas Unity multiplies the texture 
        // colour with the flat colour. 
        material.color = Color.white;
        // Second Unity's transparent  materials still show specular highlights and thus hair looks 
        // like glass sheets. The material mode "Fade" goes to full transparent. 
        if (material.GetFloat("_Mode") == 3f)
            material.SetFloat("_Mode", 2f);
    }

    void OnPostprocessMeshHierarchy(GameObject gameObject)
    {
        // This function selects only the highest resolution mesh as being activated by default.
        // You might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly")
        // to be selected. Or you could choose not to import, by changing OnPreprocessMeshHierarchy
        if (gameObject.name.ToLower().Contains("poly") &&
            !gameObject.name.ToLower().Contains("hipoly"))
            gameObject.SetActive(false);
    }
    
    void OnPreprocessTexture()
    {
        // This function changes textures that are labelled with "normal" in their title to be loaded as 
        // NormalMaps. This just avoids a warning dialogue box that would otherwise fix it.
        if (assetPath.ToLower().Contains("normal"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.convertToNormalmap = false;
        }
    }

    void OnPostprocessModel(GameObject g)
    {
        if (g.transform.Find("Bip02") != null) RenameBip(g);

        Transform pelvis = g.transform.Find("Bip01").Find("Bip01 Pelvis");
        if (pelvis == null) return;
        Transform spine2 = pelvis.Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2");
        Transform RClavicle = spine2.Find("Bip01 Neck").Find("Bip01 R Clavicle");
        Transform LClavicle = spine2.Find("Bip01 Neck").Find("Bip01 L Clavicle");


        if(!usingMixamoAnimations){
            pelvis.Find("Bip01 Spine").Find("Bip01 L Thigh").parent = pelvis;
            pelvis.Find("Bip01 Spine").Find("Bip01 R Thigh").parent = pelvis;
            LClavicle.parent = spine2;
            RClavicle.parent = spine2;


            LClavicle.rotation = new Quaternion(-0.7215106f, 0, 0, 0.6924035f);
            RClavicle.rotation = new Quaternion(0, -0.6925546f, 0.721365f, 0);
            LClavicle.Find("Bip01 L UpperArm").rotation = new Quaternion(0, 0, 0, 0);
            RClavicle.Find("Bip01 R UpperArm").rotation = new Quaternion(0, 0, 0, 0);
        }

        AddIKConstraints(g);

        var importer = (ModelImporter)assetImporter;
        //If you need a humanoid avatar, change it here
        importer.animationType = ModelImporterAnimationType.Human;


    }
    private void RenameBip(GameObject currentBone)
    {
        currentBone.name = currentBone.name.Replace("Bip02", "Bip01");
        for (int i = 0; i < currentBone.transform.childCount; i++)
        {
            RenameBip(currentBone.transform.GetChild(i).gameObject);
        }

    }

    private GameObject AddIKConstraints(GameObject avatarBase){
        
        var rigBuilder = avatarBase.AddComponent<RigBuilder>();

        var spine = avatarBase.transform.Find("Bip01").Find("Bip01 Pelvis").Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2").Find("Bip01 Neck");
        var head = spine.transform.Find("Bip01 Head");
        
        var upperarm_l = spine.transform.Find("Bip01 L Clavicle").Find("Bip01 L UpperArm");
        var upperarm_r = spine.transform.Find("Bip01 R Clavicle").Find("Bip01 R UpperArm");

        var forearm_l = upperarm_l.transform.Find("Bip01 L Forearm");
        var forearm_r = upperarm_r.transform.Find("Bip01 R Forearm");
        
        var hand_l = forearm_l.transform.Find("Bip01 L Hand");
        var hand_r = forearm_r.transform.Find("Bip01 R Hand");

        var constraintsRoot = new GameObject("ikConstraints");
        var rig = constraintsRoot.AddComponent<Rig>();

        constraintsRoot.transform.SetParent(avatarBase.transform);

        rigBuilder.layers.Add(new RigLayer(rig, true));
        
        var forearmConstraintLeft = ArmIK("left forearm", upperarm_l.gameObject, forearm_l.gameObject, hand_l.gameObject);
        var forearmConstraintRight = ArmIK("right forearm", upperarm_r.gameObject, forearm_r.gameObject, hand_r.gameObject);

        forearmConstraintLeft.transform.SetParent(constraintsRoot.transform);
        forearmConstraintRight.transform.SetParent(constraintsRoot.transform);

        var handConstraintLeft = HandIK("left hand", upperarm_l.gameObject, forearm_l.gameObject, hand_l.gameObject);
        var handConstraintRight = HandIK("right hand", upperarm_r.gameObject, forearm_r.gameObject, hand_r.gameObject);

        handConstraintLeft.transform.SetParent(constraintsRoot.transform);
        handConstraintRight.transform.SetParent(constraintsRoot.transform);

        var headConstraint = HeadIK(head.gameObject);

        headConstraint.transform.SetParent(constraintsRoot.transform);

        return constraintsRoot;
    }

    private GameObject HandIK(string name, GameObject root, GameObject mid, GameObject tip)
    {
        
        GameObject hand = new GameObject(name);
        
        var handIK = hand.AddComponent<TwoBoneIKConstraint>();

        handIK.data.tip = tip.transform;
        handIK.data.mid = mid.transform;
        handIK.data.root = root.transform;

        var target = new GameObject(name + "_target");
        var hint = new GameObject(name + "_hint");

        target.transform.SetParent(hand.transform);
        hint.transform.SetParent(hand.transform);

        handIK.data.target = target.transform;
        handIK.data.hint = hint.transform;

        target.transform.position = tip.transform.position;
        hint.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y, mid.transform.position.z-.1f);

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
        GameObject head = new GameObject("head");
        GameObject head_target = new GameObject("head_target");
        head_target.transform.SetParent(head.transform);
        head_target.transform.position = headbone.transform.position;
        

        var headIK = head.AddComponent<MultiParentConstraint>();
        headIK.data.constrainedObject = head.transform;
        
        var transforms = new WeightedTransformArray();
        transforms.Add(new WeightedTransform(head_target.transform, 1));

        headIK.data.sourceObjects = transforms;

        return head;
    }

}
