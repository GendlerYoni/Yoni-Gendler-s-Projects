using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// This agent represents a gladiator in a multi-agent arena combat simulation.
/// It handles movement, sword control, shield control, and reward shaping based on interactions.
/// </summary>

public class Gladiator1Agent : Agent
{
    private float moveSpeed = 1f;
    private float turnAmount = 200f;
    
    [SerializeField] private Gladiator1GameManager gameManager;
    [SerializeField] private Transform enemy;
    [SerializeField] private Transform enemySword;
    [SerializeField] private Transform enemyShield;
    [SerializeField] private Transform mySword;
    [SerializeField] private Transform myShield;

    private float shieldAngle = 0f;
    private float shieldRotationSpeed = 300f;
    private bool isHit = false;
    private Rigidbody shieldRb;
    private float shieldStartingRotX = -8f;
    private float shieldStartingRotZ = 50f;

    private float normSize = 4.5f;
    private float penaltyRadius  = 4f;

    private Rigidbody rb;

    public override void Initialize(){
        // Initializes references and components.
        rb = GetComponent<Rigidbody>();
        shieldRb = myShield.GetComponent<Rigidbody>();

    }

    public override void OnEpisodeBegin(){
        // Called at the start of every episode to reset state.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.Euler(0f, Random.Range(0,360), 0f);
        myShield.localRotation = Quaternion.Euler(shieldStartingRotX, Random.Range(0,360), shieldStartingRotZ);
        isHit = false;

    }

    private void AddAngleObservation(float angleDegrees, VectorSensor sensor){
        // Adds sin/cos representation of an angle (in degrees) to the observation vector.
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        sensor.AddObservation(Mathf.Sin(angleRad));
        sensor.AddObservation(Mathf.Cos(angleRad));
    }

    /// <summary>
    /// Collects agent and enemy state for the observation space.
    /// Includes rotations, positions, and relative direction.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor){
        AddAngleObservation(transform.localEulerAngles[1], sensor);

        sensor.AddObservation(transform.localPosition.x / normSize);
        sensor.AddObservation(transform.localPosition.z / normSize);

        AddAngleObservation(enemy.localEulerAngles[1], sensor);

        Vector3 deltaEnemy = enemy.localPosition - transform.localPosition;

        sensor.AddObservation(deltaEnemy.x / normSize);
        sensor.AddObservation(deltaEnemy.z / normSize);


        AddAngleObservation(myShield.localEulerAngles[1], sensor);

        AddAngleObservation(enemyShield.localEulerAngles[1], sensor);

        AddAngleObservation(mySword.localEulerAngles[1], sensor);

        AddAngleObservation(mySword.localEulerAngles[0], sensor);

        AddAngleObservation(enemySword.localEulerAngles[1], sensor);
        
        AddAngleObservation(enemySword.localEulerAngles[0], sensor);
    }

    private void HandleMovement(ActionBuffers actions){
        // Handles agent movement and rotation.
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float turnAngleY = actions.ContinuousActions[2];

        Vector3 movePos = new Vector3(moveX, 0f, moveZ).normalized;
        rb.linearVelocity = movePos * moveSpeed;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAngleY * turnAmount * Time.fixedDeltaTime, 0f));
    }

    private void HandleSword(ActionBuffers actions){
        // Handles sword rotation (X and Y axes).
        float turnAngleSwordX = actions.ContinuousActions[3];
        float turnAngleSwordY = actions.ContinuousActions[4];

        mySword.localRotation *= Quaternion.Euler(
            turnAngleSwordX * turnAmount * Time.fixedDeltaTime,
            turnAngleSwordY * turnAmount * Time.fixedDeltaTime,
            0f
        );
    }

    private void HandleShield(ActionBuffers actions){
        // Rotates and repositions the shield around the agent.
        float shieldRotationDelta = actions.ContinuousActions[5];

        shieldAngle += shieldRotationDelta * shieldRotationSpeed * Time.fixedDeltaTime;
        shieldAngle %= 360f;

        float radius = 0.8f;
        Vector3 offset = new Vector3(
            Mathf.Cos(shieldAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(shieldAngle * Mathf.Deg2Rad)
        ) * radius;

        myShield.localPosition = offset;

        Vector3 euler = myShield.localEulerAngles;
        myShield.localRotation = Quaternion.Euler(shieldStartingRotX, euler.y, shieldStartingRotZ);
    }

    private void HandleRewards(){
        // Applies time and distance penalties to encourage active engagement.
        AddReward(-0.001f);

        float distance = Vector2.Distance(
            new Vector2(enemy.localPosition.x, enemy.localPosition.z),
            new Vector2(transform.localPosition.x, transform.localPosition.z)
        );

        if (distance > penaltyRadius)
        {
            AddReward(-0.003f * (distance - penaltyRadius));
        }
    }


    public override void OnActionReceived(ActionBuffers actions){
        // Applies actions to control movement, sword, shield, and reward shaping.
        HandleMovement(actions);
        HandleSword(actions);
        HandleShield(actions);
        HandleRewards();

    }

    private void OnCollisionEnter(Collision collision){
        // Handles collision-based reward and logic such as hits and penalties.
        if (collision.gameObject == mySword.gameObject){
            AddReward(-0.1f);
        }

        if (collision.gameObject == enemySword.gameObject && !isHit){
            AddReward(-2f);
            Debug.Log($"⚔️ {gameObject.name} was struck by {collision.gameObject.name}! A devastating blow in the arena!");
            gameManager.GladiatorSwordHit();
        }

        if (collision.gameObject == enemyShield.gameObject){
            AddReward(-0.2f);
            gameManager.GladiatorShieldHit();
        }

        if (collision.gameObject.CompareTag("wall")){
            AddReward(-0.25f);
        }
    }

    public void GladiatorIsHitTrue(){
        // Called when the shield is being hit.
        isHit = true;
    }

    public void GladiatorIsHitFalse(){
        // Called when the shield isn't being hit anymore.
        isHit = false;
    }
}
