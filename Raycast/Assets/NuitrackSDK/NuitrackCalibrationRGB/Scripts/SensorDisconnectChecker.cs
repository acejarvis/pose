using UnityEngine;
using nuitrack;

namespace NuitrackSDK.VicoVRCalibration
{
    public class SensorDisconnectChecker : MonoBehaviour
    {
        public delegate void ConnectionStatusChange();
        static public event ConnectionStatusChange SensorConnectionTimeOut;
        static public event ConnectionStatusChange SensorReconnected;

        bool connection = true;
        bool connectionProblem = false;

        void OnEnable()
        {
            NuitrackManager.onSkeletonTrackerUpdate += SkeletonUpdate;
            Nuitrack.onIssueUpdateEvent += NoConnectionIssue;
        }

        void SkeletonUpdate(SkeletonData skeletonData)
        {
            if (connection)
            {
                if (connectionProblem && SensorReconnected != null)
                    SensorReconnected();
                connectionProblem = false;
            }
            else
            {
                connection = true;
                SensorReconnected?.Invoke();
            }
        }

        void NoConnectionIssue(nuitrack.issues.IssuesData issData)
        {
            if (issData.GetIssue<nuitrack.issues.SensorIssue>() != null)
            {
                if (SensorConnectionTimeOut != null)
                    SensorConnectionTimeOut();
                connectionProblem = true;
            }
            else
            {
                if (connectionProblem && SensorReconnected != null)
                    SensorReconnected();
                connectionProblem = false;
            }
        }

        void OnDisable()
        {
            NuitrackManager.onSkeletonTrackerUpdate -= SkeletonUpdate;
            Nuitrack.onIssueUpdateEvent -= NoConnectionIssue;
        }

        void OnDestroy()
        {
            NuitrackManager.onSkeletonTrackerUpdate -= SkeletonUpdate;
        }
    }
}
