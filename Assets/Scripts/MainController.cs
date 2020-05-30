using UnityEngine;
using System.IO;
using System.Threading;


public class MainController : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject robotPrefab;

    void spawnBalls(Ball[] balls)
    {
        foreach(Ball ball in balls) {
            Color color = new Color(1, 1, 1);
            ColorUtility.TryParseHtmlString(ball.color, out color);
            spawnBall(
                new Vector3(ball.position[0], ball.position[1], ball.position[2]),
                color
            );
        }
    }
    GameObject spawnPrefab(GameObject prefab, Vector3 position)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        return newObject;
    }
    void spawnBall(Vector3 position, Color color)
    {
        GameObject ball = spawnPrefab(ballPrefab, position);
        ball.GetComponent<MeshRenderer>().material.color = color;
    }
    Texture2D loadTexture(string filePath)
    {
        Texture2D texture = new Texture2D(256, 256);
        byte[] image;
        string absolutePath = $"{Application.dataPath}/{filePath}".Replace('/', Path.DirectorySeparatorChar);
        Debug.Log(absolutePath);

        if (File.Exists(absolutePath)) {
            image = File.ReadAllBytes(absolutePath);
            texture.LoadImage(image);
        }
        return texture;
    }
    void spawnRobots(Robot[] robots)
    {
        foreach(Robot robot in robots) {
            spawnRobot(
                new Vector3(robot.position[0], robot.position[1], robot.position[2]),
                robot.marker
            );
        }
    }
    void spawnRobot(Vector3 position, string texture)
    {
        GameObject robot = spawnPrefab(robotPrefab, position);
        robot.transform
            .Find("Marker")
            .GetComponent<Renderer>().material.mainTexture = loadTexture(texture);
    }

    void Start()
    {
        string jsonString = File.ReadAllText("./configuration.json");
        Configuration configuration = JsonUtility.FromJson<Configuration>(jsonString);

        Time.timeScale = configuration.timeScale;
        QualitySettings.SetQualityLevel(configuration.quality, true);

        spawnBalls(configuration.balls);
        spawnRobots(configuration.robots);
    }

    void Update ()
    {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
    }
}

[System.Serializable]
public class Ball
{
    public string color;
    public float[] position;
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
    public Ball[] balls;
}