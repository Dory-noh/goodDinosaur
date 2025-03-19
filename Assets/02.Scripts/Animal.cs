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

    public float moveSpeedMin { get; set; } = 3.0f; //�ּ� �̵� �ӵ�
    public float moveSpeedMax { get; set; } = 8.0f; //�ִ� �̵� �ӵ�
    public float maxTurnRateY { get; set; } = 5f; //�ִ� ȸ�� �ӵ�
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
    private Collider[] colliders = new Collider[10]; // �ݶ��̴� �迭 ����

    //������ �߰��ߴ��� ����
    protected bool predatorDetected = false;
    Rigidbody rb;

    void Awake()
    {
        //�ʱ�ȭ �ÿ� ������Ʈ �˻� �� ĳ��
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
        
        //1. ������ ȸ��(�ֿ켱 ����)
        AvoidPredator();
        if (predatorDetected)
        {
            Move();
            return;
        }

        //2. ��ֹ� ȸ��(�� ��° �켱 ����)
        // ��ֹ��� ���ϴ� �������� ȸ������ �� �����Ƿ�, �̵� ������ ������Ʈ�Ѵ�.
        AvoidObstacles();

        //3. �⺻ ������(��Ȳ)
        //Wander();

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
            Debug.Log("��ֹ� ����");
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
            //Debug.Log($"{gameObject.name}�� �������� ��");
            //���� ����� �����ڷκ��� ����
            predatorDetected = true;
            Vector3 preditorDirection = (transform.position - ((MonoBehaviour)closestPredator).transform.position);
            goalLookRotation = Quaternion.LookRotation(preditorDirection);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * maxTurnRateY);
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
        List<int> findLayerMask = new List<int>();
        //������ ��� ���ʹ� ���� ����
        findLayerMask.Add(LayerMask.GetMask("Carnivore"));
        //������ ���̾� ����ũ ����
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
        float speed = (float)200.0 / size; //ũ�Ⱑ Ŭ���� ���ǵ尡 ������.
        //ũ�⿡ ���� �ӵ� ��� ����
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
        //���� ǥ��
    }
}
