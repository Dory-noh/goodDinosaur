using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    private int eatCooltime; //식사 쿨타임
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
        //먹이 추적(가장 낮은 우선 순위)
        
        TraceVictim(); // 포식자 회피를 우선으로 한 후, 먹이를 추적하도록 함.
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
            // 먹이를 쫓지 않을 때는 기본 속도 유지
            moveSpeed = CalculateSpeed(size);
        }
    }
    void TraceVictim()
    {
        IDinosaur closestVictim = FindClosetVictim();

        if (closestVictim != null)
        {
            //가장 가까운 포식자 추적
            victimDetected = true;
            Vector3 victimDirection = ((MonoBehaviour)closestVictim).transform.position - transform.position;
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
            Debug.Log($"{((MonoBehaviour)other).gameObject.name}을 {gameObject.name}이 잡아먹음");
            size += ((MonoBehaviour)other).gameObject.GetComponent<Animal>().size / 2; //먹은 공룡의 절반 크기만큼 사이즈 증가
            moveSpeed = CalculateSpeed(size); //속도 재계산
            
        }
        else
        {
            Debug.Log("사냥 실패");
        }
    }

    private void resetEating()
    {
        Debug.Log("다시 먹을 수 있습니다.");
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
