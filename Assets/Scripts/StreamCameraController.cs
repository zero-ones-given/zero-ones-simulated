using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

public class StreamCameraController : MonoBehaviour
{
    public const float FRAME_INTERVAL = 1f / 20;
    private int _captureWidth = 1080;
    private int _captureHeight = 1080;

    private Camera _streamCamera;
    private VideoServer _videoServer;
    private float _lastFrameAt = 0;
    private Stopwatch _stopWatch;

    void Start()
    {
        _streamCamera = this.GetComponent<Camera>();
        _videoServer = new VideoServer();
        _videoServer.Start();
        _stopWatch = new Stopwatch();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Time.time - _lastFrameAt > FRAME_INTERVAL)
        {
            _stopWatch.Reset();
            _stopWatch.Start();
            _lastFrameAt = Time.time;
            var tempRT = RenderTexture.GetTemporary(_captureWidth, _captureHeight); 
            Graphics.Blit(source, tempRT);

            var tempTex = new Texture2D(_captureWidth, _captureHeight, TextureFormat.RGBA32, false);
            tempTex.ReadPixels(new Rect(0, 0, _captureWidth, _captureHeight), 0, 0, false);
            tempTex.Apply();


            _videoServer.LatestFrame = tempTex.GetPixels32();
            UnityEngine.Debug.Log($"Getting pixels took: {_stopWatch.ElapsedMilliseconds} ms");
            _videoServer.LatestFrameAt = _lastFrameAt;

            Destroy(tempTex);
            RenderTexture.ReleaseTemporary(tempRT);
        }
        Graphics.Blit(source, destination);
    }
}
