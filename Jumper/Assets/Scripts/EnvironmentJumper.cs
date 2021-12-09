using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentJumper : MonoBehaviour
{
    public GameObject ObstaclePrefab1;
    public GameObject ObstaclePrefab2;
    public GameObject Obstacles;
    public bool canSpawnObstacles = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        Obstacles = transform.Find("Obstacles").gameObject;
        Debug.Log("start spwn");
        
        StartCoroutine(SpawnObstacleContinuously());

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator SpawnObstacleContinuously()
    {
        while (true)
        {
            float r = Random.Range(2f, 5.0f);
            yield return new WaitForSeconds(r); 
            if(canSpawnObstacles)
               SpawnObstacle();
        }
    }

    //Spawn every X seconds

    public void SpawnObstacle()
    {
        GameObject newObstacle = null;
        int random = Random.Range(0, 3);

        if (random == 2)
        {
            newObstacle = Instantiate(ObstaclePrefab2.gameObject);
        }
        else
        {
            newObstacle = Instantiate(ObstaclePrefab1.gameObject);
        }

        newObstacle.transform.SetParent(Obstacles.transform);
        // float rx = Random.Range(-4f, 4);
        // float rz = Random.Range(-4f, 2);
        newObstacle.transform.localPosition = new Vector3(-8, 0.5f, 0);
    }

    public void ClearEnvironment()
    {        
        foreach (Transform obstacle in Obstacles.transform)
        {
            GameObject.Destroy(obstacle.gameObject);
        }

        canSpawnObstacles = true;
    }
}
