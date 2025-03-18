using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    private int eatCooltime; //식사 쿨타임

    void Start()
    {
        sizes = new int[3] { 10, 50, 100 };
    }
    
    public override void Update()
    {
        base.Update();
        TraceVictim(); // 포식자 회피를 우선으로 한 후, 먹이를 추적하도록 함.
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
           // return true; //초식 공룡 사냥도 사이즈 영향이 있을 것 같은데
        }
        return false;
    }

    void TraceVictim()
    {
        IDinosaur closestVictim = FindClosetVictim();

        if (closestVictim != null)
        {
            Debug.Log($"{gameObject.name}이 {closestVictim.ToString()}을 쫓는 중");
            //가장 가까운 포식자로부터 도망
            predatorDetected = true;
            Vector3 victimDirection = (transform.position - ((MonoBehaviour)closestVictim).transform.position);
            goalLookRotation = Quaternion.LookRotation(victimDirection);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * maxTurnRateY);
            // Y축 회전만 적용
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
            size += other.size / 2; //먹은 공룡의 절반 크기만큼 사이즈 증가
            moveSpeed = CalculateSpeed(size); //속도 재계산
            Debug.Log("사냥 성공");
            ((MonoBehaviour)other).gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("사냥 실패");
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
            Debug.Log($"{gameObject.name}이 먹음");
            Hunt(other);
        }
    }

}
