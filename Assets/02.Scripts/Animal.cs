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
    

    public float moveSpeedMin { get; set; } = 3.0f; //�ּ� �̵� �ӵ�
    public float moveSpeedMax { get; set; } = 8.0f; //�ִ� �̵� �ӵ�
    public float maxTurnRateY { get; set; } = 15f; //�ִ� ȸ�� �ӵ�
    public float maxWanderAngle { get; set; } = 45f; //�ִ� ��Ȳ ����
    public float wanderPeriodDuration { get; set; } = 0.8f; //��Ȳ �ֱ�
    public float wanderProbability { get; set; } = 0.15f; //��Ȳ Ȯ��

    private Vector3 moveDirection //���� �̵� ����
    {
        get { return transform.TransformDirection(Vector3.forward); }
    }

    private float wanderPeriodStartTime; //��Ȳ ���� �ð�
    protected Quaternion goalLookRotation; //��ǥ ȸ�� ����
    private float randomOffset; // ���� ������

    [SerializeField] private bool isBumped = false;

    // ��ֹ� ȸ�� �� ���������� ����.
    public Transform tankCenterGoal;

    // ��ֹ� ���� �Ÿ� (����: ����).  
    // �� �Ÿ� �ȿ� ��ֹ��� ������ ȸ�� �ൿ�� ������.
    public float obstacleSensingDistance = 1.5f;

    // ��ֹ��� �����ߴ��� ����
    private bool obstacleDetected = false;
    // ����׿� ���� (������ ��ǥ ������ �׸��� ���� ���)
    private Vector3 hitPoint;
    private Vector3 goalPoint;

    public float playerSensingDistance = 5f;
    protected List<IDinosaur> dinosaurs = new List<IDinosaur>(); // ������Ʈ ĳ��
    private Collider[] colliders = new Collider[10]; // �ݶ��̴� �迭

    //������ �߰��ߴ��� ����
    protected bool predatorDetected = false;
    Rigidbody rb;

    //���̾� ����ũ ����Ʈ
    protected List<int> findLayerMask = new List<int>(); 
    public virtual void Awake()
    {
        //�ʱ�ȭ �ÿ� ������Ʈ �˻� �� ĳ��
        dinosaurs.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<IDinosaur>());
        tankCenterGoal = GameObject.Find("center").transform;
        rb = GetComponent<Rigidbody>();
        
        //������ ��� ���ʹ� ���� ����
        findLayerMask.Add(LayerMask.GetMask("Carnivore"));
        //������ ���̾� ����ũ ����
        findLayerMask.Add(LayerMask.GetMask("Raptor"));
        findLayerMask.Add(LayerMask.GetMask("Herbivore"));
        findLayerMask.Add(LayerMask.GetMask("Obstacle"));
        playerSensingDistance = 30f;
        if (this is not Raptor) infoIdx = Random.Range(0, sizes.Length);
        else infoIdx = 0;
        //Debug.Log($"�ε��� ��ȣ : {infoIdx}");
    }

    public virtual void OnEnable()
    {
        moveSpeed = CalculateSpeed(infoIdx);
        hp = HPs[infoIdx];
        power = powers[infoIdx];
        randomOffset = Random.value;
        isDie = false;
    }
    public virtual void FixedUpdate()
    {
        //if (isBumped == true) Invoke("ResetBumpCheck", 3f);
        AvoidObstacles();
        if (obstacleDetected)
        {
            Move();

            return;
        }
        //1. ������ ȸ��(�ֿ켱 ����)
        AvoidPredator();
        if (predatorDetected)
        {
            Move();
            return;
        }

        //2. ��ֹ� ȸ��(�� ��° �켱 ����)
        // ��ֹ��� ���ϴ� �������� ȸ������ �� �����Ƿ�, �̵� ������ ������Ʈ�Ѵ�.
        

        //3. �⺻ ������(��Ȳ)
        Wander();
        Move();

    }
    void ResetBumpCheck()
    {
        isBumped = false;
    }

    /// ��ֹ� ���� �� ȸ�� ����.
    /// </summary>
    void AvoidObstacles()
    {
        RaycastHit hit;
        obstacleDetected = Physics.Raycast(transform.position, moveDirection, out hit, obstacleSensingDistance);
        if (obstacleDetected)
        {
            //Debug.Log("��ֹ� ����");
            hitPoint = hit.point;
            Vector3 reflectionVector = Vector3.Reflect(moveDirection, hit.normal);
            float goalPointMinDistanceFromHit = 1f;
            Vector3 reflectedPoint = hit.point + reflectionVector * Mathf.Max(hit.distance, goalPointMinDistanceFromHit);
            Debug.DrawRay(transform.position, moveDirection * obstacleSensingDistance, Color.red);
            goalPoint = (reflectedPoint + tankCenterGoal.position) / 2f;
            Vector3 goalDirection = goalPoint - transform.position;
            goalDirection.y = 0;
            goalLookRotation = Quaternion.LookRotation(goalDirection.normalized);

            float dangerLevel = Mathf.Pow(1 - (hit.distance / obstacleSensingDistance), 4f);
            dangerLevel = Mathf.Max(0.01f, dangerLevel);

            float turnRate = maxTurnRateY * dangerLevel;
            //Debug.Log($"Turn Rate: {turnRate}");
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * turnRate);
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
            //Debug.Log($"{gameObject.name}�� �������� ��");
            //���� ����� �����ڷκ��� ����
            predatorDetected = true;
            Vector3 preditorDirection = (transform.position - ((MonoBehaviour)closestPredator).transform.position);
            goalLookRotation = Quaternion.LookRotation(preditorDirection);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime * maxTurnRateY);
            // Y�� ȸ���� ����
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);

            // �������� ������ ȸ�� (�Ÿ� ���� ���)
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
            // ���� ����� ����ؼ� ������ ���� �̵��� �� �ֵ��� �Ѵ�.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                Vector3 surfaceNormal = hit.normal; // ������ ��� ����

                // �̵� ������ ���� ����� �ݿ��Ͽ� ���
                Vector3 horizontalDirection = Vector3.ProjectOnPlane(transform.forward, surfaceNormal).normalized;
                Vector3 velocity = horizontalDirection * moveSpeed;

                // Rigidbody�� ����� �̵�. y �� �ӵ��� ���� ���� �����ϰ�, x, z �� �ӵ��� ����
                rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z); // y �� �ӵ��� �״�� ����
            }
        }

    }


    protected float CalculateSpeed(int infoIdx)
    {
            return speeds[infoIdx];
        
        //ũ�⿡ ���� �ӵ� ��� ����
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
        //���� ǥ��
    }

}
