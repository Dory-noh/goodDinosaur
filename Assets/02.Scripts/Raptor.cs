using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Raptor : Carnivore
{
    public Raptor leader; //���� ����
    public List<Raptor> followers = new List<Raptor>(); //������ ���� ���

    public override void OnEnable()
    {
        if (gameObject.CompareTag("Player")) leader = this;
        size = sizes[0];
        base.OnEnable();

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
        base.Move();
        if (leader != null)
        {
            Debug.Log("���ʹ� �������� �̵����Դϴ�.");
        }
        else
        {
            //Debug.Log("���Ͱ� �̵��մϴ�.");
        }   
    }

    public override void Interact(IDinosaur other)
    {
        base.Interact(other); //���� ������ ��ȣ�ۿ� ���� ����

        if(other is Raptor otherRaptor)
        {
            if(leader == null && otherRaptor.leader == null) //�ε��� ���� �� �� ���� ���� ��
            {
                if(otherRaptor.gameObject.CompareTag("Player")|| (this.size < ((MonoBehaviour)other).GetComponent<Animal>().size)) 
                {
                    otherRaptor.AddFollower(this);
                    leader = otherRaptor;
                    otherRaptor.leader = otherRaptor;
                }
                else if(this.size >= ((MonoBehaviour)other).GetComponent<Animal>().size)//�ε��� ������Ʈ�� size�� �ش� ������Ʈ�� size�� ����
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
                    if (otherRaptor.gameObject.CompareTag("Player")|| (this.size < ((MonoBehaviour)other).GetComponent<Animal>().size)) //�浹�� ������Ʈ�� �÷��̾�� �ش� ������Ʈ�� �����ڰ� ��.
                    {
                        otherRaptor.AddFollower(this);
                        foreach(var rapter in followers)
                        {
                            otherRaptor.AddFollower(rapter);
                        }
                        followers.Clear();
                        leader = otherRaptor;
                    }
                    else if(this.size == ((MonoBehaviour)other).GetComponent<Animal>().size)//�ε��� ������Ʈ�� size�� �ش� ������Ʈ�� size�� ����
                    {//������..?
                        otherRaptor.AddFollower(this);
                        foreach (var rapter in followers)
                        {
                            otherRaptor.AddFollower(rapter);
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
    private void OnDisable()
    {
        leader = null;
        foreach (var rapter in followers)
        {
            rapter.leader = null;
        }
        followers.Clear() ;
    }
}
