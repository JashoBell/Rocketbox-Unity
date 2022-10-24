using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Rendering;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEditor.AssetImporters;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FixRocketboxMaxImport : AssetPostprocessor
{
    private bool _usingAutoRig = true;
    private bool _usingManusGloves = true;
    private bool _usingFinalIK = true;
    private bool _twistCorrection = true;
    
    static readonly string _skinMaterialPath = "Assets/Avatars/Rocketbox-Unity/Assets/Resources/Materials/Skin.mat";
    static readonly string _hairMaterialPath = "Assets/Avatars/Rocketbox-Unity/Assets/Resources/Materials/Hair.mat";
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int DiffusionProfileHash = Shader.PropertyToID("_DiffusionProfileHash");

    public override int GetPostprocessOrder()
    {
        return (1);
    }
    
    private void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] clips)
    {
        if (!assetPath.ToLower().Contains("avatars")) return;

        var hdrp = GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset");
        UnityEngine.Debug.Log("HDRP: " + hdrp + " material: " + material.name);
        if (!hdrp) return;
        
        
        if (material.name.Contains("opacity"))
        {
            var mainTexture = material.GetTexture("_MainTex");
            var hairMaterial = AssetDatabase.LoadAssetAtPath<Material>(_hairMaterialPath);
            material.shader = hairMaterial.shader;
            material.CopyPropertiesFromMaterial(hairMaterial);
            material.SetTexture("_BaseColorMap", mainTexture);
        }

        if (material.name.Contains("head") || material.name.Contains("body"))
        {
            var mainTexture = material.GetTexture("_MainTex");
            var normalTexture = material.GetTexture("_BumpMap");
            var specularTexture = material.GetTexture("_SpecGlossMap");
            var skinMaterial = AssetDatabase.LoadAssetAtPath<Material>(_skinMaterialPath);
            material.CopyPropertiesFromMaterial(skinMaterial);
            material.SetTexture("_BaseColorMap", mainTexture);
            material.SetTexture("_NormalMap", normalTexture);
        }
    }

    private void OnPostprocessMaterial(Material material)
    {
        if (!assetPath.ToLower().Contains("avatars")) return;
        // This fixes two problems with importing 3DSMax materials. The first is that the Max materials
        // assumed that diffuse material was set by the texture, whereas Unity multiplies the texture 
        // colour with the flat colour. 

        // Second Unity's transparent  materials still show specular highlights and thus hair looks 
        // like glass sheets. The material mode "Fade" goes to full transparent. 
        
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

        var avatarMappings = BoneUtilities.AvatarSkeletonCorrection(rootBone, assetPath, _twistCorrection);
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
