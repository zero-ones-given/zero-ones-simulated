using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.IO;
using System.Threading;

public class FrameUpdatedEvent : EventArgs
{
    public Color32[] Data = { };
    public int Resolution { get; set; }
}

public class StreamCameraController : MonoBehaviour
{
    public float FrameInterval = 1f / 10;
    public int Resolution = 400;

    private Camera _streamCamera;
    private VideoServer _videoServer;
    private float _lastFrameAt = 0;

    public EventHandler<FrameUpdatedEvent> FrameUpdated;

    void Start()
    {
        _streamCamera = this.GetComponent<Camera>();
    }

    public void StartVideoServer(int port)
    {
        _videoServer = new VideoServer();
        _videoServer.Start(port, this);
    }

    private void OnFrameUpdated(Color32[] frameData)
    {
        var handler = FrameUpdated;

        if (handler != null)
        {
            handler.Invoke(this, new FrameUpdatedEvent { Data = frameData, Resolution = Resolution });
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_videoServer != null && Time.time - _lastFrameAt > FrameInterval)
        {
            _lastFrameAt = Time.time;
            var tempRT = RenderTexture.GetTemporary(Resolution, Resolution);
            Graphics.Blit(source, tempRT);

            var tempTex = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
            tempTex.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0, false);
            tempTex.Apply();

            OnFrameUpdated(tempTex.GetPixels32());

            Destroy(tempTex);
            RenderTexture.ReleaseTemporary(tempRT);
        }
        Graphics.Blit(source, destination);
    }
}
