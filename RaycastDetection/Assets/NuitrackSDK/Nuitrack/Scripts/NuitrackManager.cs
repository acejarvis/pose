using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;

using System.IO;

using NuitrackSDK;

#if UNITY_ANDROID && UNITY_2018_1_OR_NEWER && !UNITY_EDITOR
using UnityEngine.Android;
#endif

[System.Serializable]
public class InitEvent : UnityEvent<NuitrackInitState>
{
}

enum WifiConnect
{
    none, VicoVR, TVico,
}

[HelpURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/")]
public class NuitrackManager : MonoBehaviour
{
    public enum RotationDegree
    {
        Normal = 0,
        _90 = 90,
        _180 = 180,
        _270 = 270
    }

    bool _threadRunning;
    Thread _thread;

    public NuitrackInitState InitState { get { return NuitrackLoader.initState; } }
    [SerializeField, NuitrackSDKInspector]
    bool
    depthModuleOn = true,
    colorModuleOn = true,
    userTrackerModuleOn = true,
    skeletonTrackerModuleOn = true,
    gesturesRecognizerModuleOn = true,
    handsTrackerModuleOn = true;

    bool nuitrackError = false;
    public LicenseInfo LicenseInfo
    {
        get;
        private set;
    } = new LicenseInfo();

    [Space]

    [SerializeField, NuitrackSDKInspector] bool runInBackground = false;
    
    [Tooltip("Only skeleton. PC, Unity Editor, MacOS and IOS")]
    [SerializeField, NuitrackSDKInspector] WifiConnect wifiConnect = WifiConnect.none;
   
    [Tooltip("ONLY PC! Nuitrack AI is the new version of Nuitrack skeleton tracking middleware")]
    [SerializeField, NuitrackSDKInspector] bool useNuitrackAi = false;
    
    [Tooltip("Track and get information about faces with Nuitrack (position, angle of rotation, box, emotions, age, gender).")]
    [SerializeField, NuitrackSDKInspector] bool useFaceTracking = false;

    [Tooltip("Depth map doesn't accurately match an RGB image. Turn on this to align them")]
    [SerializeField, NuitrackSDKInspector] bool depth2ColorRegistration = false;

    [Tooltip("Mirror sensor data")]
    [SerializeField, NuitrackSDKInspector] bool mirror = false;

    [Tooltip ("If you have the sensor installed vertically or upside down, you can level this. Sensor rotation is not available for mirror mode.")]
    [SerializeField, NuitrackSDKInspector] RotationDegree sensorRotation = RotationDegree.Normal;

    [SerializeField, NuitrackSDKInspector] bool useFileRecord;
    [SerializeField, NuitrackSDKInspector] string pathToFileRecord;

    [Tooltip("Asynchronous initialization, allows you to turn on the nuitrack more smoothly. In this case, you need to ensure that all components that use this script will start only after its initialization.")]
    [SerializeField, NuitrackSDKInspector] bool asyncInit = false;

    [SerializeField, NuitrackSDKInspector] InitEvent initEvent;

    public static bool sensorConnected = false;
    public static nuitrack.DepthSensor DepthSensor { get; private set; }
    public static nuitrack.ColorSensor ColorSensor { get; private set; }
    public static nuitrack.UserTracker UserTracker { get; private set; }
    public static nuitrack.SkeletonTracker SkeletonTracker { get; private set; }
    public static nuitrack.GestureRecognizer GestureRecognizer { get; private set; }
    public static nuitrack.HandTracker HandTracker { get; private set; }
    public static nuitrack.DepthFrame DepthFrame { get; private set; }
    public static nuitrack.ColorFrame ColorFrame { get; private set; }
    public static nuitrack.UserFrame UserFrame { get; private set; }
    public static nuitrack.SkeletonData SkeletonData { get; private set; }
    public static nuitrack.HandTrackerData HandTrackerData { get; private set; }

    public static event nuitrack.DepthSensor.OnUpdate onDepthUpdate;
    public static event nuitrack.ColorSensor.OnUpdate onColorUpdate;
    public static event nuitrack.UserTracker.OnUpdate onUserTrackerUpdate;
    public static event nuitrack.SkeletonTracker.OnSkeletonUpdate onSkeletonTrackerUpdate;
    public static event nuitrack.HandTracker.OnUpdate onHandsTrackerUpdate;

    public delegate void OnNewGestureHandler(nuitrack.Gesture gesture);
    public static event OnNewGestureHandler onNewGesture;
    public static nuitrack.UserHands СurrentHands { get; private set; }

    static NuitrackManager instance;
    NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_MANAGER_NOT_INSTALLED;

    bool prevSkel = false;
    bool prevHand = false;
    bool prevDepth = false;
    bool prevColor = false;
    bool prevGest = false;
    bool prevUser = false;

    bool pauseState = false;

    [HideInInspector] public bool nuitrackInitialized = false;

    [HideInInspector] public System.Exception initException;

    public bool UseFaceTracking
    {
        get
        {
            return useFaceTracking;
        }
    }

    public bool UseNuitrackAi
    {
        get
        {
            return useNuitrackAi;
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static int GetAndroidAPILevel()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }
#endif

    void WorkingThread()
    {
        _threadRunning = true;

        while (_threadRunning)
        {
            NuitrackInit();
        }
    }

    public static NuitrackManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NuitrackManager>();
                if (instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "NuitrackManager";
                    instance = container.AddComponent<NuitrackManager>();
                }

                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    private bool IsNuitrackLibrariesInitialized()
    {
        if (initState == NuitrackInitState.INIT_OK || wifiConnect != WifiConnect.none)
            return true;
        return false;
    }

    void Awake()
    {
        if (Instance.gameObject != gameObject)
        {
            DestroyImmediate(Instance.gameObject);
            instance = this;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(AndroidStart());
#else
        FirstStart();
#endif
    }

    void FirstStart()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Application.targetFrameRate = 60;
        Application.runInBackground = runInBackground;

        initState = NuitrackLoader.InitNuitrackLibraries();

        if (asyncInit)
        {
            StartCoroutine(InitEventStart());
            if (!_threadRunning)
            {
                _thread = new Thread(WorkingThread);
                _thread.Start();
            }
        }
        else
        {
            if (initEvent != null)
            {
                initEvent.Invoke(initState);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsNuitrackLibrariesInitialized())
#endif
            NuitrackInit();
        }
    }

    IEnumerator AndroidStart()
    {
#if UNITY_ANDROID && UNITY_2018_1_OR_NEWER && !UNITY_EDITOR

        while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            yield return null;
        }

        while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            yield return null;
        }

        while (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
            yield return null;
        }

        if (GetAndroidAPILevel() > 26) // camera permissions required for Android newer than Oreo 8
        {
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                yield return null;
            }
        }

        yield return null;
#endif

        FirstStart();

        yield return null;
    }

    public void ChangeModulesState(bool skel, bool hand, bool depth, bool color, bool gest, bool user)
    {
        skeletonTrackerModuleOn = skel;
        handsTrackerModuleOn = hand;
        depthModuleOn = depth;
        colorModuleOn = color;
        gesturesRecognizerModuleOn = gest;
        userTrackerModuleOn = user;

        if (SkeletonTracker == null)
            return;
        if (prevSkel != skel)
        {
            SkeletonData = null;
            prevSkel = skel;
            if (skel)
            {
                SkeletonTracker.OnSkeletonUpdateEvent += HandleOnSkeletonUpdateEvent;
            }
            else
            {
                SkeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
            }
        }

        if (prevHand != hand)
        {
            HandTrackerData = null;
            prevHand = hand;
            if (hand)
                HandTracker.OnUpdateEvent += HandleOnHandsUpdateEvent;
            else
                HandTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;
        }
        if (prevGest != gest)
        {
            prevGest = gest;
            if (gest)
                GestureRecognizer.OnNewGesturesEvent += OnNewGestures;
            else
                GestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
        }
        if (prevDepth != depth)
        {
            DepthFrame = null;
            prevDepth = depth;
            if (depth)
                DepthSensor.OnUpdateEvent += HandleOnDepthSensorUpdateEvent;
            else
                DepthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
        }
        if (prevColor != color)
        {
            ColorFrame = null;
            prevColor = color;
            if (color)
                ColorSensor.OnUpdateEvent += HandleOnColorSensorUpdateEvent;
            else
                ColorSensor.OnUpdateEvent -= HandleOnColorSensorUpdateEvent;
        }
        if (prevUser != user)
        {
            UserFrame = null;
            prevUser = user;
            if (user)
                UserTracker.OnUpdateEvent += HandleOnUserTrackerUpdateEvent;
            else
                UserTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
        }
    }

    void NuitrackInit()
    {
        try
        {
            if (nuitrackInitialized)
                return;

            if (wifiConnect == WifiConnect.VicoVR)
            {
                nuitrack.Nuitrack.Init("", nuitrack.Nuitrack.NuitrackMode.DEBUG);
                nuitrack.Nuitrack.SetConfigValue("Settings.IPAddress", "192.168.1.1");
            }
            else if (wifiConnect == WifiConnect.TVico)
            {
                Debug.Log("If something doesn't work, then read this (Wireless case section): github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#wireless-case");
                nuitrack.Nuitrack.Init("", nuitrack.Nuitrack.NuitrackMode.DEBUG);
                nuitrack.Nuitrack.SetConfigValue("Settings.IPAddress", "192.168.43.1");
            }
            else
            {
                nuitrack.Nuitrack.Init();

                if (useFileRecord && (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor))
                {
                    string path = pathToFileRecord.Replace('\\', '/');
                    try
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        if(fileInfo.Exists && fileInfo.Extension != string.Empty)
                        { 
                            if (fileInfo.Extension == ".oni")
                                nuitrack.Nuitrack.SetConfigValue("OpenNIModule.FileRecord", path);
                            else
                                nuitrack.Nuitrack.SetConfigValue("Realsense2Module.FileRecord", path);
                        }
                        else
                            Debug.LogError(string.Format("Check the path to the recording file! File path: {0}", path));
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError("File " + path + "  Cannot be loaded!");
                    }

                }

                if (depth2ColorRegistration)
                {
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.Depth2ColorRegistration", "true");
                }

                if (useNuitrackAi)
                {
                    if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.isEditor)
                    {
                        nuitrack.Nuitrack.SetConfigValue("DepthProvider.Depth2ColorRegistration", "true");
                        nuitrack.Nuitrack.SetConfigValue("Skeletonization.Type", "CNN_HPE");
                    }
                    else
                    {
                        Debug.LogWarning("NuitrackAI doesn't support this platform: " + Application.platform + ". https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md");
                    }
                }

                if (useFaceTracking)
                {
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.Depth2ColorRegistration", "true");
                    nuitrack.Nuitrack.SetConfigValue("Faces.ToUse", "true");
                }

                if (mirror)
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.Mirror", "true");
                else
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.RotateAngle", ((int)sensorRotation).ToString());

                string devicesInfo = "";
                if(nuitrack.Nuitrack.GetDeviceList().Count > 0)
                {
                    for (int i = 0; i < nuitrack.Nuitrack.GetDeviceList().Count; i++)
                    {
                        nuitrack.device.NuitrackDevice device = nuitrack.Nuitrack.GetDeviceList()[i];
                        string sensorName = device.GetInfo(nuitrack.device.DeviceInfoType.DEVICE_NAME);
                        if (i == 0)
                        {
                            LicenseInfo.Trial = device.GetActivationStatus() == nuitrack.device.ActivationStatus.TRIAL;
                            LicenseInfo.SensorName = sensorName;
                        }

                        devicesInfo += "\nDevice " + i + " [Sensor Name: " + sensorName + ", License: " + device.GetActivationStatus() + "] ";
                    }
                }
                
                //licenseInfo = JsonUtility.FromJson<LicenseInfo>(nuitrack.Nuitrack.GetDeviceList());

                Debug.Log(
                    "Nuitrack Start Info:\n" +
                    "Skeletonization Type: " + nuitrack.Nuitrack.GetConfigValue("Skeletonization.Type") + "\n" +
                    "Faces using: " + nuitrack.Nuitrack.GetConfigValue("Faces.ToUse") + devicesInfo);
            }

            nuitrack.Nuitrack.UpdateConfig();

            Debug.Log("Nuitrack Init OK");

            DepthSensor = nuitrack.DepthSensor.Create();

            ColorSensor = nuitrack.ColorSensor.Create();

            UserTracker = nuitrack.UserTracker.Create();

            SkeletonTracker = nuitrack.SkeletonTracker.Create();

            GestureRecognizer = nuitrack.GestureRecognizer.Create();

            HandTracker = nuitrack.HandTracker.Create();

            nuitrack.Nuitrack.Run();
            Debug.Log("Nuitrack Run OK");

            ChangeModulesState(
                skeletonTrackerModuleOn,
                handsTrackerModuleOn,
                depthModuleOn,
                colorModuleOn,
                gesturesRecognizerModuleOn,
                userTrackerModuleOn
            );

            nuitrackInitialized = true;
            _threadRunning = false;
        }
        catch (System.Exception ex)
        {
            initException = ex;
            NuitrackErrorSolver.CheckError(ex);
        }
    }

    void HandleOnDepthSensorUpdateEvent(nuitrack.DepthFrame frame)
    {
        if (DepthFrame != null)
            DepthFrame.Dispose();
        DepthFrame = (nuitrack.DepthFrame)frame.Clone();
        onDepthUpdate?.Invoke(DepthFrame);
    }

    void HandleOnColorSensorUpdateEvent(nuitrack.ColorFrame frame)
    {
        if (ColorFrame != null)
            ColorFrame.Dispose();
        ColorFrame = (nuitrack.ColorFrame)frame.Clone();
        onColorUpdate?.Invoke(ColorFrame);
    }

    void HandleOnUserTrackerUpdateEvent(nuitrack.UserFrame frame)
    {
        if (UserFrame != null)
            UserFrame.Dispose();
        UserFrame = (nuitrack.UserFrame)frame.Clone();
        onUserTrackerUpdate?.Invoke(UserFrame);
    }

    void HandleOnSkeletonUpdateEvent(nuitrack.SkeletonData _skeletonData)
    {
        if (SkeletonData != null)
            SkeletonData.Dispose();
        SkeletonData = (nuitrack.SkeletonData)_skeletonData.Clone();
        sensorConnected = true;
        onSkeletonTrackerUpdate?.Invoke(SkeletonData);
    }

    private void OnNewGestures(nuitrack.GestureData gestures)
    {
        if (gestures.NumGestures > 0)
        {
            if (onNewGesture != null)
            {
                for (int i = 0; i < gestures.Gestures.Length; i++)
                {
                    onNewGesture(gestures.Gestures[i]);
                }
            }
        }
    }

    void HandleOnHandsUpdateEvent(nuitrack.HandTrackerData _handTrackerData)
    {
        if (HandTrackerData != null)
            HandTrackerData.Dispose();
        HandTrackerData = (nuitrack.HandTrackerData)_handTrackerData.Clone();
        onHandsTrackerUpdate?.Invoke(HandTrackerData);

        if (HandTrackerData == null) return;
        if (CurrentUserTracker.CurrentUser != 0)
        {
            СurrentHands = HandTrackerData.GetUserHandsByID(CurrentUserTracker.CurrentUser);
        }
        else
        {
            СurrentHands = null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            StopNuitrack();
            pauseState = true;
        }
        else
        {
            StartCoroutine(RestartNuitrack());
        }
    }

    IEnumerator RestartNuitrack()
    {
        yield return null;

        while (pauseState)
        {
            StartNuitrack();
            pauseState = false;
            yield return null;
        }
        yield return null;
    }

    public void StartNuitrack()
    {
        nuitrackError = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsNuitrackLibrariesInitialized())
            return;
