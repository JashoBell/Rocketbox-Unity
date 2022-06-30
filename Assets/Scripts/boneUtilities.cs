using System.Collections;
using System.Collections.Generic;
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
