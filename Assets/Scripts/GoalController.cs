using UnityEngine;
using UnityEngine.UI;

public class GoalController : MonoBehaviour
{
    private int score;
    public Text scoreText;
    
    public void SetScore(int newScore)
    {
        score = newScore;
        scoreText.text = $"{score}";
    }

    public void UpdateScore(int delta)
    {
        SetScore(score + delta);
    }
}
