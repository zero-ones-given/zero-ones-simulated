using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Concurrent;


class VideoServer
{
    public int Resolution = 720;
    private StreamCameraController _cameraController;

    private ConcurrentDictionary<string, Tuple<TcpClient, System.EventHandler<FrameUpdatedEvent>>> _clients =
        new ConcurrentDictionary<string, Tuple<TcpClient, System.EventHandler<FrameUpdatedEvent>>>();

    const string Headers = "HTTP/1.1 200 OK\r\n" +
        "Content-Type: multipart/x-mixed-replace; boundary=--boundary\r\n";

    public void Start(int port, StreamCameraController cameraController)
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
                var stream = client.GetStream();
                if (!IsGetRequest(stream))
                {
                    client.Close();
                    continue;
                }
                var id = Path.GetTempFileName();

                var sendFrame = new System.EventHandler<FrameUpdatedEvent>((sender,ev) =>
                {
                    try
                    {
                        SendFrame(client, ev);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"Encountered error: {ex.Message}");
                        // Cleanup
                        _clients.TryRemove(id, out var tuple);
                        cameraController.FrameUpdated -= tuple.Item2;
                        tuple.Item1.Dispose();
                    }
                });

                WriteString(Headers, stream);
                _clients.TryAdd(id, new Tuple<TcpClient, System.EventHandler<FrameUpdatedEvent>>(client, sendFrame));

                cameraController.FrameUpdated += sendFrame;
            }
        }).Start();
    }

    private string GetImageHeaders(byte[] image)
    {
        return "\r\n--boundary\r\nContent-Type: image/jpeg\r\n" +
            $"Content-Length: {image.Length}\r\n\r\n";
    }

    private bool IsGetRequest(NetworkStream stream)
    {
        Byte[] bytes = new Byte[3];
        stream.Read(bytes, 0, bytes.Length);
        return Encoding.ASCII.GetString(bytes, 0, bytes.Length) == "GET";
    }

    private void WriteString(string str, NetworkStream stream)
    {
        var encodedString = Encoding.ASCII.GetBytes(str);
        stream.Write(encodedString, 0, encodedString.Length);
    }

    private void SendFrame(TcpClient client, FrameUpdatedEvent ev)
    {
        var stream = client.GetStream();
        var encodedFrame = ImageConversion.EncodeArrayToJPG(ev.Data, GraphicsFormat.R8G8B8A8_UNorm, (uint)ev.Resolution, (uint)ev.Resolution, 0, 75);

        WriteString(GetImageHeaders(encodedFrame), stream);
        stream.Write(encodedFrame, 0, encodedFrame.Length);
        WriteString("\r\n", stream);
    }
}
