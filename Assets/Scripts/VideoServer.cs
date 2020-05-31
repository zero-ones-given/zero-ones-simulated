using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
 
class VideoServer
{
    public void Start()
    {
        const string headers = "HTTP/1.1 200 OK\r\n" +
            "Content-Type: multipart/x-mixed-replace; boundary=--boundary\r\n";
        const int port = 8080;
        Debug.Log("Starting http server: " + port);

        TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        byte[] image0 = System.IO.File.ReadAllBytes($"{Application.dataPath}/../0.jpg");
        byte[] image1 = System.IO.File.ReadAllBytes($"{Application.dataPath}/../1.jpg");

        new System.Threading.Thread(() =>
        {
            byte[] EncodeString(string str) {
                return Encoding.ASCII.GetBytes(str);
            }
            string GetImageHeaders(byte[] image) {
                return "\r\n--boundary\r\nContent-Type: image/jpeg\r\n" +
                    $"Content-Length: {image.Length.ToString()}\r\n\r\n";
            }
            void WriteString(string str, NetworkStream stream) {
                byte[] encodedString = EncodeString(str);
                stream.Write(encodedString, 0, encodedString.Length);
            }

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                WriteString(headers, stream);

                int frame = 0;
                while(true) {
                    byte[] image = frame % 2 == 0 ? image0 : image1;
                    WriteString(GetImageHeaders(image), stream);
                    stream.Write(image, 0, image.Length);
                    WriteString("\r\n", stream);
                    Thread.Sleep(1000);
                    frame++;
                }

                client.Close();
            }
        }).Start();
    }
}
