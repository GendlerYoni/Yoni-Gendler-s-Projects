using UnityEngine;

/// <summary>
/// Manages episode flow for the Gladiators environment.
/// Controls agent resets, scoring logic, and episode duration.
/// </summary>

public class Gladiator1GameManager : MonoBehaviour
{
    [SerializeField] private Transform gladiator1;
    [SerializeField] private Transform gladiator2;

    private Gladiator1Agent gladiator1Agent;
    private Gladiator1Agent gladiator2Agent;

    private int steps;
    private int maxSteps = 2000;

    private float size = 4f;
    private float xOffset = 1f;

    void Start()
    {
        // Cache references to both agents
        gladiator1Agent = gladiator1.GetComponent<Gladiator1Agent>();
        gladiator2Agent = gladiator2.GetComponent<Gladiator1Agent>();
        StartNewEpisode();
    }

    private void FixedUpdate(){
        if (steps >= maxSteps){
            StartNewEpisode();
        }
        steps++;
    }    

    private void StartNewEpisode(){
        // Resets agent positions, step counter, and starts a new episode.
        gladiator1.localPosition = new Vector3(Random.Range(-size,size),0.5f,Random.Range(xOffset,size));
        gladiator2.localPosition = new Vector3(Random.Range(-size,size),0.5f,Random.Range(-xOffset,-size));

        steps = 0;

        gladiator1Agent.EndEpisode();
        gladiator2Agent.EndEpisode();
    }
        
    public void GladiatorSwordHit(){
        // Called when a successful sword hit occurs and call a function to start a new episode.
        gladiator1Agent.AddReward(1f);
        gladiator2Agent.AddReward(1f);
        StartNewEpisode();
    }

    public void GladiatorShieldHit(){
        // Called when a gladiator successfully blocks with a shield.
        gladiator1Agent.AddReward(0.1f);
        gladiator2Agent.AddReward(0.1f);
    }
}
