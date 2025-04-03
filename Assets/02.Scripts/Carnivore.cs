using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Carnivore : Animal, ICarnivore
{
    //private int eatCooltime; //식사 쿨타임
    private bool eating;
    private bool victimDetected;

    [SerializeField] private MonoBehaviour closestVictimObject;
    public IDinosaur closestVictim;
    Vector3 victimDirection;
    private Collider[] victimColliders = new Collider[10];

    void Start()
    {
        eating = false;
    }

    public override void FixedUpdate()
    {
        if (GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false) return;
        if (isDie == true || isAttack) return;

        // HP 자동 회복 로직
        if (Time.time - lastDamageTime >= regenerationInterval && hp < MaxHp)
        {
            RecoverHP();
        }

        InteractWithNearbyDinosaurs();
        //InteractWithNearbyDinosaurs();
        //if (isBumped == true) Invoke("ResetBumpCheck", 3f);
        AvoidObstacles();
        if (obstacleDetected)
        {
            Move();
            
            return;
        }
        TraceVictim(); 
        ChaseVictim();
        if (closestVictim != null)
        {
            closestVictimObject = (MonoBehaviour)closestVictim;
            Move();
            return;
        }

        
        AvoidPredator();
        if (predatorDetected)
        {
            Move();
            return;
        }


        Wander();
        Move();


    }

    //public override void FixedUpdate()
    //{
    //    if (isDie) return;
    //    base.FixedUpdate();
    //    //먹이 추적(가장 낮은 우선 순위)
        
    //    TraceVictim(); // 포식자 회피를 우선으로 한 후, 먹이를 추적하도록 함.
    //    ChaseVictim();
    //    closestVictimObject = (MonoBehaviour)closestVictim;
    //    Move();
    //}
    public override void OnEnable()
    {
        sizes = new int[3] { 10, 50, 100 };
        size = sizes[infoIdx];
        base.OnEnable();
        //eatCooltime = 0;
    }


    public bool canEat(IDinosaur other)
    {

        //공격하려는 동물이 이미 죽었으면 공격 불가 판정.
        if (((Animal)other).isDie) return false;
        if (other is Raptor && this is Raptor) return false;
        else if(other is Carnivore)
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
            else{
                //Debug.Log($"{gameObject.name}이 {((MonoBehaviour)other).gameObject.name}을 공격 : {size >= ((MonoBehaviour)other).gameObject.GetComponent<Animal>().size}");
                return infoIdx >= ((MonoBehaviour)other).gameObject.GetComponent<Animal>().infoIdx;
            }

                //&& this != carnivore;
        }
        else if(other is Herbivore herbivore)
        {
            if (this is Raptor)
            {
                int count;
                if (this.GetComponent<Raptor>().leader != null) count = this.GetComponent<Raptor>().leader.followers.Count + 1; //리더 포함 팀원 수 세기
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
            //return size >= herbivore.size;
            else return true;
        }
        return false;
    }

    public bool canTrace(IDinosaur other)
    {
        //추적하려는 동물이 이미 죽었으면 추적 불가 판정.
        if (((Animal)other).isDie) return false; 
        if (other is Raptor && this is Raptor)
        {
            //가까이 있는 랩터가 나의 리더이거나 팔로워면 리턴 false
            if (gameObject.GetComponent<Raptor>().leader == ((MonoBehaviour)other).GetComponent<Raptor>() 
                || gameObject.GetComponent<Raptor>().leader != null &&gameObject.GetComponent<Raptor>().leader.followers.Contains(((MonoBehaviour)other).GetComponent<Raptor>())) return false;
            return true;
        }
        else if (other is Carnivore carnivore)
        {
            if (this is Raptor)
            {
                int count;
                Raptor thisRaptor = GetComponent<Raptor>();
                if (thisRaptor.leader != null) count = thisRaptor.leader.followers.Count + 1;
                else count = 1;

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
            else
            {
                return infoIdx >= ((MonoBehaviour)other).gameObject.GetComponent<Animal>().infoIdx;
            }
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
        if (victimDetected && closestVictim != null)
        {
            if (canEat(closestVictim))
            {
                if (victimDirection != Vector3.zero)
                {
                    goalLookRotation = Quaternion.LookRotation(victimDirection);
                    moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax, Time.deltaTime);
                }
            }
            else
            {
                // Move away from the target
                Vector3 awayDirection = transform.position - ((MonoBehaviour)closestVictim).transform.position;
                goalLookRotation = Quaternion.LookRotation(awayDirection);
                moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax * 1.5f, Time.deltaTime); // Move away faster
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
            victimDetected = true;
            Vector3 victimDirection = ((MonoBehaviour)closestVictim).transform.position - transform.position;
            goalLookRotation = Quaternion.LookRotation(victimDirection);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * maxTurnRateY);
            // Y축 회전만 적용
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            float distanceToVictim = Vector3.Distance(transform.position, ((Animal)closestVictim).transform.position);
            if((infoIdx != 2 && distanceToVictim > 3f) || (infoIdx == 2 && distanceToVictim > 15f))// 두 공룡의 위치가 매우 가까울 때 빙빙 도는 문제 막기 위함
                transform.rotation = Quaternion.Euler(eulerAngles);
        }
        else
        {
            victimDetected = false;
        }
    }

    protected IDinosaur FindClosetVictim()
    {
        //Debug.Log($"[{gameObject.name}] FindClosetVictim 호출됨"); // 호출 여부 확인

        float closestSqrDistance = Mathf.Infinity;
        closestVictim = null; // Reset closestVictim at the beginning of the search

        // 콜라이더 배열 재사용
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, playerSensingDistance, victimColliders, findLayerMask[0] | findLayerMask[1] | findLayerMask[2]);
        //Debug.Log($"[{gameObject.name}] 감지된 콜라이더 수: {colliderCount}"); // 감지된 콜라이더 수 확인

        for (int i = 0; i < colliderCount; i++)
        {
            IDinosaur dinosaur = victimColliders[i].GetComponent<IDinosaur>();
            //Debug.Log($"[{gameObject.name}] 감지된 오브젝트: {victimColliders[i].gameObject.name}, IDinosaur 컴포넌트: {dinosaur != null}"); // 감지된 오브젝트 및 컴포넌트 확인
            if (this is Raptor raptor && raptor.leader != null && (Animal)dinosaur is Raptor raptor2) //해당 공룡이 랩터이고 리더가 있을 때
            {       //조사중인 공룡이 해당 공룡과 같은 리더를 갖거나 리더이면
                if (raptor.leader.followers.Contains(raptor2) || raptor.leader == raptor2)
                {
                    //Debug.Log($"[{gameObject.name}] 랩터 그룹원이라 null 반환");
                    continue;
                }
            }
            // 자기 자신과 충돌하지 않으며, 먹을 수 있는 대상을 찾는다.
            if (dinosaur != null && (Object)dinosaur != this && canTrace(dinosaur))
            {
                //Debug.Log($"[{gameObject.name}] {dinosaur.GetType().Name} 추적 가능"); // 추적 가능 여부 확인
                float sqrDistance = (transform.position - victimColliders[i].transform.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestVictim = dinosaur;
                    //Debug.Log($"[{gameObject.name}] 가장 가까운 먹잇감 발견: {closestVictim.GetType().Name}"); // 찾은 먹잇감 확인
                }
            }
            else if (dinosaur != null && (Object)dinosaur != this)
            {
                //Debug.Log($"[{gameObject.name}] {dinosaur.GetType().Name} 추적 불가능 (canTrace: {canTrace(dinosaur)})"); // 추적 불가능 이유 확인
            }
        }
        //Debug.Log($"[{gameObject.name}] 최종 closestVictim: {closestVictim?.GetType().Name}"); // 최종 결과 확인
        return closestVictim;
    }

    public void Hunt(IDinosaur other)
    {
        if (eating == true) return;
        
        if (canEat(other))
        {
            Debug.Log($"{gameObject.name}의 공격 시도");
            Attack(other as Animal);
            (other as Animal).SetAttacker(this);
        }
        else
        {

        }
    }

    public override void Move()
    {
        if (obstacleDetected)
        {
            base.Move();
        }
        if (closestVictim == null)
        {
            //Debug.Log($"[{gameObject.name}] Move 호출됨 - closestVictim이 null이므로 base.Move() 호출");
            base.Move();
        }
        else //타겟이 있을 때
        {
            float distanceToVictim = Vector3.Distance(transform.position, ((Animal)closestVictim).transform.position);
            //Debug.Log($"[{gameObject.name}] Move 호출됨 - closestVictim: {closestVictim.GetType().Name}, 거리: {distanceToVictim}");

            if ((infoIdx != 2 && distanceToVictim > 6f) || (infoIdx == 2 && distanceToVictim > 20f))
            {
                //Debug.Log($"[{gameObject.name}] 먹잇감과의 거리가 5보다 크므로 base.Move() 호출");
                base.Move();
            }
            else
            {
                //Debug.Log($"[{gameObject.name}] 먹잇감과의 거리가 5 이하이므로 움직임 멈춤");
                // base.Move()를 호출하지 않아 움직임이 멈춥니다.
                CurrentState = AnimalState.Idle;
            }
        }
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
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (isDie) PoolingManager.Instance.CallSpawn(1, infoIdx);
    }
}
