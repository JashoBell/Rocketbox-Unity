using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class unityTrackerFollow
{
    public Transform sourceObject, trackerObject;
    public Vector3 positionOffset, rotationOffset;

    public void UpdateTransform()
    {
        sourceObject.position = trackerObject.TransformPoint(positionOffset);
        sourceObject.rotation = trackerObject.rotation * Quaternion.Euler(rotationOffset);
    }
}


public class AvatarController : MonoBehaviour
{
[SerializeField] public List<unityTrackerFollow> avatarParts;
[SerializeField] private Transform headIK;
[SerializeField] private Vector3 headOffset;
private void LateUpdate() {
    transform.position = headIK.position + headOffset;    
    transform.forward = Vector3.ProjectOnPlane(headIK.forward, Vector3.up).normalized;
    foreach (unityTrackerFollow p in avatarParts)
    {
        p.UpdateTransform();
    }
}

}
