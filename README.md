# Botika ni Lolo (Grandfather's Apothecary)

A 3D educational game about Filipino traditional medicine, played inside a rural *kubo* and the yard around it. Unity 6, C#. I built the whole Unity side of it myself; two friends did the 3D models and environments in Blender.

You play as a grandchild looking after your lolo's *botika* — his herbal apothecary. You learn which plants heal what, prepare remedies the traditional way, and write everything down in a journal as you go. Five chapters, each with its own intro cutscene, objectives, and a star rating at the end.

| | |
|---|---|
| **Engine** | Unity 6 — 6000.1.1f1 |
| **Render pipeline** | Universal Render Pipeline (URP) |
| **Language** | C# |
| **Platform** | Windows |
| **My role** | Solo Unity developer — all code, UI, scene assembly, asset integration |
| **Art** | Two collaborators, modeled in Blender |
| **Status** | Content-complete prototype |

## What I built

Every line of Unity code in this project is mine, along with the scene structure, the UI, and the work of getting my collaborators' models into the game and making them behave.

**Chapter progression** — One place in the code decides what chapter the player is in, which gates what they can interact with, moves them between scenes, and feeds the results screen. I kept it in one system on purpose. Spreading that state across per-scene flags is faster to write and much worse to debug by chapter five.

**The journal** — The heart of the game, since it's where the learning actually lands. `JournalManager.cs` owns the data: what the player has found, what they've finished, and nothing about how any of it looks. The achievements page and the plant gallery each handle their own screen. That split means I can add another journal page later without touching the manager, and the manager never grows a branch for every page that exists.

**Plant identification and harvesting** — Picking things up, checking what they are, and recording the find in the journal.

**The chop minigame** — Its own input handling, separate from normal walking around. Player rotation locks while it's running; otherwise mouse-look keeps the camera turning during play and the whole thing spins. Input is scoped per gameplay mode, so the rule lives in one place instead of scattered per-frame checks.

**Star ratings and chapter results** — How the player did in a chapter, shown before they move on.

**NPC interaction** — Handles spotting an NPC nearby, showing the prompt, and handing off to the dialogue UI.

**Cutscenes** — Chapter intros and quest videos, tied into the progression state so they play at the right moment.

**UI** — Main menu, chapter select, pause panel, and the gameplay icon bar with key prompts. All built as prefabs and instantiated, never duplicated per scene. Change the prompt style once and it changes everywhere. Duplicated per-scene UI is the fastest way to end up with a menu that looks different in chapter three, and prefabs make that impossible.

**First-person controller** — Movement with a walking head-bob and footsteps, driven by movement state rather than a fixed timer so the audio matches what the player is actually doing.

## Layout

Scripts sit under `Assets/Scripts/Scripts/`. There are no nested system folders, since with one person on the code, a deep hierarchy costs me more in navigation than it gains in tidiness. The journal's split shows up in the names instead: `JournalManager`, `JournalAchievementsPage`, `JournalPlantGalleryPage`.

Tutorial logic lives in its own `Tutorial/` folder because it's the content most likely to get cut or reworked and I wanted that to happen in one place.

Shared UI lives as prefabs under `Assets/Assets/GENERAL prefabs/` — pause, dialogue, key prompts, gameplay icon bar. Scenes stay small as a result and holds references instead of copies.

Scenes line up with chapters:

| Scene | What it's for |
|---|---|
| `MainMenu` | Entry point, chapter select |
| `CH1_Intro` | Chapter 1 opening |
| `Chapter2` | Chapter 2 |
| `Chapter3A` | Chapter 3 |
| `Chapter4A` | Chapter 4 |
| `Chapter5` | Chapter 5 |
| `PekengScene` | Secondary gameplay scene |

## Running it

1. Install **Unity 6, version 6000.1.1f1**. A different version will likely ask you to upgrade the project.
2. Install Git LFS first — the cutscene videos are stored through it, and a plain clone gets you empty files instead of video:

   ```
   git lfs install
   git clone https://github.com/sugurepo/botika-ni-lolo.git
   ```

3. Open the folder through Unity Hub.
4. Open `Assets/Scenes/MainMenu.unity`.
5. Press Play.

One thing to expect: a font asset is missing on purpose (see below). Unity will flag it when the project opens. It doesn't touch the scripts, the scenes, or how the game plays.

## Controls

| Action | Key |
|---|---|
| Move | W A S D |
| Look | Mouse |
| Interact | E |
| Pause | Escape |

## Build

There's a playable Windows build under Releases. The prototype archive I kept locally (`Botika Ni Lolo_Prototype.zip`) is in `.gitignore` and stays out of the source tree.

## Credits and licenses

| What | Where it's from | Notes |
|---|---|---|
| 3D models, environments, visual assets | My two collaborators | Their work, modeled in Blender |
| Tree packs | `Assets/TreePackVol.1/`, `Assets/BrokenVector/` | Unity Asset Store terms |
| Animal models | `Assets/ithappy/Animals_FREE/` | Free pack, used under its terms |
| Tree shaders | `Assets/FreeTreeShaders/` | Free asset, used under its terms |
| Jungle UI | `Assets/Jungle_UI/` | Free asset, used under its terms |
| Font — DivebarVintage | Personal-use license | **Not in this repo** — the license doesn't allow me to redistribute it |
| Font — KCObraLetraRegular | Free font | Check the bundled license file for attribution requirements |
| Footstep SFX | Freesound (author: owlstorm) | Free sound, credit the author |
| Music and remaining audio | See the files in `Assets/Botika/Audio/` | Check individual sources for credit requirements |

**About the missing font.** One font I used is licensed for personal use only, so I left it out of this repository rather than redistribute something I don't have the rights to. Unity reports it as missing when the project opens. The build under Releases has it, so the game looks the way it should when you play it.

## Repo layout

```
Assets/            Scenes, scripts, prefabs, art
Packages/          Package manifest and lock file
ProjectSettings/   Unity project configuration
Docs/              Technical notes
.gitattributes     Git LFS rules (*.mp4)
.gitignore         Generated folders, builds, restricted assets
```

Unity's generated folders (`Library/`, `Temp/`, `Logs/`, `Build/`, `UserSettings/`) aren't tracked, they rebuild themselves when the project opens. Neither are compiled builds or my local prototype archive.
