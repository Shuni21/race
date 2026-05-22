using UnityEngine;
using System.Collections;

public class CarAudio : MonoBehaviour
{
    [Header("Звуки")]
    public AudioSource warmupSource;
    public AudioSource startSource;
    public AudioSource driveSource;

    [Header("Настройки")]
    public float startDelay = 3f;

    void Start()
    {
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        // 1. Прогрев двигателя
        warmupSource.Play();

        // Ждём перед стартом
        yield return new WaitForSeconds(startDelay);

        // Остановить прогрев
        warmupSource.Stop();

        // 2. Звук старта
        startSource.Play();

        // Ждём пока стартовый звук закончится
        yield return new WaitForSeconds(startSource.clip.length);

        // 3. Основной звук езды
        driveSource.Play();
    }
}