using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System.Linq;
public class FixRocketboxMaxImport : AssetPostprocessor
{
    bool usingMixamoAnimations = true; 
    bool usingManusGloves = true;
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
        if (assetPath.ToLower().Contains("rocketbox")) {
            if (g.transform.Find("Bip02") != null) RenameBip(g);
            Transform pelvis = g.transform.Find("Bip01").Find("Bip01 Pelvis");
            if (pelvis == null) return;
            Transform spine2 = pelvis.Find("Bip01 Spine").Find("Bip01 Spine1").Find("Bip01 Spine2");
            Transform RClavicle = spine2.Find("Bip01 Neck").Find("Bip01 R Clavicle");
            Transform LClavicle = spine2.Find("Bip01 Neck").Find("Bip01 L Clavicle");



            pelvis.Find("Bip01 Spine").Find("Bip01 L Thigh").SetParent(pelvis);
            pelvis.Find("Bip01 Spine").Find("Bip01 R Thigh").SetParent(pelvis);
            LClavicle.SetParent(spine2);
            RClavicle.SetParent(spine2);


            LClavicle.rotation = new Quaternion(-0.7215106f, 0, 0, 0.6924035f);
            RClavicle.rotation = new Quaternion(0, -0.6925546f, 0.721365f, 0);
            LClavicle.localPosition = new Vector3(-.13f, -.05f, .12f);
            RClavicle.localPosition = new Vector3(-.13f, -.05f, -.12f);
            LClavicle.Find("Bip01 L UpperArm").rotation = new Quaternion(0, 0, 0, 0);
            RClavicle.Find("Bip01 R UpperArm").rotation = new Quaternion(0, 0, 0, 0);

        var importer = (ModelImporter)assetImporter;
        //If you need a humanoid avatar, change it here
        var avatarMappings = generateAvatarBoneMappings(g);
        importer.humanDescription = avatarMappings;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;


        var avatar = AvatarBuilder.BuildHumanAvatar(g, avatarMappings);
        if(g.GetComponent(typeof(Animator)) == null){
            g.AddComponent<Animator>();
        }
        g.GetComponent<Animator>().avatar = avatar;
        importer.animationType = ModelImporterAnimationType.Generic;
        

        g.AddComponent<AutoRigAvatar>();
        if(usingManusGloves){
            g.AddComponent<AvatarManusHandSetup>();
        }
        }



    }
    private void RenameBip(GameObject currentBone)
    {
        currentBone.name = currentBone.name.Replace("Bip02", "Bip01");
        for (int i = 0; i < currentBone.transform.childCount; i++)
        {
            RenameBip(currentBone.transform.GetChild(i).gameObject);
        }

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
