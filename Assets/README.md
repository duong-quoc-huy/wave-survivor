# WaveSurvivor - 2D Action Roguelike

**WaveSurvivor** is a 2D top-down action roguelike built in Unity (6000.0.11f1) using C# and Unity's New Input System. Players navigate arena stages, battle swarming monster waves, collect experience and gold drops, manage character skill cooldowns, and upgrade persistent stats across multiple stage runs.

---

## Technical Overview & Architecture

The project architecture isolates game state management, combat resolution, persistent storage, and platform-specific UI binding across modular components:

| Component | Class File | Key Responsibilities |
| :--- | :--- | :--- |
| **Player Health** | `PlayerHealth.cs` | Manages player hitpoints, invincibility frames, healing, and damage reduction shield logic[cite: 1]. |
| **Player Stats** | `PlayerStats.cs` | Tracks experience scaling, level progression, base stats, and temporary potion multipliers[cite: 2]. |
| **Combat & AI** | `EnemyController.cs` | Handles enemy movement pathfinding toward player target, contact damage, and stun states[cite: 3]. |
| **Drops & Progression**| `EnemyHealth.cs` | Handles entity health, damage registration, and instantiates XP orbs and gold coins on death[cite: 4]. |
| **Save System** | `LocalSaveSystem.cs` | Serializes gold, stage progress, equipped loadouts, and skill tree levels to JSON. |
| **Abilities & Cutscenes**| `PlayerAbilities.cs` | Executes active character skills, manages cooldown timers, video cutscenes, and potion usage[cite: 7]. |
| **UI Coordination** | `HUDController.cs` | Binds runtime player metrics to HUD elements, health bars, timer counters, and mobile skill buttons[cite: 6]. |
| **Inventory System** | `InventoryUI.cs` | Manages consumable items, dynamic item grid generation, and inventory toggling[cite: 8, 9]. |

---

## Core Mathematical Models

### 1. Damage Mitigation Formula
Incoming player damage is calculated dynamically during skill mitigation states[cite: 1, 7]:

$$D_{\text{received}} = \max(0, D_{\text{contact}}(1 - r))$$

* $D_{\text{received}}$: Net damage subtracted from `currentHealth`[cite: 1].
* $D_{\text{contact}}$: Base contact damage dealt by the colliding enemy[cite: 3].
* $r$: Active damage reduction ratio (e.g., $r = 0.50$ for $50\%$ skill mitigation)[cite: 1, 7].

### 2. Auto-Target Acquisition
Projectiles and homing weapon logic resolve targets using distance-constrained nearest neighbor selection[cite: 3]:

$$i^* = \arg\min_i \|\mathbf{e}_i - \mathbf{p}\| \quad \text{subject to} \quad \|\mathbf{e}_i - \mathbf{p}\| \le R$$

* $\mathbf{p}$: Vector position of the player instance[cite: 3].
* $\mathbf{e}_i$: Vector position of active enemy instance $i$[cite: 3].
* $R$: Maximum target detection radius.

---

## Controls & Platform Adaptation

The project supports both Standalone Windows (Keyboard) and Android (Touch UI) using responsive input hooks[cite: 6, 7, 9].

| Action | PC Keyboard Input | Android Touch Control |
| :--- | :--- | :--- |
| **Movement** | WASD / Arrow Keys | Virtual On-Screen Joystick |
| **Skill 1 (E Skill)** | `E` Key[cite: 7] | `ESkillSlot` HUD Button (`OnESkillPressed`)[cite: 6, 7] |
| **Skill 2 (Q Skill)** | `Q` Key[cite: 7] | `QSkillSlot` HUD Button (`OnQSkillPressed`)[cite: 6, 7] |
| **Attack Potion** | `1` Key[cite: 7] | Inventory Use Button (`OnAttackPotionPressed`)[cite: 6, 7, 9] |
| **Speed Potion** | `2` Key[cite: 7] | Inventory Use Button (`OnSpeedPotionPressed`)[cite: 6, 7, 9] |
| **Open Inventory** | `B` Key[cite: 9] | HUD Bag Button (`ToggleInventory`)[cite: 9] |
| **Pause Game** | `Esc` Key | HUD Pause Button |

---

## Persistent Save Architecture

Game progress automatically persists to disk inside `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\wave_survivor_save.json` on Windows or `/data/data/com.Company.WaveSurvivor/files/` on Android.

```json
{
    "saveVersion": 2,
    "gold": 9999,
    "highestUnlockedStage": 5,
    "unlockedCharacterIds": [0],
    "stages": [
        { "stageId": 1, "completed": true, "bestSurvivalTime": 300.0, "highestLevelReached": 12, "bossDefeated": true }
    ]
}