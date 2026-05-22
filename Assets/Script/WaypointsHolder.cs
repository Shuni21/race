using UnityEngine;
using System.Collections.Generic;

public class WaypointsHolder : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();

    void OnDrawGizmos()
    {
        // Очищаем и заново собираем точки при изменении на сцене
        waypoints.Clear();
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }

        // Рисуем линии между точками в редакторе Unity для удобства
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 current = waypoints[i].position;
            Vector3 next = waypoints[(i + 1) % waypoints.Count].position; // Зацикливаем круг

            Gizmos.DrawSphere(current, 0.5f);
            Gizmos.DrawLine(current, next);
        }
    }
}