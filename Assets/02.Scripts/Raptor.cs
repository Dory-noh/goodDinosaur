using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Raptor : Carnivore
{
    private Raptor leader; //리더 랩터
    private List<Raptor> followers = new List<Raptor>(); //추종자 랩터 목록

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
            Debug.Log("랩터는 리더따라 이동중입니다.");
        }
        else
        {
            //Debug.Log("랩터가 이동합니다.");
        }   
    }

    public override void Interact(IDinosaur other)
    {
        base.Interact(other); //육식 공룡의 상호작용 로직 실행

        if(other is Raptor otherRaptor)
        {
            if(leader == null && otherRaptor.leader == null && otherRaptor != this)
            {
                if(Vector3.Distance(transform.position, otherRaptor.transform.position) < 5f)
                {
                    otherRaptor.AddFollower(this);
                    leader = otherRaptor;
                    Debug.Log("랩터가 다른 랩터를 리더로 설정합니다.");
                }
            }
            else if(leader != null && leader != otherRaptor && followers.Contains(otherRaptor))
            {
                //리더가 있고 다른 랩터가 추종자라면 리더를 따라 이동
                Move();
            }
        }
    }
}
