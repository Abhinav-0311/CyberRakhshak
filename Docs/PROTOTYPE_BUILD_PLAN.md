# CyberRakshak Prototype Build Plan

## Goal

Build a short review-ready prototype with two playable levels that show the core promise of CyberRakshak without overbuilding the full game.

## Prototype Story

The player enters a training environment with PATCH fixed above the shoulder. PATCH explains cyber concepts through short dialogue boxes while the player navigates cyber-themed obstacles.

Use the player name as a presentation label only. The final GDD player is Adi, but the prototype can use a neutral `Player` object so renaming is easy later.

## Level 1: Firewall Training

Purpose: teach that firewalls control safe and unsafe traffic paths.

Scene idea:

- Low-poly digital corridor.
- Red/orange firewall gates.
- Some gates are blocked hazards.
- One safe route is opened by finding a blue access key or reaching a safe terminal.
- PATCH explains: "A firewall is not just a wall. It decides what traffic is allowed through."

Core mechanics:

- Player movement and jumping.
- Firewall trigger hazards.
- Checkpoint respawn.
- PATCH dialogue trigger near each lesson point.
- Goal portal at the end.

Review value:

- Easy to understand visually.
- Shows cybersecurity concept as gameplay.
- Shows PATCH teaching without lecture slides.

## Level 2: Phishing Maze

Purpose: teach that phishing uses fake paths, urgency, and deceptive labels.

Scene idea:

- Maze with multiple doors/routes.
- Some signs are fake: "Free Reward", "Urgent Login", "Verify Now".
- Safe signs use calmer wording and URL-check hints.
- Wrong route triggers a popup/penalty and PATCH warning.
- Correct route reaches the secure exit.

Core mechanics:

- Phishing choice triggers.
- Wrong interaction count.
- PATCH warning dialogue.
- Optional score/rating at end.

Review value:

- Directly maps phishing to deceptive navigation.
- Easy for non-technical reviewers.
- Demonstrates learning through consequence.

## Required Scene Objects

- `Player`
  - `CharacterController`
  - `PrototypePlayerController`
  - Tag: `Player`
- `Main Camera`
  - `SimpleFollowCamera`
- `PATCH`
  - Meshy model or placeholder
  - `PatchCompanion`
  - Target: `Player`
- `PATCH Dialogue UI`
  - Canvas
  - CanvasGroup
  - Speaker TMP text
  - Body TMP text
  - `PatchDialoguePresenter`
- `AmbientMusic`
  - `AudioSource`
  - `AmbientMusicController`
- `LevelManager`
  - `PrototypeLevelManager`

## Build Order

1. Create both scenes.
2. Add player placeholder and camera.
3. Add PATCH over shoulder.
4. Add dialogue UI.
5. Build Level 1 firewall corridor.
6. Build Level 2 phishing maze.
7. Add goal triggers.
8. Add simple review build.

