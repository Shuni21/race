using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    public CarController car;
    public TextMeshProUGUI speedometerText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI lapText;

    private bool raceEnded = false;

    void Update()
    {
        if (raceEnded) return;

        if (car != null && speedometerText != null)
        {
            int speed = Mathf.RoundToInt(Mathf.Abs(car.CurrentSpeed));
            speedometerText.text = speed.ToString() + " KM/H";
        }
    }

    public void UpdateTimerText(float timeToDisplay)
    {
        if (timerText == null || raceEnded) return;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliSeconds = (timeToDisplay % 1) * 100;
        timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliSeconds);
    }

    public void UpdateTimerTextCustom(string text)
    {
        if (timerText != null) timerText.text = text;
    }

    public void UpdateLapText(string text)
    {
        if (lapText != null) lapText.text = text;
    }

    // Универсальный экран конца гонки
    public void ShowEndGameScreen(string endMessage)
    {
        raceEnded = true;
        if (lapText != null)
        {
            lapText.text = endMessage; // Покажет "YOU WIN!" или "YOU LOSE!"
        }
    }
}