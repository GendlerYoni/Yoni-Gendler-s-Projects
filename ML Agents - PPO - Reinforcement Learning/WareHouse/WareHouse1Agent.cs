using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// Controls the behavior of a warehouse agent in a multi-agent environment.
/// The agent must collect a colored target and deliver it to the matching tile.
/// </summary>
/// 
public class WareHouse1Agent : Agent
{
    private enum TargetState{
        None,        // No target
        RedTarget,   // Holding a red target
        YellowTarget // Holding a yellow target
    }

    [Header("References")]
    private WareHouse1Manager gameManager;
    private Transform redTile;
    private Transform yellowTile;

    [Header("Settings")]
    private float size = 9.5f;
    private float moveSpeed = 3f;
    private float turnAmount = 200f;
    
    private TargetState state = TargetState.None;

    int targetLayer;
    private float distance;
    private float prevDistance;

    private Color defaultColor;

    private Rigidbody rb;
    private Renderer agentRenderer;


    public override void Initialize(){
        // Called once when the agent is initialized. Caches references and sets up the environment.
        rb = GetComponent<Rigidbody>();
        agentRenderer = GetComponent<Renderer>();
        defaultColor = agentRenderer.material.color;


        if (gameManager == null){
            gameManager = GetComponentInParent<WareHouse1Manager>();
            redTile   = gameManager.GetRedTileWareHouse(); 
            yellowTile= gameManager.GetYellowTileWareHouse();
        }
        targetLayer = LayerMask.GetMask("Target");

    }

    public override void OnEpisodeBegin(){
        // Called at the start of each episode. Resets position, rotation, state, and appearance.
        transform.localPosition = new Vector3(Random.Range(-size+1.5f,size - 1.5f), 0.5f, Random.Range(-size+1.5f,size - 1.5f));
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.Euler(0f, Random.Range(0,360), 0f);
        
        state = TargetState.None;
        agentRenderer.material.color = defaultColor;
        prevDistance = -1;
    }

    public override void CollectObservations(VectorSensor sensor){
        // Gathers observations for the neural network: agent rotation, position, velocity, and direction to target or tile.
        float yRad = transform.localEulerAngles[1] * Mathf.Deg2Rad;

        sensor.AddObservation(Mathf.Sin(yRad));
        sensor.AddObservation(Mathf.Cos(yRad));

        float x = transform.localPosition.x / size;
        float z = transform.localPosition.z / size;

        sensor.AddObservation(x);
        sensor.AddObservation(z);

        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        sensor.AddObservation(localVelocity);

        Vector2 posAgentXZ = new Vector2(transform.localPosition.x, transform.localPosition.z);
        Vector2 currMin = new Vector2(transform.localPosition.x, transform.localPosition.z); //Defult

        if(state == TargetState.None){
            Collider[] hits = Physics.OverlapSphere(transform.position, size*2, targetLayer);
            float minDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        currMin = new Vector2(hit.transform.localPosition.x, hit.transform.localPosition.z);
                    }
            }

        }
            else if(state == TargetState.RedTarget){
                currMin = new Vector2(redTile.localPosition.x, redTile.localPosition.z);
            }
            else{
                currMin = new Vector2(yellowTile.localPosition.x, yellowTile.localPosition.z);
            }

            Vector2 delta = currMin - posAgentXZ;
            sensor.AddObservation(delta.x / (size * 2f));
            sensor.AddObservation(delta.y / (size * 2f));
            sensor.AddObservation(delta.magnitude / (size * 2f));

        }

   
    private void ApplyMovement(ActionBuffers actions){
        // Applies movement to the agent using continuous action inputs.
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float turnAngleY = actions.ContinuousActions[2];

        Vector3 movePos = new Vector3(moveX, 0f, moveZ).normalized;
        rb.linearVelocity = movePos * moveSpeed;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAngleY * turnAmount * Time.fixedDeltaTime, 0f));

    }

    private void HandleRewards(ActionBuffers actions){
        // Applies a small time penalty and computes reward shaping
        // based on the agent's progress toward its current target.
        AddReward(-0.002f);

        Vector2 posAgentXZ = new Vector2(transform.localPosition.x, transform.localPosition.z);

        if(state == TargetState.None){
            Collider[] hits = Physics.OverlapSphere(transform.position, size*2f, targetLayer);
            float minDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                    }
            }
            if (minDistance < float.MaxValue)
            {
                distance = minDistance;
            }
        }
        else if(state == TargetState.RedTarget){
            Vector2 posRedTileXZ = new Vector2(redTile.localPosition.x, redTile.localPosition.z);
            distance = Vector2.Distance(posAgentXZ, posRedTileXZ);
        }
        else{
            Vector2 posYellowTileXZ = new Vector2(yellowTile.localPosition.x, yellowTile.localPosition.z);
            distance = Vector2.Distance(posAgentXZ, posYellowTileXZ);
        }

        if(prevDistance != -1f){
            float delta = prevDistance - distance;
            if(delta > 0f){
                AddReward(+0.01f);
            }
            else{
                AddReward(-0.005f);
            }
        }
    
        prevDistance = distance;
    }
    public override void OnActionReceived(ActionBuffers actions){
        // Called each simulation step. 
        // Applies movement actions and calculates rewards.

        ApplyMovement(actions);
        HandleRewards(actions);


    }

    private void OnCollisionEnter(Collision collision){
        // Handles collisions with walls, agents, and targets. Rewards or penalizes appropriately.
        if (collision.gameObject.CompareTag("agent") || collision.gameObject.CompareTag("wall"))
        {
            AddReward(-5f);
        }
        if ((collision.gameObject.CompareTag("redTarget") || collision.gameObject.CompareTag("yellowTarget")) && state == TargetState.None){

            gameManager.NotifyHitTargetWareHouse(collision.gameObject);
            if(collision.gameObject.CompareTag("redTarget")){
                state = TargetState.RedTarget;
                agentRenderer.material.color = Color.red;
                Debug.Log("📦 Red Target collected successfully! +1 reward");
            }
            else{
                state = TargetState.YellowTarget;
                agentRenderer.material.color = Color.yellow;
                Debug.Log("📦 Yellow Target collected successfully! +1 reward");

            }
            AddReward(+2f);
            prevDistance = -1f;
        }
    }

    private void OnTriggerEnter(Collider other){
        // Handles trigger events when the agent drops off a target at the correct tile.
        if ((other.CompareTag("RedTile") && state == TargetState.RedTarget) || (other.CompareTag("YellowTile") && state == TargetState.YellowTarget)){
            AddReward(+4f);
            agentRenderer.material.color = defaultColor;
            gameManager.NotifyTargetReturnedWareHouse();
            string tileColor = (state == TargetState.RedTarget) ? "Red" : "Yellow";
            Debug.Log($"📬 Dropped off the package at {tileColor} Tile. +4 reward");             
            state = TargetState.None;
            prevDistance = -1f;
        }
    }
   
    public int WareHouseGetHasTarget(){
        // Returns the agent's current state as an integer:
        // 0 = None, 1 = Red, 2 = Yellow.
        // Used by the game manager to check agent progress.
        return (int)state;
    }
    

}
