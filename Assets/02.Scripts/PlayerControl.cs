using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerControl : Raptor
{
    private Camera playerCamera;
    private Rigidbody rb;
    public InputActionProperty continuousMoveAction;

    // PlayerMove ��ũ��Ʈ���� ������ ������
    public float speedMultiplier = 15f;
    public float maxSpeed = 7f;
    public float smoothSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 5f;
    private float currentSpeed = 0f;
    private Vector3 targetPosition;
    public LayerMask obstacleLayer;
    private bool obstacleDetected = false;
    public float turnSpeed = 60f; // ȸ�� �ӵ� (�ʴ� ����)

    public override void Awake()
    {
        base.Awake();
        playerCamera = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Player ������Ʈ�� Rigidbody ������Ʈ�� �����ϴ�.");
        }
        targetPosition = transform.position; // �ʱ� ��ǥ ��ġ ����
    }

    public override void FixedUpdate()
    {
        if (playerCamera == null || rb == null) return;

        // ���ڸ�� �Է� �� �б�
        Vector2 input = continuousMoveAction.action.ReadValue<Vector2>();

        // �̵� ó�� (���̽�ƽ ���� �Է�)
        Vector3 desiredMove = ComputeDesiredMove(new Vector2(0, input.y)); // ������ �Է¸� ���

        if (input.y != 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0f, maxSpeed); // ����
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - deceleration * Time.deltaTime, 0f, maxSpeed); // ����
            targetPosition = transform.position; //���̽�ƽ �Է��� ������ �� ������ ��ġ�� ���� ��ġ�� �����Ѵ�.
        }

        if (desiredMove != Vector3.zero && currentSpeed > 0)
        {
            // ��ֹ� ����
            obstacleDetected = Physics.Raycast(rb.position, desiredMove.normalized, obstacleSensingDistance, obstacleLayer);

            if (!obstacleDetected)
            {
                Vector3 movement = desiredMove * currentSpeed * Time.deltaTime;
                targetPosition += movement;
                rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * smoothSpeed));
            }
            else
            {
                currentSpeed = 0f;
                Debug.Log("��ֹ� ������");
                rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * smoothSpeed));
            }
        }
        else
        {
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * smoothSpeed));
        }

        // ȸ�� ó�� (���̽�ƽ �¿� �Է�)
        float turnAmount = input.x * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    Vector3 ComputeDesiredMove(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector3.zero;

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        Vector3 moveDirection = (cameraForward * input.y) + (cameraRight * input.x);
        moveDirection.Normalize();
        moveDirection.y = 0; // ���� �̵��� ���

        return moveDirection;
    }
}