#endif
        if (asyncInit)
        {
            if (!_threadRunning)
            {
                _thread = new Thread(WorkingThread);
                _thread.Start();
            }
        }
        else
        {
            NuitrackInit();
        }
    }

    public void StopNuitrack()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsNuitrackLibrariesInitialized())
            return;
#endif
        prevSkel = false;
        prevHand = false;
        prevGest = false;
        prevDepth = false;
        prevColor = false;
        prevUser = false;

        CloseUserGen();
    }

    IEnumerator InitEventStart()
    {
        while (!nuitrackInitialized)
        {
            yield return new WaitForEndOfFrame();
        }

        if (initEvent != null)
        {
            initEvent.Invoke(initState);
        }
    }

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (IsNuitrackLibrariesInitialized())
#endif
        if (nuitrackError)
            return;

        if (!pauseState || (asyncInit && _threadRunning))
        {
            try
            {
                nuitrack.Nuitrack.Update();
            }
            catch (System.Exception ex)
            {
                NuitrackErrorSolver.CheckError(ex, true, false);
                if (ex.ToString().Contains("LicenseNotAcquiredException"))
                {
                    nuitrackError = true;
                }
            }
        }
    }

    public void DepthModuleClose()
    {
        depthModuleOn = false;
        userTrackerModuleOn = false;
        colorModuleOn = false;
        ChangeModulesState(
            skeletonTrackerModuleOn,
            handsTrackerModuleOn,
            depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
            userTrackerModuleOn
        );
    }

    public void DepthModuleStart()
    {
        depthModuleOn = true;
        userTrackerModuleOn = true;
        colorModuleOn = true;
        Debug.Log("DepthModuleStart");
        ChangeModulesState(
            skeletonTrackerModuleOn,
            handsTrackerModuleOn,
            depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
            userTrackerModuleOn
        );
    }

    public void EnableNuitrackAI(bool use)
    {
        StopNuitrack();
        useNuitrackAi = use;
        StartNuitrack();
    }

    public static JsonInfo NuitrackJson 
    { 
        get
        {
            try
            {
                if (nuitrack.Nuitrack.GetVersion() <= 3509)
                {
                    Debug.LogError("For face tracking use newer Nuitrack Runtime version. https://github.com/3DiVi/nuitrack-sdk/tree/master/Platforms");
                }
                else
                {
                    string json = nuitrack.Nuitrack.GetInstancesJson();
                    return NuitrackUtils.FromJson<JsonInfo>(json);
                }
            }
            catch (System.Exception ex)
            {
                NuitrackErrorSolver.CheckError(ex);
            }

            return null;
        }
    }

    public void CloseUserGen()
    {
        try
        {
            if (DepthSensor != null) DepthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
            if (ColorSensor != null) ColorSensor.OnUpdateEvent -= HandleOnColorSensorUpdateEvent;
            if (UserTracker != null) UserTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
            if (SkeletonTracker != null) SkeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
            if (GestureRecognizer != null) GestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
            if (HandTracker != null) HandTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;

            DepthFrame = null;
            ColorFrame = null;
            UserFrame = null;
            SkeletonData = null;
            HandTrackerData = null;

            DepthSensor = null;
            ColorSensor = null;
            UserTracker = null;
            SkeletonTracker = null;
            GestureRecognizer = null;
            HandTracker = null;

            nuitrack.Nuitrack.Release();
            Debug.Log("Nuitrack Stop OK");
            nuitrackInitialized = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    void OnDestroy()
    {
        CloseUserGen();
    }

    void OnDisable()
    {
        StopThread();
    }

    void StopThread()
    {
        if (_threadRunning)
        {
            _threadRunning = false;
            _thread.Join();
        }
    }
}
