# Warehouse – Multi Agent Package Sorting (Unity + ML Agents)
A reinforcement learning environment where four PPO agents master **efficient package delivery**.  
They learn to **locate packages**, **avoid obstacles**, and **deliver them to the correct colored zones** — developing optimal strategies in a dynamic, randomized warehouse.

The entire codebase is fully **generalized** — the number of **agents**, **packages**, and **walls** can be adjusted by changing a single parameter.  

This repository highlights the **design decisions** (observations, actions, rewards) and showcases the **training results**.


## Demo

| Baseline → Trained | Package Delivery |
|--------------------|------------------|
| **Before training** | ![Before](gifs%20and%20screenshots/Before%20training.gif) |
| **After training**  | ![After](gifs%20and%20screenshots/After%20training.gif)  |

## Environment Design

- **Agents**: 4 warehouse robots operating simultaneously.  
- **Packages**: Red and yellow targets spawn randomly each episode.  
- **Delivery zones**: Packages must be delivered to the tile of the same color.  
- **Obstacles**: Walls spawn randomly, forcing agents to adapt navigation.  
- **Episode ends** when all packages are delivered or the step limit is reached. 


## Observation Space (per agent)

**Code Observations**  
- Agent yaw rotation (sin, cos)  
- Agent x, z position (normalized)  
- Agent velocity (x, y, z) in local space  
- Relative position Δx, Δz to nearest target *(if free)*  
- Relative position Δx, Δz to delivery tile *(if carrying)*  
- Normalized distance to that target/tile  

**Ray Perception Sensors 3D**

<p align="center">
<img src="gifs%20and%20screenshots/Ray%20sensors%20settings.png" width="30%"> <br>
<img src="gifs%20and%20screenshots/Ray%20sensors%20in%20simulation.png" width="30%"> 
</p>

## Action Space (continuous)

**3 continuous controls:**
- **Move X** (strafe left/right)  
- **Move Z** (forward/back)  
- **Turn Y** (rotation)  


## Reward Shaping

**Event rewards/penalties**
- Pick up a package: **+2.0**  
- Deliver package to correct tile: **+4.0**  
- Collision with wall or agent: **−5.0**  

**Shaping for efficiency**
- Step penalty: **−0.002** per step *(encourages faster delivery)*  
- Distance shaping:  
  - Moved closer to target/tile: **+0.01**  
  - Moved further away: **−0.005**  

---

## Termination

- All packages delivered  
- MaxStep reached  

---

## Results & Notes

Analysis shows that agents learned to:
- Seek out the nearest package efficiently  
- Deliver packages to the correct tile  
- Avoid collisions with walls/other agents  

**Color change feedback:**
- Agents turn red or yellow when carrying a package  
- Reset to default color after successful delivery  

## PPO Network Architecture

The agents were trained using a **256 × 2 fully-connected network**:

- **256 hidden units** → The observation space is fairly large (agent rotation, position, velocity, Δx/Δz offsets, and ray perception inputs).  
  A wider layer ensures enough capacity to process this high-dimensional state space.  

- **2 hidden layers** → The task itself is relatively straightforward: “go to the destination (Δx, Δz) and avoid obstacles.”  
  Two layers are sufficient to capture the decision patterns without overfitting or wasting compute.  

This balance gave stable learning and efficient training, avoiding the slower convergence that can come with deeper networks.


## Training Graphs

These plots are from TensorBoard during training.  

- **Cumulative Reward** → how well the agents are performing overall.  
  - Increases as they learn to pick up and deliver packages efficiently.  
  - Noise comes from multiple agents and randomized layouts.  

- **Episode Length** → how long episodes last.  
  - Flat lines near 749 indicate agents often reach MaxStep.  
  - Dips show successful episodes where all packages were delivered before timeout.  

<p align="center">
  <img src="gifs%20and%20screenshots/50m%20training%20graph.png" width="45%">
</p>


📌 **Project by Gendler Yoni**  
📧 gendler.yoni1990@gmail.com  
🔗 [LinkedIn](https://www.linkedin.com/in/yoni-gendler/) • [GitHub](https://github.com/GendlerYoni)