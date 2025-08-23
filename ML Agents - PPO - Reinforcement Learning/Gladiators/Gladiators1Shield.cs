using UnityEngine;

/// <summary>
/// Handles shield collisions between the gladiator and the opponent's sword.
/// Rewards the defending agent if the shield blocks the sword.
/// </summary>

public class Gladiators1Shield : MonoBehaviour
{

    [SerializeField] private Transform myGladiator;
    [SerializeField] private Transform enemyGladiator;
    [SerializeField] private Transform enemySword;

    private Gladiator1Agent myGladiatorAgent;
    private Gladiator1Agent enemyGladiatorAgent;

    void Start()
    {
        // Cache references to both agents
        myGladiatorAgent = myGladiator.GetComponent<Gladiator1Agent>();
        enemyGladiatorAgent = enemyGladiator.GetComponent<Gladiator1Agent>();
    }


    private void OnCollisionEnter(Collision collision){
        // If the shield blocks the enemy sword
        if (collision.gameObject == enemySword.gameObject){
            myGladiatorAgent.AddReward(1f); // Defender reward
            enemyGladiatorAgent.AddReward(-0.75f); // Attacker penalty
            myGladiatorAgent.GladiatorIsHitTrue(); // Notify the agent that the shield is being hit
        }
    }

    private void OnCollisionExit(Collision collision){
        if (collision.gameObject == enemySword.gameObject){
            myGladiatorAgent.GladiatorIsHitFalse(); // Notify the agent that the shield isn't being hit anymore
        }
    }
}
