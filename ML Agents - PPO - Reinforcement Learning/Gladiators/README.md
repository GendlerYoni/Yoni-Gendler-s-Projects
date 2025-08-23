# Gladiators - Multi Agent Arena Combat (Unity + ML Agents)
Self-play PPO agents learning **sword & shield duels** in Unity ML-Agents.  
Designed, built, and trained **entirely from scratch**.

Two Unity ML-Agents gladiators learn to duel through **PPO self-play**.  
I trained **two separate behaviors** that differ only in **network depth**:  
- **3 hidden layers** × **512 units**  
- **2 hidden layers** × **512 units**

Training was divided into **two phases**:  
1. **Sword only** – agents learned basic offensive strategies.  
2. **Sword + Shield** – added a shield mechanic and **new reward shaping** to encourage defensive play.

This repository highlights the **design decisions** (observations, actions, rewards) and showcases the **training results**.


## Demo

| Baseline → Trained | Sword only | With Shield |
|--------------------|-----------|-------------|
| **Before training** | ![Sword only Before](gifs%20and%20graphs/No%20shield%20before%20training.gif) | ![With Shield Before](gifs%20and%20graphs/With%20shield%20before%20training.gif) |
| **After training**  | ![Sword only After](gifs%20and%20graphs/No%20shield%20after%20training.gif)   | ![With Shield After](gifs%20and%20graphs/With%20shield%20after%20training.gif)   |


## Environment Design

Two agents spawn in an arena.
Goal: hit the opponent with a sword.
Shields can block incoming swords.
A round ends on a clean hit or MaxStep.

## Observation Space (per agent)

- Agent **yaw rotation**
- Agent **x, z position** (normalized)
- Enemy **yaw rotation**
- Relative position **Δx, Δz** (normalized)
- Both **shield rotations (y)**
- Both **sword rotations (x, y)**

## Action Space (continuous)

**6 continuous controls:**
- **Move X** (strafe)
- **Move Z** (forward/back)
- **Move Y rotation**
- **Sword X rotation**
- **Sword Y rotation**
- **Shield Orbit Δ** — increments an angle that orbits the shield around the agent at a fixed radius (~0.8) with a stable tilt; position wraps at 360°


## Reward Shaping

**Event rewards/penalties**

- Hit enemy with sword: **+1.0** *(round ends)*
- Get hit by enemy sword: **−1.0**
- Self-hit with my sword: **−0.1** *(discourage bad swings)*
- Enemy shield touches me: **−0.1**
- My shield touches enemy: **+0.1**
- Block enemy sword with my shield: **+1.0** *(explicit block encourage)*
- My sword hits enemy shield: **−0.75** *(lower than blocking to encourage aggressiveness)*

**Shaping for tempo & proximity**

- Step penalty: **−0.001** per step to encourage finishing.
- Proximity shaping: if distance to enemy `> R`, add **−0.003 × (distance − R)** to keep agents engaged.



## Termination

- On hit (successful sword strike)  
- MaxStep reached  

## Results & Notes

The analysis shows that the agent trained with three hidden layers consistently outperformed the agent with two hidden layers. Across all runs, its cumulative reward remained higher, indicating superior learning and strategy development. In practice, this means the deeper network converged on more effective behaviors and reliably won the duels against the shallower model.

Shield behavior: Agents tended to rotate the shield around themselves instead of learning precise block control.

## PPO Network Architecture

The agents were trained using two different network configurations:

- **512 hidden units** → In early tests, a width of 256 led to unstable training.  
  Increasing to 512 hidden units provided more stable learning, which was important due to the complex interactions (sword, shield, proximity).  

- **2 vs 3 hidden layers** → I trained two separate behaviors: one with 2 layers, one with 3 layers.  
  This allowed me to compare how additional depth affects performance.  

**Result:** The 3-layer network consistently achieved higher cumulative rewards and outperformed the 2-layer model in duels.


## Training Graphs

These plots are taken directly from TensorBoard during training.  
They show two key metrics:

- **Cumulative Reward (y-axis)** → how well the agent is performing.  
  - Higher rewards mean the agent is landing more sword hits, blocking effectively, and avoiding penalties.  
  - Negative dips usually indicate failed strategies (e.g., bad sword swings or being hit often).


- **Episode Length / Steps (y-axis)** → how long each duel lasts.  
  - Short episodes mean quick wins/losses.  
  - Longer episodes often happen when both agents avoid mistakes, circle each other, or use the shield.


**Sword only**
<p align="center">
  <img src="gifs%20and%20graphs/Sword%20only%203m%20training.png" width="45%">
</p>

**With shields**
<p align="center">
  <img src="gifs%20and%20graphs/With%20shield%203m%20training.png" width="45%">
  <img src="gifs%20and%20graphs/With%20shield%205m%20training.png" width="45%">
</p>

<p align="center">
  <img src="gifs%20and%20graphs/With%20shield%2020m%20training.png" width="45%">
  <img src="gifs%20and%20graphs/With%20shield%207m%20training.png" width="45%">
</p>

### Final results

**Sword only**
<p align="center">
  <img src="gifs%20and%20graphs/Last%20one%20sword%20only%207.5m%20training.png" width="45%">
</p>

**With shields**
<p align="center">
  <img src="gifs%20and%20graphs/Last%20one%20with%20shield%2025m%20training.png" width="45%">
</p>



📌 **Project by Gendler Yoni**  
📧 gendler.yoni1990@gmail.com  
🔗 [LinkedIn](https://www.linkedin.com/in/yoni-gendler/) • [GitHub](https://github.com/GendlerYoni)