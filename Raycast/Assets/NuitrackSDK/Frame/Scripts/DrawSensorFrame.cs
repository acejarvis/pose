using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NuitrackSDK.NuitrackDemos
{
    public enum FrameType
    {
        Color,
        Depth,
        User,
    }

    public class DrawSensorFrame : MonoBehaviour
    {
        [SerializeField] GameObject colorImage;
        [SerializeField] GameObject depthImage;
        [SerializeField] GameObject userImage;
        [SerializeField] GameObject segmentOverlay;
        [SerializeField] Toggle segmentToggle;
        [SerializeField] GameObject skeletonsOverlay;
        [SerializeField] Toggle skeletonToggle;
        [SerializeField] GameObject facesOverlay;
        [SerializeField] Toggle facesToggle;
        [SerializeField] FrameType defaultFrameType = FrameType.Color;

        [SerializeField] RectTransform panel;
        [SerializeField] int windowPercent = 20;
        [SerializeField] bool fullscreenDefault = true;
        [SerializeField] bool showSegmentOverlay = false;
        [SerializeField] bool showSkeletonsOverlay = false;
        [SerializeField] bool showFacesOverlay = false;

        bool isFullscreen;

        public void SwitchByIndex(int frameIndex)
        {
            if (frameIndex == 0) SelectFrame(FrameType.Color);
            if (frameIndex == 1) SelectFrame(FrameType.Depth);
            if (frameIndex == 2) SelectFrame(FrameType.User);
        }

        void Start()
        {
            SelectFrame(defaultFrameType);
            isFullscreen = fullscreenDefault;
            SwitchFullscreen();
            segmentToggle.isOn = showSegmentOverlay;
            segmentOverlay.SetActive(showSegmentOverlay);
            skeletonToggle.isOn = showSkeletonsOverlay;
            skeletonsOverlay.SetActive(showSkeletonsOverlay);
            facesToggle.isOn = showFacesOverlay;
            facesOverlay.SetActive(showFacesOverlay);

            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        void SelectFrame(FrameType frameType)
        {
            colorImage.SetActive(frameType == FrameType.Color);
            depthImage.SetActive(frameType == FrameType.Depth);
            userImage.SetActive(frameType == FrameType.User);
        }

        public void SwitchSegmentOverlay(bool value)
        {
            segmentOverlay.SetActive(value);
        }

        public void SwitchSkeletonsOverlay(bool value)
        {
            skeletonsOverlay.SetActive(value);
        }

        public void SwitchFacesOverlay(bool value)
        {
            facesOverlay.SetActive(value);
        }

        public void SwitchFullscreen()
        {
            isFullscreen = !isFullscreen;

            if (isFullscreen)
                panel.localScale = new Vector3(1.0f / 100 * windowPercent, 1.0f / 100 * windowPercent, 1.0f);
            else
                panel.localScale = new Vector3(1, 1, 1);
        }
    }
}