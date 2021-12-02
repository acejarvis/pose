using UnityEngine;
using System.Runtime.InteropServices;

public class ARNuitrack : MonoBehaviour
{
    ulong frameTimestamp;

    [Header("RGB shader")]
    Texture2D rgbTexture2D;

    [Header("Mesh generator")]
    [SerializeField] MeshGenerator meshGenerator;
    [SerializeField] new Camera camera;

    [Header("Sensor params")]
    [SerializeField, Range(0.1f, 32f), Tooltip("Check the specification of your sensor on the manufacturer's website")]
    float maxDepthSensor = 8;

    ComputeBuffer depthDataBuffer;
    byte[] depthDataArray = null;

    [Header("Floor")]
    [SerializeField] Transform sensorSpace;

    Plane floorPlane;

    [SerializeField, Range(0.001f, 1f)] float deltaHeight = 0.1f;
    [SerializeField, Range(0.1f, 90f)] float deltaAngle = 3f;
    [SerializeField, Range(0.1f, 32f)] float floorCorrectionSpeed = 8f;  

    void Update()
    {
        nuitrack.ColorFrame colorFrame = NuitrackManager.ColorFrame;
        nuitrack.DepthFrame depthFrame = NuitrackManager.DepthFrame;
        nuitrack.UserFrame userFrame = NuitrackManager.UserFrame;

        if (colorFrame == null || depthFrame == null  || userFrame == null || frameTimestamp == depthFrame.Timestamp)
            return;

        frameTimestamp = depthFrame.Timestamp;
 
        if(meshGenerator.Mesh == null)
            meshGenerator.Generate(depthFrame.Cols, depthFrame.Rows);

        UpdateRGB(colorFrame);
        UpdateHieghtMap(depthFrame);
        FitMeshIntoFrame(depthFrame);

        UpdateFloor(userFrame);
    }

    void UpdateRGB(nuitrack.ColorFrame frame)
    {
        if (rgbTexture2D == null)
        {
            rgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
            meshGenerator.Material.SetTexture("_MainTex", rgbTexture2D);
        }

        rgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
        rgbTexture2D.Apply(); 
    }

    void FitMeshIntoFrame(nuitrack.DepthFrame frame)
    {        
        float frameAspectRatio = (float)frame.Cols / frame.Rows;
        float targetAspectRatio = camera.aspect < frameAspectRatio ? camera.aspect : frameAspectRatio;

        float vAngle = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float scale = Vector3.Distance(meshGenerator.transform.position, camera.transform.position) * Mathf.Tan(vAngle) * targetAspectRatio;

        meshGenerator.transform.localScale = new Vector3(scale * 2, scale * 2, 1);
    }

    void UpdateHieghtMap(nuitrack.DepthFrame frame)
    {
        if (depthDataBuffer == null)
        {
            //We put the source data in the buffer, but the buffer does not support types
            //that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32))
            depthDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
            meshGenerator.Material.SetBuffer("_DepthFrame", depthDataBuffer);

            meshGenerator.Material.SetInt("_textureWidth", frame.Cols);
            meshGenerator.Material.SetInt("_textureHeight", frame.Rows);

            depthDataArray = new byte[frame.DataSize];
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        depthDataBuffer.SetData(depthDataArray);

        meshGenerator.Material.SetFloat("_maxDepthSensor", maxDepthSensor);
        meshGenerator.transform.localPosition = Vector3.forward * maxDepthSensor;

        Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(camera.transform.position);
        meshGenerator.Material.SetVector("_CameraPosition", localCameraPosition);
    }

    void UpdateFloor(nuitrack.UserFrame frame)
    {
        Vector3 floorPoint = frame.Floor.ToVector3() * 0.001f;
        Vector3 floorNormal = frame.FloorNormal.ToVector3().normalized;

        Plane newFloor = new Plane(floorNormal, floorPoint);

        if (floorPlane.Equals(default(Plane)))
            floorPlane = new Plane(floorNormal, floorPoint);

        Vector3 newFloorSensor = newFloor.ClosestPointOnPlane(Vector3.zero);
        Vector3 floorSensor = floorPlane.ClosestPointOnPlane(Vector3.zero);

        if (Vector3.Angle(newFloor.normal, floorPlane.normal) >= deltaAngle || Mathf.Abs(newFloorSensor.y - floorSensor.y) >= deltaHeight)
            floorPlane = new Plane(floorNormal, floorPoint);

        Vector3 reflectNormal = Vector3.Reflect(-floorPlane.normal, Vector3.up);
        Vector3 forward = sensorSpace.forward;
        Vector3.OrthoNormalize(ref reflectNormal, ref forward);

        Quaternion targetRotation = Quaternion.LookRotation(forward, reflectNormal);
        camera.transform.localRotation = Quaternion.Lerp(camera.transform.localRotation, targetRotation, Time.deltaTime * floorCorrectionSpeed);

        Vector3 localRoot = camera.transform.localPosition;
        localRoot.y = -floorSensor.y;
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, localRoot, Time.deltaTime * floorCorrectionSpeed);
    }

    private void OnDestroy()
    {
        if(rgbTexture2D != null)
            Destroy(rgbTexture2D);

        if (depthDataBuffer != null)
            depthDataBuffer.Release();
    }
}
