using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDeathHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.GameOver == true) return;
        
        
        Animal dinosaur = other.gameObject.GetComponent<Animal>();
        if (dinosaur != null)
        {
            Debug.Log("물에 빠지다");
            if (dinosaur is PlayerControl)
            {
                GameManager.Instance.GameOver = true;
                sceneManager.Instance.OnPlayerDie(sceneManager.Instance.DeathScenes[0]);
            }

            //dinosaur.Die();
            dinosaur.Die();
        }
    }
}
