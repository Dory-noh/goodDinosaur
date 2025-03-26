using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public override void Die()
    {
        base.Die();
        if (isDie) PoolingManager.Instance.CallSpawn(0, infoIdx);
    }

    private void OnDisable()
    {
        
    }
}
