using UnityEngine;

namespace NuitrackSDK.Frame
{
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu("NuitrackSDK/SensorEnvironment/Sensor Space")]
    public class SensorSpace : MonoBehaviour
    { 
        [Tooltip ("(optional) If not specified, the screen size is used.")]
        [SerializeField] Canvas viewCanvas;

        new Camera camera;
        RectTransform canvasRect;

        ulong lastTimeStamp = 0;

        public Camera Camera
        {
            get
            {
                if (camera == null)
                    camera = GetComponent<Camera>();

                return camera;
            }
        }

        RectTransform CanvasRect
        {
            get
            {
                if (viewCanvas != null && canvasRect == null)
                    canvasRect = viewCanvas.GetComponent<RectTransform>();
                
                return canvasRect;
            }
        }

        void Update()
        {
            if (NuitrackManager.ColorFrame == null || NuitrackManager.ColorFrame.Timestamp == lastTimeStamp)
                return;

            lastTimeStamp = NuitrackManager.ColorFrame.Timestamp;

            NuitrackManager_onColorUpdate(NuitrackManager.ColorFrame);
        }

        float ViewWidth
        {
            get
            {
                if (CanvasRect != null)
                    return CanvasRect.rect.width;
                else
                    return Screen.width;
            }
        }

        float ViewHeight
        {
            get
            {
                if (CanvasRect != null)
                    return CanvasRect.rect.height;
                else
                    return Screen.height;
            }
        }

        void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
        {
            float frameAspectRatio = (float)frame.Cols / frame.Rows;

            nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

            //The frame from the sensor fills the screen and the FOV is
            //determined for the axis along which the frame reaches the edges of the screen.
            //If the screen is wider than the frame from the sensor, then the alignment will
            //occur according to the inverse aspect ratio of the frame(otherwise the screen).
            float targetAspectRatio = ViewWidth / ViewHeight > frameAspectRatio ?
                (float)frame.Rows / frame.Cols : ViewHeight / ViewWidth;

            //Setting the camera's vFOV equal to the depth sensor's vFOV. 
            // Nuitrack does not yet have a representation of vFOV, so we use the hFOV to vFOV conversion.
            float vFOV = 2 * Mathf.Atan(Mathf.Tan(mode.HFOV * 0.5f) * targetAspectRatio);

            Camera.fieldOfView = vFOV * Mathf.Rad2Deg;
        }
    }
}