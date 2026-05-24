using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleSpawner : MonoBehaviour
{
    public GameObject RunningPeople;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        InvokeRepeating("CreatePeople", 1, Random.Range(2,6));
        
    }

    private void CreatePeople()
    {
        Instantiate(RunningPeople, new Vector3(0, 2, 0), Quaternion.identity);
        Destroy(RunningPeople, 31f);
        //Instantiate(runningPeople, new Vector3(8.22f,3.15f,0f), Quaternion.identity);
    } 
}
