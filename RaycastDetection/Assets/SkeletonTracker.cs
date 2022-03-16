#region Description

// The script performs a direct translation of the skeleton using only the position data of the joints.
// Objects in the skeleton will be created when the scene starts.

#endregion

using UnityEngine;
using Assets.Scenes;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;

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
    Renderer deviceRenderer2;
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

        GetDeviceStatus();
        StartCoroutine(ExampleCoroutine());


        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = new Vector3(0, 0.5f, 0);

        GameObject light0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        light0.transform.position = new Vector3(-1.271f, 0.6f, 2.272f);
        light0.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        light0.name = "LightBulb0";
        deviceRenderer = light0.GetComponent<Renderer>();

    }

    IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);
        createObjectsFromJson();
        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
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
                if (hit.collider.gameObject.name == "LightBulb0")
                {
                    hitCount++;
                    detectMessage = "Hit Ball";
                    deviceRenderer.material.SetColor("_Color", Color.green);
                }
                else if (hit.collider.gameObject.name == "LightBulb1")
                {
                    detectMessage = "Hit Ball";
                    deviceRenderer2.material.SetColor("_Color", Color.green);
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



    void createObjectsFromJson()
    {

        using (StreamReader r = new StreamReader("file.json"))
        {
            string jsonString = r.ReadToEnd();
            List<Vector3> objectList = JsonConvert.DeserializeObject<List<Vector3>>(jsonString);
            int nameOrder = 1;

            foreach (var item in objectList)
            {
                GameObject light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                light.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                light.transform.position = item;
                light.name = "LightBulb" + nameOrder.ToString();
                nameOrder++;
            }
        }
    }

    // Device Control Command
    async Task DeviceControl(bool isTurnOn)
    {
        hitCount = 0;
        if (isTurnOn)
        {
            deviceRenderer.material.SetColor("_Color", Color.yellow);
            await TurnON();
        }
        else
        {
            deviceRenderer.material.SetColor("_Color", Color.white);
            await TurnON();
        }
    }

    // IoT Control
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

     async Task GetDeviceStatus()
    {
        var device = new TuyaPlug()
        {
            IP = "192.168.31.88",
            LocalKey = "68b3c9e37b8c2cfc",
            Id = "137107483c71bf2296d3"
        };
        var status = await device.GetStatus();
        deviceStatusON = status.Powered;
    }
}