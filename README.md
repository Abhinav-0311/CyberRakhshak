# CyberRakshak: The Ethical Hacker

CyberRakshak is a Unity-based capstone prototype for teaching cybersecurity awareness through playable ethical-hacking scenarios.

The project is designed as a low-poly 3D cyber-infiltration platformer where Adi, a cybersecurity intern, learns safe and ethical security thinking through missions, consequences, and guidance from PATCH, a companion assistant. The current prototype direction focuses on a Windows PC build with two review/demo levels: a Firewall training level and a Phishing Maze level.

## Short Description

CyberRakshak converts cybersecurity concepts into gameplay mechanics. Firewalls become physical barriers, phishing becomes deceptive routes, wrong interactions create visible consequences, and PATCH explains the lesson through short in-game dialogue.

## Team

- Abhinav Jain
- Kush Sharma
- Aniket Singh Rawat
- Parush Nimje
- Priyanshu Jain

Supervisor: Dr. Ramraj Dangi

## Project Setup

Open this folder in Unity Hub as an existing project:

```text
E:\College\Project\Capstone
```

Recommended Unity version:

```text
Unity 6000.2.7f2
```

Recommended starting workflow:

1. Open the project in Unity Hub.
2. Keep all project-owned assets under `Assets/_CyberRakshak`.
3. Let Unity regenerate local folders such as `Library`, `Temp`, `Logs`, and `UserSettings`.
4. Start with the Windows PC prototype before expanding to Android AR or optional VR.

## Folder Map

- `Assets/_CyberRakshak/Scenes` - Unity scenes.
- `Assets/_CyberRakshak/Scripts` - C# scripts grouped by gameplay area.
- `Assets/_CyberRakshak/Prefabs` - reusable Unity prefabs.
- `Assets/_CyberRakshak/Art` - source art and imported visual assets.
- `Assets/_CyberRakshak/Audio` - music, sound effects, and voice assets.
- `Assets/_CyberRakshak/ScriptableObjects` - data assets for levels, tasks, choices, scoring, and content.
- `Assets/_CyberRakshak/Tests` - Unity edit mode and play mode tests.
- `Docs` - GDD, research notes, sprint notes, and implementation planning.

## Current Build Target

Current prototype target:

- Windows PC build.
- Third-person follow camera.
- Adi as the playable character.
- PATCH companion fixed near Adi's shoulder/head.
- Text-only PATCH dialogue.
- Firewall training level.
- Phishing maze level.
- Wrong interaction tracking.
- Basic rating/result feedback.

## Planned Prototype Levels

### Level 1: Firewall Training

Demonstrates firewall/access-control concepts through blocked routes, safe access paths, and interactable switches or tokens.

### Level 2: Phishing Maze

Demonstrates phishing recognition through fake doors, suspicious URLs, urgency traps, and wrong-route consequences.

## Core Systems

- Player controller
- Third-person follow camera
- PATCH companion follow behavior
- PATCH dialogue UI
- Trigger-based learning prompts
- Firewall hazards
- Phishing choice triggers
- Checkpoint/respawn support
- Prototype run statistics
- Ambient background audio controller

## Scope Notes

The prototype intentionally avoids real hacking, real networks, real credentials, and offensive tooling. Cybersecurity concepts are represented through fictional, safe, game-based mechanics.
