using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections.Specialized;


public class MainController : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject robotPrefab;
    public GameObject trafficConePrefab;

    void SpawnDynamicObjects(DynamicObject[] dynamicObjects)
    {
        foreach(DynamicObject dynamicObject in dynamicObjects) {
            SpawnDynamicObject(dynamicObject);
        }
    }
    GameObject SpawnPrefab(GameObject prefab, Vector3 position)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        return newObject;
    }
    GameObject GetPrefab(string type) {
        switch (type)
        {
            case "traffic-cone":
                return trafficConePrefab;
        }
        return ballPrefab;
    }
    void SpawnDynamicObject(DynamicObject dynamicObject)
    {
        Color color = new Color(1, 1, 1);
        ColorUtility.TryParseHtmlString(dynamicObject.color, out color);
        Vector3 position = new Vector3(
            dynamicObject.position[0],
            dynamicObject.position[1],
            dynamicObject.position[2]
        );

        GameObject prefab = GetPrefab(dynamicObject.type);
        GameObject newObject = SpawnPrefab(prefab, position);
        MeshRenderer renderer = newObject.GetComponent<MeshRenderer>();
        if (renderer) {
            renderer.material.color = color;
        }
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
            SpawnRobot(
                new Vector3(robot.position[0], robot.position[1], robot.position[2]),
                robot.marker,
                robot.control
            );
        }
    }
    void SpawnRobot(Vector3 position, string texture, string control)
    {
        GameObject robot = SpawnPrefab(robotPrefab, position);
        robot.transform
            .Find("Marker")
            .GetComponent<Renderer>().material.mainTexture = LoadTexture(texture);
        robot.GetComponent<RobotController>().control = control;
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