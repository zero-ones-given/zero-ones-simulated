using UnityEngine;
using System;
using System.IO;
using System.Threading;

public class MainController : MonoBehaviour
{
    public GameObject BallPrefab;
    public GameObject RobotPrefab;
    public GameObject TrafficConePrefab;
    public GameObject CubePrefab;
    public GameObject StreamCamera;

    void SpawnDynamicObjects(DynamicObject[] dynamicObjects)
    {
        foreach(DynamicObject dynamicObject in dynamicObjects)
        {
            SpawnDynamicObject(dynamicObject);
        }
    }

    GameObject SpawnPrefab(GameObject prefab, Vector3 position, string hexColor)
    {
        var newObject = Instantiate(prefab);
        newObject.transform.position = position;
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

    void SpawnDynamicObject(DynamicObject dynamicObject)
    {
        var position = new Vector3(
            dynamicObject.position[0],
            dynamicObject.position[1],
            dynamicObject.position[2]
        );
        var prefab = GetPrefab(dynamicObject.type);
        var newObject = SpawnPrefab(prefab, position, dynamicObject.color);
        newObject.GetComponent<Rigidbody>().mass = dynamicObject.mass;
        newObject.transform.localScale = new Vector3(
            dynamicObject.size,
            dynamicObject.size,
            dynamicObject.size
        );

    }

    Texture2D LoadTexture(string filePath)
    {
        var texture = new Texture2D(256, 256);
        var absolutePath = $"{Application.dataPath}/{filePath}".Replace('/', Path.DirectorySeparatorChar);

        if (File.Exists(absolutePath))
        {
            var image = File.ReadAllBytes(absolutePath);
            texture.LoadImage(image);
        }
        return texture;
    }

    void SpawnRobots(Robot[] robots)
    {
        foreach (Robot robot in robots)
        {
            SpawnRobot(robot);
        }
    }

    void SpawnRobot(Robot robot)
    {
        Vector3 position = new Vector3(robot.position[0], robot.position[1], robot.position[2]);
        var newRobot = SpawnPrefab(RobotPrefab, position, robot.color);
        newRobot.transform
            .Find("Marker")
            .GetComponent<Renderer>().material.mainTexture = LoadTexture(robot.marker);

        if (robot.position.Length == 4)
        {
            newRobot.transform.rotation = Quaternion.Euler(0, robot.position[3], 0);
        }
        var controlParts = robot.control.Split(':');
        RobotController robotController = newRobot.GetComponent<RobotController>();
        robotController.Control = controlParts[0];
        if (controlParts.Length == 2)
        {
            Int32.TryParse(controlParts[1], out var port);
            robotController.Port = port;
        }
    }

    void SetCameraOptions(Configuration configuration) {
        StreamCameraController cameraController = StreamCamera.GetComponent<StreamCameraController>();
        cameraController.FrameInterval = 1f / configuration.streamFPS;
        cameraController.Resolution = configuration.streamResolution;
    }

    void Start()
    {
        var jsonString = File.ReadAllText("./configuration.json");
        var configuration = JsonUtility.FromJson<Configuration>(jsonString);

        Time.timeScale = configuration.timeScale;
        QualitySettings.SetQualityLevel(configuration.quality, true);

        Screen.SetResolution(configuration.streamResolution, configuration.streamResolution, false);
        SpawnDynamicObjects(configuration.dynamicObjects);
        SpawnRobots(configuration.robots);
        SetCameraOptions(configuration);
    }

    void Update ()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
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
    public int streamFPS;
    public int streamResolution;
    public Robot[] robots;
    public DynamicObject[] dynamicObjects;
}
