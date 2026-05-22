using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float motorForce = 35f;
    public float steerSpeed = 120f;
    public float brakeForce = 5f;
    public float maxSpeed = 30f;

    [Header("Настройки дрифта")]
    [Range(0f, 1f)] public float normalTriction = 0.95f;
    [Range(0f, 1f)] public float driftTriction = 0.6f;
    public float driftThresholdSpeed = 20f;

    [Header("Стабилизация")]
    public float centerOfMassY = -1.2f;

    [Header("Респавн")]
    public KeyCode resetKey = KeyCode.R;
    public float resetHeightOffset = 1.5f;

    private Rigidbody rb;

    private float moveInput;
    private float turnInput;
    private bool isBraking;

    public float CurrentSpeed { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, centerOfMassY, 0);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 1.5f;
    }

    void Update()
    {
        if (RaceManager.instance != null && !RaceManager.instance.isRaceStarted)
        {
            moveInput = 0;
            turnInput = 0;
            isBraking = true;
            return;
        }

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(resetKey))
        {
            ResetCarPosition();
        }
    }

    void FixedUpdate()
    {
        MoveCar();
        TurnCar();
        ApplyFriction();
        ClampSpeed();
    }

    void MoveCar()
    {
        CurrentSpeed = rb.linearVelocity.magnitude * 3.6f;

        // движение вперёд / назад
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            rb.AddForce(transform.forward * motorForce * moveInput, ForceMode.Acceleration);
        }

        // ограничение скорости (важно — без этого назад будет ломаться)
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // тормоз
        if (isBraking)
        {
            rb.linearVelocity *= 0.92f;
        }
    }

    void TurnCar()
    {
        float speedFactor = Mathf.Clamp(rb.linearVelocity.magnitude / maxSpeed, 0.2f, 1f);

        float turn = turnInput * steerSpeed * speedFactor * Time.fixedDeltaTime;

        Quaternion rotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * rotation);
    }

    void ApplyFriction()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        float friction = normalTriction;

        if (isBraking || rb.linearVelocity.magnitude > driftThresholdSpeed && Mathf.Abs(turnInput) > 0.5f)
        {
            friction = driftTriction;
        }

        localVel.x *= friction;

        rb.linearVelocity = transform.TransformDirection(localVel);
    }

    void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void ResetCarPosition()
    {
        float y = transform.eulerAngles.y;

        transform.rotation = Quaternion.Euler(0, y, 0);

        transform.position += Vector3.up * resetHeightOffset;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void OnValidate()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, centerOfMassY, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (RaceManager.instance != null)
        {
            RaceManager.instance.PlayerTriggered(other, this.gameObject);
        }
    }
}