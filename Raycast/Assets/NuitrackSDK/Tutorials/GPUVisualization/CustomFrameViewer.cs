using UnityEngine;

using NuitrackSDK.Frame;

namespace NuitrackSDK.Tutorials.GPUVisualization
{
    public class CustomFrameViewer : MonoBehaviour
    {
        public enum Mode
        {
            Cut,
            ReverseCut,
            Mul,
            Mix
        }

        [SerializeField] Texture mainTexture;
        [SerializeField] Texture altTexture;

        [SerializeField] Mode mode = Mode.Cut;
        [SerializeField] FrameViewer.TextureEvent onFrameUpdate;

        RenderTexture renderTexture;

        void Update()
        {
            switch (mode)
            {
                case Mode.Cut:
                    FrameUtils.TextureUtils.Cut(mainTexture, altTexture, ref renderTexture);
                    break;
                case Mode.ReverseCut:
                    FrameUtils.TextureUtils.ReverseCut(mainTexture, altTexture, ref renderTexture);
                    break;
                case Mode.Mul:
                    FrameUtils.TextureUtils.Mul(mainTexture, altTexture, ref renderTexture);
                    break;
                case Mode.Mix:
                    FrameUtils.TextureUtils.MixMask(mainTexture, altTexture, ref renderTexture);
                    break;
            }

            onFrameUpdate.Invoke(renderTexture);
        }

        public void MainTexture(Texture texture)
        {
            mainTexture = texture;
        }

        public void AltTexture(Texture texture)
        {
            altTexture = texture;
        }
    }
}