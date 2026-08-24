# CyberRakshak AI Handoff Context

Hello to the next AI Agent! You are taking over the development of **CyberRakshak**, an ethical hacking Unity game built for a capstone project. Below is a comprehensive summary of the project state, recent completions, and the immediate next goals.

## 1. Project Overview & Rules
*   **Engine:** Unity (Editor Version: 6000.2.7f2).
*   **Rules:**
    *   Keep all owned assets inside `Assets/_CyberRakshak`.
    *   Use the `CyberRakshak` namespace for C#.
    *   Prefer `ScriptableObject`s for game data (tasks, dialogue, metadata).
    *   The project is heavily based on a Game Design Document (GDD) located in `Docs/`.
    *   The overarching goal is a polished vertical slice.

## 2. Recent Completions (Do not redo these)
*   **UI Overhaul (Settings):** 
    *   The Settings panel (Music, SFX, Camera Sensitivity sliders) has been completely fixed. The sliders use proportional `RectTransform` anchors to stay perfectly aligned across any resolution.
    *   The backend logic in `SettingsOverlayController.cs` correctly binds these sliders to the AudioMixer and the `ThirdPersonController` sensitivity via `PlayerPrefs`.
    *   The "Back" buttons (`SettingsBackHit`) across all scenes (`MainMenu`, `Game_Level01`, `Game_Tutorial`) have been wired up via C# script to correctly close the panel.
*   **Official Unity CLI:**
    *   The project is fully configured for the **official Unity CLI** (version `1.0.0-beta.5`).
    *   See `UNITY_CLI_WORKFLOW.md` for exact terminal commands (`unity open`, `unity doctor`, `unity test`).

## 3. Current State of Level 1 (`Game_Level01.unity`)
*   Based on `Docs/PROTOTYPE_BUILD_PLAN.md`, Level 1 is supposed to be a "Firewall Training" corridor.
*   **Current Reality:** A recent script scan of the scene revealed that the level is currently a mostly blank slate. 
    *   It contains the `PlayerArmature` (spawning at 0,0,0) and several modular environment blocks named `TrainingGround`, `TrainingGround (1)`, etc.
    *   It does **not** yet contain the actual Firewall Gates, Access Keys, or the `PATCH` companion object/dialogue triggers mentioned in the design docs.

## 4. Official Unity CLI Pipeline
Coplay/Unity MCP is intentionally not used in this project. Use the official Unity CLI for project health checks, builds, and automated tests. See `UNITY_CLI_WORKFLOW.md` for the supported commands.

For gameplay verification, prefer deterministic Unity PlayMode/EditMode tests and a documented manual smoke-test checklist. Do not add an LLM-driven in-editor playtester unless the team explicitly reprioritizes it.

## Instructions for the Next Agent
1.  **Do not modify** the existing UI sliders or the Unity CLI configurations.
2.  The immediate product task is to continue building the Level 1 Firewall corridor: gates, safe access route, PATCH feedback, checkpoint, and goal.
3.  Always check the `Docs/` folder for design constraints.
