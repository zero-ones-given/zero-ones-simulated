using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ControlDevices
{
    public static string ArrowKeys = "arrows";
    public static string UDP = "udp";
}

public class RobotController : MonoBehaviour
{
    public const int FullThrottle = 7;
    public string control = ControlDevices.UDP;
    public int port = 0;

    public WheelCollider[] rightWheels;
    public WheelCollider[] leftWheels;
    Thread receiveThread;
    UdpClient socket;
    int leftTorque = 0;
    int rightTorque = 0;

    void ListenForUDP()
    {
        socket.BeginReceive(new AsyncCallback(ReceiveData), new {});
    }

    void ParseCommand(string command)
    {
        string[] commandValues = command.Split(',');
        if (commandValues.Length == 2)
        {
            Debug.Log("Left:" + commandValues[0].ToString());
            Int32.TryParse(commandValues[0], out leftTorque);
            Int32.TryParse(commandValues[1], out rightTorque);
        }
    }

    void ReceiveData(IAsyncResult result)
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = socket.EndReceive(result, ref anyIP);
        string command = Encoding.UTF8.GetString(data);
        Debug.Log("Recieved: " + command);
        ParseCommand(command);
        ListenForUDP();
    }

    void ListenArrowKeys()
    {
        leftTorque = 0;
        rightTorque = 0;
        int direction = 0;

        if (Input.GetKey(KeyCode.UpArrow)) {
            direction = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            direction = -1;
        }
        leftTorque = direction * FullThrottle;
        rightTorque = direction * FullThrottle;

        if (Input.GetKey(KeyCode.LeftArrow)) {
            if (direction == 0) {
                leftTorque = -FullThrottle;
                rightTorque = FullThrottle;
                return;
            }
            leftTorque = 0;
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            if (direction == 0) {
                leftTorque = FullThrottle;
                rightTorque = -FullThrottle;
                return;
            }
            rightTorque = 0;
        }
    }

    void Start()
    {
        if (port > 0) {
            socket = new UdpClient(port);
            Debug.Log("Listening for UDP packets on port: " + port);
            ListenForUDP();
        }
    }

    void Update()
    {
        if (control == "arrows") {
            ListenArrowKeys();
        }

        foreach (WheelCollider wheelCollider in leftWheels) {
            wheelCollider.motorTorque = leftTorque;
        }
        foreach (WheelCollider wheelCollider in rightWheels) {
            wheelCollider.motorTorque = rightTorque;
        }
    }
}
