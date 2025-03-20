using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbivore : Animal, IHerbivore
{
    void Start()
    {
        sizes = new int[2] { 200, 1000 };
    }

    public override void OnEnable( )
    {
        size = sizes[Random.Range(0, sizes.Length)];
        base.OnEnable( );
    }

    public override void Interact(IDinosaur other)
    {
        base.Interact(other);
    }

    public override void Die()
    {
        base.Die();
        if (isDie) StartCoroutine(PoolingManager.Instance.waitSpawnDino(2));
    }

    private void OnDisable()
    {
        
    }
}
