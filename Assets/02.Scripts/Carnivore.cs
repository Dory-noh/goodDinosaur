using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    private int eatCooltime; //�Ļ� ��Ÿ��

    void Start()
    {
        sizes = new int[3] { 10, 50, 100 };
    }
    
    public override void Update()
    {
        base.Update();
        TraceVictim(); // ������ ȸ�Ǹ� �켱���� �� ��, ���̸� �����ϵ��� ��.
    }
    public override void OnEnable()
    {
        size = sizes[Random.Range(0, sizes.Length)];
        base.OnEnable();
        eatCooltime = 0;
    }


    public bool canEat(IDinosaur other)
    {
        if(other is Carnivore carnivore)
        {
            return size >= carnivore.size && this != carnivore;
        }
        else if(other is Herbivore herbivore)
        {
            return size >= herbivore.size;
           // return true; //�ʽ� ���� ��ɵ� ������ ������ ���� �� ������
        }
        return false;
    }

    void TraceVictim()
    {
        IDinosaur closestVictim = FindClosetVictim();

        if (closestVictim != null)
        {
            Debug.Log($"{gameObject.name}�� {closestVictim.ToString()}�� �Ѵ� ��");
            //���� ����� �����ڷκ��� ����
            predatorDetected = true;
            Vector3 victimDirection = (transform.position - ((MonoBehaviour)closestVictim).transform.position);
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
            predatorDetected = false;
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
        if (canEat(other))
        {
            eatCooltime = 3;
            size += other.size / 2; //���� ������ ���� ũ�⸸ŭ ������ ����
            moveSpeed = CalculateSpeed(size); //�ӵ� ����
            Debug.Log("��� ����");
            ((MonoBehaviour)other).gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("��� ����");
        }
    }

    public override void Move()
    {
        base.Move();
    }

    public override void Interact(IDinosaur other)
    {
        if(other is IHerbivore herbivore)
        {
            Debug.Log($"{gameObject.name}�� ����");
            Hunt(other);
        }
    }

}
