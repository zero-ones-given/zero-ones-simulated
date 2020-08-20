using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.IO;
using System.Threading;

public class StreamCameraController : MonoBehaviour
{
    public float FrameInterval = 1f / 10;
    private int _captureWidth = 1080;
    private int _captureHeight = 1080;

    private Camera _streamCamera;
    private VideoServer _videoServer;
    private float _lastFrameAt = 0;

    void Start()
    {
        _streamCamera = this.GetComponent<Camera>();
        _videoServer = new VideoServer();
        _videoServer.Start();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Time.time - _lastFrameAt > FrameInterval)
        {
            _lastFrameAt = Time.time;
            var tempRT = RenderTexture.GetTemporary(_captureWidth, _captureHeight); 
            Graphics.Blit(source, tempRT);

            var tempTex = new Texture2D(_captureWidth, _captureHeight, TextureFormat.RGBA32, false);
            tempTex.ReadPixels(new Rect(0, 0, _captureWidth, _captureHeight), 0, 0, false);
            tempTex.Apply();

            _videoServer.LatestFrame = tempTex.GetPixels32();
            _videoServer.LatestFrameAt = _lastFrameAt;

            Destroy(tempTex);
            RenderTexture.ReleaseTemporary(tempRT);
        }
        Graphics.Blit(source, destination);
    }
}
