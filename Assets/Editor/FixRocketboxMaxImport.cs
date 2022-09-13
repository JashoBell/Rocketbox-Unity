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
    bool twistCorrection = true;

    void OnPostprocessMaterial(Material material)
    {
        if (assetPath.ToLower().Contains("avatars"))
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
    }

    void OnPostprocessMeshHierarchy(GameObject g)
    {
        // This function selects only the highest resolution mesh as being activated by default.
        // You might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly")
        // to be selected. Or you could choose not to import, by changing OnPreprocessMeshHierarchy
        if (g.name.ToLower().Contains("poly") && assetPath.ToLower().Contains("avatars") &&
            !g.name.ToLower().Contains("hipoly"))
            g.SetActive(false);

    }

    void OnPreprocessTexture()
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

    void OnPostprocessModel(GameObject g)
    {
        if (!assetPath.ToLower().Contains("avatars")) return;
        if (g.transform.Find("Bip02") != null) RenameBip(g);

        Transform rootBone = g.transform.Find("Bip01");
        Transform pelvis = rootBone.Find("Bip01 Pelvis");

        if (pelvis == null) return;

        Transform spine2 = BoneUtilities.SearchHierarchyForBone(rootBone, "Bip01 Spine2");

        if (spine2.Find("Bip01 L Clavicle") == null)
        {
            BoneUtilities.SearchHierarchyForBone(rootBone, "Bip01 L Clavicle").SetParent(spine2);
            BoneUtilities.SearchHierarchyForBone(rootBone, "Bip01 R Clavicle").SetParent(spine2);
        }

        if (pelvis.Find("Bip01 L Thigh") == null)
        {
            BoneUtilities.SearchHierarchyForBone(rootBone, "Bip01 L Thigh").SetParent(pelvis);
            BoneUtilities.SearchHierarchyForBone(rootBone, "Bip01 R Thigh").SetParent(pelvis);
        }

        fixBones(rootBone);

        var importer = (ModelImporter)assetImporter;

        if (g.GetComponent(typeof(Animator)) == null)
        {
            g.AddComponent<Animator>();
        }

        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.animationType = ModelImporterAnimationType.Human;
        var avatarMappings = generateAvatarBoneMappings(g);
        importer.humanDescription = avatarMappings;
        var avatar = AvatarBuilder.BuildHumanAvatar(g, avatarMappings);

        importer.SaveAndReimport();



        if (assetPath.ToLower().Contains("rocketbox"))
        {
            if (g.GetComponent(typeof(AutoRigAvatar)) == null & UsingAutoRig)
            {
                var ik = g.AddComponent<AutoRigAvatar>();
                ik.IKSetupChooser(UsingFinalIK ? AutoRigAvatar.IKSolver.FinalIK : AutoRigAvatar.IKSolver.UnityXR, g);
            }

            if (g.GetComponent(typeof(AvatarManusHandSetup)) == null & usingManusGloves)
            {
                g.AddComponent<AvatarManusHandSetup>();
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

        /// <summary>
        /// Updates the transforms of the rocketbox avatar bones to place it in t-pose, including the hands and fingers. If "twistCorrection"
        /// is true, divides the forearm bones into two pieces (for the FinalIK twist relaxers to work well, this needs to be done).
        /// </summary>
        /// <param name="avatarBase">The root of the avatar hierarchy.</param>
        private void fixBones(Transform avatarBase)
        {
            avatarBase.eulerAngles = new Vector3(-90, 90, 0);

            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").localEulerAngles = new Vector3(160, 90, 0);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").localEulerAngles = new Vector3(-160, -90, 0);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").localPosition = new Vector3(-0.1f, -0.01f, 0.075f);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").localPosition = new Vector3(-0.1f, -0.01f, -0.075f);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L UpperArm").localEulerAngles = Vector3.zero;
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R UpperArm").localEulerAngles = Vector3.zero;
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Forearm").localEulerAngles = Vector3.zero;
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Forearm").localEulerAngles = Vector3.zero;
            if (BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Wrist") == null & twistCorrection)
            {
<<<<<<< Updated upstream
                var r_wrist = new GameObject();
                r_wrist.name = "Bip01 R Wrist";
                r_wrist.transform.SetParent(BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Forearm"));
=======
            var rWrist = new GameObject
            {
                name = "Bip01 R Wrist"
            };
            rWrist.transform.SetParent(BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Forearm"));
>>>>>>> Stashed changes

                r_wrist.transform.position = (BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Forearm").position + BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Hand").position) / 2;
                r_wrist.transform.localEulerAngles = Vector3.zero;

                BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Hand").SetParent(r_wrist.transform);
            }
            if (BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Wrist") == null & twistCorrection)
            {
<<<<<<< Updated upstream
                var l_wrist = new GameObject();
                l_wrist.name = "Bip01 L Wrist";
                l_wrist.transform.SetParent(BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Forearm"));

=======
            var lWrist = new GameObject
            {
                name = "Bip01 L Wrist"
            };
            lWrist.transform.SetParent(BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Forearm"));
>>>>>>> Stashed changes
                l_wrist.transform.position = (BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Forearm").position + BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Hand").position) / 2;
                l_wrist.transform.localEulerAngles = Vector3.zero;

                BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Hand").SetParent(l_wrist.transform);
            }

            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Hand").localEulerAngles = new Vector3(310, 340, 20);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger0").localEulerAngles = new Vector3(87, -31, 8);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger1").localEulerAngles = new Vector3(4, 4, -3);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger2").localEulerAngles = new Vector3(-13, 7, -6);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger3").localEulerAngles = new Vector3(-15, 7, -6);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger4").localEulerAngles = new Vector3(-34, 11, -2);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger01").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger11").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger21").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger31").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger41").localEulerAngles = new Vector3(0, 0, -4);


            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Hand").localEulerAngles = new Vector3(50, 20, 20);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger0").localEulerAngles = new Vector3(-87, 31, 8);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger1").localEulerAngles = new Vector3(-4, -4, -3);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger2").localEulerAngles = new Vector3(13, -7, -6);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger3").localEulerAngles = new Vector3(15, -7, -6);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger4").localEulerAngles = new Vector3(34, -11, -2);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger01").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger11").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger21").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger31").localEulerAngles = new Vector3(0, 0, -4);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger41").localEulerAngles = new Vector3(0, 0, -4);
        }

        /// <summary>
        /// Pairs the model's bones with avatar bones and forms a human description. If "twistCorrection" is true,
        /// additionally adds a "wrist bone" to help with mesh deformation upon twisting.
        /// </summary>
        /// <param name="g">The model with bones.</param>
        /// <returns>A mapped HumanDescription to be used in generating an avatar.</returns>
        private HumanDescription generateAvatarBoneMappings(GameObject g)
        {
            Dictionary<string, string> boneName = new System.Collections.Generic.Dictionary<string, string>
            {
                ["Hips"] = "Bip01 Pelvis",
                ["Spine"] = "Bip01 Spine",
                ["Chest"] = "Bip01 Spine1",
                ["UpperChest"] = "Bip01 Spine2",
                ["RightShoulder"] = "Bip01 R Clavicle",
                ["RightUpperArm"] = "Bip01 R UpperArm",
                ["RightLowerArm"] = "Bip01 R Forearm",
                ["RightHand"] = "Bip01 R Hand",
                ["LeftShoulder"] = "Bip01 L Clavicle",
                ["LeftUpperArm"] = "Bip01 L UpperArm",
                ["LeftLowerArm"] = "Bip01 L Forearm",
                ["LeftHand"] = "Bip01 L Hand",
                ["Neck"] = "Bip01 Neck",
                ["Head"] = "Bip01 Head",
                ["Jaw"] = "Bip01 MJaw",
                ["LeftEye"] = "Bip01 LEye",
                ["RightEye"] = "Bip01 REye",
                ["LeftUpperLeg"] = "Bip01 L Thigh",
                ["LeftLowerLeg"] = "Bip01 L Calf",
                ["LeftFoot"] = "Bip01 L Foot",
                ["LeftToes"] = "Bip01 L Toe0",
                ["RightUpperLeg"] = "Bip01 R Thigh",
                ["RightLowerLeg"] = "Bip01 R Calf",
                ["RightFoot"] = "Bip01 R Foot",
                ["RightToes"] = "Bip01 R Toe0",
                ["Left Thumb Proximal"] = "Bip01 L Finger0",
                ["Left Thumb Intermediate"] = "Bip01 L Finger01",
                ["Left Thumb Distal"] = "Bip01 L Finger02",
                ["Left Index Proximal"] = "Bip01 L Finger1",
                ["Left Index Intermediate"] = "Bip01 L Finger11",
                ["Left Index Distal"] = "Bip01 L Finger12",
                ["Left Middle Proximal"] = "Bip01 L Finger2",
                ["Left Middle Intermediate"] = "Bip01 L Finger21",
                ["Left Middle Distal"] = "Bip01 L Finger22",
                ["Left Ring Proximal"] = "Bip01 L Finger3",
                ["Left Ring Intermediate"] = "Bip01 L Finger31",
                ["Left Ring Distal"] = "Bip01 L Finger32",
                ["Left Little Proximal"] = "Bip01 L Finger4",
                ["Left Little Intermediate"] = "Bip01 L Finger41",
                ["Left Little Distal"] = "Bip01 L Finger42",
                ["Right Thumb Proximal"] = "Bip01 R Finger0",
                ["Right Thumb Intermediate"] = "Bip01 R Finger01",
                ["Right Thumb Distal"] = "Bip01 R Finger02",
                ["Right Index Proximal"] = "Bip01 R Finger1",
                ["Right Index Intermediate"] = "Bip01 R Finger11",
                ["Right Index Distal"] = "Bip01 R Finger12",
                ["Right Middle Proximal"] = "Bip01 R Finger2",
                ["Right Middle Intermediate"] = "Bip01 R Finger21",
                ["Right Middle Distal"] = "Bip01 R Finger22",
                ["Right Ring Proximal"] = "Bip01 R Finger3",
                ["Right Ring Intermediate"] = "Bip01 R Finger31",
                ["Right Ring Distal"] = "Bip01 R Finger32",
                ["Right Little Proximal"] = "Bip01 R Finger4",
                ["Right Little Intermediate"] = "Bip01 R Finger41",
                ["Right Little Distal"] = "Bip01 R Finger42"
            };


            string[] humanName = boneName.Keys.ToArray();
            HumanBone[] humanBones = new HumanBone[boneName.Count];



            var skeletonBones = new List<SkeletonBone>();
            var rootBone = new SkeletonBone();
            var parentObject = new SkeletonBone
            {
                name = g.name,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                scale = g.transform.lossyScale
            };

            skeletonBones.Add(parentObject);

            rootBone.name = "Bip01";
            var rootBoneTransform = g.transform.Find("Bip01");
            rootBone.position = rootBoneTransform.localPosition;
            rootBone.rotation = Quaternion.Euler(-90, 90, 0);
            rootBone.scale = rootBoneTransform.lossyScale;

            skeletonBones.Add(rootBone);

            int j = 0;
            int i = 0;
            while (i < humanName.Length)
            {
                if (boneName.ContainsKey(humanName[i]))
                {
                    HumanBone humanBone = new HumanBone
                    {
                        humanName = humanName[i],
                        boneName = boneName[humanName[i]]
                    };
                    var skeletonBone = new SkeletonBone();
                    var currentBoneName = boneName[humanName[i]];
                    skeletonBone.name = currentBoneName;
                    var currentBone = BoneUtilities.SearchHierarchyForBone(g.transform, currentBoneName);
                    skeletonBone.position = currentBone.localPosition;
                    skeletonBone.rotation = currentBone.localRotation;
                    skeletonBone.scale = currentBone.lossyScale;
                    skeletonBones.Add(skeletonBone);
                    humanBone.limit.useDefaultValues = true;
                    humanBones[j++] = humanBone;
                }

                //Add additional bones for wrist to reduce mesh deformation
                if (BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 R Wrist") != null & twistCorrection)
                {
<<<<<<< Updated upstream
                    var right_wrist = new SkeletonBone();
                    right_wrist.name = "Bip01 R Wrist";
                    var right_wrist_transform = BoneUtilities.SearchHierarchyForBone(g.transform, right_wrist.name);
                    right_wrist.position = right_wrist_transform.localPosition;
                    right_wrist.rotation = right_wrist_transform.localRotation;
                    right_wrist.scale = BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 R Forearm").lossyScale;
                    skeletonBones.Add(right_wrist);
=======
                var rightWrist = new SkeletonBone
                {
                    name = "Bip01 R Wrist"
                };
                var rightWristTransform = BoneUtilities.SearchHierarchyForBone(g.transform, rightWrist.name);
                rightWrist.position = rightWristTransform.localPosition;
                rightWrist.rotation = rightWristTransform.localRotation;
                rightWrist.scale = BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 R Forearm").lossyScale;
                skeletonBones.Add(rightWrist);
>>>>>>> Stashed changes
                }
                if (BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 L Wrist") != null & twistCorrection)
                {
<<<<<<< Updated upstream
                    var left_wrist = new SkeletonBone();
                    left_wrist.name = "Bip01 L Wrist";
                    var left_wrist_transform = BoneUtilities.SearchHierarchyForBone(g.transform, left_wrist.name);
                    left_wrist.position = left_wrist_transform.localPosition;
                    left_wrist.rotation = left_wrist_transform.localRotation;
                    left_wrist.scale = BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 L Forearm").lossyScale;
                    skeletonBones.Add(left_wrist);
=======
                var leftWrist = new SkeletonBone
                {
                    name = "Bip01 L Wrist"
                };
                var leftWristTransform = BoneUtilities.SearchHierarchyForBone(g.transform, leftWrist.name);
                leftWrist.position = leftWristTransform.localPosition;
                leftWrist.rotation = leftWristTransform.localRotation;
                leftWrist.scale = BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 L Forearm").lossyScale;
                skeletonBones.Add(leftWrist);
>>>>>>> Stashed changes
                }



                i++;
            }
            var humanDescription = new HumanDescription
            {
                human = humanBones,
                hasTranslationDoF = true,
                skeleton = skeletonBones.ToArray()
            };
            return humanDescription;
        }


    }
