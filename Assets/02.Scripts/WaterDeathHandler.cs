using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDeathHandler : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("물에 빠지다");
        Animal dinosaur = other.gameObject.GetComponent<Animal>();
        if (dinosaur != null)
        {
            dinosaur.Die();
        }
    }
}
