using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Raptor : Carnivore
{
    public Raptor leader; //���� ����
    public List<Raptor> followers = new List<Raptor>(); //������ ���� ���
    [SerializeField] NavMeshAgent agent;
    public int raptorLevel = 0;

    public override void OnEnable()
    {
        base.OnEnable();
        if (gameObject.CompareTag("Player")) leader = this;
        
        if (leader != null)
        {
            leader.AddFollower(this);
        }
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
        if (leader == this || leader == null || gameObject.CompareTag("Player")) base.Move();
        else if(leader != null)
        {

            if (agent == null)
            {
                gameObject.AddComponent<NavMeshAgent>();
                agent = GetComponent<NavMeshAgent>();
                agent.stoppingDistance = 1.5f;
                agent.speed = moveSpeed;
            }
            if (agent.isOnNavMesh) // NavMesh �� ���� ���� SetDestination�� ȣ��
            {
                agent.SetDestination(((MonoBehaviour)leader).gameObject.transform.position);
            }
        }
        else
        {
            Debug.Log("���� ���� �߻�");
        }
    }

    public override void Interact(IDinosaur other)
    {
        base.Interact(other); //���� ������ ��ȣ�ۿ� ���� ����
        //���� �� ���� ���� 5���� ���Ϸ� ����
        if (leader != null && (leader.followers.Count + 1 >= 5)) return;
        //�ε��� ������ �ش� ������ �����ų� ���� ������ �ȷο��� ������� ����.
        if ((leader == (Object)other) || (leader != null && leader.followers.Contains(((MonoBehaviour)other).GetComponent<Raptor>()))) return;
            if (other is Raptor otherRaptor) //�ε��� ���浵 ������ ��� �����ϴ� �κ�
        {
            if (otherRaptor.leader != null && otherRaptor.leader.followers.Count + 1 >= 5) return;

            if (leader == null && otherRaptor.leader == null) //�ε��� ���� �� �� ���� ���� ��
            {
                if(otherRaptor.gameObject.CompareTag("Player")||(!otherRaptor.gameObject.CompareTag("Player")&&this.raptorLevel < ((MonoBehaviour)other).GetComponent<Raptor>().raptorLevel))                
                {
                    otherRaptor.AddFollower(this);
                    leader = otherRaptor;
                    otherRaptor.leader = otherRaptor;
                }
                else if(this.raptorLevel >= ((MonoBehaviour)other).GetComponent<Raptor>().raptorLevel)//�ε��� ������Ʈ�� size�� �ش� ������Ʈ�� size�� ����
                {
                    //if (followers.Contains(otherRaptor)) return;
                    this.AddFollower(otherRaptor);
                    otherRaptor.leader = this;
                    leader = this;
                }
            }
            else if((leader != null && otherRaptor.leader == null)) //���� ���� �ְ� �ε��� ������Ʈ�� ���� ���� ��
            {
                //�� ������ �ε��� ģ���� ������ ��´�.
                leader.AddFollower(otherRaptor);
                otherRaptor.leader = leader;
                Debug.Log($"������ Follower�� �����մϴ�.");
            }
            //else if (leader == null && otherRaptor.leader != null) //���� ���� ���� �ε��� ģ���� ���� ���� �� - �� else if���� ���� ó�� ������ �� ����.
            //{
            //    otherRaptor.leader.AddFollower(this);
            //    leader = otherRaptor.leader;
            //    Debug.Log($"����� ������ Follower�� �����մϴ�.");
            //}
            else if(leader == otherRaptor.leader) //���� ������ �����ڸ� �浹 ����
            {
                //������ �ְ� �ٸ� ���Ͱ� �����ڶ�� ������ ���� �̵�
            }
            else if(leader != otherRaptor.leader) //�ٸ� ������ ������
            {
                //�浹�� ������Ʈ �� �� ������ ��
                if (leader == this && otherRaptor.leader == otherRaptor)
                {
                    if ((otherRaptor.gameObject.CompareTag("Player"))|| (!otherRaptor.gameObject.CompareTag("Player") &&(this.raptorLevel <= ((MonoBehaviour)other).GetComponent<Raptor>().raptorLevel))) //�浹�� ������Ʈ�� �÷��̾�� �ش� ������Ʈ�� �����ڰ� ��.
                    {
                        otherRaptor.AddFollower(this);
                        foreach(var rapter in followers)
                        {
                            if (rapter == this) continue;
                            otherRaptor.AddFollower(rapter);
                            rapter.leader = otherRaptor;
                        }
                        followers.Clear();
                        leader = otherRaptor;
                        Debug.Log("���� ���� �Ϸ�");
                    }
                    else
                    {
                        Debug.Log("���� �߻� - �� �� ������ ���");
                    }
                }
                //�� �ʸ� ������ ��


                //�� �� ���� �ƴ� ��
            }
        }
    }
    public override void Die()
    {
        base.Die();
        if (isDie) PoolingManager.Instance.CallSpawn(0);
    }


    private void OnDisable()
    {
        if (leader != null)
        {
            if (leader != this) leader.followers.Remove(this); //�ش� Rapter�� ������ �ƴҶ�, ������ Follow ��Ͽ��� �ش� Rapter�� �����.
            leader = null;
            if (this == leader)
            {
                foreach (var rapter in followers)
                {
                    rapter.leader = null;
                }

            }
        }
        followers.Clear();
        
    }
}
