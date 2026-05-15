# CampusQuest

**CampusQuest** is a Unity-based 3D exploration game of the **Arizona State University (ASU) Polytechnic campus**.

This project uses the **Mapbox SDK for Unity** for map/location data and world generation.

## Overview

CampusQuest is designed as a virtual campus exploration experience. Players can navigate a 3D environment, explore points of interest, and experience the ASU Polytechnic campus in an interactive way.

## Tech Stack

- **Engine:** Unity
- **Primary language:** C#
- **Maps/Geo:** Mapbox SDK for Unity
- **Other:** ShaderLab (shaders), small amount of Objective-C (platform-specific / Unity-generated)

## Project Structure (Unity)

This repository follows the standard Unity project layout:

- `Assets/` — game assets and scripts (scenes, prefabs, materials, audio, C# code, etc.)
- `Packages/` — Unity package dependencies (manifest, lock file)
- `ProjectSettings/` — Unity project settings
- `Disabled_Assets/` — assets currently not used in the build (project-specific)

## Getting Started

### Prerequisites

- **Unity Hub**
- A compatible **Unity Editor** version (use the one the project was built with, if known)

> If you know the exact Unity version (e.g., `2021.3.x LTS`), add it here to make onboarding easier.

### Open the project

1. Clone the repository:
   ```bash
   git clone https://github.com/ashishsangale/CampusQuest.git
   ```
2. Open **Unity Hub** → **Add** → select the cloned `CampusQuest` folder.
3. Open the project in Unity.
4. In Unity, open a scene from `Assets/` (for example, the main scene if present).
5. Press **Play** to run in the editor.

## Mapbox Setup

Because this project uses the **Mapbox SDK for Unity**, you will typically need a **Mapbox access token**.

Common setup steps (may vary depending on how the SDK is configured in your scene):

1. Create a Mapbox account and generate an access token.
2. In Unity, set the token in the Mapbox configuration (often under **Mapbox** settings or on a Mapbox component in the scene).
3. Re-open the main scene and press **Play**.

> If you tell me where you store the token/config (Project Settings vs scene object vs environment file), I can make this section precise.

## Build / Run

- **Editor:** Press **Play** in Unity.
- **Build:** `File` → `Build Settings…` → choose target platform → **Build** (or **Build and Run**).

## Controls

Controls depend on the current scene/player-controller setup.

If you tell me which controller you’re using (e.g., First Person Controller, CharacterController, custom input system), I can add an accurate controls table.

## Media (Screenshots / Demo)

Consider adding screenshots or a short gameplay demo:

- `docs/screenshots/…`
- or a YouTube link / GitHub Release with a playable build

## Contributing

Contributions are welcome:

1. Fork the repo
2. Create a feature branch
3. Open a Pull Request

## License

No license file is currently included in the repository.

If you intend others to use/modify/distribute the project, consider adding a license (e.g., MIT, Apache-2.0, GPL-3.0, or a custom academic/non-commercial license).

## Acknowledgements

- Arizona State University Polytechnic campus inspiration/reference
- Mapbox SDK for Unity
