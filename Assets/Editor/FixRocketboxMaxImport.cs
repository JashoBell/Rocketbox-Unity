using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class FixRocketboxMaxImport : AssetPostprocessor
{
    // True will attach the AutoRigAvatar script to the avatar and use it to attach IK components.
    private bool _usingAutoRig = true;

    // True will attach Manus components to the avatar.
    private bool _usingManusGloves = true;

    // True will attach FinalIK components to the avatar.
    private bool _usingFinalIK = true;

    // Generate wrist bones to help with IK.
    private bool _twistCorrection = true;

    // Generate finger tips for the avatar. This is useful for the Manus gloves, which assume fingertips are present.
    private bool _createFingerTips = true;

    private void OnPostprocessMaterial(Material material)
    {
        if (!assetPath.ToLower().Contains("avatars")) return;
        // This fixes two problems with importing 3DSMax materials. The first is that the Max materials
        // assumed that diffuse material was set by the texture, whereas Unity multiplies the texture 
        // colour with the flat colour. 
        material.color = Color.white;
        // Second Unity's transparent  materials still show specular highlights and thus hair looks 
        // like glass sheets. The material mode "Fade" goes to full transparent. 
        if (Math.Abs(material.GetFloat("_Mode") - 3f) < .01f)
            material.SetFloat("_Mode", 2f);
    }

    private void OnPostprocessMeshHierarchy(GameObject g)
    {
        // This function selects only the highest resolution mesh as being activated by default.
        // You might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly")
        // to be selected. Or you could choose not to import, by changing OnPreprocessMeshHierarchy
        if (g.name.ToLower().Contains("poly") && assetPath.ToLower().Contains("avatars") &&
            !g.name.ToLower().Contains("hipoly"))
            g.SetActive(false);

    }

    private void OnPreprocessTexture()
    {
        // This function changes textures that are labelled with "normal" in their title to be loaded as 
        // NormalMaps. This just avoids a warning dialogue box that would otherwise fix it.
        if (assetPath.ToLower().Contains("normal") && assetPath.ToLower().Contains("avatars"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.convertToNormalmap = false;
        }
    }

    private void OnPostprocessModel(GameObject g)
    {
        if (!assetPath.ToLower().Contains("avatars")) return;
        if (g.transform.Find("Bip02") != null) RenameBip(g);

        Transform rootBone = g.transform.Find("Bip01");
        
 

        var importer = (ModelImporter)assetImporter;

        if (g.GetComponent(typeof(Animator)) == null)
        {
            g.AddComponent<Animator>();
        }

        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.animationType = ModelImporterAnimationType.Human;

        var avatarMappings = BoneUtilities.AvatarSkeletonCorrection(rootBone, assetPath, _twistCorrection, _createFingerTips);
        importer.humanDescription = avatarMappings;
        importer.SaveAndReimport();
        
        AvatarBuilder.BuildHumanAvatar(g, avatarMappings);        

        if (g.GetComponent(typeof(AutoRigAvatar)) == null & _usingAutoRig)
        {
            var ik = g.AddComponent<AutoRigAvatar>();
            ik.IKSetupChooser(_usingFinalIK ? AutoRigAvatar.IKSolver.FinalIK : AutoRigAvatar.IKSolver.UnityXR, g);
        }

        if (g.GetComponent(typeof(AvatarManusHandSetup)) == null & _usingManusGloves)
        {
            var manus = g.AddComponent<AvatarManusHandSetup>();
            manus.ImportSetup();
        }
    }
    
    private static void RenameBip(GameObject currentBone)
    {
        currentBone.name = currentBone.name.Replace("Bip02", "Bip01");
        for (int i = 0; i < currentBone.transform.childCount; i++)
        {
            RenameBip(currentBone.transform.GetChild(i).gameObject);
        }

    }
    }
