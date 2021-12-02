using UnityEngine;


namespace NuitrackSDK.Avatar
{
    /// <summary>
    /// Wrapper for Nuitrack Joint <see cref="nuitrack.Joint"/>
    /// </summary>
    public class JointTransform
    {
        nuitrack.Joint joint;

        public JointTransform(bool isActive, nuitrack.Joint joint)
        {
            this.joint = joint;
            IsActive = isActive;
        }

        /// <summary>
        /// Is this joint active
        /// </summary>
        public bool IsActive
        {
            get;
            private set;
        }

        /// <summary>
        /// The corresponding type of Unity bone
        /// </summary>
        public HumanBodyBones HumanBodyBone
        {
            get
            {
                return joint.Type.ToUnityBones();
            }
        }

        /// <summary>
        /// Joint position in meters
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return joint.ToVector3() * 0.001f;
            }
        }

        /// <summary>
        /// Joint orientation
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return joint.ToQuaternion(); //Quaternion.Inverse(CalibrationInfo.SensorOrientation)
            }
        }

        /// <summary>
        /// Mirrored orientation of the joint
        /// </summary>
        public Quaternion RotationMirrored
        {
            get
            {
                return joint.ToQuaternionMirrored();
            }
        }

        /// <summary>
        /// Projection and normalized joint coordinates
        /// </summary>
        public Vector2 Proj
        {
            get
            {
                return new Vector2(Mathf.Clamp01(joint.Proj.X), Mathf.Clamp01(joint.Proj.Y));
            }
        }
    }

    /// <summary>
    /// The base class of the avatar. Use it to create your own avatars.
    /// </summary>
    public abstract class BaseAvatar : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField, NuitrackSDKInspector] 
        bool useCurrentUserTracker = true;
        
        [SerializeField, NuitrackSDKInspector]
        int skeletonID = 1;

        [SerializeField, NuitrackSDKInspector] 
        float jointConfidence = 0.1f;

        protected ulong lastTimeStamp = 0;

        /// <summary>
        /// If True, the current user tracker is used, otherwise the user specified by ID is used <see cref="SkeletonID"/>
        /// </summary>
        public bool UseCurrentUserTracker
        {
            get
            {
                return useCurrentUserTracker;
            }
            set
            {
                useCurrentUserTracker = value;
            }
        }

        /// <summary>
        /// ID of the current user
        /// For the case when current user tracker <see cref="UseCurrentUserTracker"/> of is used, the ID of the active user will be returned
        /// If current user tracker is used and a new ID is set, tracking of the current user will stop
        /// </summary>
        public int SkeletonID
        {
            get
            {
                if(UseCurrentUserTracker)
                    return CurrentUserTracker.CurrentSkeleton != null ? CurrentUserTracker.CurrentSkeleton.ID : 0;
                else
                    return skeletonID;
            }
            set
            {
                if (value >= MinSkeletonID && value <= MaxSkeletonID)
                {
                    skeletonID = value;

                    if(useCurrentUserTracker)
                        Debug.Log(string.Format("CurrentUserTracker mode was disabled for {0}", gameObject.name));

                    useCurrentUserTracker = false;
                }
                else
                    throw new System.Exception(string.Format("The Skeleton ID must be within the bounds of [{0}, {1}]", MinSkeletonID, MaxSkeletonID));
            }
        }

        /// <summary>
        /// Minimum allowed ID
        /// </summary>
        public int MinSkeletonID
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Maximum allowed ID
        /// </summary>
        public int MaxSkeletonID
        {
            get
            {
                return 6;
            }
        }

        /// <summary>
        /// Confidence threshold for detected joints
        /// </summary>
        public float JointConfidence
        {
            get
            {
                return jointConfidence;
            }
            set
            {
                jointConfidence = value;
            }
        }

        /// <summary>
        /// True if there is a control skeleton.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return ControllerSkeleton != null;
            }
        }

        /// <summary>
        /// The controler skeleton. Maybe null
        /// </summary>
        public nuitrack.Skeleton ControllerSkeleton
        {
            get
            {
                if (useCurrentUserTracker)
                    return CurrentUserTracker.CurrentSkeleton;
                else
                    return NuitrackManager.SkeletonData?.GetSkeletonByID(skeletonID);
            }
        }

        /// <summary>
        /// Get a shell object for the specified joint
        /// </summary>
        /// <param name="jointType">Joint type</param>
        /// <returns>Shell object <see cref="JointTransform"/></returns>
        public JointTransform GetJoint(nuitrack.JointType jointType)
        {
            if (!IsActive)
                return null;

            nuitrack.Joint joint = ControllerSkeleton.GetJoint(jointType);
            JointTransform jointTransform = new JointTransform(joint.Confidence >= jointConfidence, joint);

            return jointTransform;
        }

        protected virtual void Update()
        {
            nuitrack.Skeleton skeleton = ControllerSkeleton;

            if (skeleton == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
                return;

            lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;
            if(skeleton != null)
                ProcessSkeleton(skeleton);
        }

        /// <summary>
        /// Redefine this method to implement skeleton processing
        /// </summary>
        /// <param name="skeleton">Skeleton <see cref="nuitrack.Skeleton/>"/></param>
        protected abstract void ProcessSkeleton(nuitrack.Skeleton skeleton);
    }
}