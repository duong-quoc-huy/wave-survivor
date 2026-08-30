# WaveSurvivor - 2D Action Roguelike

**WaveSurvivor** is a 2D top-down action roguelike built with **Unity 6 (6000.0.11f1)** and **C#**, using Unity's **New Input System**.

Players navigate arena stages, battle swarming monster waves, collect experience and gold, manage character skill cooldowns, and upgrade persistent stats across multiple stage runs.

---

## Table of Contents

- [Technical Overview & Architecture](#technical-overview--architecture)
- [Core Mathematical Models](#core-mathematical-models)
  - [Damage Mitigation Formula](#1-damage-mitigation-formula)
  - [Auto-Target Acquisition](#2-auto-target-acquisition)
- [Controls & Platform Adaptation](#controls--platform-adaptation)
- [Persistent Save Architecture](#persistent-save-architecture)
- [External Cheat Trainer & Testing Tool](#external-cheat-trainer--testing-tool)
- [Build & Installation](#build--installation)
  - [Windows Standalone](#windows-standalone-exe)
  - [Android APK](#android-apk)

---

## Technical Overview & Architecture

The project architecture separates **game state management**, **combat resolution**, **persistent storage**, and **platform-specific UI binding** into modular components.

| Component | Class File | Key Responsibilities |
|---|---|---|
| **Player Health** | `PlayerHealth.cs` | Manages player hitpoints, invincibility frames, healing, and damage reduction shield logic. |
| **Player Stats** | `PlayerStats.cs` | Tracks experience scaling, level progression, base stats, and temporary potion multipliers. |
| **Combat & AI** | `EnemyController.cs` | Handles enemy movement toward the player, contact damage, and stun states. |
| **Drops & Progression** | `EnemyHealth.cs` | Handles entity health, damage registration, and instantiates XP orbs and gold coins when an enemy dies. |
| **Save System** | `LocalSaveSystem.cs` | Serializes gold, stage progress, equipped loadouts, and skill tree levels to JSON. |
| **Abilities & Cutscenes** | `PlayerAbilities.cs` | Executes active character skills, manages cooldown timers, video cutscenes, and potion usage. |
| **UI Coordination** | `HUDController.cs` | Binds runtime player metrics to HUD elements, health bars, timer counters, and mobile skill buttons. |
| **Inventory System** | `InventoryUI.cs` | Manages consumable items, dynamic item grid generation, and inventory toggling. |

---

## Core Mathematical Models

### 1. Damage Mitigation Formula

Incoming player damage is calculated dynamically during damage mitigation states:

$$
D_{\text{received}} = \max\left(0, D_{\text{contact}}(1-r)\right)
$$

Where:

- $D_{\text{received}}$ — Net damage subtracted from `currentHealth`.
- $D_{\text{contact}}$ — Base contact damage dealt by the colliding enemy.
- $r$ — Active damage reduction ratio. For example, $r = 0.50$ represents **50% damage mitigation**.

This formula ensures that the resulting damage cannot be lower than zero.

---

### 2. Auto-Target Acquisition

Projectiles and homing weapon logic resolve targets using a distance-constrained nearest-neighbor selection:

$$
i^* = \arg\min_i \left\|\mathbf{e}_i-\mathbf{p}\right\|
\quad
\text{subject to}
\quad
\left\|\mathbf{e}_i-\mathbf{p}\right\| \le R
$$

Where:

- $\mathbf{p}$ — Vector position of the player instance.
- $\mathbf{e}_i$ — Vector position of active enemy instance $i$.
- $R$ — Maximum target detection radius.
- $i^*$ — Index of the nearest valid enemy within the detection radius.

---

## Controls & Platform Adaptation

The project supports both **Windows Standalone** and **Android** through platform-specific input handling.

| Action | PC Keyboard | Android Touch Control |
|---|---|---|
| **Movement** | WASD / Arrow Keys | Virtual on-screen joystick |
| **Skill 1 (E Skill)** | `E` | `ESkillSlot` HUD Button (`OnESkillPressed`) |
| **Skill 2 (Q Skill)** | `Q` | `QSkillSlot` HUD Button (`OnQSkillPressed`) |
| **Attack Potion** | `1` | Inventory Use Button (`OnAttackPotionPressed`) |
| **Speed Potion** | `2` | Inventory Use Button (`OnSpeedPotionPressed`) |
| **Open Inventory** | `B` | HUD Bag Button (`ToggleInventory`) |
| **Pause Game** | `Esc` | HUD Pause Button |

---

## Persistent Save Architecture

Game progress is automatically persisted to disk.

### Windows

```text
%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\wave_survivor_save.json
```

### Android

```text
/data/data/com.Company.WaveSurvivor/files/
```

### Save Data Format

The save data is stored in JSON format:

```json
{
    "saveVersion": 2,
    "gold": 9999,
    "highestUnlockedStage": 5,
    "unlockedCharacterIds": [0],
    "stages": [
        {
            "stageId": 1,
            "completed": true,
            "bestSurvivalTime": 300.0,
            "highestLevelReached": 12,
            "bossDefeated": true
        }
    ]
}
```

### Progression Systems

#### Stage Yield Decay

Gold yields decrease on repeated stage clears according to the following multiplier:

$$
M = \max(0.2,\;1.0-(\text{clears}\times0.2))
$$

The multiplier decreases by `0.2` for each repeated clear until reaching the minimum value of `0.2`.

#### Skill Tree

Persistent skill-tree upgrades increase character base statistics, including:

- Base damage output
- Movement velocity
- Other persistent character attributes

These upgrades remain available across subsequent stage runs.

---

## External Cheat Trainer & Testing Tool

The project includes an external Python testing tool, `wave_survivor_trainer.py`, which provides offline save editing and live process-memory manipulation using the [`pymem`](https://github.com/giampaolo/psutil) library.

> **Note:** This tool is intended for offline development and testing of the WaveSurvivor project. It is not part of the normal gameplay system.

### Key Features

#### Save File Editor

The trainer can locate the game's save file under `AppData\LocalLow` and modify:

- Gold amount
- Stage unlock status
- Other persistent progression values

#### Live Memory Manipulation

The trainer can attach to `WaveSurvivor.exe` using its process ID and modify runtime values such as:

- Player health
- Experience counters
- Damage multipliers

#### Enemy Controls

The trainer can modify runtime enemy values for testing purposes, including:

- Enemy movement velocity
- Enemy health
- Enemy behavior

These features can be used to test gameplay mechanics under controlled conditions.

### Running the Trainer

Install the required Python dependency:

```bash
pip install pymem
```

Then run:

```bash
python wave_survivor_trainer.py
```

---

## Build & Installation

### Windows Standalone (.exe)

1. Open the project using **Unity 6 (6000.0.11f1)**.
2. Open **File → Build Settings**.
3. Select **Windows** as the target platform.
4. Select the appropriate architecture, such as **x86_64**.
5. Click **Build and Run**.
6. Select an output directory and build the executable.

### Android APK (.apk)

1. Open **File → Build Settings**.
2. Switch the active platform to **Android**.
3. Navigate to **Project Settings → Player → Other Settings**.
4. Ensure **Active Input Handling** is set to:
   - `Input System Package (New)`, or
   - `Both`
5. Configure the required Android settings.
6. Select an appropriate graphics API, such as:
   - Vulkan
   - OpenGLES3
7. Connect an Android device if required.
8. Click **Build** or **Build and Run** to generate the APK.

---

## Requirements

### Development

- **Unity:** 6000.0.11f1
- **Language:** C#
- **Input:** Unity Input System
- **Target Platforms:** Windows / Android

### Testing Tool

- **Python 3.x**
- **pymem**

---

## Project Structure

A typical project structure is organized around the major gameplay systems:

```text
WaveSurvivor/
├── Assets/
│   ├── Scripts/
│   │   ├── PlayerHealth.cs
│   │   ├── PlayerStats.cs
│   │   ├── PlayerAbilities.cs
│   │   ├── EnemyController.cs
│   │   ├── EnemyHealth.cs
│   │   ├── LocalSaveSystem.cs
│   │   ├── HUDController.cs
│   │   └── InventoryUI.cs
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Materials/
│   └── ...
├── wave_survivor_trainer.py
└── README.md
```

---

## License

This project was developed for educational and academic purposes.

The materials used in this project were selected from sources that permit their use under their respective licenses or usage terms. Third-party materials remain subject to the licenses and terms provided by their original authors or distributors.

### Music Attribution

The following music tracks are used in the project:

1. **bg_music_1**

   * **Track:** Lost Sky - Where We Started (feat. Jex) [NCS Release]
   * **Music provided by:** NoCopyrightSounds
   * **Source:** [Lost Sky - Where We Started (feat. Jex) | Melodic Dubstep | NCS - Copyright Free Music](https://youtu.be/U9pGr6KMdyg)

2. **bg_music_2**

   * **Track:** Janji - Heroes Tonight (feat. Johnning) [NCS Release]
   * **Music provided by:** NoCopyrightSounds
   * **Source:** [Janji - Heroes Tonight (feat. Johnning) | Progressive House | NCS - Copyright Free Music](https://youtu.be/3nQNiWdeH2Q)

3. **bg_music_3**

   * **Track:** Lost Sky - Dreams pt. II (feat. Sara Skinner) [NCS Release]
   * **Music provided by:** NoCopyrightSounds
   * **Source:** [Lost Sky - Dreams pt. II (feat. Sara Skinner) | Trap | NCS - Copyright Free Music](https://youtu.be/L7kF4MXXCoA)

4. **bg_music_4**

   * **Track:** Jim Yosef - Link [NCS Release]
   * **Music provided by:** NoCopyrightSounds
   * **Source:** [Jim Yosef - Link | House | NCS - Copyright Free Music](https://youtu.be/9iHM6X6uUH8)

5. **bg_music_5**

   * **Track:** Vanze - Forever (feat. Brenton Mattheus) [NCS Release]
   * **Music provided by:** NoCopyrightSounds
   * **Source:** [Vanze - Forever (feat. Brenton Mattheus) | Progressive House | NCS - Copyright Free Music](https://youtu.be/RX7fZ5I709Y)

6. **bg_music_6**

   * **Track:** Different Heaven & EH!DE - My Heart [NCS Release]
   * **Music provided by:** NoCopyrightSounds
   * **Source:** [Different Heaven & EH!DE - My Heart | Drumstep | NCS - Copyright Free Music](https://youtu.be/jK2aIUmmdP4)

### Image and Sprite Assets

The following third-party image and sprite assets are used in the project:

1. **UI Pack - Pixel Adventure**
   [Kenney - UI Pack: Pixel Adventure](https://kenney.nl/assets/ui-pack-pixel-adventure)

2. **Tiny Dungeon**
   [Kenney - Tiny Dungeon](https://kenney.nl/assets/tiny-dungeon)

3. **Game Icons**
   [Game-icons.net](https://game-icons.net/)

### Third-Party Materials

All third-party materials listed above belong to their respective creators and organizations. Their use in this project does not imply ownership or authorship by the WaveSurvivor development team.
