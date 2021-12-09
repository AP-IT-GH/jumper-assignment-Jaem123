using System.Collections;
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
