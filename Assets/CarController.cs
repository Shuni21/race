using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float motorForce = 1500f;
    public float steerSpeed = 100f;
    public float brakeForce = 3000f;
    public float maxSpeed = 30f;

    [Header("Настройки дрифта")]
    [Range(0f, 1f)] public float normalTriction = 0.95f;
    [Range(0f, 1f)] public float driftTriction = 0.05f;
    public float driftLeftOver = 0.1f;

    [Header("Стабилизация и Центр Масс")]
    [Tooltip("Смещение центра масс вниз. Чем ниже (например, -1.5), тем машина устойчивее.")]
    public float centerOfMassY = -1.5f;

    [Header("Клавиша восстановления (Переворот)")]
    public KeyCode resetKey = KeyCode.R; // Кнопка R для возврата на колеса
    public float resetHeightOffset = 1.5f; // На какую высоту приподнять машину при сбросе

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private bool isBraking;

    public float CurrentSpeed { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Применяем второй способ: настраиваем центр масс из переменной
        SetCenterOfMass();
    }

    void Update()
    {
        // Если гонка ещё не началась (идет отсчёт), блокируем ввод игрока
        if (RaceManager.instance != null && !RaceManager.instance.isRaceStarted)
        {
            moveInput = 0f;
            turnInput = 0f;
            isBraking = true; // Удерживаем машину тормозом
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
    }

    // Метод для настройки центра масс
    public void SetCenterOfMass()
    {
        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, centerOfMassY, 0);
        }
    }

    void MoveCar()
    {
        CurrentSpeed = transform.InverseTransformDirection(rb.linearVelocity).z * 3.6f;

        if (Mathf.Abs(CurrentSpeed) < maxSpeed * 3.6f)
        {
            Vector3 forceVector = transform.forward * moveInput * motorForce;
            rb.AddForce(forceVector, ForceMode.Acceleration);
        }

        if (isBraking)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
        }
    }

    void TurnCar()
    {
        float speedFactor = transform.InverseTransformDirection(rb.linearVelocity).z;
        float direction = speedFactor >= 0 ? 1f : -1f;

        if (Mathf.Abs(speedFactor) > 0.1f)
        {
            float currentSteerSpeed = isBraking ? steerSpeed * 1.5f : steerSpeed;
            float turnAmount = turnInput * currentSteerSpeed * direction * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    void ApplyFriction()
    {
        Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        float currentFriction = normalTriction;
        if (isBraking || (Mathf.Abs(turnInput) > 0.5f && Mathf.Abs(CurrentSpeed) > 20f))
        {
            currentFriction = driftTriction;
        }

        rb.linearVelocity = rb.linearVelocity - lateralVelocity * (currentFriction * (1f - driftLeftOver));
    }

    // Функция, которая ставит машину обратно на колеса
    void ResetCarPosition()
    {
        // 1. Сбрасываем вращение по X и Z, оставляя только текущее направление взгляда (Y)
        float currentYRotation = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        // 2. Слегка приподнимаем машину, чтобы она не застряла в текстурах дороги
        transform.position = new Vector3(transform.position.x, transform.position.y + resetHeightOffset, transform.position.z);

        // 3. Полностью гасим физические скорости (чтобы машина не продолжала лететь кувырком)
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Автоматическое обновление центра масс, если ты меняешь ползунок прямо во время игры
    void OnValidate()
    {
        rb = GetComponent<Rigidbody>();
        SetCenterOfMass();
    }

    // Вызывается автоматически Unity, когда машина въезжает в Is Trigger коллайдер
    void OnTriggerEnter(Collider other)
    {
        if (RaceManager.instance != null)
        {
            // Передаем коллайдер и сам объект игрока
            RaceManager.instance.PlayerTriggered(other, this.gameObject);
        }
    }
}