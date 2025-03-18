using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Raptor : Carnivore
{
    private Raptor leader; //���� ����
    private List<Raptor> followers = new List<Raptor>(); //������ ���� ���

    public Raptor Leader { get; private set; }
    public List<Raptor> Followers { get; private set; } = new List<Raptor>();

    public override void OnEnable()
    {
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
            if(leader == null && otherRaptor.leader == null && otherRaptor != this)
            {
                if(Vector3.Distance(transform.position, otherRaptor.transform.position) < 5f)
                {
                    otherRaptor.AddFollower(this);
                    leader = otherRaptor;
                    Debug.Log("���Ͱ� �ٸ� ���͸� ������ �����մϴ�.");
                }
            }
            else if(leader != null && leader != otherRaptor && followers.Contains(otherRaptor))
            {
                //������ �ְ� �ٸ� ���Ͱ� �����ڶ�� ������ ���� �̵�
                Move();
            }
        }
    }
}
