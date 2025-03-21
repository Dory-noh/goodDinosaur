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

    // PlayerMove 스크립트에서 가져온 변수들
    public float speedMultiplier = 15f;
    public float maxSpeed = 7f;
    public float smoothSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 5f;
    private float currentSpeed = 0f;
    private Vector3 targetPosition;
    public LayerMask obstacleLayer;
    private bool obstacleDetected = false;
    public float turnSpeed = 60f; // 회전 속도 (초당 각도)

    public override void Awake()
    {
        base.Awake();
        playerCamera = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Player 오브젝트에 Rigidbody 컴포넌트가 없습니다.");
        }
        targetPosition = transform.position; // 초기 목표 위치 설정
    }

    public override void FixedUpdate()
    {
        if (playerCamera == null || rb == null) return;

        // 로코모션 입력 값 읽기
        Vector2 input = continuousMoveAction.action.ReadValue<Vector2>();

        // 이동 처리 (조이스틱 상하 입력)
        Vector3 desiredMove = ComputeDesiredMove(new Vector2(0, input.y)); // 세로축 입력만 사용

        if (input.y != 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0f, maxSpeed); // 가속
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - deceleration * Time.deltaTime, 0f, maxSpeed); // 감속
            targetPosition = transform.position; //조이스틱 입력을 멈췄을 때 움직인 위치를 현재 위치로 설정한다.
        }

        if (desiredMove != Vector3.zero && currentSpeed > 0)
        {
            // 장애물 감지
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
                Debug.Log("장애물 감지됨");
                rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * smoothSpeed));
            }
        }
        else
        {
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * smoothSpeed));
        }

        // 회전 처리 (조이스틱 좌우 입력)
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
        moveDirection.y = 0; // 수평 이동만 고려

        return moveDirection;
    }
}