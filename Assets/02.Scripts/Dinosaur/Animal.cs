using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum AnimalState
{
    Idle,         // 대기 상태
    Move,         // 이동 상태
    Eat,
    Attack,       // 공격 상태
    Die           // 죽음 상태
}

public class Animal : MonoBehaviour, IMovable, IDinosaur
{
    public int[] sizes = new int[3] { 10, 50, 100 };
    public float[] speeds = new float[3] { 8, 5, 3 };
    public int[] HPs = new int[3] { 5, 10, 20 };
    public float[] powers = new float[3] { 1, 2, 5 };
    public int infoIdx = 0;
    public int size;
    public float moveSpeed;
    public float MaxHp;
    public float hp;
    public float power;
    public bool isDie;
    public Animal Attacker;


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

    //[SerializeField] private bool isBumped = false;

    // 장애물 회피 시 기준점으로 사용됨.
    //public Transform tankCenterGoal;
    public Transform[] DetectPoint;
    // 장애물 감지 거리 (단위: 미터).
    // 이 거리 안에 장애물이 있으면 회피 행동을 시작함.
    public float obstacleSensingDistance = 5f;

    // 장애물을 감지했는지 여부
    protected bool obstacleDetected = false;
    // 디버그용 변수 (광선과 목표 지점을 그리기 위해 사용)
    private Vector3 hitPoint;
    private Vector3 goalPoint;

    public float playerSensingDistance = 30f;
    protected List<IDinosaur> dinosaurs = new List<IDinosaur>(); // 오브젝트 캐싱
    private Collider[] colliders = new Collider[10]; // 콜라이더 배열

    //포식자 발견했는지 여부
    protected bool predatorDetected = false;
    Rigidbody rb;
    CharacterController characterController; // CharacterController 변수 추가
    //private float gravity = -9.81f; // 중력
    private Vector3 verticalVelocity; // 수직 속도

    //레이어 마스크 리스트
    protected List<int> findLayerMask = new List<int>();

    float unclimbableSlopeAngle;
    public float maxSlopeAngle = 30f; // 오를 수 있는 최대 경사 각도
    public float slideThresholdAngle = 30f; // 미끄러지기 시작하는 경사 각도

    private bool attemptingSecondaryAvoidance = false;
    private Quaternion secondaryGoalLookRotation;
    private bool initialAvoidanceTried = false;

    private bool isGrounded;
    protected bool isAttack;
    //private bool isOnSlope;
    private Vector3 slopeNormal;

    //애니메이션
    protected Animator animator;
    protected readonly int hashMove = Animator.StringToHash("Move");
    protected readonly int hashDie = Animator.StringToHash("Die");
    protected readonly int hashAttack = Animator.StringToHash("Attack");
    protected readonly int hashEat = Animator.StringToHash("Eat");

    //애니메이션 속도를 공룡마다 조금씩 다르게 한다.
    public float minSpeedMultiplier = 0.9f;
    public float maxSpeedMultiplier = 1.1f; // 재생 속도 배율 범위

    //공룡 울음소리 출력 이벤트
    public UnityEvent AttackSFX;
    public UnityEvent dinoDieSFX;

    private AnimalState currentState;

    public AnimalState CurrentState
    {
        get { return currentState; }
        set
        {
            if (value == AnimalState.Eat && isEating == false && isDie == false)
            {
                StartCoroutine(EatingDelay());
            }
            currentState = value;
            ChangeStateAni();
        }
    }

    public AnimalState StateForCheck;
    private bool isEating = false; // 먹는 상태 여부를 추적하는 변수

    //hp바
    public Image hpImg;

    // 상호작용 거리
    public float interactionDistance = 8f;

    // HP 자동 회복 관련 변수
    protected float lastDamageTime = 0f;
    protected float regenerationInterval = 60f; // 1분 (60초)
    public virtual void Awake()
    {
        //초기화 시에 오브젝트 검색 및 캐싱
        dinosaurs.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<IDinosaur>());
        //tankCenterGoal = GameObject.Find("center").transform;
        rb = GetComponent<Rigidbody>();

