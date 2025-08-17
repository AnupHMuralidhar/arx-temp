using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using System;

public class ARCameraStreamer : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public WebSocketClient websocketClient;
    private Texture2D cameraTexture;

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            int width = image.width;
            int height = image.height;

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, width, height),
                outputDimensions = new Vector2Int(width, height),  // use full resolution
                outputFormat = TextureFormat.RGB24,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            image.Convert(conversionParams, buffer);
            image.Dispose();

            // Only recreate texture if size changes
            if (cameraTexture == null || cameraTexture.width != width || cameraTexture.height != height)
                cameraTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

            cameraTexture.LoadRawTextureData(buffer);
            cameraTexture.Apply();
            buffer.Dispose();

            byte[] jpgBytes = cameraTexture.EncodeToJPG(80);  // 80 is a good quality/size balance
            string base64 = Convert.ToBase64String(jpgBytes);

            websocketClient.SendToBackend("IMG:" + base64);
        }
    }
}
