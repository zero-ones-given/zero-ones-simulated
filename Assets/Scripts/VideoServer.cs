using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;


class VideoServer
{
    public Color32[] LatestFrame = {};
    public float LatestFrameAt = 0;
    private float _latestStartedFrameTimestamp = -1;
    public float latestSentFrameTimestamp = -1;
    public uint Resolution = 400;

    public void Start(int port = 8080)
    {
        Debug.Log($"Starting http server: {port}");
        var applicationPath = Application.dataPath;

        var tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        new Thread(() =>
        {
            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                var videoThread = new Thread(() => StreamVideo(client, applicationPath));
                videoThread.Start();
            }
        }).Start();
    }

    private string GetImageHeaders(byte[] image)
    {
        return "\r\n--boundary\r\nContent-Type: image/jpeg\r\n" +
            $"Content-Length: {image.Length}\r\n\r\n";
    }

    private void WriteString(string str, NetworkStream stream)
    {
        var encodedString = Encoding.ASCII.GetBytes(str);
        stream.Write(encodedString, 0, encodedString.Length);
    }

    private void StreamVideo(TcpClient client, string applicationPath)
    {
        using(client)
        using(var stream = client.GetStream())
        {
            const string headers = "HTTP/1.1 200 OK\r\n" +
            "Content-Type: multipart/x-mixed-replace; boundary=--boundary\r\n";

            WriteString(headers, stream);

            while(true)
            {
                try
                {
                    if (_latestStartedFrameTimestamp < LatestFrameAt)
                    {
                        _latestStartedFrameTimestamp = LatestFrameAt;
                        
                        var encodedFrame = ImageConversion.EncodeArrayToJPG(LatestFrame, GraphicsFormat.R8G8B8A8_UNorm, Resolution, Resolution);

                        WriteString(GetImageHeaders(encodedFrame), stream);
                        stream.Write(encodedFrame, 0, encodedFrame.Length);
                        WriteString("\r\n", stream);
                        latestSentFrameTimestamp = _latestStartedFrameTimestamp;
                    }
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    // Disconnected?
                    Debug.Log($"Encountered error: {ex.Message}");
                    break;
                }
            }
        };
    }
}
