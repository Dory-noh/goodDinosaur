using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if(GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false) return;
        transform.forward = cam.forward;
    }
}

