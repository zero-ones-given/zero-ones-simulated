using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.IO;
using System.Threading;

public class FrameUpdatedEvent : EventArgs
{
    public Color32[] Data = { };
    public int Width { get; set; }
    public int Height { get; set; }
}

public class StreamCameraController : MonoBehaviour
{
    public float FrameInterval = 1f / 10;
    public int Width = 720;
    public int Height = 720;

    private Camera _streamCamera;
    private VideoServer _videoServer;
    private float _lastFrameAt = 0;
    private float _lastFrameRenderTime = 0;
    private RenderTexture _renderTexture;
    private Texture2D _tempTexture;

    public EventHandler<FrameUpdatedEvent> FrameUpdated;

    public void SetCameraOffset(float[] offset) {
        var initialCameraPosition = new Vector3(0, 1.8f, 0);
        transform.position = initialCameraPosition + new Vector3(offset[0], offset[1], offset[2]);
        transform.eulerAngles = new Vector3(90 + getOffset(offset, 3), 270 + getOffset(offset, 4), getOffset(offset, 5));
    }

    public void StartVideoServer(int port)
    {
        _streamCamera = this.GetComponent<Camera>();
        _renderTexture = new RenderTexture(Width, Height, 16, RenderTextureFormat.ARGB32);
        _renderTexture.Create();
        _streamCamera.targetTexture = _renderTexture;
        _tempTexture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

        _videoServer = new VideoServer();
        _videoServer.Start(port, this);
    }

    private float getOffset(float[] offset, int index) {
        if (offset.Length > index) {
            return offset[index];
        }
        return 0f;
    }

    private void OnFrameUpdated(Color32[] frameData)
    {
        if (FrameUpdated != null)
        {
            FrameUpdated.Invoke(this, new FrameUpdatedEvent { Data = frameData, Width = Width, Height = Height });
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_videoServer != null && Time.realtimeSinceStartup - _lastFrameAt >= FrameInterval - _lastFrameRenderTime / 2)
        {
            _lastFrameAt = Time.realtimeSinceStartup;
            RenderTexture.active = _renderTexture;
            _tempTexture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false);
            _tempTexture.Apply();
            OnFrameUpdated(_tempTexture.GetPixels32());
            RenderTexture.active = null;
            Graphics.Blit(source, destination);
            _lastFrameRenderTime = Time.realtimeSinceStartup - _lastFrameAt;
        }
    }
}
