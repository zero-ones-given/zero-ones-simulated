using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

public class StreamCameraController : MonoBehaviour
{
    private int _captureWidth = 1080;
    private int _captureHeight = 1080;

    private Camera _streamCamera;
    private VideoServer _videoServer;
    private Stopwatch _stopWatch;

    void Start()
    {
        _streamCamera = this.GetComponent<Camera>();
        _streamCamera.enabled = false;
        _videoServer = new VideoServer();
        _videoServer.Start();
        _stopWatch = new Stopwatch();
        _stopWatch.Start();
    }

    void Update()
    {
        if (_stopWatch.Elapsed.Milliseconds > 66)
        {
            RenderFrame();
            _stopWatch.Reset();
            _stopWatch.Start();
        }
    }

    void RenderFrame()
    {
        RenderTexture renderTexture = new RenderTexture(_captureWidth, _captureHeight, 24);
        Texture2D screenShot = new Texture2D(_captureWidth, _captureHeight, TextureFormat.RGB24, false);
        _streamCamera.targetTexture = renderTexture;
        _streamCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, _captureWidth, _captureHeight), 0, 0);

        _streamCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        _videoServer.LatestFrame = screenShot.EncodeToJPG();
    }
}
