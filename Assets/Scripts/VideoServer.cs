using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Net;
 
class VideoServer
{        
    public void start()
    {
        const string msg = "Hello World!";        
        const int port = 8080;
        bool serverRunning = true;
        Debug.Log("Starting http server: " + port);

        TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        while (serverRunning)
        {
            Socket socketConnection = tcpListener.AcceptSocket();
            byte[] bMsg = Encoding.ASCII.GetBytes(msg.ToCharArray(), 0, (int)msg.Length);
            socketConnection.Send(bMsg);
            //socketConnection.Disconnect(true);
        }
    }
}
