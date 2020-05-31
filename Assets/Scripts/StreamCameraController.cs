using UnityEngine;
using System.IO;
using System.Threading;

public class StreamCameraController : MonoBehaviour
{
    public int captureWidth = 1080;
    public int captureHeight = 1080;

    Camera streamCamera;
    Rect rect;
    private int imageNumber = 0;

    void Start()
    {
        rect = new Rect(0, 0, captureWidth, captureHeight);
        streamCamera = this.GetComponent<Camera>();
        streamCamera.enabled = false;
    }

    void Update()
    {
        if (!Input.GetKeyDown("space")) {
            return;
        }
        RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        streamCamera.targetTexture = renderTexture;
        streamCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);

        streamCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        byte[] fileData = screenShot.EncodeToJPG();

        string fileName = $"{Application.dataPath}/../{imageNumber}.jpg";
        new System.Threading.Thread(() =>
        {
            // create file and write optional header with image bytes
            var f = File.Create(fileName);
            f.Write(fileData, 0, fileData.Length);
            f.Close();
            Debug.Log($"Saved screenshot {fileName} (size {fileData.Length})");
        }).Start();

        imageNumber++;
    }
}
