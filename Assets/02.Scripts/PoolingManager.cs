using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    [SerializeField] private Transform[] SpawnPoints;
    [SerializeField] GameObject[] dinoPrefabs;
    [SerializeField] List<List<GameObject>> dinoLists = new List<List<GameObject>>();
    void Awake()
    {
        createDinos();
    }

    void Update()
    {
        
    }

    void createDinos()
    {
        for(int i = 0; i < dinoPrefabs.Length; i++)
        {
            List<GameObject> dinos = new List<GameObject>();
            for(int j = 0; j < 10; j++)
            {
                GameObject dinosaur = Instantiate(dinoPrefabs[i], transform);
                dinosaur.SetActive(false);
                dinos.Add(dinosaur);
            }
            dinoLists.Add(dinos);
        }
        SetDinos();
    }

    void SetDinos()
    {
        for(int i = 0; i < dinoPrefabs.Length; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                SpawnDino(i);
            }
        }
    }

    void SpawnDino(int i)
    {
        foreach(var dino in dinoLists[i])
        {
            if(dino.activeSelf == false)
            {
                dino.transform.position = SpawnPoints[(Random.Range(0, SpawnPoints.Length))].position;
                dino.transform.rotation = Quaternion.identity;
                dino.gameObject.SetActive(true);
                break;
            }
        }
    }
}
