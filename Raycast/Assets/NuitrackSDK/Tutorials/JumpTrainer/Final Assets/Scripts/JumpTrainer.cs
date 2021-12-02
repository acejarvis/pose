using System;

using UnityEngine;
using UnityEngine.UI;


public class JumpTrainer : MonoBehaviour
{
    [SerializeField] AspectRatioFitter aspectRatioFitter;
    Plane floorPlane;

    [SerializeField] float jumpStartHeight = 0.1f;
    [SerializeField] RectTransform baseRect;

    [Header("Current jump UI")]
    [SerializeField] RectTransform currentJumpLine;
    [SerializeField] Text currentJumpLabel;

    bool lefOnFloorLastFrame = true;

    [Header ("Floor UI")]
    [SerializeField] RectTransform floorLine;

    [Header ("Best jump UI")]
    [SerializeField] RectTransform bestJumpLine;
    [SerializeField] Text bestJumpLabel;

    public float BestJumpHeight
    {
        get;
        private set;
    }

    public float CurrentJumpHeight
    {
        get;
        private set;
    }

    bool LegOnFloor(nuitrack.Skeleton skeleton)
    {
        Vector3 leftAnkle = skeleton.GetJoint(nuitrack.JointType.LeftAnkle).ToVector3();
        Vector3 rightAnkle = skeleton.GetJoint(nuitrack.JointType.RightAnkle).ToVector3();

        Vector3 floorLeftAnkle = floorPlane.ClosestPointOnPlane(leftAnkle);
        Vector3 floorRightAnkle = floorPlane.ClosestPointOnPlane(rightAnkle);

        return Vector3.Distance(leftAnkle, floorLeftAnkle) <= jumpStartHeight * 1000 || 
            Vector3.Distance(rightAnkle, floorRightAnkle) <= jumpStartHeight * 1000;
    }

    float JumpHeight(nuitrack.Skeleton skeleton, out nuitrack.JointType lowerJointType)
    {
        lowerJointType = nuitrack.JointType.Head;
        float jumpHeight = float.MaxValue;

        foreach (nuitrack.JointType jointType in Enum.GetValues(typeof(nuitrack.JointType)))
        {
            Vector3 jointPoint = skeleton.GetJoint(jointType).ToVector3();
            Vector3 floorPoint = floorPlane.ClosestPointOnPlane(jointPoint);

            float floorDistance = Vector3.Distance(jointPoint, floorPoint);

            if (floorDistance < jumpHeight)
            {
                lowerJointType = jointType;
                jumpHeight = floorDistance;
            }
        }

        return jumpHeight * 0.001f;
    }

    Vector3 FrameSpaceProjPoint(nuitrack.Vector3 realPoint, nuitrack.DepthFrame frame)
    {
        nuitrack.Vector3 point = NuitrackManager.DepthSensor.ConvertRealToProjCoords(realPoint);

        point.X /= frame.Cols;
        point.Y /= frame.Rows;

        return point.ToVector3();
    }

    nuitrack.Vector3 NuitrackVector(Vector3 vector)
    {
        return new nuitrack.Vector3(vector.x, vector.y, vector.z);
    }

    void Start()
    {
        DisplayLines(false);
    }

    void DisplayLines(bool visible)
    {
        currentJumpLine.gameObject.SetActive(visible);
        floorLine.gameObject.SetActive(visible);
        bestJumpLine.gameObject.SetActive(visible);
    }
    
    void Update()
    {
        if(NuitrackManager.ColorFrame != null)
            aspectRatioFitter.aspectRatio = (float)NuitrackManager.ColorFrame.Cols / NuitrackManager.ColorFrame.Rows;

        if (NuitrackManager.UserFrame == null || CurrentUserTracker.CurrentSkeleton == null)
        {
            DisplayLines(false);
            return;
        }

        DisplayLines(true);   

        Vector3 floorPoint = NuitrackManager.UserFrame.Floor.ToVector3();
        Vector3 floorNormal = NuitrackManager.UserFrame.FloorNormal.ToVector3().normalized;

        floorPlane = new Plane(floorNormal, floorPoint);

        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        nuitrack.JointType lowerJoint;
        float jumpHeight = JumpHeight(skeleton, out lowerJoint);

        if (!LegOnFloor(skeleton))
        {
            if (lefOnFloorLastFrame)
            {
                CurrentJumpHeight = 0;
                lefOnFloorLastFrame = false;
            }

            if (jumpHeight > CurrentJumpHeight)
                CurrentJumpHeight = jumpHeight;

            if (CurrentJumpHeight > BestJumpHeight)
            {
                BestJumpHeight = CurrentJumpHeight;
                bestJumpLabel.text = string.Format("Best: {0:F2}", BestJumpHeight);
            }
        }
        else
            lefOnFloorLastFrame = true;

        nuitrack.Vector3 jointPosition = skeleton.GetJoint(lowerJoint).Real;
        Vector3 currentJumpScreenPoint = FrameSpaceProjPoint(jointPosition, NuitrackManager.DepthFrame);

        currentJumpLine.anchoredPosition = new Vector2(0, baseRect.rect.height * currentJumpScreenPoint.y);
        currentJumpLabel.text = string.Format("Current: {0:F2}", CurrentJumpHeight);

        Vector3 waistPosition = skeleton.GetJoint(nuitrack.JointType.Waist).ToVector3();
        Vector3 floorWaistPosition = floorPlane.ClosestPointOnPlane(waistPosition);
        Vector3 screenFloorWaistPoint = FrameSpaceProjPoint(NuitrackVector(floorWaistPosition), NuitrackManager.DepthFrame);

        floorLine.anchoredPosition = new Vector2(0, baseRect.rect.height * screenFloorWaistPoint.y);

        Vector3 bestJumpPosition = floorWaistPosition + floorNormal * BestJumpHeight * 1000;
        Vector3 screenBestJumpPoint = FrameSpaceProjPoint(NuitrackVector(bestJumpPosition), NuitrackManager.DepthFrame);

        bestJumpLine.anchoredPosition = new Vector2(0, baseRect.rect.height * screenBestJumpPoint.y);
    }
}
