using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDinosaur
{
    void Move(); //이동
    void Interact(IDinosaur other); //상호작용
    void Display(); //정보 표시
}

//육식 공룡 인터페이스
public interface ICarnivore : IDinosaur
{
    void Hunt(IDinosaur other); //사냥
    bool canEat(IDinosaur other); //먹을 수 있는 지 여부
}

public interface IHerbivore : IDinosaur
{
    //void Flee(ICarnivore carnivore);
}

public interface IMovable
{
    float moveSpeedMin { get; set; }
    float moveSpeedMax { get; set; }
    float maxTurnRateY { get; set; }
    float maxWanderAngle { get; set; }
    float wanderPeriodDuration { get; set; }
    float wanderProbability { get; set; }
    //void Wander();
    void UpdatePosition();
}