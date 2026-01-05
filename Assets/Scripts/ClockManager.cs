using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI clockText;

    private float elapsedTime;

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateClockDisplay();
    }

    void UpdateClockDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        clockText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}
