using UnityEngine;
using UnityEngine.UI;

public class GoalController : MonoBehaviour
{
    private int score;
    public Text scoreText;
    
    public void updateScore(int delta)
    {
        score += delta;
        scoreText.text = $"{score}";
    }
}
