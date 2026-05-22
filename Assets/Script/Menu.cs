using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Панель настроек")]
    public GameObject settingsPanel;

    [Header("Слайдер громкости")]
    public Slider volumeSlider;

    void Start()
    {
        // Загружаем сохранённую громкость
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);

        AudioListener.volume = savedVolume;

        volumeSlider.value = savedVolume;

        // Скрыть настройки
        settingsPanel.SetActive(false);

        // Подписка на изменение громкости
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    // Играть
    public void Play()
    {
        SceneManager.LoadScene("Main");
    }

    // Выход
    public void Exit()
    {
        Application.Quit();
    }

    // Открыть настройки
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Закрыть настройки
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // Изменение громкости
    public void ChangeVolume(float volume)
    {
        AudioListener.volume = volume;

        // Сохранение
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
    }
}