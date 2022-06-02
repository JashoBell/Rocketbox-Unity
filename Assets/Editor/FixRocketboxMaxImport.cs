using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System.Linq;
using System.Reflection;

public class FixRocketboxMaxImport : AssetPostprocessor
{
    bool usingAutoRig = true;
    bool usingManusGloves = true;
    bool usingFinalIK = true;
    void OnPostprocessMaterial(Material material)
    {
        if(assetPath.ToLower().Contains("rocketbox")){
        // This fixes two problems with importing 3DSMax materials. The first is that the Max materials
        // assumed that diffuse material was set by the texture, whereas Unity multiplies the texture 
        // colour with the flat colour. 
        material.color = Color.white;
        // Second Unity's transparent  materials still show specular highlights and thus hair looks 
        // like glass sheets. The material mode "Fade" goes to full transparent. 
        if (material.GetFloat("_Mode") == 3f)
            material.SetFloat("_Mode", 2f);
        }
    }

    void OnPostprocessMeshHierarchy(GameObject g)
    {
        // This function selects only the highest resolution mesh as being activated by default.
        // You might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly")
        // to be selected. Or you could choose not to import, by changing OnPreprocessMeshHierarchy
        if (g.name.ToLower().Contains("poly") && assetPath.ToLower().Contains("rocketbox") &&
            !g.name.ToLower().Contains("hipoly"))
            g.SetActive(false);
        
    }
    
