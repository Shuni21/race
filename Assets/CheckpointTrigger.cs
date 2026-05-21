using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (RaceManager.instance == null) return;

        Debug.Log($"[CHECKPOINT] Объект вошёл в триггер: {other.gameObject.name} | Tag: {other.gameObject.tag} | Trigger: {gameObject.name}");

        // Дополнительный лог именно для финиша (если есть тег Finish)
        if (gameObject.CompareTag("Finish"))
        {
            Debug.Log($"🏁 ФИНИШ ПЕРЕСЁК: {other.gameObject.name}");
        }

        RaceManager.instance.PlayerTriggered(
            GetComponent<Collider>(),
            other.gameObject
        );
    }
}