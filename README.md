# 🏔️ 2D Hill Climbing Racing Game

![Unity](https://img.shields.io/badge/Engine-Unity-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/Language-C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Mobile%20Style-2EA44F?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Playable%20Prototype-blue?style=for-the-badge)
![Genre](https://img.shields.io/badge/Genre-2D%20Racing-orange?style=for-the-badge)

> A 2D physics-based hill climbing racing game where players drive across endless terrain, collect coins and fuel, unlock maps, choose vehicles, spin for rewards, and try to reach the longest distance possible.

---

## 📖 Table of Contents

- [About the Game](#-about-the-game)
- [Gameplay Overview](#-gameplay-overview)
- [Core Features](#-core-features)
- [Maps](#-maps)
- [Vehicles](#-vehicles)
- [Reward Systems](#-reward-systems)
- [Controls](#-controls)
- [Technologies Used](#-technologies-used)
- [Project Structure](#-project-structure)
- [Installation](#-installation)
- [Screenshots](#-screenshots)
- [Roadmap](#-roadmap)
- [Credits](#-credits)

---

## 🎮 About the Game

**2D Hill Climbing Racing Game** is a casual side-scrolling racing game developed in Unity. The player controls a vehicle across uneven terrain, collects coins and fuel, avoids crashing, and tries to travel as far as possible.

Unlike traditional racing games, this game does not have a fixed finish line. The main goal is to survive longer, beat the previous high score, and use collected coins to unlock more maps and rewards.

The game includes multiple systems such as:

- Endless terrain gameplay
- Coin and fuel collection
- Map unlocking
- Garage and vehicle selection
- Lucky Spin rewards
- Daily Reward system
- Save system using PlayerPrefs
- Global sound toggle

---

## 🕹️ Gameplay Overview

The gameplay starts from the **Main Menu**. The player can choose to play, enter the Garage, open Lucky Spin, claim Daily Reward, or toggle sound.

To start a run, the player selects a map from the Level Select screen. If the map is unlocked, the game loads the corresponding gameplay scene. During gameplay, the player controls the vehicle, collects coins and fuel, and tries to avoid crashing.

A run ends when:

- The vehicle runs out of fuel
- The vehicle crashes seriously
- The player's head hits the terrain

After the run ends, collected coins and high score progress are saved.

---

## ✨ Core Features

### 🏁 Endless Racing Gameplay

- Physics-based 2D vehicle movement
- Uneven terrain and slope-based driving
- Endless map gameplay
- Distance and high score tracking

### 💰 Coin System

- Coins can be collected during gameplay
- Coins are saved locally
- Coins are used to unlock maps and spin Lucky Spin

### ⛽ Fuel System

- Fuel decreases during gameplay
- Fuel pickups help the player continue driving
- Running out of fuel triggers Game Over

### 🗺️ Map Unlock System

- Multiple maps are available across different Level Select pages
- Locked maps require coins to unlock
- Unlocked maps can be played anytime

### 🚗 Garage System

- Players can view available vehicles
- Locked vehicles are displayed in the Garage
- Unlocked vehicles can be selected and used in gameplay

### 🎁 Reward Systems

- Lucky Spin allows the player to spend coins for random rewards
- Daily Reward gives daily login rewards
- Special vehicles can be unlocked from reward systems

### 🔊 Audio Toggle

- Sound can be muted or unmuted from the Main Menu
- The mute state is saved using PlayerPrefs
- The speaker icon changes based on audio state

---

## 🗺️ Maps

The game currently includes **8 maps** across 3 map selection pages.

| No. | Map | Price | Unlock Method |
|---:|---|---:|---|
| 1 | Ground | Free | Default |
| 2 | Desert | 500 Coins | Coin Purchase |
| 3 | Mars | 1,500 Coins | Coin Purchase |
| 4 | Forest | 3,000 Coins | Coin Purchase |
| 5 | Arctic | 4,500 Coins | Coin Purchase |
| 6 | Highway | 6,000 Coins | Coin Purchase |
| 7 | Moon | 8,500 Coins | Coin Purchase |
| 8 | Alien | 10,000 Coins | Coin Purchase |

### Level Select Pages

| Page | Maps |
|---|---|
| LevelSelect | Ground, Desert, Mars |
| LevelSelect2 | Forest, Arctic, Highway |
| LevelSelect3 | Moon, Alien |

---

## 🚘 Vehicles

The game currently includes **3 playable vehicles**.

| Vehicle | Unlock Method | Description |
|---|---|---|
| Basic Car | Default | Available from the beginning |
| F1 Car | Lucky Spin | Special reward from Lucky Spin |
| Motor | Daily Reward Day 7 | Special reward from Daily Reward |

The selected vehicle is saved and automatically loaded when entering gameplay.

---

## 🎁 Reward Systems

### Lucky Spin

Lucky Spin is a reward system where the player spends coins to spin a wheel and receive a reward.

| Reward Type | Description |
|---|---|
| Coins | Adds coins to the player's total coins |
| F1 Car | Unlocks the F1 Car |
| Duplicate F1 | Converted into 500 coins |

### Daily Reward

Daily Reward gives the player a reward for each day in a 7-day cycle.

| Day | Reward |
|---|---|
| Day 1 | 50 Coins |
| Day 2 | 100 Coins |
| Day 3 | 150 Coins |
| Day 4 | 200 Coins |
| Day 5 | 250 Coins |
| Day 6 | 300 Coins |
| Day 7 | Motor Vehicle |

If the Motor is already unlocked, the Day 7 reward is converted into coins.

---

## 🎮 Controls

| Action | Description |
|---|---|
| Gas / Accelerate | Move the vehicle forward |
| Brake | Slow down or help balance the vehicle |
| Pause | Pause the current gameplay session |
| Restart | Restart the current run |
| Menu / Home | Return to the Main Menu |

---

## 🛠️ Technologies Used

| Category | Technology |
|---|---|
| Game Engine | Unity |
| Programming Language | C# |
| Physics | Unity 2D Physics / Rigidbody2D |
| UI | Unity UI / TextMeshPro |
| Save System | PlayerPrefs |
| Audio | Unity Audio System |
| Graphics | 2D Sprites / SpriteShape |
| Version Control | Git / GitHub |

---

## 📁 Project Structure

```text
Assets/
├── Scenes/
│   ├── Menu.unity
│   ├── LevelSelect.unity
│   ├── LevelSelect2.unity
│   ├── LevelSelect3.unity
│   ├── GarageScene.unity
│   ├── SpinScene.unity
│   ├── DailyRewardScene.unity
│   ├── GroundMap.unity
│   ├── DessertMap.unity
│   ├── MarsMap.unity
│   ├── ForestMap.unity
│   ├── ArcticMap.unity
│   ├── HighwayMap.unity
│   ├── MoonMap.unity
│   └── AlienMap.unity
│
├── Scipts/
│   ├── Economy/
│   │   ├── GameSession.cs
│   │   ├── SaveSystem.cs
│   │   ├── HUDController.cs
│   │   ├── CoinPickup.cs
│   │   ├── FuelPickup.cs
│   │   └── GameOverPanel.cs
│   │
│   ├── Loadgame/
│   │   ├── MainMenuController.cs
│   │   ├── MapSelectController.cs
│   │   ├── MapSelect2Controller.cs
│   │   ├── MapSelect3Controller.cs
│   │   ├── MapCardUI.cs
│   │   ├── GaragePlaceholderController.cs
│   │   ├── LuckySpinController.cs
│   │   ├── DailyRewardController.cs
│   │   └── PauseManager.cs
│   │
│   ├── Vehicel/
│   │   ├── VehicelControl.cs
│   │   ├── SelectedVehicleLoader.cs
│   │   └── PlayerHeadCrashDetector.cs
│   │
│   ├── Maps/
│   │   ├── InfiniteTerrain.cs
│   │   └── PickupSpawnerRuntime.cs
│   │
│   └── Audio/
│       └── PersistentBackgroundMusic.cs
│
├── Prefab/
│   └── Vehicel/
│       ├── Car.prefab
│       ├── F1Car.prefab
│       └── Motor.prefab
│
├── Sprites/
├── Audio/
└── Materials/
```

> Note: Some folder names are kept as they currently exist in the project, such as `Scipts` and `Vehicel`.

---

## 🚀 Installation

### Prerequisites

Before running the project, make sure the following tools are installed:

- Unity Hub
- Unity Editor
- Visual Studio, JetBrains Rider, or VS Code with Unity support
- Git

### Clone the Repository

```bash
git clone https://github.com/your-username/your-repository-name.git
```

### Open the Project

1. Open Unity Hub.
2. Click **Add project from disk**.
3. Select the cloned project folder.
4. Open the project with the correct Unity version.

### Run the Game

1. Open the `Menu` scene.
2. Press the **Play** button in Unity Editor.
3. Start the game from the Main Menu.

---

## 📸 Screenshots

| Main Menu | Gameplay | Garage |
|---|---|---|
| Coming soon | Coming soon | Coming soon |

| Level Select | Lucky Spin | Daily Reward |
|---|---|---|
| Coming soon | Coming soon | Coming soon |

---

## 🗺️ Roadmap

### Completed

- [x] Main Menu
- [x] Level Select page 1
- [x] Level Select page 2
- [x] Level Select page 3
- [x] Multiple maps
- [x] Coin system
- [x] Fuel system
- [x] Endless terrain
- [x] Game Over system
- [x] Garage system
- [x] Vehicle selection
- [x] Lucky Spin system
- [x] Daily Reward system
- [x] Save system
- [x] Sound toggle button
---

## 🧪 Testing Checklist

| Test Case | Expected Result |
|---|---|
| Click Play | Opens Level Select |
| Buy locked map with enough coins | Map becomes unlocked |
| Buy map without enough coins | Purchase is rejected |
| Click PLAY on unlocked map | Loads correct gameplay scene |
| Select vehicle in Garage | Vehicle is saved as selected |
| Start gameplay | Selected vehicle is spawned |
| Collect coin | Coin amount increases |
| Collect fuel | Fuel amount increases |
| Fuel reaches zero | Game Over is triggered |
| Head hits terrain | Game Over is triggered |
| Spin Lucky Spin | Reward is received |
| Claim Daily Reward | Daily reward is received |
| Toggle sound | All audio is muted/unmuted |

---

## 👥 Credits

This project was developed as a Unity 2D game project.

### Assets and Resources

- Unity Engine
- Unity UI System
- TextMeshPro
- 2D sprite assets
- Audio assets
- AI-generated UI and map assets

---

## 📌 Project Status

The project is currently a playable prototype with complete core gameplay and multiple supporting systems. It includes map selection, garage, reward systems, local save data, and audio control.

---

> 🎯 Drive carefully. Collect fuel. Unlock maps. Beat your high score.