using UnityEngine;

using System.Collections.Generic;

public class RigidbodySkeletonController : MonoBehaviour
{
    [Header ("Rigidbody")]
    [SerializeField] List<nuitrack.JointType> targetJoints;
    [SerializeField] GameObject rigidBodyJoint;

    [SerializeField, Range(0.1f, 64f)] float smoothSpeed = 24f;

    Transform space;

    Dictionary<nuitrack.JointType, Rigidbody> rigidbodyObj;

    public int ID
    {
        get;
        private set;
    }


    public void Initialize(int id, Transform space)
    {
        ID = id;
        this.space = space;

        rigidbodyObj = new Dictionary<nuitrack.JointType, Rigidbody>();

        foreach (nuitrack.JointType jointType in targetJoints)
        {
            GameObject jointObj = Instantiate(rigidBodyJoint, transform);
            jointObj.name = string.Format("{0}_rigidbody_{1}", jointType.ToString(), id);

            Rigidbody rigidbody = jointObj.GetComponent<Rigidbody>();
            rigidbodyObj.Add(jointType, rigidbody);
        }
    }

    void FixedUpdate()
    {
        if (NuitrackManager.SkeletonData == null)
            return;

        nuitrack.Skeleton skeleton = NuitrackManager.SkeletonData.GetSkeletonByID(ID);

        if (skeleton == null)
            return;

        foreach(KeyValuePair<nuitrack.JointType, Rigidbody> rigidbodyJoint in rigidbodyObj)
        {
            Vector3 newPosition = skeleton.GetJoint(rigidbodyJoint.Key).Real.ToVector3() * 0.001f;

            Vector3 spacePostion = space == null ? newPosition : space.TransformPoint(newPosition);
            Vector3 lerpPosition = Vector3.Lerp(rigidbodyJoint.Value.position, spacePostion, Time.deltaTime * smoothSpeed);

            rigidbodyJoint.Value.MovePosition(lerpPosition);
        }
    }
}
