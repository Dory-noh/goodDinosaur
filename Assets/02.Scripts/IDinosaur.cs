using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDinosaur
{
    void Move(); //�̵�
    void Interact(IDinosaur other); //��ȣ�ۿ�
    void Display(); //���� ǥ��
}

//���� ���� �������̽�
public interface ICarnivore : IDinosaur
{
    void Hunt(IDinosaur other); //���
    bool canEat(IDinosaur other); //���� �� �ִ� �� ����
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