    void OnPreprocessTexture()
    {
        // This function changes textures that are labelled with "normal" in their title to be loaded as 
        // NormalMaps. This just avoids a warning dialogue box that would otherwise fix it.
        if (assetPath.ToLower().Contains("normal") && assetPath.ToLower().Contains("rocketbox"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.convertToNormalmap = false;
        }
    }

    void OnPostprocessModel(GameObject g)
    {
        
        if (g.transform.Find("Bip02") != null) RenameBip(g);

        Transform rootBone = g.transform.Find("Bip01");
        Transform pelvis = rootBone.Find("Bip01 Pelvis");

        if (pelvis == null) return;

        Transform spine2 = SearchHierarchyForBone(rootBone, "Bip01 Spine2");

        if(spine2.Find("Bip01 L Clavicle") == null)
        {
            SearchHierarchyForBone(rootBone, "Bip01 L Clavicle").SetParent(spine2);
            SearchHierarchyForBone(rootBone, "Bip01 R Clavicle").SetParent(spine2);
        }

        if(pelvis.Find("Bip01 L Thigh") == null)
        {
            SearchHierarchyForBone(rootBone, "Bip01 L Thigh").SetParent(pelvis);
            SearchHierarchyForBone(rootBone, "Bip01 R Thigh").SetParent(pelvis);
        }

        fixBones(rootBone);

        var importer = (ModelImporter)assetImporter;
        
        if(g.GetComponent(typeof(Animator)) == null){
            g.AddComponent<Animator>();
        }     

        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.animationType = ModelImporterAnimationType.Human;
        var avatarMappings = generateAvatarBoneMappings(g);
        importer.humanDescription = avatarMappings;
        var avatar = AvatarBuilder.BuildHumanAvatar(g, avatarMappings);

        importer.SaveAndReimport();

   

        if (assetPath.ToLower().Contains("rocketbox")) {
            if(g.GetComponent(typeof(AutoRigAvatar)) == null & usingAutoRig)
            {
                var ik = g.AddComponent<AutoRigAvatar>();
                if(usingFinalIK)
                {
                    ik.IKSetupChooser(AutoRigAvatar.ikSolver.FinalIK, g);
                } else {
                    ik.IKSetupChooser(AutoRigAvatar.ikSolver.UnityXR, g);
                }
            }

            if(g.GetComponent(typeof(AvatarManusHandSetup)) == null & usingManusGloves){
                g.AddComponent<AvatarManusHandSetup>();
            }
        }

        fixBones(rootBone);
    }

    private void RenameBip(GameObject currentBone)
    {
        currentBone.name = currentBone.name.Replace("Bip02", "Bip01");
        for (int i = 0; i < currentBone.transform.childCount; i++)
        {
            RenameBip(currentBone.transform.GetChild(i).gameObject);
        }

    }

    private void fixBones(Transform rootBone_)
    {
        rootBone_.eulerAngles = new Vector3(-90, 90, 0);

        SearchHierarchyForBone(rootBone_, "Bip01 L Clavicle").localEulerAngles = new Vector3(160, 90, 0);
        SearchHierarchyForBone(rootBone_, "Bip01 R Clavicle").localEulerAngles = new Vector3(-160, -90, 0);
        SearchHierarchyForBone(rootBone_, "Bip01 L Clavicle").localPosition = new Vector3(-0.1f,-0.01f,0.075f);
        SearchHierarchyForBone(rootBone_, "Bip01 R Clavicle").localPosition = new Vector3(-0.1f,-0.01f,-0.075f);
        SearchHierarchyForBone(rootBone_, "Bip01 L UpperArm").localEulerAngles = Vector3.zero;
        SearchHierarchyForBone(rootBone_, "Bip01 R UpperArm").localEulerAngles = Vector3.zero;
        SearchHierarchyForBone(rootBone_, "Bip01 L Forearm").localEulerAngles = Vector3.zero;
        SearchHierarchyForBone(rootBone_, "Bip01 R Forearm").localEulerAngles = Vector3.zero;


        SearchHierarchyForBone(rootBone_, "Bip01 L Hand").localEulerAngles = new Vector3(310, 340, 20);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger0").localEulerAngles = new Vector3(87, -31, 8);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger1").localEulerAngles = new Vector3(4, 4, -3);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger2").localEulerAngles = new Vector3(-13, 7, -6);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger3").localEulerAngles = new Vector3(-15, 7, -6);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger4").localEulerAngles = new Vector3(-34, 7, -7);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger01").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger11").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger21").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger31").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 L Finger41").localEulerAngles = new Vector3(0, 0, -4);

        SearchHierarchyForBone(rootBone_, "Bip01 R Hand").localEulerAngles = new Vector3(50, 20, 20);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger0").localEulerAngles = new Vector3(-87, 31, 8);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger1").localEulerAngles = new Vector3(-4, -4, -3);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger2").localEulerAngles = new Vector3(13, -7, -6);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger3").localEulerAngles = new Vector3(15, -7, -6);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger4").localEulerAngles = new Vector3(34, -7, -7);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger01").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger11").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger21").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger31").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(rootBone_, "Bip01 R Finger41").localEulerAngles = new Vector3(0, 0, -4);
    }

    private HumanDescription generateAvatarBoneMappings(GameObject g)
    {
        Dictionary<string, string> boneName = new System.Collections.Generic.Dictionary<string, string>();
        boneName["Hips"] = "Bip01 Pelvis";
        boneName["Spine"] = "Bip01 Spine";        
        boneName["Chest"] = "Bip01 Spine1";
        boneName["UpperChest"] = "Bip01 Spine2";

        boneName["RightShoulder"] = "Bip01 R Clavicle";
        boneName["RightUpperArm"] = "Bip01 R UpperArm";
        boneName["RightLowerArm"] = "Bip01 R Forearm";
        boneName["RightHand"] = "Bip01 R Hand";

        boneName["LeftShoulder"] = "Bip01 L Clavicle";
        boneName["LeftUpperArm"] = "Bip01 L UpperArm";
        boneName["LeftLowerArm"] = "Bip01 L Forearm";
        boneName["LeftHand"] = "Bip01 L Hand";

        boneName["Neck"] = "Bip01 Neck";
        boneName["Head"] = "Bip01 Head";
        boneName["Jaw"] = "Bip01 MJaw";
        boneName["LeftEye"] = "Bip01 LEye";
        boneName["RightEye"] = "Bip01 REye";

        boneName["LeftUpperLeg"] = "Bip01 L Thigh";
        boneName["LeftLowerLeg"] = "Bip01 L Calf";
        boneName["LeftFoot"] = "Bip01 L Foot";
        boneName["LeftToes"] = "Bip01 L Toe0";


        boneName["RightUpperLeg"] = "Bip01 R Thigh";
        boneName["RightLowerLeg"] = "Bip01 R Calf";
        boneName["RightFoot"] = "Bip01 R Foot";
        boneName["RightToes"] = "Bip01 R Toe0";

        boneName["Left Thumb Proximal"] = "Bip01 L Finger0";
        boneName["Left Thumb Intermediate"] = "Bip01 L Finger01";
        boneName["Left Thumb Distal"] = "Bip01 L Finger02";
        boneName["Left Index Proximal"] = "Bip01 L Finger1";
        boneName["Left Index Intermediate"] = "Bip01 L Finger11";
        boneName["Left Index Distal"] = "Bip01 L Finger12";
        boneName["Left Middle Proximal"] = "Bip01 L Finger2";
        boneName["Left Middle Intermediate"] = "Bip01 L Finger21";
        boneName["Left Middle Distal"] = "Bip01 L Finger22";
        boneName["Left Ring Proximal"] = "Bip01 L Finger3";
        boneName["Left Ring Intermediate"] = "Bip01 L Finger31";
        boneName["Left Ring Distal"] = "Bip01 L Finger32";
        boneName["Left Little Proximal"] = "Bip01 L Finger4";
        boneName["Left Little Intermediate"] = "Bip01 L Finger41";
        boneName["Left Little Distal"] = "Bip01 L Finger42";

        boneName["Right Thumb Proximal"] = "Bip01 R Finger0";
        boneName["Right Thumb Intermediate"] = "Bip01 R Finger01";
        boneName["Right Thumb Distal"] = "Bip01 R Finger02";
        boneName["Right Index Proximal"] = "Bip01 R Finger1";
        boneName["Right Index Intermediate"] = "Bip01 R Finger11";
        boneName["Right Index Distal"] = "Bip01 R Finger12";
        boneName["Right Middle Proximal"] = "Bip01 R Finger2";
        boneName["Right Middle Intermediate"] = "Bip01 R Finger21";
        boneName["Right Middle Distal"] = "Bip01 R Finger22";
        boneName["Right Ring Proximal"] = "Bip01 R Finger3";
        boneName["Right Ring Intermediate"] = "Bip01 R Finger31";
        boneName["Right Ring Distal"] = "Bip01 R Finger32";
        boneName["Right Little Proximal"] = "Bip01 R Finger4";
        boneName["Right Little Intermediate"] = "Bip01 R Finger41";
        boneName["Right Little Distal"] = "Bip01 R Finger42";

        string[] humanName = boneName.Keys.ToArray();
        HumanBone[] humanBones = new HumanBone[boneName.Count];



        var skeletonBones = new List<SkeletonBone>();
        var rootBone = new SkeletonBone();
        var parentObject = new SkeletonBone();
        
        parentObject.name = g.name;
        parentObject.position = Vector3.zero;
        parentObject.rotation = Quaternion.identity;
        parentObject.scale = g.transform.lossyScale;

        skeletonBones.Add(parentObject);

        rootBone.name = "Bip01";
        var rootBoneTransform = g.transform.Find("Bip01");
        rootBone.position = rootBoneTransform.localPosition;
        rootBone.rotation =  Quaternion.Euler(-90, 90, 0);
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
                skeletonBone.position = currentBone.localPosition;
                skeletonBone.rotation = currentBone.localRotation;
                skeletonBone.scale = currentBone.lossyScale;
                skeletonBones.Add(skeletonBone);
                humanBone.limit.useDefaultValues = true;
                humanBones[j++] = humanBone;
                Debug.Log(skeletonBone.name);
            }
            i++;
        }
        var humanDescription = new HumanDescription();
        humanDescription.human = humanBones;
        humanDescription.hasTranslationDoF = true;
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
