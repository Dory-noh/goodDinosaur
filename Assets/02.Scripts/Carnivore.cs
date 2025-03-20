using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    private int eatCooltime; //�Ļ� ��Ÿ��
    private bool eating;
    private bool victimDetected;
    [SerializeField] private MonoBehaviour closestVictimObject;
    IDinosaur closestVictim;
    Vector3 victimDirection;
    private Collider[] victimColliders = new Collider[10];

    void Start()
    {
        
        eating = false;
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        //���� ����(���� ���� �켱 ����)
        
        TraceVictim(); // ������ ȸ�Ǹ� �켱���� �� ��, ���̸� �����ϵ��� ��.
        ChaseVictim();
        closestVictimObject = (MonoBehaviour)closestVictim;
        Move();
    }
    public override void OnEnable()
    {
        sizes = new int[3] { 10, 50, 100 };
        size = sizes[Random.Range(0, sizes.Length)];
        base.OnEnable();
        eatCooltime = 0;
    }


    public bool canEat(IDinosaur other)
    {
        if (other is Raptor && this is Raptor) return false;
        else if(other is Carnivore carnivore)
        {
            return size >= ((MonoBehaviour)other).gameObject.GetComponent<Animal>().size; 
                //&& this != carnivore;
        }
        else if(other is Herbivore herbivore)
        {
            //return size >= herbivore.size;
            return true;
        }
        return false;
    }

    public bool canTrace(IDinosaur other)
    {
        if (other is Raptor && this is Raptor)
        {
            //������ �ִ� ���Ͱ� ���� �����̰ų� �ȷο��� ���� false
            if (gameObject.GetComponent<Raptor>().leader == ((MonoBehaviour)other).GetComponent<Raptor>() 
                || gameObject.GetComponent<Raptor>().leader != null &&gameObject.GetComponent<Raptor>().leader.followers.Contains(((MonoBehaviour)other).GetComponent<Raptor>())) return false;
            return true;
        }
        else if (other is Carnivore carnivore)
        {
            return size >= ((MonoBehaviour)other).gameObject.GetComponent<Animal>().size;
            //&& this != carnivore;
        }
        else if (other is Herbivore herbivore)
        {
            //return size >= herbivore.size;
            return true;
        }
        return false;
    }

    void ChaseVictim()
    {
        if (victimDetected)
        {
            if (closestVictim != null) 
            {
                if (victimDirection != Vector3.zero)
                {
                    goalLookRotation = Quaternion.LookRotation(victimDirection);
                    moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax , Time.deltaTime);
                }
            }
        }
        else
        {
            // ���̸� ���� ���� ���� �⺻ �ӵ� ����
            moveSpeed = CalculateSpeed(size);
        }
    }
    void TraceVictim()
    {
        closestVictim = FindClosetVictim();
        if (closestVictim != null)
        {
            //���� ����� ������ ����
            victimDetected = true;
            Vector3 victimDirection = ((MonoBehaviour)closestVictim).transform.position - transform.position;
            goalLookRotation = Quaternion.LookRotation(victimDirection);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * maxTurnRateY);
            // Y�� ȸ���� ����
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);
        }
        else
        {
            victimDetected = false;
        }
    }
    //IDinosaur FindClosetVictim()
    //{
    //    closestVictim = null;
    //    float closeatSqrDistance = Mathf.Infinity;
        
    //    foreach(var dinosaur in dinosaurs)
    //    {
    //        if(dinosaur != null && (object)dinosaur != this && canEat(dinosaur))
    //        {
    //            float sqrDistance = (transform.position - ((MonoBehaviour)dinosaur).transform.position).sqrMagnitude;
    //            if(sqrDistance < closeatSqrDistance)
    //            {
    //                closeatSqrDistance = sqrDistance;
    //                closestVictim = dinosaur;
    //            }
    //        }
    //    }
    //    return closestVictim;
    //}

    IDinosaur FindClosetVictim()
    {
        if (this is Raptor && gameObject.GetComponent<Raptor>().leader != null)
        {
            if (gameObject.GetComponent<Raptor>().leader.followers.Contains(((MonoBehaviour)this).GetComponent<Raptor>())) { return null; }
        }
        closestVictim = null;
        float closestSqrDistance = Mathf.Infinity;

        // �ݶ��̴� �迭 ����
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, victimColliders, findLayerMask[0] | findLayerMask[1] | findLayerMask[2]);

        for (int i = 0; i < colliderCount; i++)
        {
            IDinosaur dinosaur = victimColliders[i].GetComponent<IDinosaur>();

            // �ڱ� �ڽŰ� �浹���� ������, ���� �� �ִ� ����� ã�´�.
            if (dinosaur != null && (Object)dinosaur != this && canTrace(dinosaur))
            {
                float sqrDistance = (transform.position - victimColliders[i].transform.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestVictim = dinosaur;
                }
            }
        }

        return closestVictim;
    }

    public void Hunt(IDinosaur other)
    {
        if (eating == true) return;
        
        if (canEat(other))
        {
            eating = true;
            Invoke("resetEating", eatCooltime);
            ((MonoBehaviour)other).GetComponent<Animal>().Die();
            Debug.Log($"{((MonoBehaviour)other).gameObject.name}�� {gameObject.name}�� ��Ƹ���");
            size += ((MonoBehaviour)other).gameObject.GetComponent<Animal>().size; //���� ������ ���� ũ�⸸ŭ ������ ����
            moveSpeed = CalculateSpeed(size); //�ӵ� ����
            
        }
        else
        {
            //Debug.Log("��� ����");
        }
    }

    private void resetEating()
    {
        Debug.Log("�ٽ� ���� �� �ֽ��ϴ�.");
        eating = false;
    }

    public override void Move()
    {
        base.Move();
    }

    public override void Interact(IDinosaur other)
    {
        if (other is IHerbivore herbivore)
        {
            Hunt(other);
        }
        if (other is Carnivore carnivore)
        {
            Hunt(other);
        }
    }

    public override void Die()
    {
        base.Die();
        if (isDie) StartCoroutine(PoolingManager.Instance.waitSpawnDino(1));
    }

    private void OnDisable()
    {
        
    }
}
