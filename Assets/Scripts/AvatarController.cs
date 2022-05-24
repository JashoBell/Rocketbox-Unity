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
    public void FindOffset()
    {
        sourceObject.LookAt(Vector3.ProjectOnPlane(trackerObject.forward, Vector3.up).normalized);
    }
}


public class AvatarController : MonoBehaviour
{
[SerializeField] public List<unityTrackerFollow> avatarParts;
[SerializeField] private Transform headIK, headCam;
[SerializeField] private Vector3 headOffset;
private Vector3 beginOrientation;


private void LateUpdate() {
    if(beginOrientation == Vector3.zero)
    {
        beginOrientation = transform.rotation.eulerAngles;
    }
      
    transform.InverseTransformDirection(Vector3.ProjectOnPlane(headIK.forward, Vector3.up).normalized);
    transform.TransformPoint(headIK.position + headOffset);  
    if(headOffset == Vector3.zero)
    {
        var headCamForward = new Vector3(headCam.transform.forward.x, 0f, headCam.forward.z);
        Vector3 angle = Quaternion.LookRotation(headCamForward).eulerAngles;
        headOffset = transform.position - headIK.position;
    }
    foreach (unityTrackerFollow p in avatarParts)
    {
        p.UpdateTransform();
        //p.FindOffset();
    }
}

}
