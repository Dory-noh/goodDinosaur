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
            for(int j = 0; j < 30; j++)
            {
                GameObject dinosaur = Instantiate(dinoPrefabs[i], transform);
                dinosaur.name = $"{j+1}¹øÂ° {dinoPrefabs[i].name.ToString()}";
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
            for(int j = 0; j < 30; j++)
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
                Vector3 pos = SpawnPoints[(Random.Range(0, SpawnPoints.Length))].position;
                dino.transform.position = new Vector3(Random.Range(pos.x-30, pos.x+30), pos.y+10, Random.Range(pos.z-30, pos.z+30));
                dino.transform.rotation = Quaternion.identity;
                dino.gameObject.SetActive(true);
                break;
            }
        }
    }
}
