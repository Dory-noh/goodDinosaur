using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Animal : MonoBehaviour, IMovable, IDinosaur
{
    public int[] sizes;
    public int size;

    public float moveSpeed;

    public float moveSpeedMin { get; set; } = 3.0f; //최소 이동 속도
    public float moveSpeedMax { get; set; } = 8.0f; //최대 이동 속도
    public float maxTurnRateY { get; set; } = 5f; //최대 회전 속도
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
    private Collider[] colliders = new Collider[10]; // 콜라이더 배열 재사용

    //포식자 발견했는지 여부
    protected bool predatorDetected = false;
    Rigidbody rb;

    void Awake()
    {
        //초기화 시에 오브젝트 검색 및 캐싱
        dinosaurs.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<IDinosaur>());
        tankCenterGoal = GameObject.Find("center").transform;
        rb = GetComponent<Rigidbody>();
    }

    public virtual void OnEnable()
    {
        moveSpeed = CalculateSpeed(size);
        randomOffset = Random.value;
    }
    public virtual void Update()
    {
        if (isBumped == true) Invoke("ResetBumpCheck", 3f);
        
        //1. 포식자 회피(최우선 순위)
        AvoidPredator();
        if (predatorDetected)
        {
            Move();
            return;
        }

        //2. 장애물 회피(두 번째 우선 순위)
        // 장애물을 피하는 방향으로 회전했을 수 있으므로, 이동 방향을 업데이트한다.
        AvoidObstacles();

        //3. 기본 움직임(방황)
        //Wander();

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
            Debug.Log("장애물 감지");
            hitPoint = hit.point;
            Vector3 reflectionVector = Vector3.Reflect(moveDirection, hit.normal);
            float goalPointMinDistanceFromHit = 1f;
            Vector3 reflectedPoint = hit.point + reflectionVector * Mathf.Max(hit.distance, goalPointMinDistanceFromHit);
            Debug.DrawRay(transform.position, moveDirection * obstacleSensingDistance, Color.red);
            goalPoint = (reflectedPoint + tankCenterGoal.position) / 2f;
            Vector3 goalDirection = transform.position - goalPoint;
            goalDirection.y = 0;
            goalLookRotation = Quaternion.LookRotation(goalDirection.normalized);

            float dangerLevel = Mathf.Pow(1 - (hit.distance / obstacleSensingDistance), 4f);
            dangerLevel = Mathf.Max(0.01f, dangerLevel);

            float turnRate = maxTurnRateY * dangerLevel;
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * turnRate);
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);
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

        transform.rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime / 2f);
    }


    public void UpdatePosition()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(transform.position + moveDirection * (float)moveSpeed * Time.fixedDeltaTime);
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
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * maxTurnRateY);
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
        List<int> findLayerMask = new List<int>();
        //랩터인 경우 랩터는 추적 제외
        findLayerMask.Add(LayerMask.GetMask("Carnivore"));
        //포식자 레이어 마스크 설정
        findLayerMask.Add(LayerMask.GetMask("Raptor"));
        int colliderCount;
            if (this is Raptor) colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, colliders, findLayerMask[0]);
            else colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, colliders, findLayerMask[0]| findLayerMask[1]);
            IDinosaur closestPredator = null;
        float closestSqrDistance = 20f;
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
            Vector3 horizontalVelocity = moveDirection * moveSpeed;
            rb.velocity = (horizontalVelocity*3) + Physics.gravity; 

        }
        //UpdatePosition();
    }


    protected float CalculateSpeed(int size)
    {
        float speed = (float)200.0 / size; //크기가 클수록 스피드가 느리다.
        //크기에 따른 속도 계산 로직
        return Mathf.Clamp(speed, moveSpeedMin, moveSpeedMax);
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
