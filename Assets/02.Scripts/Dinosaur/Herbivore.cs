using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Herbivore : Animal, IHerbivore
{
    void Start()
    {
        sizes = new int[3] { 0, 500, 1000 };
    }

    public override void OnEnable( )
    {
        base.OnEnable();
        size = sizes[infoIdx];
    }

    public override void Interact(IDinosaur other)
    {
        base.Interact(other);
        if(other is Carnivore) Attack(other as Animal);

    }

    public override void Die()
    {
        base.Die();
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        if (isDie) PoolingManager.Instance.CallSpawn(0, infoIdx);
    }
}
