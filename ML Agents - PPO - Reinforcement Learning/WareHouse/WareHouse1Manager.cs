using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

/// <summary>
/// Manages the Warehouse environment by spawning agents, walls, and targets.
/// Controls episode lifecycle and communicates with agents.
/// </summary>
/// 
public class WareHouse1Manager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform redTile;
    [SerializeField] private Transform yellowTile;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wareHouseAgent;
    [SerializeField] private GameObject redTargetPrefab;
    [SerializeField] private GameObject yellowTargetPrefab;

    private List<GameObject> spawnedWalls = new List<GameObject>();
    private List<GameObject> spawnedAgents = new List<GameObject>();
    private List<GameObject> spawnedTargets = new List<GameObject>();


    [Header("Settings")]
    private int wallsNum = 4;
    private float size = 9.5f;
    private int agentsNum = 4;
    private int targetsNum = 16;
    private float padding = 1.5f;


    private int steps;
    private int currTargets;
    private int targetsReturned;
    private int maxSteps = 1500;

    void Start(){
        // Initializes the agents and starts the first episode.
        for (int i = 0; i < agentsNum; i++)
        {

            GameObject newAgent = Instantiate(wareHouseAgent, transform); 
            spawnedAgents.Add(newAgent); 

        }
        StartNewEpisode();
    }
    private Vector3 GetValidRandomPosition(float y = 0.5f) {
        // Returns a random valid position, not too close to the tiles.
        Vector3 pos;
        do {
            pos = new Vector3(Random.Range(-size, size), y, Random.Range(-size, size));
        } while (
            Vector3.Distance(pos, redTile.localPosition) < padding ||
            Vector3.Distance(pos, yellowTile.localPosition) < padding);
        return pos;
    }

    private void StartNewEpisode(){
        // Starts a new episode by resetting tiles, walls, agents, and targets.
        redTile.localPosition = GetValidRandomPosition(0.0005f);
        yellowTile.localPosition = GetValidRandomPosition(0.0005f);


        RestartWalls();
        RestartAgents();
        RestartTargets();

        steps = 0;
        currTargets = 0;
        targetsReturned = 0;
    }

    private void RestartAgents(){
        // Ends and reactivates all agents for a new episode.
        foreach (GameObject agent in spawnedAgents)
        {
            agent.GetComponent<WareHouse1Agent>().EndEpisode();
            agent.SetActive(true);
        }
    }

    private void RestartWalls(){
        // Destroys all current walls and spawns new ones at valid positions.
        foreach (GameObject wall in spawnedWalls)
        {
            Destroy(wall);
        }
        spawnedWalls.Clear(); 

        for (int i = 0; i < wallsNum; i++)
        {
            Vector3 localPos = GetValidRandomPosition(0.5f);


            GameObject newWall = Instantiate(wallPrefab, transform); 
            newWall.transform.localPosition = localPos;
            newWall.transform.localRotation = Quaternion.identity;

            spawnedWalls.Add(newWall); 
        }
    }

    private void RestartTargets(){
        // Destroys all current targets and spawns new ones, randomly red or yellow.
        foreach (GameObject target in spawnedTargets)
        {
            Destroy(target);
        }
        spawnedTargets.Clear(); 

        for (int i = 0; i < targetsNum; i++)
        {
            Vector3 localPos = GetValidRandomPosition(0.5f);


            GameObject newTarget;
            if(Random.Range(0,2) == 0){ // 50% red and 50% yellow to spawn.
                newTarget = Instantiate(redTargetPrefab, transform); 
            }
            else{
                newTarget = Instantiate(yellowTargetPrefab, transform); 
            }
            newTarget.transform.localPosition = localPos;
            newTarget.transform.localRotation = Quaternion.identity;

            spawnedTargets.Add(newTarget); 
        }
    }

    private void DisableAgent(GameObject agent) {
        // Disables an agent by ending its episode and moving it far away.
        agent.GetComponent<WareHouse1Agent>().EndEpisode();
        agent.SetActive(false);
        agent.transform.localPosition = new Vector3(999, 1000f, 999);
    }
    void FixedUpdate(){
        // Runs every physics step; tracks episode duration and resets when needed.
        steps++;

        if (currTargets >= targetsNum) {
            foreach (GameObject agent in spawnedAgents) {
                if (agent.GetComponent<WareHouse1Agent>().WareHouseGetHasTarget() == 0 && agent.activeSelf) {
                    DisableAgent(agent);
                }
            }
        }


        if(targetsReturned >= targetsNum || steps >= maxSteps){
            StartNewEpisode();
        }
    }

    public void NotifyHitTargetWareHouse(GameObject hitTarget){
        // Called when an agent collects a target.
        Destroy(hitTarget);
        currTargets++;
    }

    public void NotifyTargetReturnedWareHouse(){
        // Called when an agent successfully returns a target to a tile.
        targetsReturned++;

    }

    public Transform GetRedTileWareHouse(){
        // Returns the red tile's transform.
        return redTile;
    }

    public Transform GetYellowTileWareHouse(){
        // Returns the yellow tile's transform.
        return yellowTile;
    }

}
