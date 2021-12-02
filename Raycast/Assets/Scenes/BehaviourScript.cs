#region Description

// The script performs a direct translation of the skeleton using only the position data of the joints.
// Objects in the skeleton will be created when the scene starts.

#endregion


using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Assets.Scenes;


[AddComponentMenu("NuitrackSDK/Tutorials/First Project/NativeAvatar")]
public class BehaviourScript : MonoBehaviour
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
            Vector3 fromPosition = CreatedJoint[7].transform.localPosition;
            Vector3 toPosition = CreatedJoint[9].transform.localPosition;
            Foward = (toPosition - fromPosition) * 10;
            Debug.DrawRay(toPosition, Foward, Color.green);
            RaycastHit hit;

            if (Physics.Raycast(toPosition, Foward, out hit))
            {
                detectMessage = "Active";
                if (hit.collider.gameObject.name == "LightBulb")
                {
                    hitCount++;
                    detectMessage = "Hit Ball";

                }

            }
            else
            {
                hitCount = 0;
                detectMessage = "Inactive";

            }

            if (hitCount >= 30)
            {
                deviceStatusON = !deviceStatusON;
                DeviceControl(deviceStatusON);
            }




            message += " Pointer Ray:" + Foward.ToString() + " " + detectMessage;
            print(message);
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

    void DeviceControl(bool isTurnOn)
    {
        hitCount = 0;
        if (isTurnOn)
        {
            deviceRenderer.material.SetColor("_Color", Color.yellow);

        }
        else
        {
            deviceRenderer.material.SetColor("_Color", Color.white);

        }
    }

    static async Task TurnON()
    {
        var device = new TuyaPlug()
        {
            IP = "192.168.31.88",
            LocalKey = "68b3c9e37b8c2cfc",
            Id = "137107483c71bf2296d3"
        };
        var status = await device.GetStatus();
        await device.SetStatus(!status.Powered); // toggle power

    }

    // Display the message on the screen
    void OnGUI()
    {
        if (color) GUI.color = Color.green;
        else GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }



}





