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
    

    public void Flee(ICarnivore carnivore)
    {
        Debug.Log("초식 동물 도망");
    }


    public override void Interact(IDinosaur other)
    {
        if(other is ICarnivore carnivore)
        {
            Flee(carnivore);
        }
    }
}
