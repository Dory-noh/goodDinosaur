using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DictionaryManager : MonoBehaviour
{
    public static DictionaryManager Instance;
    public GameObject dinoInfo;
    public Transform infoContainer;
    [SerializeField] List<DinoInfo> collectedDino = new List<DinoInfo> ();
    public GameObject DictionaryUi;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void AddDino(DinoInfo newDino)
    {
        if (!collectedDino.Contains(newDino)) 
        {
            collectedDino.Add(newDino);
            CreateDino(newDino);
            //Debug.Log($"Dino Name : {newDino.name}");
        }
    }
    private void CreateDino(DinoInfo Dino) 
    {
        GameObject newDinoUi = Instantiate(dinoInfo, infoContainer);

        TMP_Text nameTxt = newDinoUi.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text descriptionTxt = newDinoUi.transform.Find("Description").GetComponent<TMP_Text>();

        if (nameTxt != null) nameTxt.text = Dino.name;
        if(descriptionTxt != null)descriptionTxt.text = Dino.description;
    }
    void Update()
    {
        
    }
}
