using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DropCheck : MonoBehaviour
{
    private void OnColliderEnter(Collision other)
    {
        Debug.Log("충돌 체크 A");
        Animal animal = other.gameObject.GetComponent<Animal>();
        if (animal != null)
        {
            if (animal is PlayerControl) other.transform.position = new Vector3(38, 5, 59);
            else
            {
                animal.Die();
            }
        }
    }
}
