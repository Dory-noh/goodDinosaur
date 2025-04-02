using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDeathHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("물에 빠지다");
        Animal dinosaur = other.gameObject.GetComponent<Animal>();
        if (dinosaur != null)
        {

            if (dinosaur is PlayerControl)
                sceneManager.Instance.OnPlayerDie(sceneManager.Instance.DeathScenes[0]);
            //dinosaur.Die();
            else dinosaur.Die();
        }
    }
}
