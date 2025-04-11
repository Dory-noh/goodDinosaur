using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
//using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerControl : Raptor //랩터 클래스 상속
{
    private Camera playerCamera;
    private Rigidbody rb_player;
    public InputActionProperty continuousMoveAction;
    public InputActionProperty continuousTurnAction;
    public InputActionProperty AttackAction;

    // PlayerMove 스크립트에서 가져온 변수들
    public float speedMultiplier = 15f;
    public float maxSpeed = 7f;
    public float smoothSpeed = 5f;
    public float acceleration = 15f;
    public float deceleration = 5f;
    private float currentSpeed = 0f;
    private Vector3 targetPosition;
    public LayerMask obstacleLayer;
    //private bool obstacleDetected_player = false;
    public float turnSpeed = 60f; // 회전 속도 (초당 각도)

    private int hungerLV;
    public int HungerLV
    {
        get { return hungerLV = GameManager.Instance.currentHungerLevel; }
        set
        {
            hungerLV = value;
            GameManager.Instance.currentHungerLevel = hungerLV;
        }
    }

    [Header("사운드 관련")]
    public AudioSource _audio;
    public AudioClip walk;

    [Header("햅틱 피드백 설정")]
    [Tooltip("햅틱 효과 강도 (0 ~ 1")]
    [Range(0f, 1f)]
    [SerializeField] private float hapticIntansity = 1f;

    [Tooltip("햅틱 효과 지속 시간 (초)")]
    [SerializeField] float hapticDuration = 0.2f;

    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private ActionBasedController rightController;


    public override void Awake()
    {
        base.Awake();
        playerCamera = GetComponentInChildren<Camera>();
        rb_player = GetComponent<Rigidbody>();
        HungerLV = GameManager.Instance.currentHungerLevel;
        targetPosition = transform.position; // 초기 목표 위치 설정
        if (leftController != null && rightController != null) Debug.Log("컨트롤러 연결 완료");
        else Debug.Log("컨트롤러 연결 실패");
    }
    public override void OnEnable()
    {
        base.OnEnable();
        if(GameManager.Instance.Day == 1)
        {
            leader = null;
            followers.Clear();
            leader = this;
        }
        AttackAction.action.performed += OnAttackPerformed;
    }

    public override void FixedUpdate()
    {
        if (playerCamera == null || rb_player == null || GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false)
        {
            return;
        }

        // HP 자동 회복 로직
        if (Time.time - lastDamageTime >= regenerationInterval && hp < MaxHp)
        {
            RecoverHP();
        }

        Move();
        Turn(); // 회전 기능
    }

    public override IEnumerator Damage(float power)
    {
        StartCoroutine(base.Damage(power));
        TriggerHaptic(leftController, hapticIntansity, hapticDuration);
        TriggerHaptic(rightController, hapticIntansity, hapticDuration);
        yield return null;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        closestVictim = FindClosestVictim();
        if (closestVictim != null) // closestVictim이 null이 아닌지 확인
        {
            Hunt(closestVictim);
            Debug.Log("A 키로 공격합니다.");
            TriggerHaptic(leftController, hapticIntansity, hapticDuration);
            TriggerHaptic(rightController, hapticIntansity, hapticDuration);
        }
        else
        {
            Debug.Log("공격할 대상이 없습니다."); // 공격 대상이 없을 경우 메시지 출력 (선택 사항)
        }
    }

    public void TriggerHaptic(ActionBasedController controller, float intensity, float duration)
    {
        if (controller != null)
        {
            if (controller.hapticDeviceAction != null)
            {
                    controller.SendHapticImpulse(intensity, duration);
            }
        }
    }

    public override void Move()
    {

        // 로코모션 입력 값 읽기
        Vector2 inputMove = continuousMoveAction.action.ReadValue<Vector2>();

        // 이동 처리 (조이스틱 상하좌우 입력 모두 사용)
        Vector3 desiredMove = ComputeDesiredMove(inputMove); // inputMove 전체를 전달

        if (inputMove.magnitude > 0.1f) // 입력이 있을 때만 가속 (미세한 떨림 방지)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0f, maxSpeed); // 가속
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - deceleration * Time.deltaTime, 0f, maxSpeed); // 감속
        }

        if (desiredMove != Vector3.zero && currentSpeed > 0)
        {
            Vector3 movement = desiredMove * currentSpeed * Time.deltaTime;
            RaycastHit hit;


            // 이동 방향으로 장애물이 있는지 Raycast
            if (Physics.Raycast(rb_player.position, movement.normalized, out hit, movement.magnitude, obstacleLayer))
            {
                // 장애물에 부딪혔다면 충돌 지점까지만 이동
                rb_player.MovePosition(rb_player.position + movement.normalized * hit.distance);
                currentSpeed = 0f; // 충돌했으므로 속도 0으로
                Debug.Log("장애물 감지됨 (이동 제한)");
            }
            else
            {
                PlaySound(walk, true);
                // 장애물이 없다면 계획된 거리만큼 이동
                rb_player.MovePosition(rb_player.position + movement);
            }
        }
        else
        {
            _audio.Stop();
        }

    }

    void Turn()
    {
        // 회전 입력 값 읽기 (x축)
        float turnInputValue = continuousTurnAction.action.ReadValue<Vector2>().x;

        // 회전 입력이 있을 경우에만 회전
        if (turnInputValue != 0)
        {
            // 회전 방향과 속도를 적용
            float rotationAmount = turnInputValue * turnSpeed * Time.deltaTime;

            // Rigidbody를 사용하여 안전하게 회전
            Quaternion targetRotation = rb_player.rotation * Quaternion.Euler(0f, rotationAmount, 0f);
            float rotationDamping = 20f;
            rb_player.MoveRotation(Quaternion.Slerp(rb_player.rotation, targetRotation, Time.deltaTime * rotationDamping));
        }
    }

    
    Vector3 ComputeDesiredMove(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector3.zero;

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // 카메라 전방 방향과 오른쪽 방향을 기준으로 이동 방향 계산 (x축, y축 모두 사용)
        Vector3 moveDirection = (cameraForward * input.y) + (cameraRight * input.x);
        moveDirection.Normalize();
        moveDirection.y = 0; // 수평 이동만 고려

        return moveDirection;
    }

    public void IncreaseHungerLevel()
    {
        Debug.Log($"HungerLevel변경 전: {HungerLV} : {GameManager.Instance.currentHungerLevel}");
        HungerLV++;
        HungerLV = Mathf.Clamp(HungerLV, 0, GameManager.Instance.maxHungerLevel);
        Debug.Log($"HungerLevel변경 후: {HungerLV} : {GameManager.Instance.currentHungerLevel}");
    }

    public override void Die()
    {
        base.Die();
        //GameManager.Instance.GameOver = true;
        //GameManager.Instance.IsPlay = false;
        GetComponent<ControllerEvent>().EnableRayInteractor();
    }

    void PlaySound(AudioClip clip, bool loop)
    {
        //같은 소리 재생 중이면 무시
        if (_audio.isPlaying && _audio.clip == clip) return;

        _audio.Stop();
        _audio.clip = clip;
        _audio.loop = loop;
        _audio.Play();
    }



    public override void OnDisable()
    {
        AttackAction.action.performed -= OnAttackPerformed;
    }

}