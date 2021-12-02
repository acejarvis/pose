using UnityEngine;

using UnityEngine.Events;

namespace NuitrackSDK.Frame
{
    [AddComponentMenu("NuitrackSDK/Frame/FrameViewer")]
    public class FrameViewer : MonoBehaviour
    {
        [System.Serializable]
        public class TextureEvent : UnityEvent<Texture> { }

        public enum FrameMode
        {
            Color = 0,
            Depth = 1,
            Segment = 2
        }

        public enum TextureMode
        {
            RenderTexture = 0,
            Texture2D = 1,
            Texture = 2
        }

        [SerializeField] FrameMode frameMode;
        [SerializeField] TextureMode textureMode;

        [SerializeField] TextureEvent onFrameUpdate;

        private void Update()
        {
            Texture texture = GetTexture();

            onFrameUpdate.Invoke(texture);
        }

        Texture GetTexture()
        {
            switch (frameMode)
            {
                case FrameMode.Color:
                    return GetColorTexture();
                case FrameMode.Depth:
                    return GetDepthTexture();
                case FrameMode.Segment:
                    return GetSegmentTexture();
                default:
                    return null;
            }
        }

        Texture GetColorTexture()
        {
            switch (textureMode)
            {
                case TextureMode.RenderTexture:
                    return NuitrackManager.ColorFrame.ToRenderTexture();
                case TextureMode.Texture2D:
                    return NuitrackManager.ColorFrame.ToTexture2D();
                case TextureMode.Texture:
                    return NuitrackManager.ColorFrame.ToTexture();
                default:
                    return null;
            }
        }

        Texture GetDepthTexture()
        {
            switch (textureMode)
            {
                case TextureMode.RenderTexture:
                    return NuitrackManager.DepthFrame.ToRenderTexture();
                case TextureMode.Texture2D:
                    return NuitrackManager.DepthFrame.ToTexture2D();
                case TextureMode.Texture:
                    return NuitrackManager.DepthFrame.ToTexture();
                default:
                    return null;
            }
        }

        Texture GetSegmentTexture()
        {
            switch (textureMode)
            {
                case TextureMode.RenderTexture:
                    return NuitrackManager.UserFrame.ToRenderTexture();
                case TextureMode.Texture2D:
                    return NuitrackManager.UserFrame.ToTexture2D();
                case TextureMode.Texture:
                    return NuitrackManager.UserFrame.ToTexture();
                default:
                    return null;
            }
        }
    }
}