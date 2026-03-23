
# Game Design Document: Legend of Blood (Unity Migration)

## 1. Overview

This document outlines the detailed game design for "Legend of Blood" and serves as a blueprint for migrating the project from Cocos Creator to the Unity engine. The goal is to replicate the existing gameplay mechanics, systems, and data structures in a new, robust Unity project.

## 2. Core Systems (Managers)

These are the central singleton classes that manage the game's state and core logic. In Unity, these can be implemented as persistent GameObjects with attached scripts.

### 2.1. GameManager

*   **Role:** The central hub and service locator for all other systems.
*   **Unity Implementation:** A persistent singleton (`DontDestroyOnLoad`) that holds public references to all other manager/system instances.
*   **Systems:**
    *   `UIManager`
    *   `CombatSystem`
    *   `BuildingSystem`
    *   `EvolutionSystem`
    *   `HospitalSystem`
    *   `BreedingSystem`
    *   `MaturationSystem`
    *   `InventoryManager`

### 2.2. DataManager

*   **Role:** Responsible for loading and providing access to all game data, both configuration data (traits, skills) and player state (heroes, buildings).
*   **Unity Implementation:** A singleton that loads data from ScriptableObjects or JSON files at startup.
*   **Data:**
    *   `heroes`: `List<HeroData>` - Player's current heroes.
    *   `buildings`: `List<Building>` - Player's current buildings.
    *   `expTable`: `Dictionary<int, int>` - Experience needed for each level.
    *   `traits`: `Dictionary<string, Trait>` - All available traits, keyed by ID.
    *   `skills`: `Dictionary<string, Skill>` - All available skills, keyed by ID.
    *   `bosses`: `Dictionary<string, Boss>` - All available bosses, keyed by ID.
    *   `evolutionData`: Data structure for profession evolution.

### 2.3. EventManager

*   **Role:** A global event bus for decoupled communication between different game systems.
*   **Unity Implementation:** A static class with `Action<...>` delegates or a dedicated event system from the Unity Asset Store.
*   **Events:**
    *   `EXPEDITION_STARTED`
    *   `EXPEDITION_RETURNING`
    *   `EXPEDITION_FINISHED`
    *   `HERO_LIST_CHANGED`
    *   `RESOURCES_CHANGED`

### 2.4. InventoryManager

*   **Role:** Manages the player's resources (Gold, Wood, Stone) and items.
*   **Unity Implementation:** A component on the GameManager that handles resource logic.
*   **Resources:**
    *   `Gold`
    *   `Wood`
    *   `Stone`

### 2.5. ExpeditionManager

*   **Role:** Manages active expeditions, including their state, timing, and rewards.
*   **Unity Implementation:** A component on the GameManager that runs an `Update` loop to check on expedition progress.
*   **Logic:**
    *   Starts expeditions.
    *   Tracks expedition status (`Traveling`, `Exploring`, `Returning`, `Finished`).
    *   Advances expedition state based on timers.
    *   Finalizes expeditions and distributes rewards.

## 3. Game Systems

These systems encapsulate specific gameplay mechanics. In Unity, they will be components attached to the GameManager or their own GameObjects.

### 3.1. CombatSystem

*   **Role:** Simulates combat between a squad of heroes and a destination (e.g., a boss).
*   **Unity Implementation:** A class that can be called by the `ExpeditionManager` to simulate background combat.

### 3.2. BuildingSystem

*   **Role:** Manages the construction and upgrading of buildings.
*   **Unity Implementation:** A class that handles building-related logic.

### 3.3. EvolutionSystem

*   **Role:** Handles the evolution of hero professions based on level milestones.
*   **Unity Implementation:** A class that checks for and applies profession evolutions.

### 3.4. HospitalSystem

*   **Role:** Manages the healing of injured heroes.
*   **Unity Implementation:** A class that handles hero injury and recovery.

### 3.5. BreedingSystem

