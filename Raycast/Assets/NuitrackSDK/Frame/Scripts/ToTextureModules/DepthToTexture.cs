/*
 * This script converts the source data of nuitrack.DepthFrame to textures (RenderTexture / Texture2D / Texture) 
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
    public class DepthToTexture : FrameToTexture<DepthFrame, ushort>
    {
        [Range(0f, 32.0f)]
        [SerializeField] float maxDepthSensor = 10f;

        ComputeBuffer sourceDataBuffer;

        byte[] depthDataArray = null;
        byte[] outDepth = null;

        public float MaxSensorDepth
        {
            get
            {
                return maxDepthSensor;
            }
        }

        /// <summary>
        /// Get the hFOV of the DepthFrame in degrees
        /// </summary>
        public float hFOV
        {
            get
            {
                OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();
                return mode.HFOV * Mathf.Rad2Deg;
            }
        }

        /// <summary>
        /// Get the vFOV of the DepthFrame in degrees
        /// </summary>
        public float vFOV
        {
            get
            {
                OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

                float vFOV = mode.HFOV * ((float)mode.YRes / mode.XRes);
                return vFOV * Mathf.Rad2Deg;

            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (sourceDataBuffer != null)
            {
                sourceDataBuffer.Release();
                sourceDataBuffer = null;
            }

            depthDataArray = null;
            outDepth = null;
        }

        Texture2D GetCPUTexture(DepthFrame frame, TextureCache textureCache)
        {
            ref Texture2D destTexture = ref textureCache.texture2D;

            if (frame.Timestamp == textureCache.timeStamp && destTexture == localCache.texture2D && localCache.texture2D != null)
                return localCache.texture2D;
            else
            {
                if (outDepth == null)
                    outDepth = new byte[(frame.Cols * frame.Rows) * 4];

                Marshal.Copy(frame.Data, outDepth, 0, frame.DataSize);

                float depthDivisor = 1f / (1000 * maxDepthSensor);

                //The conversion can be performed without an additional array, 
                //since after copying, the bytes are clumped at the beginning of the array.
                //Let's start the crawl from the end of the array by "stretching" the source data.
                for (int i = frame.DataSize - 1, ptr = outDepth.Length - 1; i > 0; i -= 2, ptr -= 4)
                {
                    float uDepth = outDepth[i] << 8 | outDepth[i - 1];
                    byte depth = (byte)(255f * uDepth * depthDivisor);

                    outDepth[ptr - 3] = 255;    // a
                    outDepth[ptr - 2] = depth;  // r
                    outDepth[ptr - 1] = depth;  // g
                    outDepth[ptr] = depth;      // b
                }

                if (destTexture == null)
                    destTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                destTexture.LoadRawTextureData(outDepth);
                destTexture.Apply();

                textureCache.timeStamp = frame.Timestamp;

                return destTexture;
            }
        }

        RenderTexture GetGPUTexture(DepthFrame frame, TextureCache textureCache)
        {
            ref RenderTexture destTexture = ref textureCache.renderTexture;

            if (frame.Timestamp == textureCache.timeStamp && destTexture == localCache.renderTexture && localCache.renderTexture != null)
                return localCache.renderTexture;
            else
            {
                textureCache.timeStamp = frame.Timestamp;

                if (instanceShader == null)
                {
                    InitShader("Depth2Texture");
                    instanceShader.SetInt("textureWidth", frame.Cols);

                    /*
                         We put the source data in the buffer, but the buffer does not support types 
                         that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

                         For optimization, we specify a length half the original length,
                         since the data is correctly projected into memory
                         (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
                    */

                    sourceDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
                    instanceShader.SetBuffer(kernelIndex, "DepthFrame", sourceDataBuffer);
                }

                if (destTexture == null)
                    destTexture = InitRenderTexture(frame.Cols, frame.Rows);

                if(depthDataArray == null)
                    depthDataArray = new byte[frame.DataSize];

                instanceShader.SetTexture(kernelIndex, "Result", destTexture);

                Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
                sourceDataBuffer.SetData(depthDataArray);

                instanceShader.SetFloat("maxDepthSensor", maxDepthSensor);
                instanceShader.Dispatch(kernelIndex, destTexture.width / (int)x, destTexture.height / (int)y, (int)z);

                return destTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <returns>DepthFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture(DepthFrame frame, TextureCache textureCache = null)
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
        /// <returns>DepthFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D(DepthFrame frame, TextureCache textureCache = null)
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