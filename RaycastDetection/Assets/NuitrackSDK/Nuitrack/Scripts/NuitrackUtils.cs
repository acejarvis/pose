using UnityEngine;

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using JointType = nuitrack.JointType;

public static class NuitrackUtils
{
    #region Transform
    public static Vector3 ToVector3(this nuitrack.Vector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Vector3 ToVector3(this nuitrack.Joint joint)
    {
        return new Vector3(joint.Real.X, joint.Real.Y, joint.Real.Z);
    }

    public static Quaternion ToQuaternion(this nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(joint.Orient.Matrix[1], joint.Orient.Matrix[4], joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        return Quaternion.LookRotation(jointForward, jointUp);
    }

    public static Quaternion ToQuaternionMirrored(this nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(-joint.Orient.Matrix[1], joint.Orient.Matrix[4], -joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], -joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        if (jointForward.magnitude < 0.01f)
            return Quaternion.identity; //should not happen

        return Quaternion.LookRotation(jointForward, jointUp);
    }

    #endregion

    #region SkeletonUltils

    static readonly Dictionary<JointType, HumanBodyBones> nuitrackToUnity = new Dictionary<JointType, HumanBodyBones>()
    {
        {JointType.Head,                HumanBodyBones.Head},
        {JointType.Neck,                HumanBodyBones.Neck},
        {JointType.LeftCollar,          HumanBodyBones.LeftShoulder},
        {JointType.RightCollar,         HumanBodyBones.RightShoulder},
        {JointType.Torso,               HumanBodyBones.Spine},
        {JointType.Waist,               HumanBodyBones.Hips},


        {JointType.LeftFingertip,       HumanBodyBones.LeftMiddleDistal},
        {JointType.LeftHand,            HumanBodyBones.LeftMiddleProximal},
        {JointType.LeftWrist,           HumanBodyBones.LeftHand},
        {JointType.LeftElbow,           HumanBodyBones.LeftLowerArm},
        {JointType.LeftShoulder,        HumanBodyBones.LeftUpperArm},

        {JointType.RightFingertip,      HumanBodyBones.RightMiddleDistal},
        {JointType.RightHand,           HumanBodyBones.RightMiddleProximal},
        {JointType.RightWrist,          HumanBodyBones.RightHand},
        {JointType.RightElbow,          HumanBodyBones.RightLowerArm},
        {JointType.RightShoulder,       HumanBodyBones.RightUpperArm},


        {JointType.LeftFoot,            HumanBodyBones.LeftToes},
        {JointType.LeftAnkle,           HumanBodyBones.LeftFoot},
        {JointType.LeftKnee,            HumanBodyBones.LeftLowerLeg},
        {JointType.LeftHip,             HumanBodyBones.LeftUpperLeg},

        {JointType.RightFoot,           HumanBodyBones.RightToes},
        {JointType.RightAnkle,          HumanBodyBones.RightFoot},
        {JointType.RightKnee,           HumanBodyBones.RightLowerLeg},
        {JointType.RightHip,            HumanBodyBones.RightUpperLeg},
    };

    /// <summary>
    /// Returns the appropriate HumanBodyBones  for nuitrack.JointType
    /// </summary>
    /// <param name="nuitrackJoint">nuitrack.JointType</param>
    /// <returns>HumanBodyBones</returns>
    public static HumanBodyBones ToUnityBones(this JointType nuitrackJoint)
    {
        if (nuitrackToUnity.ContainsKey(nuitrackJoint))
            return nuitrackToUnity[nuitrackJoint];
        else
            return HumanBodyBones.LastBone;
    }

    static readonly Dictionary<JointType, JointType> mirroredJoints = new Dictionary<JointType, JointType>() {
        {JointType.LeftShoulder, JointType.RightShoulder},
        {JointType.RightShoulder, JointType.LeftShoulder},
        {JointType.LeftElbow, JointType.RightElbow},
        {JointType.RightElbow, JointType.LeftElbow},
        {JointType.LeftWrist, JointType.RightWrist},
        {JointType.RightWrist, JointType.LeftWrist},
        {JointType.LeftFingertip, JointType.RightFingertip},
        {JointType.RightFingertip, JointType.LeftFingertip},

        {JointType.LeftHip, JointType.RightHip},
        {JointType.RightHip, JointType.LeftHip},
        {JointType.LeftKnee, JointType.RightKnee},
        {JointType.RightKnee, JointType.LeftKnee},
        {JointType.LeftAnkle, JointType.RightAnkle},
        {JointType.RightAnkle, JointType.LeftAnkle},
    };

    public static JointType TryGetMirrored(this JointType joint)
    {
        JointType mirroredJoint = joint;
        if (NuitrackManager.DepthSensor.IsMirror() && mirroredJoints.ContainsKey(joint))
        {
            mirroredJoints.TryGetValue(joint, out mirroredJoint);
        }

        return mirroredJoint;
    }

    static readonly Dictionary<JointType, JointType> parentJoints = new Dictionary<JointType, JointType>()
    {
        {JointType.Head,                JointType.Neck},
        {JointType.Neck,                JointType.LeftCollar},
        {JointType.LeftCollar,          JointType.Torso},
        {JointType.RightCollar,         JointType.Torso},     
        {JointType.Torso,               JointType.Waist},
        {JointType.Waist,               JointType.None},

        {JointType.LeftShoulder,        JointType.LeftCollar},
        {JointType.LeftElbow,           JointType.LeftShoulder},
        {JointType.LeftWrist,           JointType.LeftElbow},
        {JointType.LeftHand,            JointType.LeftWrist},

        {JointType.RightShoulder,       JointType.RightCollar},
        {JointType.RightElbow,          JointType.RightShoulder},
        {JointType.RightWrist,          JointType.RightElbow},
        {JointType.RightHand,           JointType.RightWrist},

        {JointType.LeftHip,             JointType.Waist},
        {JointType.LeftKnee,            JointType.LeftHip},
        {JointType.LeftAnkle,           JointType.LeftKnee},
        {JointType.LeftFoot,            JointType.LeftAnkle},

        {JointType.RightHip,            JointType.Waist},
        {JointType.RightKnee,           JointType.RightHip},
        {JointType.RightAnkle,          JointType.RightKnee},
        {JointType.RightFoot,           JointType.RightAnkle},
    };

    public static JointType GetParent(this JointType jointType)
    {
        if (parentJoints.ContainsKey(jointType))
            return parentJoints[jointType];
        else
            return JointType.None;
    }

    #endregion

    #region JsonUtils

    static Regex regex = null;

    // A pattern for detecting any numbers, including exponential notation
    static string pattern = "\"-?[\\d]*\\.?[\\d]+(e[-+][\\d]+)?\"";

    public static T FromJson<T>(string json)
    {
        try
        {
            json = json.Replace("\"\"", "[]");

            if (regex == null)
                regex = new Regex(pattern);

            foreach (Match match in regex.Matches(json))
            {
                string withot_quotation_marks = match.Value.Replace("\"", "");
                json = json.Replace(match.Value, withot_quotation_marks);
            }

            T outData = JsonUtility.FromJson<T>(json);

            return outData;
        }
        catch (System.Exception e)
        {
            Debug.Log(string.Format("Json parsing failure\n{0}", e.Message));
            return default(T);
        }
    }

    #endregion
}