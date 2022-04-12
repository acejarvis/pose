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
using UnityEngine.Networking;

public class SkeletonTracker : MonoBehaviour
{


    string message = "";
    string detectMessage = "";
    bool color = false;

    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;
    public Vector3 Foward, Foward1;
    List<GameObject> deviceObjects;
    List<Renderer> deviceRenderers;
    int[] hitCount = { 0, 0, 0 };
    bool[] statuss = { false, false, false };
    //List<TuyaPlug> deviceList;
    string[] deviceList = {"Floorlamp", "Sofa Lamp",  "Christmas Tree" };



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
        // StartCoroutine(ExampleCoroutine());
        initializeDevices();


    }

    /*
    IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);
        initializeDevices();
        //createObjectsFromJson();
        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
    */

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
            Vector3 fromPosition = CreatedJoint[7].transform.localPosition;
            Vector3 toPosition = CreatedJoint[8].transform.localPosition;
            Foward = (toPosition - fromPosition) * 80;


            RaycastHit hit;
            if (Physics.Raycast(toPosition, Foward, out hit))
            {
                detectMessage = "Active";
                // Targeting specific devices
                if (hit.collider.gameObject.name == "LightBulb0")
                {
                    clearHitCount(0);
                    detectMessage = hitCount[0].ToString();

                    deviceRenderers[0].material.SetColor("_Color", Color.green);

                }
                else if (hit.collider.gameObject.name == "LightBulb1")
                {
                    clearHitCount(1);

                    detectMessage = hitCount[1].ToString();

                    deviceRenderers[1].material.SetColor("_Color", Color.green);
                }
                else if (hit.collider.gameObject.name == "LightBulb2")
                {
                    clearHitCount(2);

                    detectMessage = hitCount[2].ToString();
                    deviceRenderers[2].material.SetColor("_Color", Color.green);
                }


            }
            else
            {
                detectMessage = "Inactive";
                for (int i = 0; i < deviceList.Length; i++)
                {
                    StartCoroutine(RefreshStatus(i));
                }

            }
            // Added hit count to filter out jitter motion
            if (hitCount[0] >= 30)
            {
                StartCoroutine(DeviceControl(0));
            }
            if (hitCount[1] >= 30)
            {
                StartCoroutine(DeviceControl(1));
            }
            if (hitCount[2] >= 30)
            {
                StartCoroutine(DeviceControl(2));
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

    void clearHitCount(int index)
    {
        hitCount[index]++;
        for (int i = 0; i < 3; i++)
        {
            if (i != index) hitCount[i] = 0;
        }
    }

    // Device Control Command

    void initializeDevices()
    {
        //deviceList = new List<TuyaPlug>();
        deviceObjects = new List<GameObject>();
        deviceRenderers = new List<Renderer>();
        //var device0 = new TuyaPlug()
        //{
        //    IP = "192.168.31.88",
        //    LocalKey = "90552857e69fc11c",
        //    Id = "137107483c71bf2296d3"
        //};
        //deviceList.Add(device0);

        //var device1 = new TuyaPlug()
        //{
        //    IP = "192.168.31.194",
        //    LocalKey = "df12a78a691fc089",
        //    Id = "137107483c71bf225465"
        //};
        //deviceList.Add(device1);

        //var device2 = new TuyaPlug()
        //{
        //    IP = "192.168.31.68",
        //    LocalKey = "8196c2b6154ede04",
        //    Id = "eb6f00bbeca9f3a971jxxx"
        //};
        //deviceList.Add(device2);


        GameObject light0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        light0.transform.position = new Vector3(2.2f, 0.5f, -0.5f);
        light0.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        light0.name = "LightBulb0";
        deviceObjects.Add(light0);
        deviceRenderers.Add(light0.GetComponent<Renderer>());
        GameObject light1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        light1.transform.position = new Vector3(1.5f, 0.5f, -0.5f);
        light1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        light1.name = "LightBulb1";
        deviceObjects.Add(light1);
        deviceRenderers.Add(light1.GetComponent<Renderer>());
        GameObject light2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        light2.transform.position = new Vector3(2.29f, 0.5f, 2.51f);
        light2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        light2.name = "LightBulb2";
        deviceObjects.Add(light2);
        deviceRenderers.Add(light2.GetComponent<Renderer>());

        foreach (Renderer element in deviceRenderers)
        {
            element.material.SetColor("_Color", Color.gray);
        }

    }
    // async Task DeviceControl(int index)
    // {
    //     //var status = await deviceList[index].GetStatus();
    //     //await deviceList[index].SetStatus(!status.Powered);

    //     statuss[index] = !statuss[index];
    //     if (statuss[index])
    //     {
    //         deviceRenderers[index].material.SetColor("_Color", Color.yellow);
    //     }
    //     else
    //     {
    //         deviceRenderers[index].material.SetColor("_Color", Color.white);
    //     }
    //     hitCount[index] = 0;
    // }

    // async Task RefreshStatus(int index)
    // {
    //     var status = await deviceList[index].GetStatus();

    //     if (status.Powered)
    //     {
    //         deviceRenderers[index].material.SetColor("_Color", Color.yellow);
    //     }
    //     else
    //     {
    //         deviceRenderers[index].material.SetColor("_Color", Color.white);
    //     }
    // }

    IEnumerator RefreshStatus(int index)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://192.168.31.13:3000/status?device=" + deviceList[index]);
        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);
        if(request.downloadHandler.text == "True")
        {
            deviceRenderers[index].material.SetColor("_Color", Color.yellow);
        }
        else
        {
            deviceRenderers[index].material.SetColor("_Color", Color.gray);
        }
    }

    IEnumerator DeviceControl(int index)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://192.168.31.13:3000/control?device=" + deviceList[index]);
        request.SendWebRequest();
        hitCount[index] = 0;
        yield return RefreshStatus(index);

    }

}