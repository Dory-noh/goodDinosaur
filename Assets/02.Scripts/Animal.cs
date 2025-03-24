using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Animal : MonoBehaviour, IMovable, IDinosaur
{
    public int[] sizes = new int[3] { 10, 50 ,100};
    public float[] speeds = new float[3] {8, 5, 3};
    public int[] HPs = new int[3] { 5, 10, 20 };
    public float[] powers = new float[3] { 1, 2, 5 };
    public int infoIdx = 0;
    public int size;
    public float moveSpeed;
    public float hp;
    public float power;

    public bool isDie;
    

    public float moveSpeedMin { get; set; } = 3.0f; //최소 이동 속도
    public float moveSpeedMax { get; set; } = 8.0f; //최대 이동 속도
    public float maxTurnRateY { get; set; } = 15f; //최대 회전 속도
    public float maxWanderAngle { get; set; } = 45f; //최대 방황 각도
    public float wanderPeriodDuration { get; set; } = 0.8f; //방황 주기
    public float wanderProbability { get; set; } = 0.15f; //방황 확률

    private Vector3 moveDirection //현재 이동 방향
    {
        get { return transform.TransformDirection(Vector3.forward); }
    }

    private float wanderPeriodStartTime; //방황 시작 시간
    protected Quaternion goalLookRotation; //목표 회전 방향
    private float randomOffset; // 랜덤 오프셋

    [SerializeField] private bool isBumped = false;

    // 장애물 회피 시 기준점으로 사용됨.
    public Transform tankCenterGoal;

    // 장애물 감지 거리 (단위: 미터).  
    // 이 거리 안에 장애물이 있으면 회피 행동을 시작함.
    public float obstacleSensingDistance = 1.5f;

    // 장애물을 감지했는지 여부
    private bool obstacleDetected = false;
    // 디버그용 변수 (광선과 목표 지점을 그리기 위해 사용)
    private Vector3 hitPoint;
    private Vector3 goalPoint;

    public float playerSensingDistance = 5f;
    protected List<IDinosaur> dinosaurs = new List<IDinosaur>(); // 오브젝트 캐싱
    private Collider[] colliders = new Collider[10]; // 콜라이더 배열

    //포식자 발견했는지 여부
    protected bool predatorDetected = false;
    Rigidbody rb;
    CharacterController characterController; // CharacterController 변수 추가
    private float gravity = -9.81f; // 중력
    private Vector3 verticalVelocity; // 수직 속도

    //레이어 마스크 리스트
    protected List<int> findLayerMask = new List<int>();

    public float maxSlopeAngle = 0.1f; // 오를 수 있는 최대 경사 각도 (수정 가능)

    private bool attemptingSecondaryAvoidance = false;
    private Quaternion secondaryGoalLookRotation;
    private bool initialAvoidanceTried = false;

    private bool isGrounded;
    private bool isOnSlope;
    private Vector3 slopeNormal;

    public virtual void Awake()
    {
        //초기화 시에 오브젝트 검색 및 캐싱
        dinosaurs.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<IDinosaur>());
        tankCenterGoal = GameObject.Find("center").transform;
        rb = GetComponent<Rigidbody>();
        
        //랩터인 경우 랩터는 추적 제외
        findLayerMask.Add(LayerMask.GetMask("Carnivore"));
        //포식자 레이어 마스크 설정
        findLayerMask.Add(LayerMask.GetMask("Raptor"));
        findLayerMask.Add(LayerMask.GetMask("Herbivore"));
        findLayerMask.Add(LayerMask.GetMask("Obstacle"));
        findLayerMask.Add(LayerMask.GetMask("Ground"));
        
        playerSensingDistance = 30f;
        if (this is not Raptor) infoIdx = Random.Range(0, sizes.Length);
        else infoIdx = 0;
        //Debug.Log($"인덱스 번호 : {infoIdx}");
    }

    public virtual void OnEnable()
    {
        moveSpeed = CalculateSpeed(infoIdx);
        hp = HPs[infoIdx];
        power = powers[infoIdx];
        randomOffset = Random.value;
        isDie = false;
        verticalVelocity.y = 0f; // 활성화 시 수직 속도 초기화
    }
    public virtual void FixedUpdate()
    {
        //// 중력 적용
        //if (characterController.isGrounded)
        //{
        //    verticalVelocity.y = -1f; // 바닥에 붙어있도록 작은 음수 값 설정
        //}
        //else
        //{
        //    verticalVelocity.y += gravity * Time.fixedDeltaTime;
        //}

        //Vector3 movement = Vector3.zero;

        //if (isBumped == true) Invoke("ResetBumpCheck", 3f);
        AvoidObstacles();
        if (obstacleDetected)
        {
            Move();

            return;
        }
        //1. 포식자 회피(최우선 순위)
        AvoidPredator();
        if (predatorDetected)
        {
            Move();
            return;
        }

        //2. 장애물 회피(두 번째 우선 순위)
        // 장애물을 피하는 방향으로 회전했을 수 있으므로, 이동 방향을 업데이트한다.
        

        //3. 기본 움직임(방황)
        Wander();
        Move();

    }
    void ResetBumpCheck()
    {
        isBumped = false;
    }

    /// 장애물 감지 및 회피 로직.
    /// </summary>
    void AvoidObstacles()
    {
        RaycastHit hit;
        obstacleDetected = Physics.Raycast(transform.position, moveDirection, out hit, obstacleSensingDistance);

        if (obstacleDetected)
        {
            if (!attemptingSecondaryAvoidance)
            {
                // Initial avoidance attempt (reflection)
                hitPoint = hit.point;
                Vector3 reflectionVector = Vector3.Reflect(moveDirection, hit.normal);
                float goalPointMinDistanceFromHit = 1f;
                Vector3 reflectedPoint = hit.point + reflectionVector * Mathf.Max(hit.distance, goalPointMinDistanceFromHit);
                Debug.DrawRay(transform.position, moveDirection * obstacleSensingDistance, Color.red);
                goalPoint = (reflectedPoint * 0.7f + tankCenterGoal.position * 0.3f); // tankCenterGoal 영향 감소
                Vector3 goalDirection = goalPoint - transform.position;
                goalDirection.y = 0;
                goalLookRotation = Quaternion.LookRotation(goalDirection.normalized);

                float dangerLevel = Mathf.Clamp01(1 - (hit.distance / obstacleSensingDistance));
                dangerLevel = Mathf.Max(0.01f, dangerLevel);

                float turnRate = maxTurnRateY * dangerLevel;
                Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * turnRate);
                Vector3 eulerAngles = rotation.eulerAngles;
                eulerAngles.x = 0;
                eulerAngles.z = 0;
                transform.rotation = Quaternion.Euler(eulerAngles);
                initialAvoidanceTried = true;
                attemptingSecondaryAvoidance = false; // 초기 회피 시도 후 이차 시도 플래그 초기화
            }
            else
            {
                // Secondary avoidance attempt (turn opposite)
                Vector3 oppositeDirection = -transform.forward;
                secondaryGoalLookRotation = Quaternion.LookRotation(oppositeDirection);
                Quaternion rotation = Quaternion.Slerp(transform.rotation, secondaryGoalLookRotation, Time.fixedDeltaTime * maxTurnRateY * 0.5f); // 약간 느린 회전
                Vector3 eulerAngles = rotation.eulerAngles;
                eulerAngles.x = 0;
                eulerAngles.z = 0;
                transform.rotation = Quaternion.Euler(eulerAngles);
            }
        }
        else
        {
            attemptingSecondaryAvoidance = false; // 장애물 감지 안 되면 이차 시도 플래그 초기화
            initialAvoidanceTried = false;
        }

        // 초기 회피 시도 후에도 여전히 장애물이 감지되면 이차 회피 시도
        if (obstacleDetected && !attemptingSecondaryAvoidance && initialAvoidanceTried && Vector3.Distance(transform.position, goalPoint) < 2f)
        {
            attemptingSecondaryAvoidance = true;
        }
        else if (!obstacleDetected && attemptingSecondaryAvoidance)
        {
            attemptingSecondaryAvoidance = false;
            initialAvoidanceTried = false;
        }

        // 높은 곳에서 떨어지면 "추락" 메시지 출력
        if (!isGrounded && verticalVelocity.y < -5f) // 수직 속도가 일정 값보다 빠르게 떨어지면 추락으로 간주
        {
            Debug.Log("추락");
        }
    }


    public void Wander()
    {
        float noiseScale = .5f;
        float speedPercent = Mathf.PerlinNoise(Time.time * noiseScale + randomOffset, randomOffset);
        speedPercent = Mathf.Pow(speedPercent, 2);
        moveSpeed = Mathf.Lerp(moveSpeedMin, moveSpeedMax, speedPercent);

        if (Time.time > wanderPeriodStartTime + wanderPeriodDuration)
        {
            wanderPeriodStartTime = Time.time;

            if (Random.value < wanderProbability)
            {
                var randomAngle = Random.Range(-maxWanderAngle, maxWanderAngle);
                var relativeWanderRotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
                goalLookRotation = transform.rotation * relativeWanderRotation;
                goalLookRotation.z = 0;
                goalLookRotation.x = 0;
            }
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime / 2f);
    }
    void Attack(Animal animal)
    {
        animal.hp -= power;
    }

    public virtual void Die()
    {
        isDie = true;
        gameObject.SetActive(false);
    }

    public void UpdatePosition()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(transform.position + transform.forward * (float)moveSpeed * Time.fixedDeltaTime);
            //rb.MovePosition(transform.position + moveDirection * (float)moveSpeed * Time.fixedDeltaTime);
        }
    }

    void AvoidPredator()
    {
        
        IDinosaur closestPredator = FindClosetPredator();

        if (closestPredator != null)
        {
            //Debug.Log($"{gameObject.name}이 도망가는 중");
            //가장 가까운 포식자로부터 도망
            predatorDetected = true;
            Vector3 preditorDirection = (transform.position - ((MonoBehaviour)closestPredator).transform.position);
            goalLookRotation = Quaternion.LookRotation(preditorDirection);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * maxTurnRateY);
            // Y축 회전만 적용
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);

            // 가까울수록 빠르게 회피 (거리 비율 기반)
            float speedFactor = 1 - (Mathf.Sqrt((transform.position-((MonoBehaviour)closestPredator).transform.position).sqrMagnitude) / playerSensingDistance);
            moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax, speedFactor);
        }
        else { predatorDetected = false; }
    }

    IDinosaur FindClosetPredator()
    {
        int colliderCount;
            if (this is Raptor) colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, colliders, findLayerMask[0]);
            else colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, colliders, findLayerMask[0]| findLayerMask[1]);
            IDinosaur closestPredator = null;
        float closestSqrDistance = Mathf.Infinity;
            for (int i = 0; i < colliderCount; i++)
            {
                IDinosaur dinosaur = colliders[i].GetComponent<IDinosaur>();
                if (dinosaur != null && (Object)dinosaur != this && colliders[i].GetComponent<Animal>().size > size)
                {
                    float sqrDistance = (transform.position - colliders[i].transform.position).sqrMagnitude;
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closestPredator = dinosaur;
                    }
                }
            }
        
        return closestPredator;
    }

    public virtual void Move()
    {
        if (rb != null)
        {
            RaycastHit hit;
            float raycastDistance = 0.6f; // Raycast 거리 증가

            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, raycastDistance))
            {
                Vector3 surfaceNormal = hit.normal;
                float slopeAngle = Vector3.Angle(Vector3.up, surfaceNormal);

                if (slopeAngle > maxSlopeAngle)
                {
                    // 경사가 너무 심하면 수평 속도를 0으로 만들고, 아래로 약간의 힘을 가하여 미끄러지도록 유도
                    rb.velocity = new Vector3(0, rb.velocity.y - 0.5f, 0); // y축 속도에 약간의 음수 값을 더함
                    return;
                }

                Vector3 horizontalDirection = Vector3.ProjectOnPlane(transform.forward, surfaceNormal).normalized;
                Vector3 velocity = horizontalDirection * moveSpeed;
                rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            else
            {
                Vector3 horizontalDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                Vector3 velocity = horizontalDirection * moveSpeed;
                rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
            }
        }
    }


    private void CheckSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f, findLayerMask[4]))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > maxSlopeAngle)
            {
                isOnSlope = false; // 너무 가파르면 경사 아님
            }
            else
            {
                isOnSlope = true;
                slopeNormal = hit.normal;
            }
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            isOnSlope = false;
        }
    }

    protected float CalculateSpeed(int infoIdx)
    {
            return speeds[infoIdx];
        
        //크기에 따른 속도 계산 로직
        //return Mathf.Clamp(speed, moveSpeedMin, moveSpeedMax);
    }

    public virtual void Interact(IDinosaur other)
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        IDinosaur dinosaur = collision.transform.GetComponent<IDinosaur>();
        if (dinosaur != null)
        {
            Interact(dinosaur);
        }
    }
    public void Display()
    {
        //정보 표시
    }

}
