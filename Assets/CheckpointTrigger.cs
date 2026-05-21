using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (RaceManager.instance == null) return;

        RaceManager.instance.PlayerTriggered(
            GetComponent<Collider>(),
            other.gameObject
        );
    }
}