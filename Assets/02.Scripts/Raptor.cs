using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Raptor : Carnivore
{
    public Raptor leader; //리더 랩터
    public List<Raptor> followers = new List<Raptor>(); //추종자 랩터 목록

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
            if(leader == null && otherRaptor.leader == null) //부딪힌 랩터 둘 다 리더 없을 때
            {
                if(otherRaptor.gameObject.CompareTag("Player")|| (this.size < ((MonoBehaviour)other).GetComponent<Animal>().size)) 
                {
                    otherRaptor.AddFollower(this);
                    leader = otherRaptor;
                    otherRaptor.leader = otherRaptor;
                }
                else if(this.size >= ((MonoBehaviour)other).GetComponent<Animal>().size)//부딪힌 오브젝트의 size가 해당 오브젝트의 size와 같음
                {
                    //if (followers.Contains(otherRaptor)) return;
                    this.AddFollower(otherRaptor);
                    otherRaptor.leader = this;
                    leader = this;
                }
            }
            else if((leader != null && otherRaptor.leader == null)) //나는 리더 있고 부딪힌 오브젝트는 리더 없을 때
            {
                //내 리더를 부딪힌 친구의 리더로 삼는다.
                leader.AddFollower(otherRaptor);
                otherRaptor.leader = leader;
                Debug.Log($"리더의 Follower가 증가합니다.");
            }
            //else if (leader == null && otherRaptor.leader != null) //나는 리더 없고 부딪힌 친구는 리더 있을 때 - 위 else if문을 통해 처리 가능할 것 같음.
            //{
            //    otherRaptor.leader.AddFollower(this);
            //    leader = otherRaptor.leader;
            //    Debug.Log($"상대편 리더의 Follower가 증가합니다.");
            //}
            else if(leader == otherRaptor.leader) //같은 리더의 추종자면 충돌 무시
            {
                //리더가 있고 다른 랩터가 추종자라면 리더를 따라 이동
            }
            else if(leader != otherRaptor.leader) //다른 리더의 추종자
            {
                //충돌한 오브젝트 둘 다 리더일 때
                if (leader == this && otherRaptor.leader == otherRaptor)
                {
                    if (otherRaptor.gameObject.CompareTag("Player")|| (this.size < ((MonoBehaviour)other).GetComponent<Animal>().size)) //충돌한 오브젝트가 플레이어면 해당 오브젝트는 추종자가 됨.
                    {
                        otherRaptor.AddFollower(this);
                        foreach(var rapter in followers)
                        {
                            otherRaptor.AddFollower(rapter);
                        }
                        followers.Clear();
                        leader = otherRaptor;
                    }
                    else if(this.size == ((MonoBehaviour)other).GetComponent<Animal>().size)//부딪힌 오브젝트의 size가 해당 오브젝트의 size와 같음
                    {//선착순..?
                        otherRaptor.AddFollower(this);
                        foreach (var rapter in followers)
                        {
                            otherRaptor.AddFollower(rapter);
                        }
                        followers.Clear();
                        leader = otherRaptor;
                        Debug.Log("리더 수정 완료");
                    }
                    else
                    {
                        Debug.Log("예외 발생 - 둘 다 리더인 경우");
                    }
                }
                //한 쪽만 리더일 때


                //둘 다 리더 아닐 때
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
