#region Description

// The script performs a direct translation of the skeleton using only the position data of the joints.
// Objects in the skeleton will be created when the scene starts.

#endregion

using UnityEngine;
using Assets.Scenes;
using System.Threading.Tasks;

public class SkeletonTracker : MonoBehaviour
{
    string message = "";
    string detectMessage = "";
    bool color = false;

    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;
    public Vector3 Foward;
    GameObject device;
    Renderer deviceRenderer;
    int hitCount = 0;
    bool deviceStatusON = false;


    void Start()
    {
        // Create Skeleton GameObject
        Application.targetFrameRate = 30;
        CreatedJoint = new GameObject[typeJoint.Length];
        for (int q = 0; q < typeJoint.Length; q++)
        {
            CreatedJoint[q] = Instantiate(PrefabJoint);
            CreatedJoint[q].transform.SetParent(transform);
        }
        message = "Skeleton created";

        device = GameObject.Find("LightBulb");
        deviceRenderer = device.GetComponent<Renderer>();
    }

    void Update()
    {
        if (CurrentUserTracker.CurrentUser != 0)
        {
            // Skeleton Tracking
            nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
            message = "Skeleton found";
            color = true;
            for (int q = 0; q < typeJoint.Length; q++)
            {
                nuitrack.Joint joint = skeleton.GetJoint(typeJoint[q]);
                Vector3 newPosition = 0.001f * joint.ToVector3();
                // next step: add filter to smooth the joint data
                CreatedJoint[q].transform.localPosition = newPosition;
            }

            // Raycast Detection
            Vector3 fromPosition = CreatedJoint[6].transform.localPosition;
            Vector3 toPosition = CreatedJoint[8].transform.localPosition;
            Foward = (toPosition - fromPosition) * 10;
            Debug.DrawRay(toPosition, Foward, Color.green);
            RaycastHit hit;
            if (Physics.Raycast(toPosition, Foward, out hit))
            {
                detectMessage = "Active";
                // Targeting specific devices
                if (hit.collider.gameObject.name == "LightBulb")
                {
                    hitCount++;
                    detectMessage = "Hit Ball";
                    deviceRenderer.material.SetColor("_Color", Color.green);
                }
            }
            else
            {
                hitCount = 0;
                detectMessage = "Inactive";
                if (deviceStatusON) deviceRenderer.material.SetColor("_Color", Color.yellow);
                else deviceRenderer.material.SetColor("_Color", Color.white);
            }
            // Added hit count to filter out jitter motion
            if (hitCount >= 30)
            {
                deviceStatusON = !deviceStatusON;
                DeviceControl(deviceStatusON);
            }
            message += " Ray Vector:" + Foward.ToString() + " " + detectMessage;
        }
        else
        {
            message = "Skeleton not found";
            color = false;
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    // Display the message on the screen
    void OnGUI()
    {
        if (color) GUI.color = Color.green;
        else GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }

    // Device Control Command
    void DeviceControl(bool isTurnOn)
    {
        hitCount = 0;
        if (isTurnOn)
        {
            deviceRenderer.material.SetColor("_Color", Color.yellow);
            TurnON();
        }
        else
        {
            deviceRenderer.material.SetColor("_Color", Color.white);
            TurnON();
        }
    }

    // IoT Control
    static async Task TurnON()
    {
        var device = new TuyaPlug()
        {
            IP = "YOUR_TUYA_DEVICE_IP",
            LocalKey = "YOUR_TUYA_LOCAL_KEY",
            Id = "YOUR_TUYA_DEVICE_ID"
        };
        var status = await device.GetStatus();
        await device.SetStatus(!status.Powered); // toggle power
    }
}