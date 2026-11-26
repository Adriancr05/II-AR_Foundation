using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private int score = 0;

    void Start()
    {
        GameObject scoreObj = GameObject.FindWithTag("ScoreText");
        scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    void OnTriggerEnter(Collider other)
    {
        // Usamos CompareTag para identificar si es un paquete o una araña
        if (other.CompareTag("Package"))
        {
            score++;
            UpdateText();
        }
        else if (other.CompareTag("Spider"))
        {
            score--;
            UpdateText();
        }
    }

    void UpdateText()
    {
        scoreText.text = "Score: " + score;
    }
}
