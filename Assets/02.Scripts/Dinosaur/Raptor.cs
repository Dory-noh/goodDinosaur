using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Raptor : Carnivore
{
    public Raptor leader; //리더 랩터
    public List<Raptor> followers = new List<Raptor>(); //추종자 랩터 목록
    public int raptorLevel = 0;
    public float followingDistance = 5f; // 리더와의 유지 거리
    public float rotationSpeed = 5f; // 회전 속도
    public float leaderSpeedMultiplier = 1.7f; //리더 있을 때 속도 증가 배율
    public float followerSpacing = 4f; // 팀원 간 유지 거리
    public float spacingForce = 1f; // 밀어내는 힘의 강도
    public override void OnEnable()
    {
        base.OnEnable();
        leader = null;
        followers.Clear();
        if (gameObject.CompareTag("Player")) leader = this;
    }

    public void AddFollower(Raptor follower)
    {
        followers.Add(follower);
    }

    public void RemoveFollower(Raptor follower)
    {
        followers.Remove(follower);
    }

    public override void Move()
    {
        CurrentState = AnimalState.Move;
        float currentMoveSpeed = base.moveSpeed;

        if (leader != null && leader is PlayerControl && leader != this)
        {
            currentMoveSpeed *= leaderSpeedMultiplier;
        }
        else
        {
            currentMoveSpeed = base.moveSpeed;
        }
        moveSpeed = currentMoveSpeed;
        if (leader == this || leader == null || this is PlayerControl)
        {
            base.Move();
        }
        else if (leader != null)
        {
            Vector3 directionToLeader = leader.transform.position - transform.position;
            directionToLeader.y = 0;

            if (closestVictim == null)
            {
                if (directionToLeader != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToLeader);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
            else
            {
                Vector3 directionToVictim = ((MonoBehaviour)closestVictim).transform.position - transform.position;
                if (directionToVictim != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToVictim);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }

                float distanceToTarget = Vector3.Distance(transform.position, ((MonoBehaviour)closestVictim).transform.position);
                float attackDistance = ((Animal)closestVictim).infoIdx == 0 ? 3f : ((Animal)closestVictim).infoIdx == 1 ? 7f : 15f;

                if (distanceToTarget > attackDistance)
                {
                    base.Move();
                }
                else
                {
                    CurrentState = AnimalState.Idle;
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                    }
                    Hunt(closestVictim);
                }
            }

            foreach (var follower in leader.followers)
            {
                if (follower != null && follower != this)
                {
                    float distanceToFollower = Vector3.Distance(transform.position, follower.transform.position);
                    if (distanceToFollower < followerSpacing * 2)
                    {
                        Vector3 pushDirection = (transform.position - follower.transform.position).normalized;
                        Quaternion pushRotation = Quaternion.LookRotation(pushDirection);
                        goalLookRotation = Quaternion.Slerp(goalLookRotation, pushRotation, Time.deltaTime * rotationSpeed * spacingForce);
                    }
                }
            }

            float distanceToLeader = directionToLeader.magnitude;
            if (distanceToLeader > followingDistance * 2)
            {
                base.Move();
            }
            else
            {
                CurrentState = AnimalState.Idle;
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }
        else
        {
            Debug.Log("랩터 예외 발생");
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.fixedDeltaTime / 2f);
    }

    public void InitiateAttack(IDinosaur target)
    {
        closestVictim = target;

        foreach (var follower in followers)
        {
            if (follower != null && !follower.isDie)
            {
                follower.closestVictim = target;
            }
        }
    }

    public override void Interact(IDinosaur other)
    {
        if (this is PlayerControl playerThis) { Debug.Log("플레이어의 리더 결정전을 시작합니다."); }
        base.Interact(other); // 육식 공룡의 상호작용 로직 실행
        if (other == (this as IDinosaur)) return; // 본인을 감지했을 때 리턴

        if (other is PlayerControl playerOther)
        {
            Debug.Log($"Interact: {gameObject.name} (leader: {leader?.gameObject.name}), 부딪힌 대상: Player (leader: {playerOther.leader?.gameObject.name})");
            if (leader != playerOther) // 자신이 플레이어의 팔로워가 아니면
            {
                if (playerOther.followers.Count < 4)
                {
                    // 이미 팔로워로 등록되어 있는지 확인
                    bool IsAlreadyFollower(Raptor potentialFollower, Raptor currentLeader)
                    {
                        if (currentLeader == null) return false;
                        return currentLeader.followers.Contains(potentialFollower);
                    }

                    if (leader == null || !IsAlreadyFollower(this, playerOther))
                    {
                        // 기존 리더가 있다면 팔로워 목록에서 제거
                        if (leader != null)
                        {
                            leader.RemoveFollower(this);
                            leader = null; // 기존 리더 해제
                        }
                        playerOther.AddFollower(this);
                        this.leader = playerOther;
                        Debug.Log($"{gameObject.name}이(가) 플레이어의 팔로워가 되었습니다.");
                    }
                    else
                    {
                        Debug.Log($"{gameObject.name}은(는) 이미 플레이어의 팔로워입니다.");
                    }
                }
                else
                {
                    Debug.Log($"플레이어 팀이 가득 찼습니다.");
                }
            }
            return; // 플레이어와의 상호작용 후에는 더 이상 다른 랩터 로직을 실행하지 않음
        }

        if (other is Raptor otherRaptor) // 부딪힌 공룡도 랩터인 경우 실행하는 부분
        {
            Debug.Log($"Interact: {gameObject.name} (leader: {leader?.gameObject.name}), 부딪힌 대상: {otherRaptor.gameObject.name} (leader: {otherRaptor.leader?.gameObject.name})");

            // 이미 팔로워로 등록되어 있는지 확인하는 함수
            bool IsAlreadyFollower(Raptor potentialFollower, Raptor currentLeader)
            {
                if (currentLeader == null) return false;
                return currentLeader.followers.Contains(potentialFollower);
            }

            // 새로운 팔로워를 추가하고 기존 리더십을 정리하는 함수
            void AddNewFollower(Raptor newFollower, Raptor newLeader)
            {
                if (newFollower == null || newLeader == null || newFollower == newLeader || IsAlreadyFollower(newFollower, newLeader)) return;

                // 기존 리더가 있다면 팔로워 목록에서 제거
                if (newFollower.leader != null && newFollower.leader != newFollower)
                {
                    newFollower.leader.RemoveFollower(newFollower);
                }

                newLeader.AddFollower(newFollower);
                newFollower.leader = newLeader;
                Debug.Log($"{newFollower?.gameObject.name}이(가) {newLeader?.gameObject.name}의 팔로워가 됨");
            }

            if ((leader == (Object)other) || (leader != null && leader.followers.Contains(otherRaptor)))
            {
                Debug.Log("Interact: 부딪힌 랩터가 이미 리더이거나 같은 팀의 팔로워입니다.");
                return;
            }

            if (leader == null && otherRaptor.leader == null) // 둘 다 리더가 없을 때
            {
                if (this.raptorLevel < otherRaptor.raptorLevel)
                {
                    AddNewFollower(this, otherRaptor);
                }
                else if (this.raptorLevel >= otherRaptor.raptorLevel)
                {
                    AddNewFollower(otherRaptor, this);
                    leader = this;
                }
            }
            else if (leader != null && otherRaptor.leader == null) // 나는 리더 있고 상대는 리더 없을 때
            {
                if (leader.followers.Count < 4)
                {
                    AddNewFollower(otherRaptor, leader);
                }
            }
            else if (leader == null && otherRaptor.leader != null) // 나는 리더 없고 상대는 리더 있을 때
            {
                if (otherRaptor.leader.followers.Count < 4)
                {
                    AddNewFollower(this, otherRaptor.leader);
                }
            }
            else if (leader != null && otherRaptor.leader != null && leader != otherRaptor.leader) // 서로 다른 리더가 있을 때
            {
                if (this.raptorLevel > otherRaptor.raptorLevel)
                {
                    if (leader.followers.Count + otherRaptor.followers.Count < 4)
                    {
                        foreach (var follower in otherRaptor.followers.ToList())
                        {
                            AddNewFollower(follower, this);
                            otherRaptor.RemoveFollower(follower);
                        }
                        AddNewFollower(otherRaptor, this); // 상대방도 자신의 팔로워로
                    }
                    else
                    {
                        Debug.Log("팀 규모 초과로 팔로워 흡수 실패");
                    }
                }
                else if (this.raptorLevel < otherRaptor.raptorLevel)
                {
                    if (otherRaptor.leader.followers.Count + followers.Count < 4)
                    {
                        foreach (var follower in followers.ToList())
                        {
                            AddNewFollower(follower, otherRaptor.leader);
                            RemoveFollower(follower);
                        }
                        AddNewFollower(this, otherRaptor.leader); // 자신도 상대방의 팔로워로
                    }
                    else
                    {
                        Debug.Log("팀 규모 초과로 팔로워 합류 실패");
                    }
                }
                else // 레벨이 같으면 무시
                {
                    Debug.Log("랩터 레벨이 같아 리더십 변경 없음");
                }
            }
        }
    }

    public override void Die()
    {
        raptorLevel = 0;
        if (leader != null)
        {
            Debug.Log($"{gameObject.name}이(가) 죽어 {leader.gameObject.name}의 팔로워 리스트에서 제거합니다.");
            if (leader != this) leader.followers.Remove(this);

            if (this == leader)
            {
                foreach (var rapter in followers)
                {
                    rapter.leader = null;
                }
            }
            leader = null;
        }
        leader = null;
        followers.Clear();
        base.Die();
    }


    public override void OnDisable()
    {
        base.OnDisable();
        raptorLevel = 0;
        if (leader != null)
        {
            Debug.Log($"{gameObject.name}이(가) 비활성화되어 {leader.gameObject.name}의 팔로워 리스트에서 제거합니다.");
            if (leader != this) leader.followers.Remove(this);

            if (this == leader)
            {
                foreach (var rapter in followers)
                {
                    rapter.leader = null;
                }
            }
            leader = null;
        }
        leader = null;
        followers.Clear();
        if (isDie) PoolingManager.Instance.CallSpawn(1, infoIdx);
    }
}