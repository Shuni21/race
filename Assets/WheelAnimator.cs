using UnityEngine;

public class WheelAnimator : MonoBehaviour
{
    [Header("������ �� �����")]
    public Transform frontLeftVisual;
    public Transform frontRightVisual;
    public Transform rearLeftVisual;
    public Transform rearRightVisual;

    [Header("��������� ��������")]
    public float maxSteerAngle = 30f; // ������������ ���� �������� �������� �����

    private Rigidbody carRigidbody;
    private float turnInput;

    void Start()
    {
        // ���� Rigidbody �� ������������ ������� ������
        carRigidbody = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // 1. �������� ���� �������� �� ������ (A/D ��� �������)
        turnInput = Input.GetAxis("Horizontal");

        // 2. �������� ��˨� (������ �������� ������/�����)
        if (carRigidbody != null)
        {
            // �������� ��������� �������� ������ �� ��� Z
            float speed = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

            // ������� ���� �������� �� ������ �������� (�������� �� ����������� ��� ��������������)
            float rotationAmount = speed * 10f * Time.deltaTime;

            // ������ ��� 4 ������ �� �� ��������� ��� X
            frontLeftVisual.Rotate(Vector3.right, rotationAmount, Space.Self);
            frontRightVisual.Rotate(Vector3.right, rotationAmount, Space.Self);
            rearLeftVisual.Rotate(Vector3.right, rotationAmount, Space.Self);
            rearRightVisual.Rotate(Vector3.right, rotationAmount, Space.Self);
        }

        // 3. ������� �������� ��˨� (�����/������)
        float targetSteerAngle = turnInput * maxSteerAngle;

        // ������ ������������ ��������� Y �������� ���� � ������� ����
        // ��� ���� ��������� �� ������� ������ �� X, ����� �������� �� �������� �� ������������!
        RotateFrontWheel(frontLeftVisual, targetSteerAngle);
        RotateFrontWheel(frontRightVisual, targetSteerAngle);
    }

    void RotateFrontWheel(Transform wheel, float targetAngle)
    {
        // ��������� ������� ���� ������ ������
        Vector3 currentLocalEuler = wheel.localEulerAngles;

        // ������ ������ ��� Y (�������), �������� X (��������) � Z �����������
        wheel.localRotation = Quaternion.Euler(currentLocalEuler.x, targetAngle, 0f);
    }
}