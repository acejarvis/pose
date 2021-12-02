/*
 * This script converts the source data of nuitrack.ColorFrame to textures (RenderTexture / Texture2D / Texture) 
 * using the fastest available method for this platform. 
 * If the platform supports ComputeShader, the conversion is performed using the GPU, which is several times faster than the CPU conversion.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using System.Runtime.InteropServices;

using nuitrack;

namespace NuitrackSDK.Frame
{
    public class RGBToTexture : FrameToTexture<ColorFrame, Color3>
    {
        Texture2D dstRgbTexture2D;
        byte[] colorDataArray = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(dstRgbTexture2D);

            colorDataArray = null;
        }

        Texture2D GetCPUTexture(ColorFrame frame, TextureCache textureCache)
        {
            ref Texture2D destTexture = ref textureCache.texture2D;

            if (frame.Timestamp == textureCache.timeStamp && destTexture == localCache.texture2D && localCache.texture2D != null)
                return localCache.texture2D;
            else
            {
                int datasize = frame.DataSize;

                if (colorDataArray == null)
                    colorDataArray = new byte[frame.Cols * frame.Rows * 4];

                Marshal.Copy(frame.Data, colorDataArray, 0, datasize);

                //The conversion can be performed without an additional array, 
                //since after copying, the bytes are clumped at the beginning of the array.
                //Let's start the crawl from the end of the array by "stretching" the source data.
                for (int i = datasize - 1, ptr = colorDataArray.Length - 1; i > 0; i -= 3, ptr -= 4)
                {
                    byte r = colorDataArray[i - 2];
                    byte g = colorDataArray[i - 1];
                    byte b = colorDataArray[i];

                    colorDataArray[ptr - 3] = 255;
                    colorDataArray[ptr - 2] = b;
                    colorDataArray[ptr - 1] = g;
                    colorDataArray[ptr] = r;
                }
  
                if(destTexture == null)
                    destTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                destTexture.LoadRawTextureData(colorDataArray);
                destTexture.Apply();

                textureCache.timeStamp = frame.Timestamp;

                return destTexture;
            }
        }

        RenderTexture GetGPUTexture(ColorFrame frame, TextureCache textureCache)
        {
            ref RenderTexture destTexture = ref textureCache.renderTexture;

            if (frame.Timestamp == textureCache.timeStamp && destTexture == localCache.renderTexture && localCache.renderTexture != null)
                return localCache.renderTexture;
            else
            {
                textureCache.timeStamp = frame.Timestamp;

                if (instanceShader == null)
                    InitShader("BGR2RGB");

                if (dstRgbTexture2D == null)
                {
                    dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
                    instanceShader.SetTexture(kernelIndex, "Texture", dstRgbTexture2D);
                }

                if (destTexture == null)
                    destTexture = InitRenderTexture(frame.Cols, frame.Rows);

                instanceShader.SetTexture(kernelIndex, "Result", destTexture);

                dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
                dstRgbTexture2D.Apply();

                instanceShader.Dispatch(kernelIndex, dstRgbTexture2D.width / (int)x, dstRgbTexture2D.height / (int)y, (int)z);

                return destTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <returns>ColorFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture(ColorFrame frame, TextureCache textureCache = null)
        {
            if (frame == null)
                return null;

            if (GPUSupported)
                return GetGPUTexture(frame, textureCache != null ? textureCache : localCache);
            else
            {
                TextureCache cache = textureCache != null ? textureCache : localCache;

                cache.texture2D = GetCPUTexture(frame, cache);
                FrameUtils.TextureUtils.Copy(cache.texture2D, ref cache.renderTexture);

                return cache.renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <returns>ColorFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D(ColorFrame frame, TextureCache textureCache = null)
        {
            if (frame == null)
                return null;

            if (GPUSupported)
            {
                TextureCache cache = textureCache != null ? textureCache : localCache;

                cache.renderTexture = GetGPUTexture(frame, cache);
                FrameUtils.TextureUtils.Copy(cache.renderTexture, ref cache.texture2D);
                return cache.texture2D;
            }
            else
                return GetCPUTexture(frame, textureCache != null ? textureCache : localCache);
        }
    }
}