# Botika ni Lolo (Grandfather's Apothecary)

Top 5, DTI Young Creatives Challenge Season 3 (Game Development Category) · Third Place, TIP Innovation Fair 2026 · Fourth Place, Best Capstone 2026

A 3D educational game about Department of Health-approved Filipino herbal medicinal plants, set inside a rural *bahay kubo*. Unity 6, C#. I built the entire Unity side; two collaborators modeled the 3D assets and environments in Blender.

You play as a grandchild looking after your lolo's *botika* (herbal apothecary). You learn which plants heal what, prepare remedies the traditional way, and record everything in a journal as you go. Five chapters, each with its own intro cutscene, objectives, and closing star rating.

| | |
|---|---|
| **Engine** | Unity 6 — 6000.1.1f1 |
| **Render pipeline** | Universal Render Pipeline (URP) |
| **Language** | C# |
| **Platform** | Windows |
| **My role** | Solo Unity developer — all code, UI, scene assembly, asset integration |
| **Art** | Two collaborators, modeled in Blender |
| **Status** | Content-complete prototype — Windows build available |

## Awards

| Award | Awarding body | Placement | Year |
|---|---|---|---|
| Young Creatives Challenge, Season 3 — Game Development Category | Philippine Department of Trade and Industry (DTI) | Top 5 | 2026 |
| TIP Innovation Fair | Technological Institute of the Philippines — Manila | Third Place | 2026 |
| Best Capstone | Technological Institute of the Philippines — College of Computer Studies (CCS), Manila and Quezon City campuses | Fourth Place | 2026 |

## My Contributions:

Every line of Unity code, the scene structure, the UI, with the integration of my collaborators' models.

**Chapter progression** — One system decides the current chapter, gates interactions by it, moves the player between scenes, and feeds the results screen. Per-scene flags would be faster to write and much worse to debug by chapter five.

**The journal** — Where player learns about the plants. `JournalManager.cs` has the data (what's been found and what's finished) and nothing about visuals. `JournalAchievementsPage` and `JournalPlantGalleryPage` each own one screen, and adding a page means a new page script and a registration and not a new branch in the manager.

**Plant identification and harvesting** — Picking up plants, examining them, recording the find in the journal.

**The chop minigame** — Runs its own input handling apart from locomotion. Player rotation locks while it's active, otherwise mouse-look keeps turning the camera and the whole view spins. Input is scoped per gameplay mode so each rule lives in one place instead of scattered per-frame checks.

**Star ratings and chapter results** — `ChapterResult.cs` scores the chapter from player performance and shows the result before the next one starts.

**NPC interaction** — Proximity detection, interaction prompt, hand-off to dialogue, and cleanup when dialogue closes, all in `NPCInteraction.cs`.

**Cutscenes** — Chapter intros and quest videos, driven by progression state so they fire at the right moment.

**UI** — Main menu, chapter select, pause panel, and the gameplay icon bar with key prompts. All prefabs, instantiated rather than copied per scene, so a styling change lands everywhere at once and scene files stay small.

**First-person controller** — Movement with a walking head-bob. Footsteps follow movement state rather than a fixed timer, so the audio matches what the player is doing.

## Layout

Scripts sit under `Assets/Scripts/Scripts/` with no nested system folders — with one person on the code, a deep hierarchy costs more in navigation than it gains in tidiness. The journal's split lives in the file names instead: `JournalManager`, `JournalAchievementsPage`, `JournalPlantGalleryPage`.

`Tutorial/` is separate because it's the content most likely to be cut or reworked, and that should happen without touching progression code.

Shared UI lives as prefabs under `Assets/Assets/GENERAL prefabs/` — pause, dialogue, key prompts, gameplay icon bar.

Scenes map one-to-one to chapters:

| Scene | What it's for |
|---|---|
| `MainMenu` | Entry point, chapter select |
| `CH1_Intro` | Chapter 1 opening |
| `PekengScene` | Chapter 1 |
| `Chapter2` | Chapter 2 |
| `Chapter3A` | Chapter 3 |
| `Chapter4A` | Chapter 4 |
| `Chapter5` | Chapter 5 |


## Running it

1. Install **Unity 6, version 6000.1.1f1**. Other versions will prompt you to upgrade the project.
2. Install Git LFS first — cutscene videos are stored through it, and a plain clone gives you empty files instead of video:

   ```
   git lfs install
   git clone https://github.com/sugurepo/botika-ni-lolo.git
   ```

3. Open the folder through Unity Hub.
4. Open `Assets/Scenes/MainMenu.unity`.
5. Press Play.

Unity will flag one missing font asset when the project opens (see below) as the fonts we used are not for public release, but it does not affect the scripts, scenes, or gameplay.

## Controls

| Action | Key |
|---|---|
| Move | W A S D |
| Look | Mouse |
| Interact | E |
| Pause | Escape |

## Build

A playable Windows build is under Releases. The local prototype archive (`Botika Ni Lolo_Prototype.zip`) is in `.gitignore` and stays out of the source tree.

## Credits and licenses

| What | Where it's from | Notes |
|---|---|---|
| 3D models, environments, visual assets | My two collaborators | Their work, modeled in Blender |
| Tree packs | `Assets/TreePackVol.1/`, `Assets/BrokenVector/` | Unity Asset Store terms |
| Animal models | `Assets/ithappy/Animals_FREE/` | Free pack, used under its terms |
| Tree shaders | `Assets/FreeTreeShaders/` | Free asset, used under its terms |
| Jungle UI | `Assets/Jungle_UI/` | Free asset, used under its terms |
| Font — DivebarVintage | Personal-use license | **Not in this repo** — the license doesn't allow redistribution |
| Font — KCObraLetraRegular | Free font | Check the bundled license file for attribution requirements |
| Footstep SFX | Freesound (author: owlstorm) | Free sound, credit the author |
| Music and remaining audio | Files under `Assets/Botika/Audio/` | Check individual sources for credit requirements |

**Note on the missing font** - One font is licensed for personal use only and stays out of this repository since I can't redistribute something I have no right to. Unity will report it as missing on open, but the build under Releases includes it so the game renders as intended when played.

## Repo layout

```
Assets/            Scenes, scripts, prefabs, art
Packages/          Package manifest and lock file
ProjectSettings/   Unity project configuration
Docs/              Technical notes
.gitattributes     Git LFS rules (*.mp4)
.gitignore         Generated folders, builds, restricted assets
```

Unity's generated folders (`Library/`, `Temp/`, `Logs/`, `Build/`, `UserSettings/`) aren't tracked — they rebuild on open. Neither are compiled builds or the local prototype archive.
