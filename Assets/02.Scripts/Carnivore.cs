using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    private int eatCooltime; //�Ļ� ��Ÿ��
    private bool eating;
    private bool victimDetected;
    void Start()
    {
        sizes = new int[3] { 10, 50, 100 };
        eating = false;
    }
    
    public override void Update()
    {
        base.Update();
        //���� ����(���� ���� �켱 ����)
        
        TraceVictim(); // ������ ȸ�Ǹ� �켱���� �� ��, ���̸� �����ϵ��� ��.
        ChaseVictim();
    }
    public override void OnEnable()
    {
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
    void ChaseVictim()
    {
        if (victimDetected)
        {
            IDinosaur closestVictim = FindClosetVictim();
            if (closestVictim != null) 
            {
                Vector3 victimDirection = ((MonoBehaviour)closestVictim).transform.position - transform.position;
                if (victimDirection != Vector3.zero)
                {
                    goalLookRotation = Quaternion.LookRotation(victimDirection);

                    moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax , Time.deltaTime * 2f);
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
        IDinosaur closestVictim = FindClosetVictim();

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
    IDinosaur FindClosetVictim()
    {
        IDinosaur closestVictim = null;
        float closeatSqrDistance = Mathf.Infinity;
        
        foreach(var dinosaur in dinosaurs)
        {
            if(dinosaur != null && (object)dinosaur != this && canEat(dinosaur))
            {
                float sqrDistance = (transform.position - ((MonoBehaviour)dinosaur).transform.position).sqrMagnitude;
                if(sqrDistance < closeatSqrDistance)
                {
                    closeatSqrDistance = sqrDistance;
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
            ((MonoBehaviour)other).gameObject.SetActive(false);
            Debug.Log($"{((MonoBehaviour)other).gameObject.name}�� {gameObject.name}�� ��Ƹ���");
            size += ((MonoBehaviour)other).gameObject.GetComponent<Animal>().size / 2; //���� ������ ���� ũ�⸸ŭ ������ ����
            moveSpeed = CalculateSpeed(size); //�ӵ� ����
            
        }
        else
        {
            Debug.Log("��� ����");
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
        if(other is IHerbivore herbivore)
        {
            Hunt(other);
        }
        if (other is Carnivore carnivore)
        {
            Hunt(other);
        }
    }

}
