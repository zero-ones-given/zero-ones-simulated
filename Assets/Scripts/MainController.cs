﻿using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public class MainController : MonoBehaviour
{
    public GameObject BallPrefab;
    public GameObject RobotPrefab;
    public GameObject TrafficConePrefab;
    public GameObject CubePrefab;
    public GameObject StreamCamera;
    GameObject _selectedObject;
    GameObject _highlightedObject;
    UdpClient _socket;
    string _command;
    string _configurationFilePath = "configuration.json";
    GameObject[] _dynamicObjects = {};
    GameObject[] _robots = {};
    Configuration _configuration;

    void ListenForUDP()
    {
        _socket.BeginReceive(new AsyncCallback(ReceiveData), new {});
    }

    void ReceiveData(IAsyncResult result)
    {
        var anyIP = new IPEndPoint(IPAddress.Any, 0);
        var data = _socket.EndReceive(result, ref anyIP);
        _command = Encoding.UTF8.GetString(data);
        Debug.Log($"Main controller recieved upd message: {_command}");
        ListenForUDP();
    }

    void ResetSimulation()
    {
        for (int index = 0; index < _dynamicObjects.Length; index++)
        {
            SetPosition(_dynamicObjects[index], _configuration.dynamicObjects[index].position);
        }
        for (int index = 0; index < _robots.Length; index++)
        {
            _robots[index].GetComponent<RobotController>().reset();
            SetPosition(_robots[index], _configuration.robots[index].position);
        }
    }

    GameObject SpawnPrefab(GameObject prefab, string hexColor)
    {
        var newObject = Instantiate(prefab);
        var color = new Color(1, 1, 1);
        ColorUtility.TryParseHtmlString(hexColor, out color);
        var renderer = newObject.GetComponent<MeshRenderer>();
        if (renderer)
        {
            renderer.material.color = color;
        }
        return newObject;
    }

    GameObject GetPrefab(string type)
    {
        switch (type)
        {
            case "traffic-cone":
                return TrafficConePrefab;
            case "cube":
                return CubePrefab;
            default:
                return BallPrefab;
        }
    }
    void SpawnDynamicObjects(DynamicObject[] dynamicObjects)
    {
        _dynamicObjects = new GameObject[dynamicObjects.Length];
        for (int index = 0; index < dynamicObjects.Length; index++)
        {
            _dynamicObjects[index] = SpawnDynamicObject(dynamicObjects[index]);
        }
    }
    GameObject SpawnDynamicObject(DynamicObject dynamicObject)
    {
        var prefab = GetPrefab(dynamicObject.type);
        var newObject = SpawnPrefab(prefab, dynamicObject.color);
        newObject.GetComponent<Rigidbody>().mass = dynamicObject.mass;
        newObject.transform.localScale = new Vector3(
            dynamicObject.size,
            dynamicObject.size,
            dynamicObject.size
        );
        SetPosition(newObject, dynamicObject.position);
        var isGhost = dynamicObject.type == "ghost-ball";
        newObject.GetComponent<DynamicObjectController>().isGhost = isGhost;
        newObject.GetComponent<DynamicObjectController>().isFlickering = dynamicObject.type == "flickering-ball";

        if (isGhost)
        {
            newObject.GetComponent<Collider>().enabled = false;
        }

        return newObject;
    }

    string getAbsolutePath(string filePath, string subFolder = "")
    {
        return $"{Application.dataPath}/{subFolder}{filePath}".Replace('/', Path.DirectorySeparatorChar);
    }

    Texture2D LoadTexture(string filePath)
    {
        var texture = new Texture2D(256, 256);
        var absolutePath = getAbsolutePath(filePath);
        // On different systems the image folder might be on a parent folder
        if (!File.Exists(absolutePath))
        {
            absolutePath = getAbsolutePath(filePath, "../");
        }

        if (File.Exists(absolutePath))
        {
            var image = File.ReadAllBytes(absolutePath);
            texture.LoadImage(image);
        }
        return texture;
    }

    void SpawnRobots(Robot[] robots)
    {
        _robots = new GameObject[robots.Length];
        for (int index = 0; index < robots.Length; index++)
        {
            _robots[index] = SpawnRobot(robots[index]);
        }
    }

    GameObject SpawnRobot(Robot robot)
    {
        var newRobot = SpawnPrefab(RobotPrefab, robot.color);
        newRobot.transform
            .Find("Marker")
            .GetComponent<Renderer>().material.mainTexture = LoadTexture(robot.marker);
        SetPosition(newRobot, robot.position);

        var controlParts = robot.control.Split(':');
        var robotController = newRobot.GetComponent<RobotController>();
        robotController.Control = controlParts[0];
        if (controlParts.Length == 2)
        {
            Int32.TryParse(controlParts[1], out var port);
            robotController.Port = port;
        }
        return newRobot;
    }

    float[] GetPosition(GameObject gameObject)
    {
        var position = gameObject.transform.position;
        return new [] { position.x, position.y, position.z, gameObject.transform.eulerAngles.y };
    }
    void SetPosition(GameObject gameObject, float[] position)
    {
        var rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.velocity = new Vector3(0,0,0);
        rigidbody.angularVelocity = new Vector3(0,0,0);
        gameObject.transform.position = new Vector3(position[0], position[1], position[2]);
        if (position.Length == 4)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, position[3], 0);
        }
    }

    void SetCameraOptions(Configuration configuration) {
        StreamCameraController cameraController = StreamCamera.GetComponent<StreamCameraController>();
        cameraController.FrameInterval = 1f / configuration.streamFPS;
        cameraController.Resolution = configuration.streamResolution;
        cameraController.StartVideoServer(configuration.streamPort);
    }

    void StartUDPServer(int port) {
        if (port > 0)
        {
            _socket = new UdpClient(port);
            Debug.Log($"Listening for UDP packets on port: {port}");
            ListenForUDP();
        }
    }

    void SaveConfiguration()
    {
        for (int index = 0; index < _dynamicObjects.Length; index++)
        {
            _configuration.dynamicObjects[index].position = GetPosition(_dynamicObjects[index]);
        }
        for (int index = 0; index < _robots.Length; index++)
        {
            _configuration.robots[index].position = GetPosition(_robots[index]);
        }
        File.WriteAllText(_configurationFilePath, JsonUtility.ToJson(_configuration));
    }

    void OpenConfiguration()
    {
        LoadConfiguration(_configurationFilePath);
        SpawnConfigurationObjects();
    }

    void LoadConfiguration(string filePath)
    {
        var jsonString = File.ReadAllText(filePath);
        _configuration = JsonUtility.FromJson<Configuration>(jsonString);
    }

    void SpawnConfigurationObjects()
    {
        _highlightedObject = null;
        _selectedObject = null;
        foreach (var item in _dynamicObjects.Concat(_robots).ToArray())
        {
            if (item != null)
            {
                item.GetComponent<RobotController>()?.Stop();
                Destroy(item);
            }
        }
        _dynamicObjects = new GameObject[]{};
        _robots = new GameObject[]{};
        SpawnDynamicObjects(_configuration.dynamicObjects);
        SpawnRobots(_configuration.robots);
    }

    void Start()
    {
        LoadConfiguration("./configuration.json");
        SpawnConfigurationObjects();

        Time.timeScale = _configuration.timeScale;
        QualitySettings.SetQualityLevel(_configuration.quality, true);
        Screen.SetResolution(_configuration.streamResolution, _configuration.streamResolution, false);
        SetCameraOptions(_configuration);
        StartUDPServer(_configuration.controlPort);
    }

    void Update ()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (_command == "reset" || Input.GetKey("q"))
        {
            ResetSimulation();
            _command = null;
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var mouseDown = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
        var mouseUp = Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var target = hit.collider.gameObject;
            // Select a new object only if no object is selected
            if (_selectedObject == null && mouseDown) {
                _selectedObject = target;
            }
            if (mouseUp) {
                _selectedObject = null;
            }

            _highlightedObject?.GetComponent<Draggable>()?.ResetHighlight();
            _highlightedObject = _selectedObject ?? target;
            var controller = _highlightedObject?.GetComponent<Draggable>();
            if (controller != null) {
                controller.Highlight();
                if (Input.GetMouseButton(0)) {
                    controller.Drag(hit.point);
                }
                if (Input.GetMouseButton(1)) {
                    controller.PointAt(hit.point);
                }
            }
        }
        var isControlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.RightApple);
        if (isControlPressed && Input.GetKeyDown(KeyCode.S))
        {
            SaveConfiguration();
        }
        if (isControlPressed && Input.GetKeyDown(KeyCode.O))
        {
            OpenConfiguration();
        }
    }
}

[System.Serializable]
public class DynamicObject
{
    public string type;
    public string color;
    public float[] position;
    public float mass;
    public float size;
}
[System.Serializable]
public class Robot
{
    public string marker;
    public string color;
    public string control;
    public float[] position;
} 
[System.Serializable]
public class Configuration
{
    public int quality;
    public float timeScale; 
    public int controlPort;
    public int streamFPS;
    public int streamResolution;
    public int streamPort;
    public Robot[] robots;
    public DynamicObject[] dynamicObjects;
}
