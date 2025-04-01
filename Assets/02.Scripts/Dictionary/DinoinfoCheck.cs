using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

public class DinoinfoCheck : MonoBehaviour
{
    public DinoInfo dinoInfo;
    bool isRegistered = false;
    void OnEnable()
    {
        var Interactable = GetComponent<XRSimpleInteractable>();
        if (Interactable != null)
        {
            Interactable.hoverEntered.AddListener(RegisterDinoInfo);
        }
    }

    // Update is called once per frame
    void OnDisable()
    {
        var Interactable = GetComponent<XRSimpleInteractable>();
        if (Interactable != null)
        {
            Interactable.hoverEntered.RemoveListener(RegisterDinoInfo);
        }
    }
    public void RegisterDinoInfo(HoverEnterEventArgs Args)
    {
        if (!isRegistered)
        {
            DictionaryManager.Instance.AddDino(dinoInfo);
            isRegistered = true;
        }
    }
}
