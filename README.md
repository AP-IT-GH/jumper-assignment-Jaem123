# Inleiding tot de usecase
Een agent wordt getraind om eerst over een balk te springen en vervolgens alleen de blauwe balk te raken.
# Observaties, acties en beloning

In het geval van onze agent zijn de observaties diegene die hij maakt door "vooruit" te kijken. 
Dit verwezenlijken we door onze agent rayperception sensor 3D componenenten te geven.

# Acties

Op de ene of andere manier moet het algoritme leren om de agent a.h.w. te besturen. Met andere woorden, het algoritme moet een actie voorstellen. 
Wij, de ontwikkelaar mappen de acties naar beweging. In het geval van de agent zijn er 6 mogelijke acties.
* 1 voorwaartse beweging actie: voorwaarts bewegen
* 2 spring beweging acties: springen en niet springen

Springen is belangrijk omdat onze agent moet kunnen vooruit kijken waar het blauwe balk zich bevindt en wanneer het moet springen.

# Beloning

Het beloningsmechanisme (eng: rewards, incentives) vertelt het leeralgoritme of de voorgestelde actie de agent dichter bij het einddoel van de leeroefening brengt of niet.

We belonen onze agent met +1 waarde als hij het blauwe balk kan aanraken en als het springt krijgt die aftraffing van -0.01. Als het blokje op het einde van het platform komt of de rode platform aanraakt stopt dan ook de episode.

Verder krijgt onze agent een afstraffing van -1 als hij van het platform valt of de wall aanraakt. In deze situatie stopt de episode ook onmiddellijk.

# Unity omgeving aanmaken

Open de scene : Jumper. We maken een empty object dat we de naam environment geven. Daaronder maken we onze speelobjecten aan, namelijk onze Plane, Wall, Player (of agent), Obstacles (of balk).

# Environment

Aan het parent object Environment wijzen we een script toe (EnvironmentJumper.cs)

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

We hebben van onze obstacles een prefab gemaakt, en via het "EnvironmentJumper" script gaan we elke episode de enemy dynamisch in onze omgeving plaatsen. Op die manier kunnen we heel makkelijk uitbreiden naar verschillende vijanden. Ook is er een CleanEnvironment methode aangemaakt die bij het aanroepen van EndEpisode() de obstacles zal verwijderen. Dit is nodig in bijvoorbeeld het geval dat onze agent de obstacles nog niet gevonden heeft en van het vlak valt. Dan wordt er opnieuw "SpawnEnemy()" opgeroepen en zullen er 2 obstacles op het speelbord verschijnen, wat in onze use case niet de bedoeling is.

Verder is het script JumperAgent aan onze agent toegewezen. In de Start() methode refereren we naar EnvironmentJumper door volgend commando: environment = GetComponentInParent();

Op die manier kunnen we bij OnEpisodeBegin() steeds ClearEnvironment() en SpawnEnemy() oproepen.

"using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class JumperAgent : Agent
{
    [SerializeField]
    private float jumpStrength = 5f;
    [SerializeField]
    private TextMeshPro scoreboard;
    //[SerializeField]
    //private Transform resetAgent = null;
    //private Vector3 reset;
    private bool canJump = true;
    private Rigidbody body;
    private EnvironmentJumper environment;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        environment = GetComponentInParent<EnvironmentJumper>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreboard.text = GetCumulativeReward().ToString("f4");
    }

    public override void OnEpisodeBegin()
    {
        environment.ClearEnvironment();
        transform.localPosition = new Vector3(7, 0.5f, 0);
        transform.localRotation = Quaternion.Euler(0, -90, 0f);
        //reset = new Vector3(resetAgent.position.x, resetAgent.position.y, resetAgent.position.z);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        var vectorAction = actions.DiscreteActions;
        if (vectorAction[0] == 1)
        {
            //punish with small negative award to prevent jumping all the time
            AddReward(-0.01f);
            Jump();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //map actions to movement
        var jump = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            jump = 1;
        }
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = jump;
    }

    //private void ResetPlayer()
    //{
    //    this.transform.position = reset;
    //}

    private void Jump()
    {
        if (canJump)
        {
            body.AddForce(new Vector3(0, jumpStrength, 0), ForceMode.VelocityChange);
            canJump = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Plane"))
        {
            canJump = true;
        }

        if (collision.transform.CompareTag("Obstacle"))
        {
            Debug.Log("collide with obstacle");
            Destroy(collision.gameObject);
            AddReward(-1f);
            EndEpisode();
        }

        if (collision.transform.CompareTag("BonusObstacle"))
        {
            Debug.Log("collide with bonus obstacle");
            Destroy(collision.gameObject);
            AddReward(1f);
        }

        if (collision.transform.CompareTag("Resetzone") || collision.transform.CompareTag("Wall"))
        {
            EndEpisode();
            //ResetPlayer();
        }
        /*if (collision.transform.CompareTag("HiddenCollider"))
        {
            Debug.Log("collide with hidden collider");
            AddReward(1f);
        }*/

    }
}
"
