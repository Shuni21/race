using UnityEngine;

public class AICarController : MonoBehaviour
{
    public int aiId;

    [Header("Waypoints")]
    public WaypointsHolder waypointsHolder;

    [Header("Movement")]
    public float motorForce = 1300f;
    public float steerSpeed = 90f;
    public float maxSpeed = 25f;
    public float reachDistance = 5f;

    public enum Difficulty
    {
        Easy,
        Hard
    }

    [Header("Difficulty")]
    public Difficulty difficulty = Difficulty.Easy;

    [Header("Avoidance")]
    public float avoidDistance = 5f;
    public float avoidStrength = 2f;

    [Header("Lane Offset")]
    public float laneOffset = 1.5f;

    private Rigidbody rb;

    private int waypointIndex = 0;

    private bool finished = false;

    private float currentMotorForce = 0f;

    private Vector3 offsetTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Более стабильная физика
        rb.mass = 900f;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 3f;

        // Чтобы машина не переворачивалась
        rb.centerOfMass = new Vector3(0, -0.7f, 0);

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        // AI не сталкиваются друг с другом
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("AI"),
            LayerMask.NameToLayer("AI"),
            true
        );

        if (waypointsHolder == null)
        {
            waypointsHolder =
                Object.FindAnyObjectByType<WaypointsHolder>();
        }

        FindClosestWaypoint();

        // Разные полосы движения
        float randomOffset =
            Random.Range(-laneOffset, laneOffset);

        offsetTarget = new Vector3(randomOffset, 0, 0);
    }

    void FixedUpdate()
    {
        if (RaceManager.instance == null || finished)
            return;

        // До старта гонки стоим
        if (!RaceManager.instance.isRaceStarted)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Плавный разгон
        currentMotorForce = Mathf.Lerp(
            currentMotorForce,
            motorForce,
            0.5f * Time.fixedDeltaTime
        );

        Move();
    }

    void Move()
    {
        if (waypointsHolder == null)
            return;

        if (waypointsHolder.waypoints.Count == 0)
            return;

        Transform wp =
            waypointsHolder.waypoints[waypointIndex];

        Vector3 target =
            wp.position +
            wp.right * offsetTarget.x;

        Vector3 localTarget =
            transform.InverseTransformPoint(target);

        localTarget.y = 0;

        // =========================
        // AVOIDANCE
        // =========================

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                avoidDistance
            );

        Vector3 avoid = Vector3.zero;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            AICarController otherAI =
                hit.GetComponent<AICarController>();

            if (otherAI != null)
            {
                Vector3 dir =
                    transform.position -
                    hit.transform.position;

                avoid +=
                    dir.normalized /
                    Mathf.Max(dir.magnitude, 0.1f);
            }
        }

        localTarget +=
            transform.InverseTransformDirection(
                avoid * avoidStrength
            );

        // =========================
        // РАСЧЕТ ПОВОРОТА
        // =========================

        float distance =
            Mathf.Max(localTarget.magnitude, 0.1f);

        float steer =
            Mathf.Clamp(localTarget.x / distance, -1f, 1f);

        float speed =
            transform.InverseTransformDirection(
                rb.linearVelocity
            ).z;

        float speedMul =
            difficulty == Difficulty.Easy
            ? 0.9f
            : 1.2f;

        float max =
            maxSpeed * speedMul;

        // =========================
        // ПОВОРОТ
        // =========================

        float forwardDirection =
            speed >= 0 ? 1f : -1f;

        float turnStrength =
            steer *
            steerSpeed *
            forwardDirection *
            Time.fixedDeltaTime;

        Quaternion turn =
            Quaternion.Euler(0f, turnStrength, 0f);

        rb.MoveRotation(rb.rotation * turn);

        // Гасим вращение
        Vector3 angVel = rb.angularVelocity;

        angVel.y *= 0.6f;

        rb.angularVelocity = angVel;

        // =========================
        // ДВИЖЕНИЕ
        // =========================

        if (speed < max)
        {
            float forwardPower =
                Mathf.Lerp(
                    1.0f,
                    0.1f,
                    Mathf.Abs(steer)
                );

            // Торможение в крутых поворотах
            if (Mathf.Abs(steer) > 0.5f &&
                speed > max * 0.5f)
            {
                rb.AddForce(
                    -transform.forward *
                    currentMotorForce *
                    0.8f,
                    ForceMode.Acceleration
                );
            }
            else
            {
                rb.AddForce(
                    transform.forward *
                    currentMotorForce *
                    forwardPower,
                    ForceMode.Acceleration
                );
            }
        }

        // =========================
        // УБИРАЕМ СКОЛЬЖЕНИЕ
        // =========================

        Vector3 forwardVel =
            transform.forward *
            Vector3.Dot(
                rb.linearVelocity,
                transform.forward
            );

        rb.linearVelocity =
            Vector3.Lerp(
                rb.linearVelocity,
                forwardVel,
                15f * Time.fixedDeltaTime
            );

        // =========================
        // СМЕНА WAYPOINT
        // =========================

        if ((transform.position - target).sqrMagnitude <
            reachDistance * reachDistance)
        {
            waypointIndex++;

            if (waypointIndex >=
                waypointsHolder.waypoints.Count)
            {
                waypointIndex = 0;
            }
        }
    }

    void FindClosestWaypoint()
    {
        if (waypointsHolder == null)
            return;

        float best = Mathf.Infinity;

        for (int i = 0;
             i < waypointsHolder.waypoints.Count;
             i++)
        {
            float d =
                Vector3.Distance(
                    transform.position,
                    waypointsHolder.waypoints[i].position
                );

            if (d < best)
            {
                best = d;
                waypointIndex = i;
            }
        }
    }

    public void Finish()
    {
        finished = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (RaceManager.instance != null)
        {
            RaceManager.instance.PlayerTriggered(
                other,
                this.gameObject
            );
        }
    }
}