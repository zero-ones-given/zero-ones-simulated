using UnityEngine;
using System;
using System.IO;
using System.Threading;

public class MainController : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject robotPrefab;
    public GameObject trafficConePrefab;
    public GameObject cubePrefab;

    void SpawnDynamicObjects(DynamicObject[] dynamicObjects)
    {
        foreach(DynamicObject dynamicObject in dynamicObjects) {
            SpawnDynamicObject(dynamicObject);
        }
    }
    GameObject SpawnPrefab(GameObject prefab, Vector3 position, string hexColor)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        Color color = new Color(1, 1, 1);
        ColorUtility.TryParseHtmlString(hexColor, out color);
        MeshRenderer renderer = newObject.GetComponent<MeshRenderer>();
        if (renderer) {
            renderer.material.color = color;
        }
        return newObject;
    }
    GameObject GetPrefab(string type) {
        switch (type)
        {
            case "traffic-cone":
                return trafficConePrefab;
            case "cube":
                return cubePrefab;
        }
        return ballPrefab;
    }
    void SpawnDynamicObject(DynamicObject dynamicObject)
    {
        Vector3 position = new Vector3(
            dynamicObject.position[0],
            dynamicObject.position[1],
            dynamicObject.position[2]
        );
        GameObject prefab = GetPrefab(dynamicObject.type);
        GameObject newObject = SpawnPrefab(prefab, position, dynamicObject.color);
        newObject.GetComponent<Rigidbody>().mass = dynamicObject.mass;
        newObject.transform.localScale = new Vector3(
            dynamicObject.size,
            dynamicObject.size,
            dynamicObject.size
        );

    }
    Texture2D LoadTexture(string filePath)
    {
        Texture2D texture = new Texture2D(256, 256);
        byte[] image;
        string absolutePath = $"{Application.dataPath}/{filePath}".Replace('/', Path.DirectorySeparatorChar);

        if (File.Exists(absolutePath)) {
            image = File.ReadAllBytes(absolutePath);
            texture.LoadImage(image);
        }
        return texture;
    }
    void SpawnRobots(Robot[] robots)
    {
        foreach(Robot robot in robots) {
            SpawnRobot(robot);
        }
    }
    void SpawnRobot(Robot robot)
    {
        Vector3 position = new Vector3(robot.position[0], robot.position[1], robot.position[2]);
        GameObject newRobot = SpawnPrefab(robotPrefab, position, robot.color);
        newRobot.transform
            .Find("Marker")
            .GetComponent<Renderer>().material.mainTexture = LoadTexture(robot.marker);

        if (robot.position.Length == 4) {
            newRobot.transform.rotation = Quaternion.Euler(0, robot.position[3], 0);
        }
        string[] controlParts = robot.control.Split(':');
        RobotController robotController = newRobot.GetComponent<RobotController>();
        robotController.control = controlParts[0];
        if (controlParts.Length == 2) {
            int port;
            Int32.TryParse(controlParts[1], out port);
            robotController.port = port;
        }
    }

    void Start()
    {
        string jsonString = File.ReadAllText("./configuration.json");
        Configuration configuration = JsonUtility.FromJson<Configuration>(jsonString);

        Time.timeScale = configuration.timeScale;
        QualitySettings.SetQualityLevel(configuration.quality, true);

        SpawnDynamicObjects(configuration.dynamicObjects);
        SpawnRobots(configuration.robots);
    }

    void Update ()
    {
        if (Input.GetKey("escape")) {
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
    public Robot[] robots;
    public DynamicObject[] dynamicObjects;
}