        //랩터인 경우 랩터는 추적 제외
        findLayerMask.Add(LayerMask.GetMask("Carnivore"));
        //포식자 레이어 마스크 설정
        findLayerMask.Add(LayerMask.GetMask("Raptor"));
        findLayerMask.Add(LayerMask.GetMask("Herbivore"));
        findLayerMask.Add(LayerMask.GetMask("Obstacle"));
        findLayerMask.Add(LayerMask.GetMask("Ground"));

        playerSensingDistance = 30f;
        //Debug.Log($"인덱스 번호 : {infoIdx}");
        animator = GetComponentInChildren<Animator>();
        if(hpImg == null) hpImg = GetComponentsInChildren<Image>()[1];
        if (gameObject.CompareTag("Player") == false)
            DetectPoint = new Transform[2] { transform.GetChild(2), transform.GetChild(3) };
    }

    public virtual void OnEnable()
    {
        moveSpeed = CalculateSpeed(infoIdx);
        MaxHp = HPs[infoIdx];
        if (this is PlayerControl player) MaxHp *= 1.5f;
        hp = MaxHp;
        power = powers[infoIdx];
        randomOffset = Random.value;
        isDie = false;
        verticalVelocity.y = 0f; // 활성화 시 수직 속도 초기화
        hpImg.fillAmount = hp / MaxHp;
        isAttack = false;
        CurrentState = AnimalState.Idle;
        TogglePhysicsComponents(true);
        lastDamageTime = Time.time; // 활성화 시 초기화
        // 랜덤한 재생 속도 배율 계산
        float randomSpeed = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
        // 애니메이션 재생 속도 설정
        if(this is not PlayerControl) animator.speed = randomSpeed;
    }
    public virtual void FixedUpdate() //육식 공룡은 해당 메서드를 오버라이드해서 사용하기 때문에 해당 메서드에 접근하지 않음.
    {
        
        if (GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false) return;
        if (isDie == true || isAttack || isEating) return;

        // HP 자동 회복 로직
        if (Time.time - lastDamageTime >= regenerationInterval && hp < MaxHp)
        {
            //Debug.Log($"{gameObject.name} Hp회복합니다.");
            RecoverHP();
        }

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

    public void ChangeStateAni()
    {
        if (gameObject.CompareTag("Player")) return;

        try
        {
            // 상태에 맞는 애니메이션 실행
            switch (currentState)
            {
                case AnimalState.Idle:
                    animator.SetBool(hashMove, false);
                    break;
                case AnimalState.Move:
                    animator.SetBool(hashMove, true);
                    break;
                case AnimalState.Attack:
                        animator.SetTrigger(hashAttack); // Attack 애니메이션 트리거
                        animator.SetBool(hashMove, false);
                    //AttackSFX?.Invoke();
                    //Debug.Log("공격 애니메이션 재생");
                    break;
                case AnimalState.Die:
                    animator.SetBool(hashMove, false);
                    animator.SetTrigger(hashDie); // Die 애니메이션 트리거
                    //dinoDieSFX?.Invoke();
                    break;
                
            }
        }
        catch
        {
            //Debug.Log($"{gameObject.name} 애니메이션 에러 발생");
        }


    }

    /// 장애물 감지 및 회피 로직.
    protected void AvoidObstacles()
    {
        RaycastHit headHit;
        bool headObstacleDetected = Physics.Raycast(DetectPoint[0].position, moveDirection, out headHit, obstacleSensingDistance, findLayerMask[3] | findLayerMask[4]);

        RaycastHit feetHit;
        bool feetObstacleDetected = Physics.Raycast(DetectPoint[1].position, moveDirection, out feetHit, obstacleSensingDistance, findLayerMask[3] | findLayerMask[4]);

        obstacleDetected = headObstacleDetected || feetObstacleDetected;

        RaycastHit hit = new RaycastHit(); // hit 변수 초기화

        if (headObstacleDetected)
        {
            hit = headHit;
        }
        else if (feetObstacleDetected)
        {
            hit = feetHit;
        }
        else
        {
            attemptingSecondaryAvoidance = false;
            initialAvoidanceTried = false;
            // return; // 직접적인 장애물이 없으면 바로 리턴하지 않음
        }

        // obstacleDetected가 true인 경우
        if (obstacleDetected)
        {
            //Debug.Log($"{gameObject.name} 장애물 감지함");
            hitPoint = hit.point;
            Vector3 reflectionVector = Vector3.Reflect(moveDirection, hit.normal);
            float goalPointMinDistanceFromHit = 1f;
            Vector3 reflectedPoint = hit.point + reflectionVector * Mathf.Max(hit.distance*1.5f, goalPointMinDistanceFromHit*2f);
            Debug.DrawRay(transform.position, moveDirection * obstacleSensingDistance, Color.red);
            goalPoint = reflectedPoint; //+ tankCenterGoal.position * 0.3f
            Vector3 goalDirection = goalPoint - transform.position;
            goalDirection.y = 0;
            goalLookRotation = Quaternion.LookRotation(goalDirection.normalized);

            float dangerLevel = Mathf.Clamp01(1 - (hit.distance / obstacleSensingDistance));
            dangerLevel = Mathf.Max(0.01f, dangerLevel);

            float turnRate = maxTurnRateY * dangerLevel * 3;
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * turnRate);
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);
            initialAvoidanceTried = true;
            attemptingSecondaryAvoidance = false;
            return;
        }

        if (obstacleDetected && !attemptingSecondaryAvoidance && initialAvoidanceTried && Vector3.Distance(transform.position, goalPoint) < 2f)
        {
            attemptingSecondaryAvoidance = true;
        }
        else if (!obstacleDetected && attemptingSecondaryAvoidance)
        {
            attemptingSecondaryAvoidance = false;
            initialAvoidanceTried = false;
        }

        // 가파른 경사면 감지 로직 (수정)
        RaycastHit hitGroundForward;
        float groundCheckDistance = 2f;
        Vector3 groundCheckOrigin = transform.position + Vector3.up * 0.1f + moveDirection * 1f;
        if (Physics.Raycast(groundCheckOrigin, Vector3.down, out hitGroundForward, groundCheckDistance, findLayerMask[4]))
        {
            Vector3 groundNormalForward = hitGroundForward.normal;
            float slopeAngleForward = Vector3.Angle(Vector3.up, groundNormalForward);

            // 경사각이 오를 수 없는 각도보다 크면 장애물로 처리
            if (slopeAngleForward > maxSlopeAngle) // unclimbableSlopeAngle 대신 maxSlopeAngle 사용
            {
                obstacleDetected = true;
                if (!attemptingSecondaryAvoidance)
                {
                    Vector3 oppositeDirection = -moveDirection;
                    goalLookRotation = Quaternion.LookRotation(oppositeDirection);
                    Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * maxTurnRateY * 3f); // 회전 속도 증가
                    Vector3 eulerAngles = rotation.eulerAngles;
                    eulerAngles.x = 0;
                    eulerAngles.z = 0;
                    transform.rotation = Quaternion.Euler(eulerAngles);
                    attemptingSecondaryAvoidance = true;
                    initialAvoidanceTried = true;
                }
                return;
            }
            // 전방에 장애물이 감지되었고, 경사각이 maxSlopeAngle보다 약간 작은 경우에도 회피 시도
            else if (obstacleDetected && slopeAngleForward > maxSlopeAngle - 5f) // 추가 조건
            {
                if (!attemptingSecondaryAvoidance)
                {
                    Vector3 oppositeDirection = -moveDirection;
                    goalLookRotation = Quaternion.LookRotation(oppositeDirection);
                    Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * maxTurnRateY * 2f); // 회전 속도 증가
                    Vector3 eulerAngles = rotation.eulerAngles;
                    eulerAngles.x = 0;
                    eulerAngles.z = 0;
                    transform.rotation = Quaternion.Euler(eulerAngles);
                    attemptingSecondaryAvoidance = true;
                    initialAvoidanceTried = true;
                }
                return;
            }
            else
            {
                if (attemptingSecondaryAvoidance)
                {
                    attemptingSecondaryAvoidance = false;
                    initialAvoidanceTried = false;
                }
            }
        }
        else
        {
            if (attemptingSecondaryAvoidance)
            {
                attemptingSecondaryAvoidance = false;
                initialAvoidanceTried = false;
            }
        }

        // 직접적인 장애물이나 가파른 경사가 감지되지 않았을 경우 obstacleDetected 초기화 (수정)
        // 가파른 경사로 인해 obstacleDetected가 설정되었을 수도 있으므로,
        // 전방에 직접적인 장애물이 감지되지 않았을 때 && 가파른 경사도 아닐 때만 초기화합니다.
        if (!headObstacleDetected && !feetObstacleDetected)
        {
            RaycastHit hitGroundCheck;
            Vector3 groundCheckOriginForReset = transform.position + Vector3.up * 0.1f + moveDirection * 1f;
            if (Physics.Raycast(groundCheckOriginForReset, Vector3.down, out hitGroundCheck, groundCheckDistance, findLayerMask[4]))
            {
                Vector3 groundNormalForReset = hitGroundCheck.normal;
                float slopeAngleForReset = Vector3.Angle(Vector3.up, groundNormalForReset);
                if (slopeAngleForReset <= maxSlopeAngle) // 현재 발 밑 경사도 완만하면 초기화
                {
                    obstacleDetected = false;
                }
            }
            else // 땅이 없으면 초기화 (공중에 떠 있는 경우)
            {
                obstacleDetected = false;
            }
        }

        if (!isGrounded && verticalVelocity.y < -5f)
        {
            Debug.Log("추락");
        }
    }

    public void SetAttacker(Animal other)
    {
        Attacker = other;
    }

    public virtual IEnumerator Damage(float power)
    {
        if (Attacker != null && Attacker.infoIdx == 2) yield return new WaitForSeconds(0.6f);
        else yield return new WaitForSeconds(0.3f);
        hp -= power;
        hp = Mathf.Clamp(hp, 0, MaxHp);
        lastDamageTime = Time.time; // 데미지 받은 시간 업데이트
        hpImg.fillAmount = (float)hp / MaxHp;
        if(Attacker != null && this is Carnivore carn)
        {
            carn.closestVictim = Attacker;
            if(this is not PlayerControl)
                carn.Hunt(carn.closestVictim);
        }
        if(this is PlayerControl player)
        {
            player.InitiateAttack(Attacker);
        }
        if (hp <= 0)
        {
            if(this is PlayerControl)
            {
                dinoDieSFX?.Invoke();
                
                GameManager.Instance.GameOver = true;
                sceneManager.Instance.OnPlayCutScene(sceneManager.Instance.DeathScenes[2]);
            }
            Die();

        }
    }

    private IEnumerator EatingDelay()
    {
        yield return null;
        if (isDie != true)
        {
            isEating = true;
            if (this is not PlayerControl && this is Carnivore carn && carn.closestVictim != null)
            {
                //공격 대상을 바라보도록 회전  방향 = 목표 지점 - 기준 지점
                Vector3 directionToTarget = ((Animal)carn.closestVictim).transform.position - transform.position;
                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    Vector3 targetEuler = targetRotation.eulerAngles;
                    targetEuler.x = 0f;
                    targetEuler.z = 0f;
                    Quaternion yRotationOnly = Quaternion.Euler(targetEuler);
                    transform.rotation = Quaternion.Slerp(transform.rotation, yRotationOnly, Time.fixedDeltaTime * maxTurnRateY * 3f);
                }
            }
            if (isDie) currentState = AnimalState.Die;
            animator.SetTrigger(hashEat);
            animator.SetBool(hashMove, false);
            yield return new WaitForSeconds(2f); // 2초 동안 대기
            isEating = false;
            if (currentState == AnimalState.Eat) // 먹는 애니메이션이 끝났다면 상태를 Idle로 변경
            {
                if(isDie == true) {
                    CurrentState = AnimalState.Die;
                    yield break;
                }
                else CurrentState = AnimalState.Idle;
            }
        }
    }

    public void RecoverHP()
    {
        lastDamageTime = Time.time; // 데미지 타임 초기화
        hp++;
        hp = Mathf.Clamp(hp, 0, MaxHp);
        hpImg.fillAmount = (float)hp / MaxHp;
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
    public void Attack(Animal animal)
    {
        AttackSFX?.Invoke();
        if (!isAttack && animal != null)
        {
            isAttack = true;
            if(isDie == false)
                CurrentState = AnimalState.Attack;
            else
            {
                CurrentState = AnimalState.Die;
                return;
            }

            if(this is not PlayerControl)
            {
                //공격 대상을 바라보도록 회전  방향 = 목표 지점 - 기준 지점
                Vector3 directionToTarget = animal.transform.position - transform.position;
                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    Vector3 targetEuler = targetRotation.eulerAngles;
                    targetEuler.x = 0f;
                    targetEuler.z = 0f;
                    Quaternion yRotationOnly = Quaternion.Euler(targetEuler);
                    transform.rotation = Quaternion.Slerp(transform.rotation, yRotationOnly, Time.fixedDeltaTime * maxTurnRateY * 3f);
                }
            }
            if(this is Raptor raptor) raptor.power = raptor.leader == null ? powers[0]*1.3f : raptor.leader.followers.Count + 1 < 3 ? powers[0] : raptor.leader.followers.Count + 1 < 5 ? powers[1] : powers[2];
            //if (this is PlayerControl player) player.power = powers[0]*2;
            //{
            //    Debug.Log($"{gameObject.name}의 공격. 데미지 {power}");
            //    Debug.Log($"공격 전 공격한 공룡 HP값 {animal.hp}");
            //}
            StartCoroutine(animal.Damage(power));

            //if(this is PlayerControl)
                //Debug.Log($"공격 후 공격한 공룡 HP값 {animal.hp}");
            animal.Attacker = this;
            if (this.gameObject.activeSelf)
            {
                StartCoroutine(ResetAttack(1.5f));
            }
        }
    }

    private IEnumerator ResetAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isDie == true)
        {
            CurrentState = AnimalState.Die;
            yield break;
            //animator.SetTrigger(hashDie);
            //animator.SetBool(hashMove, false);
        }
        else CurrentState = AnimalState.Idle;
        isAttack = false;
    }

    public virtual void Die()
    {
        if (!isDie)
        {
            isDie = true;
            CurrentState = AnimalState.Die;

            if (this is not PlayerControl)
            {
                TogglePhysicsComponents(false);
                StartCoroutine(HideDelay(3f));
            }
            // 공격에 성공했을 때 (상대방 체력이 0 이하가 되었을 때) 공격자의 상태를 Eat으로 변경
            if (Attacker is Carnivore && Attacker is not Raptor && Attacker.CurrentState != AnimalState.Die)
            {
                StartCoroutine(SetEatState(Attacker));
            }
            else if (Attacker is Raptor _raptor && Attacker.CurrentState != AnimalState.Die)
            {
                if (_raptor.leader == null)
                {
                    _raptor.raptorLevel++;
                    StartCoroutine(SetEatState(_raptor));
                }
                else
                {
                    _raptor.leader.raptorLevel++;
                    StartCoroutine(SetEatState(_raptor.leader));
                    foreach (var rap in _raptor.leader.followers)
                    {
                        if (rap.currentState != AnimalState.Die)
                            StartCoroutine(SetEatState(rap));
                    }
                    if (_raptor.leader is PlayerControl player)
                    {
                        player.IncreaseHungerLevel();
                        GameManager.Instance.nextHungerTime = Time.time + GameManager.Instance.hungerInterval;
                    }
                }
            }
        }
    }

    IEnumerator SetEatState(Animal target)
    {
        yield return new WaitForSeconds(0.3f);
        target.CurrentState = AnimalState.Eat;
    }

    private void TogglePhysicsComponents(bool isActive)
    {
        CapsuleCollider[] capsuleColliders = gameObject.GetComponents<CapsuleCollider>();
        foreach (var collider in capsuleColliders)
        {
            collider.enabled = isActive;
        }
        GetComponent<Rigidbody>().isKinematic = !isActive;
    }

    private IEnumerator HideDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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

    protected void AvoidPredator()
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
            float speedFactor = 1 - (Mathf.Sqrt((transform.position - ((MonoBehaviour)closestPredator).transform.position).sqrMagnitude) / playerSensingDistance);
            moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax, speedFactor);
        }
        else { predatorDetected = false; }
    }

    IDinosaur FindClosetPredator()
    {
        int colliderCount;
        if (this is Raptor) colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, colliders, findLayerMask[0]);
        else colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, colliders, findLayerMask[0] | findLayerMask[1]);
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
        if (isDie || isAttack) return;
        CurrentState = AnimalState.Move;


        if (rb != null)
        {
            RaycastHit hit;
            float maxVelocity = 20f;
            float raycastDistance = 0.5f;
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, raycastDistance))
            {
                Vector3 surfaceNormal = hit.normal;
                float slopeAngle = Vector3.Angle(Vector3.up, surfaceNormal);
                //Debug.Log($"{gameObject.name}의 현재 경사 : {slopeAngle}");
                if (slopeAngle > maxSlopeAngle)
                {
                    if (surfaceNormal != Vector3.zero)
                    {
                        Vector3 cross1 = Vector3.Cross(surfaceNormal, Vector3.up);
                        if (cross1 != Vector3.zero)
                        {
                            Vector3 slideDirection = Vector3.Cross(surfaceNormal, cross1).normalized;
                            if (slideDirection == Vector3.zero)
                            {
                                rb.velocity = Vector3.zero;
                                return;
                            }
                            slideDirection.Normalize();
                            if (slideThresholdAngle > 0)
                            {
                                float slideForceMultiplier = Mathf.Pow(slopeAngle / slideThresholdAngle, 2f) * 5f;
                                Vector3 rawVelocity = slideDirection * moveSpeed * slideForceMultiplier;
                                // 속도 성분을 특정 범위로 제한
                                
                                rb.velocity = new Vector3(
                                    Mathf.Clamp(rawVelocity.x, -maxVelocity, maxVelocity),
                                    Mathf.Clamp(rawVelocity.y, -maxVelocity, maxVelocity),
                                    Mathf.Clamp(rawVelocity.z, -maxVelocity, maxVelocity)
                                );
                                rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
                                return;
                            }
                            else
                            {
                                //Debug.LogError($"slideThresholdAngle is zero on {gameObject.name}");
                            }
                        }
                        else
                        {
                            //Debug.LogError($"First cross product resulted in zero vector on {gameObject.name}");
                        }
                    }
                    else
                    {
                        //Debug.LogError($"Surface normal is zero on {gameObject.name}");
                    }
                }
                Vector3 projectedVelocity = Vector3.ProjectOnPlane(transform.forward, surfaceNormal);
                if(projectedVelocity == Vector3.zero) rb.velocity = Vector3.zero;
                else
                {
                    Vector3 horizontalDirection = projectedVelocity.normalized;
                    Vector3 velocity = horizontalDirection * moveSpeed;
                    rb.velocity = new Vector3(
                        Mathf.Clamp(velocity.x, -maxVelocity, maxVelocity),
                        0f,
                        Mathf.Clamp(velocity.z, -maxVelocity, maxVelocity)
                    );
                }
            }
            else
            {
                Vector3 projectedVelocity = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                if(projectedVelocity == Vector3.zero)
                {
                    rb.velocity = Vector3.zero;
                }
                else
                {
                    Vector3 horizontalDirection = projectedVelocity.normalized;
                    Vector3 velocity = horizontalDirection * moveSpeed;
                    rb.velocity = new Vector3(
                        Mathf.Clamp(velocity.x, -maxVelocity, maxVelocity),
                        Mathf.Clamp(rb.velocity.y, -maxVelocity, maxVelocity),
                        Mathf.Clamp(velocity.z, -maxVelocity, maxVelocity)
                    );
                }
            }

            // obstacleDetected가 true이면 위로 올라가는 속도 제한
            if (obstacleDetected && rb.velocity.y > 0) //위로 올라가는 경우에만 제한
            {
                rb.velocity = new Vector3(
                    Mathf.Clamp(rb.velocity.x, -maxVelocity, maxVelocity),
                    0f,
                    Mathf.Clamp(rb.velocity.z, -maxVelocity, maxVelocity)
                );
            }
        }
    }


    protected float CalculateSpeed(int infoIdx)
    {
        return speeds[infoIdx];

        //크기에 따른 속도 계산 로직
        //return Mathf.Clamp(speed, moveSpeedMin, moveSpeedMax);
    }
    protected void InteractWithNearbyDinosaurs()
    {
        //if (this is Herbivore) return;
        Collider[] nearbyColliders = new Collider[5];
        if(this is not PlayerControl || (this is PlayerControl player) && player.followers.Count == 0) interactionDistance = infoIdx == 0 ? 7f : infoIdx == 1 ? 15f : 26f;
        else interactionDistance = infoIdx == 0 ? 7f : infoIdx == 1 ? 15f : 35f;
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, interactionDistance, nearbyColliders, findLayerMask[0] | findLayerMask[1] | findLayerMask[2]);

        for (int i = 0; i < colliderCount; i++)
        {
            IDinosaur otherDinosaur = nearbyColliders[i].GetComponent<IDinosaur>();
            if (otherDinosaur != null && (Object)otherDinosaur != this)
            {
                //if (otherDinosaur is PlayerControl) Debug.Log($"{gameObject.name}이 플레이어와 상호작용합니다.");
                Interact(otherDinosaur);
                break;
            }
            else
            {
                //Debug.Log("실패");
            }
        }
    }

    public virtual void Interact(IDinosaur other)
    {
        if (isDie) return;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (isDie) return;
    //    IDinosaur dinosaur = other.transform.GetComponent<IDinosaur>();
    //    if (dinosaur != null)
    //    {
    //        Interact(dinosaur);
    //    }
    //}


    //두 공룡 충돌 시, 인터렉션 실행
    private void OnCollisionEnter(Collision collision)
    {
        //if (this is not Herbivore) return;
        if (isDie) return;
        //Debug.Log($"초식 공룡{gameObject.name}의 공격");
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

    public virtual void OnDisable()
    {
        //CurrentState = AnimalState.Idle;
        //(해당 공룡을 공격하고 있던) 육식 공룡의 타겟에서 해당 공룡을 해제한다.
        if (Attacker != null && Attacker is Carnivore carnDino && carnDino.closestVictim != null && carnDino.closestVictim == (IDinosaur)this)
            carnDino.closestVictim = null;
    }
}