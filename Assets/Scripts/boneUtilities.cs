using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class BoneUtilities {

/// <summary>
/// Recursively searches the transform hierarchy for a bone, and returns it. Useful for adjusting bones on import.
/// </summary>
/// <param name="current">The root of the bone hierarchy (or just a level above the bone)</param>
/// <param name="name">The GameObject name of the bone</param>
/// <returns>The named bone's transform</returns>
    public static Transform SearchHierarchyForBone(Transform current, string name)   
    {
        // check if the current bone is the bone we're looking for, if so return it
        if (current.name.Contains(name) && !current.name.Contains("Tip"))
        { 
            return current;
        }
        else if (current.name == name)
        {
            return current;
        }
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

    public static HumanDescription AvatarSkeletonCorrection(Transform avatarBase, string assetPath, bool _twistCorrection, bool _createFingerTips)
    {
        FixBones(avatarBase, assetPath, _twistCorrection, _createFingerTips);
        var humanDescription = GenerateAvatarBoneMappings(avatarBase, _twistCorrection, _createFingerTips);
        FixBones(avatarBase, assetPath, _twistCorrection, _createFingerTips);
        return humanDescription;
    }

    /// <summary>
    /// Updates the transforms of the rocketbox avatar bones to place it in t-pose, including the hands and fingers. If "twistCorrection"
    /// is true, divides the forearm bones into two pieces (for the FinalIK twist relaxers to work well, this needs to be done). Also optionally 
    /// creates finger tips for the Manus Core skeleton.
    /// </summary>
    /// <param name="avatarBase">The root of the avatar hierarchy.</param>
    /// <param name="assetPath">The path to the asset being imported.</param>
    /// <param name="twistCorrection">If true, divides the forearm bones into two pieces.</param>
    /// <param name="createFingerTips">If true, creates finger tips for the Manus Core skeleton.</param>
    private static void FixBones(Transform avatarBase, string assetPath, bool _twistCorrection, bool _createFingerTips)
    {
        bool female = assetPath.ToLower().Contains("female");

        avatarBase.eulerAngles = new Vector3(-90, 90, 0);

        Transform pelvis = avatarBase.Find("Bip01 Pelvis");

        if (pelvis == null) return;

        Transform spine2 = SearchHierarchyForBone(avatarBase, "Bip01 Spine2");

        // Fix the parents of the thigh and clavicle bones if not already done.
        if (spine2.Find("Bip01 L Clavicle") == null)
        {
            SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").SetParent(spine2);
            SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").SetParent(spine2);
        }

        if (pelvis.Find("Bip01 L Thigh") == null)
        {
            SearchHierarchyForBone(avatarBase, "Bip01 L Thigh").SetParent(pelvis);
            SearchHierarchyForBone(avatarBase, "Bip01 R Thigh").SetParent(pelvis);
        }
        
        // Ensure t-pose
        SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").localEulerAngles = new Vector3(160, 90, 0);
        SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").localEulerAngles = new Vector3(-160, -90, 0);
        SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").localPosition = new Vector3(-0.1f, -0.01f, 0.075f);
        SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").localPosition = new Vector3(-0.1f, -0.01f, -0.075f);
        SearchHierarchyForBone(avatarBase, "Bip01 L UpperArm").localEulerAngles = Vector3.zero;
        SearchHierarchyForBone(avatarBase, "Bip01 R UpperArm").localEulerAngles = Vector3.zero;
        SearchHierarchyForBone(avatarBase, "Bip01 L Forearm").localEulerAngles = Vector3.zero;
        SearchHierarchyForBone(avatarBase, "Bip01 R Forearm").localEulerAngles = Vector3.zero;
        
        // If _twistCorrection is set to true, split the forearm bones into two pieces. This is needed for the FinalIK twist relaxers to work.
        bool hasWrist = SearchHierarchyForBone(avatarBase, "Bip01 L Wrist") != null;
        if (!hasWrist & _twistCorrection)
        {
            CreateWristBones(avatarBase);
        }

        // Fix the finger bones, adding a very slight curl to the tips
        SearchHierarchyForBone(avatarBase, "Bip01 L Hand").localEulerAngles =
            female ? new Vector3(-50, -20, 20) : new Vector3(-52, -5, 5.5f);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger0").localEulerAngles =
            female ? new Vector3(87, -31, 8) : new Vector3(55, -31, 8);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger1").localEulerAngles = new Vector3(4, 4, -3);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger2").localEulerAngles = new Vector3(-13, 7, -6);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger3").localEulerAngles = new Vector3(-15, 7, -6);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger4").localEulerAngles = new Vector3(-34, 11, -2);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger01").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger11").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger21").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger31").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 L Finger41").localEulerAngles = new Vector3(0, 0, -4);


        SearchHierarchyForBone(avatarBase, "Bip01 R Hand").localEulerAngles =
            female ? new Vector3(50, 20, 20) : new Vector3(52, 5, 5.5f);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger0").localEulerAngles = 
            female ? new Vector3(-65, 24, 17) : new Vector3(-55, 31, 8);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger1").localEulerAngles = new Vector3(-4, -4, -3);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger2").localEulerAngles = new Vector3(13, -7, -6);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger3").localEulerAngles = new Vector3(15, -7, -6);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger4").localEulerAngles = new Vector3(34, -11, -2);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger01").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger11").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger21").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger31").localEulerAngles = new Vector3(0, 0, -4);
        SearchHierarchyForBone(avatarBase, "Bip01 R Finger41").localEulerAngles = new Vector3(0, 0, -4);

        // Optionally create fingertips by adding child objects to the ending finger bones.
        // This is needed for the Manus VR gloves to properly track the fingers.
        // The local position of the child objects is set to be exactly at the fingertip.
        bool hasFingerTips = SearchHierarchyForBone(avatarBase, "Bip01 R Finger0 Tip") != null;
        if (_createFingerTips && !hasFingerTips)
        {
            CreateFingerTips(avatarBase, female);
        }
    }

    /// <summary>
    /// Fingertip generation function. Adds child objects to the ending finger bones, positioning them at the fingertip.
    /// The local position differs based the finger, and on whether the rocketbox avatar is female or male. All positions adjustments are
    /// on the x axis only, and negative.
    /// Female:
    ///  Thumb (Finger0): -.022
    ///  Index (Finger1): -.0185
    ///  Middle (Finger2): -.0185
    ///  Ring (Finger3): -.0172
    ///  Pinky (Finger4): -.013
    /// Male:
    /// Thumb (Finger0): -.026
    /// Index (Finger1): -.0205
    /// Middle (Finger2): -.02165
    /// Ring (Finger3): -.02
    /// Pinky (Finger4): -.0175
    /// </summary>
    /// <param name="avatarBase"></param>
    private static void CreateFingerTips(Transform avatarBase, bool female)
    {
        
        // Find the finger bones
        var rFinger02 = SearchHierarchyForBone(avatarBase, "Bip01 R Finger02");
        var rFinger12 = SearchHierarchyForBone(avatarBase, "Bip01 R Finger12");
        var rFinger22 = SearchHierarchyForBone(avatarBase, "Bip01 R Finger22");
        var rFinger32 = SearchHierarchyForBone(avatarBase, "Bip01 R Finger32");
        var rFinger42 = SearchHierarchyForBone(avatarBase, "Bip01 R Finger42");

        var lFinger02 = SearchHierarchyForBone(avatarBase, "Bip01 L Finger02");
        var lFinger12 = SearchHierarchyForBone(avatarBase, "Bip01 L Finger12");
        var lFinger22 = SearchHierarchyForBone(avatarBase, "Bip01 L Finger22");
        var lFinger32 = SearchHierarchyForBone(avatarBase, "Bip01 L Finger32");
        var lFinger42 = SearchHierarchyForBone(avatarBase, "Bip01 L Finger42");

        // Create fingertip objects
        var rFinger0Tip = new GameObject
        {
            name = "Bip01 R Finger0 Tip",
        };
        var rFinger1Tip = new GameObject
        {
            name = "Bip01 R Finger1 Tip"
        };
        var rFinger2Tip = new GameObject
        {
            name = "Bip01 R Finger2 Tip"
        };
        var rFinger3Tip = new GameObject
        {
            name = "Bip01 R Finger3 Tip"
        };
        var rFinger4Tip = new GameObject
        {
            name = "Bip01 R Finger4 Tip"
        };

        // Set parent of fingertips to the ending finger bones
        rFinger0Tip.transform.SetParent(rFinger02);
        rFinger1Tip.transform.SetParent(rFinger12);
        rFinger2Tip.transform.SetParent(rFinger22);
        rFinger3Tip.transform.SetParent(rFinger32);
        rFinger4Tip.transform.SetParent(rFinger42);

        // Set the local position of the child objects to be exactly at the fingertip.
        switch(female)
        {
            case true:
                rFinger0Tip.transform.localPosition = new Vector3(-.022f, 0, 0);
                rFinger1Tip.transform.localPosition = new Vector3(-.0185f, 0, 0);
                rFinger2Tip.transform.localPosition = new Vector3(-.0185f, 0, 0);
                rFinger3Tip.transform.localPosition = new Vector3(-.0172f, 0, 0);
                rFinger4Tip.transform.localPosition = new Vector3(-.013f, 0, 0);
                break;
            case false:
                rFinger0Tip.transform.localPosition = new Vector3(-.026f, 0, 0);
                rFinger1Tip.transform.localPosition = new Vector3(-.0205f, 0, 0);
                rFinger2Tip.transform.localPosition = new Vector3(-.02165f, 0, 0);
                rFinger3Tip.transform.localPosition = new Vector3(-.02f, 0, 0);
                rFinger4Tip.transform.localPosition = new Vector3(-.0175f, 0, 0);
                break;
        }



        // Create left fingertips
        var lFinger0Tip = new GameObject
        {
            name = "Bip01 L Finger0 Tip"
        };
        var lFinger1Tip = new GameObject
        {
            name = "Bip01 L Finger1 Tip"
        };
        var lFinger2Tip = new GameObject
        {
            name = "Bip01 L Finger2 Tip"
        };
        var lFinger3Tip = new GameObject
        {
            name = "Bip01 L Finger3 Tip"
        };
        var lFinger4Tip = new GameObject
        {
            name = "Bip01 L Finger4 Tip"
        };

        // Set the parent of the finger tips to the last finger bones
        lFinger0Tip.transform.SetParent(lFinger02);
        lFinger1Tip.transform.SetParent(lFinger12);
        lFinger2Tip.transform.SetParent(lFinger22);
        lFinger3Tip.transform.SetParent(lFinger32);
        lFinger4Tip.transform.SetParent(lFinger42);

        // Set the local position of the child objects to be exactly at the fingertip.
        switch (female)
        {
            case true:
                lFinger0Tip.transform.localPosition = new Vector3(-.022f, 0, 0);
                lFinger1Tip.transform.localPosition = new Vector3(-.0185f, 0, 0);
                lFinger2Tip.transform.localPosition = new Vector3(-.0185f, 0, 0);
                lFinger3Tip.transform.localPosition = new Vector3(-.0172f, 0, 0);
                lFinger4Tip.transform.localPosition = new Vector3(-.013f, 0, 0);
                break;
            case false:
                lFinger0Tip.transform.localPosition = new Vector3(-.026f, 0, 0);
                lFinger1Tip.transform.localPosition = new Vector3(-.0205f, 0, 0);
                lFinger2Tip.transform.localPosition = new Vector3(-.02165f, 0, 0);
                lFinger3Tip.transform.localPosition = new Vector3(-.02f, 0, 0);
                lFinger4Tip.transform.localPosition = new Vector3(-.0175f, 0, 0);
                break;
        }



    }


    /// <summary>
    /// Creates the wrist bones for the avatar.
    /// </summary>
    /// <param name="avatarBase"></param>
    private static void CreateWristBones(Transform avatarBase)
    {
        var rWrist = new GameObject
        {
            name = "Bip01 R Wrist"
        };
        var lWrist = new GameObject
        {
            name = "Bip01 L Wrist"
        };
        rWrist.transform.SetParent(SearchHierarchyForBone(avatarBase, "Bip01 R Forearm"));
        rWrist.transform.position = (SearchHierarchyForBone(avatarBase, "Bip01 R Forearm").position + SearchHierarchyForBone(avatarBase, "Bip01 R Hand").position) / 2;
        rWrist.transform.localEulerAngles = Vector3.zero;

        lWrist.transform.SetParent(SearchHierarchyForBone(avatarBase, "Bip01 L Forearm"));
        lWrist.transform.position = (SearchHierarchyForBone(avatarBase, "Bip01 L Forearm").position + SearchHierarchyForBone(avatarBase, "Bip01 L Hand").position) / 2;
        lWrist.transform.localEulerAngles = Vector3.zero;

        SearchHierarchyForBone(avatarBase, "Bip01 R Hand").SetParent(rWrist.transform);
        SearchHierarchyForBone(avatarBase, "Bip01 L Hand").SetParent(lWrist.transform);
    }


    /// <summary>
    /// Pairs the model's bones with avatar bones and forms a human description. If "twistCorrection" is true,
    /// additionally adds a "wrist bone" to help with mesh deformation upon twisting.
    /// </summary>
    /// <param name="avatarBase">The model with bones.</param>
    /// <returns>A mapped HumanDescription to be used in generating an avatar.</returns>
    /// <param name="twistCorrection">If set to <c>true</c> adds an additional bone to both arms ("Bip01 Wrist").</param>
    public static HumanDescription GenerateAvatarBoneMappings(Transform avatarBase, bool _twistCorrection, bool _createFingerTips)
    {
        // Define the bone collection.
        Dictionary<string, string> boneName = new Dictionary<string, string>
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
        
        var parentObject = new SkeletonBone
        {
            name = avatarBase.name,
            position = Vector3.zero,
            rotation = Quaternion.identity,
            scale = avatarBase.lossyScale
        };
        var rootBone = new SkeletonBone
        {
            name = "Bip01",
            position = avatarBase.localPosition,
            rotation = Quaternion.Euler(-90, 90, 0),
            scale = avatarBase.lossyScale
        };
        
        skeletonBones.Add(parentObject);
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
                humanBone.limit.useDefaultValues = true;
                humanBones[j++] = humanBone;
                
                string currentBoneName = boneName[humanName[i]];
                Transform currentBone = SearchHierarchyForBone(avatarBase, currentBoneName);
                SkeletonBone skeletonBone = new SkeletonBone
                {
                    name = currentBoneName,
                    position = currentBone.localPosition,
                    rotation = currentBone.localRotation,
                    scale = currentBone.lossyScale
                };
                skeletonBones.Add(skeletonBone);
            }

            //Add additional bones for wrist to reduce mesh deformation
            if (SearchHierarchyForBone(avatarBase, "Bip01 R Wrist") != null & _twistCorrection)
            {
                SkeletonBone rightWrist = new SkeletonBone
                {
                    name = "Bip01 R Wrist"
                };
                Transform rightWristTransform = SearchHierarchyForBone(avatarBase, rightWrist.name);
                rightWrist.position = rightWristTransform.localPosition;
                rightWrist.rotation = rightWristTransform.localRotation;
                rightWrist.scale = SearchHierarchyForBone(avatarBase, "Bip01 R Forearm").lossyScale;
                skeletonBones.Add(rightWrist);
                
                SkeletonBone leftWrist = new SkeletonBone
                {
                    name = "Bip01 L Wrist"
                };
                Transform leftWristTransform = SearchHierarchyForBone(avatarBase, leftWrist.name);
                leftWrist.position = leftWristTransform.localPosition;
                leftWrist.rotation = leftWristTransform.localRotation;
                leftWrist.scale = SearchHierarchyForBone(avatarBase, "Bip01 L Forearm").lossyScale;
                skeletonBones.Add(leftWrist);
            }

            // Add fingertip bones "Bip01 [side] Finger# Tip" to all fingers, 
            // "Bip01 [side] Finger#2" being the parent, if _createFingerTips is true
            if (_createFingerTips)
            {
                foreach (string side in new[] {"L", "R"})
                {
                    foreach (int finger in new[] {0, 1, 2, 3, 4})
                    {
                        string fingerName = $"Bip01 {side} Finger{finger}";
                        string fingerTipName = $"{fingerName} Tip";
                        Transform fingerTipTransform = SearchHierarchyForBone(avatarBase, fingerTipName);
                        if (fingerTipTransform != null)
                        {
                        SkeletonBone fingerTip = new()
                        {
                            name = fingerTipName,
                            position = fingerTipTransform.localPosition,
                            rotation = fingerTipTransform.localRotation,
                            scale = fingerTipTransform.lossyScale
                        };
                        skeletonBones.Add(fingerTip);
                        }                        

                    }
                }
            }




            i++;
        }
        // Use the generated HumanBone array and the SkeletonBone list (as array) to complete
        // the HumanDescription and assign it to the AvatarBuilder.
        var humanDescription = new HumanDescription
        {
            human = humanBones,
            hasTranslationDoF = true,
            skeleton = skeletonBones.ToArray()
        };
        return humanDescription;
    }
}
