using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    private int eatCooltime; //식사 쿨타임
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
        //먹이 추적(가장 낮은 우선 순위)
        
        TraceVictim(); // 포식자 회피를 우선으로 한 후, 먹이를 추적하도록 함.
        ChaseVictim();
        closestVictimObject = (MonoBehaviour)closestVictim;
        Move();
    }
    public override void OnEnable()
    {
        sizes = new int[3] { 10, 50, 100 };
        size = sizes[infoIdx];
        base.OnEnable();
        eatCooltime = 0;
    }


    public bool canEat(IDinosaur other)
    {
        if (other is Raptor && this is Raptor) return false;
        else if(other is Carnivore carnivore)
        {
            if(this is Raptor)
            {
                int count;
                if(this.GetComponent<Raptor>().leader != null) count = this.GetComponent<Raptor>().leader.followers.Count+1; //리더 포함 팀원 수 세기
                else
                {
                    count = 1;
                }
                if (count >= 5) return true;
                else if (count >= 3)
                {
                    if (((MonoBehaviour)other).GetComponent<Animal>().infoIdx <= 1) return true;
                    return false;
                }
                else
                {
                    if (((MonoBehaviour)other).GetComponent<Animal>().infoIdx == 0) return true;
                    else return false;
                }
            }  
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
            //가까이 있는 랩터가 나의 리더이거나 팔로워면 리턴 false
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
            // 먹이를 쫓지 않을 때는 기본 속도 유지
            moveSpeed = CalculateSpeed(infoIdx);
        }
    }
    void TraceVictim()
    {
        closestVictim = FindClosetVictim();
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

        // 콜라이더 배열 재사용
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, victimColliders, findLayerMask[0] | findLayerMask[1] | findLayerMask[2]);

        for (int i = 0; i < colliderCount; i++)
        {
            IDinosaur dinosaur = victimColliders[i].GetComponent<IDinosaur>();

            // 자기 자신과 충돌하지 않으며, 먹을 수 있는 대상을 찾는다.
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
            Debug.Log($"{((MonoBehaviour)other).gameObject.name}을 {gameObject.name}이 잡아먹음");
            if(this is Raptor) gameObject.GetComponent<Raptor>().raptorLevel++; //랩터 레벨 증가
            //사이즈 비교 시 편의성을 위해 랩터를 제외한 나머지 공룡들은 사이즈가 변동되지 않도록 함.
            //moveSpeed = CalculateSpeed(size); //속도 재계산 //사이즈 바뀌지 않으므로 필요없음.
            
        }
        else
        {
            //Debug.Log("사냥 실패");
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
        if (isDie) PoolingManager.Instance.CallSpawn(1);
    }

    private void OnDisable()
    {
        
    }
}
