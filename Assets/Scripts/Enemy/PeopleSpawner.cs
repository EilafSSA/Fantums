using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleSpawner : MonoBehaviour
{
    public GameObject RunningPeople;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(Random.Range(2, 5));
    }


    IEnumerator Start()
    {
            for(int i=0; i< 25; i++)
        {
        Invoke("CreatePeople", Random.Range(2, 5));

        yield return StartCoroutine("Wait");
        }
    }


    private void CreatePeople()
    {
        Instantiate(RunningPeople, new Vector3(0, 2, 0), Quaternion.identity);
        //Instantiate(runningPeople, new Vector3(8.22f,3.15f,0f), Quaternion.identity);
    } 

}