*   **Role:** Manages the breeding of heroes to create new offspring.
*   **Unity Implementation:** A class that handles the logic for hero breeding.

### 3.6. MaturationSystem

*   **Role:** Manages the maturation process of young heroes.
*   **Unity Implementation:** A class that handles the maturation timer and state changes.

## 4. Data Structures

These are the primary data classes for the game. In Unity, these will be implemented as C# classes, possibly using `[System.Serializable]` to be edited in the Inspector or used with ScriptableObjects.

### 4.1. HeroData

*   `id`: `string`
*   `name`: `string`
*   `gender`: `Gender` (enum)
*   `level`: `int`
*   `experience`: `int`
*   `potential`: `int`
*   `baseStats`: `HeroStats` (class with `hp`, `atk`, `def`, `spd`)
*   `currentHp`: `int`
*   `traits`: `List<Trait>`
*   `skills`: `List<Skill>`
*   `profession`: `Profession` (enum)
*   `isSeverelyInjured`, `isLightlyInjured`, `isMature`, `isBusy`: `bool`
*   `injuryEndTime`, `lightInjuryEndTime`, `maturationEndTime`: `long` (timestamps)

### 4.2. BossData

*   `id`: `string`
*   `name`: `string`
*   `map`: `string`
*   `cp`: `int`
*   `stats`: `HeroStats`
*   `skills`: (class with `normal` and `aoe` skill descriptions)
*   `mechanic`: (class with `name` and `description`)

### 4.3. Building

*   `id`: `string`
*   `type`: `BuildingType` (enum)
*   `level`: `int`
*   `isUnderConstruction`: `bool`
*   `constructionEndTime`: `long`

### 4.4. Skill

*   `id`: `string`
*   `name`: `string`
*   `description`: `string`

### 4.5. Trait

*   `id`: `string`
*   `name`: `string`
*   `description`: `string`
*   `effects`: `List<TraitEffect>`

### 4.6. Expedition

*   `id`: `string`
*   `squad`: `List<HeroData>`
*   `destination`: `POIData`
*   `status`: `ExpeditionStatus` (enum)
*   `startTime`, `endTime`: `long`

## 5. Game Data (Configuration)

This is the static data that defines the game's content. In Unity, this data should be stored in ScriptableObjects for easy editing by designers.

*   **AllSkills:** A list of all available `Skill` objects.
*   **AllTraits:** A list of all available `Trait` objects.
*   **AllBosses:** A list of all available `Boss` objects.
*   **ExpTableData:** The experience table.
*   **ProfessionEvolutionData:** The data for profession evolution.

## 6. Unity Implementation Strategy

### 6.1. Project Structure

*   `Assets/Scripts/Core`: For the core manager classes.
*   `Assets/Scripts/Systems`: For the gameplay system classes.
*   `Assets/Scripts/Data`: For the data structure classes.
*   `Assets/ScriptableObjects`: For the game data assets.
*   `Assets/Prefabs`: For UI elements, heroes, etc.
*   `Assets/Scenes`: For the main game scene.

### 6.2. Mapping Cocos to Unity

*   **Nodes/Components:** Cocos Nodes with attached scripts map directly to Unity GameObjects with attached C# scripts.
*   **Prefabs:** Cocos prefabs can be recreated as Unity prefabs.
*   **`@property`:** Cocos `@property` decorators for assigning references in the editor are equivalent to `[SerializeField]` in C# for private fields, or public fields.
*   **Singletons:** The singleton pattern used in the Cocos project can be replicated in Unity.
*   **`director.addPersistRootNode(this.node)`:** This is equivalent to `DontDestroyOnLoad(this.gameObject)` in Unity.

### 6.3. UI Implementation

*   The UI can be rebuilt using Unity's UGUI system (Canvas, Panels, Buttons, Text, etc.).
*   Create prefabs for reusable UI elements like hero portraits, stat displays, and list items.
*   The `UIManager` will manage showing, hiding, and updating different UI panels.

