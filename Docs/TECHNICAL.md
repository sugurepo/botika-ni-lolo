# Botika ni Lolo — Technical Guide

Internal engineering documentation for the Botika ni Lolo Unity project. Covers project structure, script inventory, systems, scenes, art pipeline, and version control.

| Item | Value |
| --- | --- |
| Engine | Unity 6 (6000.1.1f1) |
| Render pipeline | Universal Render Pipeline (URP) |
| Language | C# |
| Platform | Windows |
| Version control | Git with Git LFS |

Setup instructions and a project overview are in the README.

## 1. Scope and Constraints

The project's architecture is shaped by four fixed constraints. They are recorded here because they explain why specific patterns exist elsewhere in this document.

**Single developer** — All Unity work (architecture, gameplay code, UI, scene assembly, asset integration) is by one person. There is no division between systems work and UI work, so the codebase is maintained by the same author who wrote it. This rules out deep abstraction layers and heavy indirection.

**One location** — All gameplay takes place in a single setting: the kubo and its yard. There is no streaming, no level budgeting, and no traversal optimization. Engineering effort is directed at interaction density rather than spatial scale.

**Externally produced art** — All 3D models and environments are produced by two project collaborators in Blender. Assets arrive in batches and are revised in batches. The Unity side must absorb revisions without invalidating scene references.

**Educational objective** — The project teaches plant identification and traditional remedies. Spatial layout and content placement serve that objective.

## 2. Repository Layout

```
Assets/
├── Assets/
│   ├── GENERAL prefabs/     Shared UI and object prefabs
│   ├── Journal/             Journal page art
│   ├── ChapterSelect/
│   └── Boundary.prefab
├── Botika/                  Core content — gameplay assets, audio
├── Cutscenes/               Chapter intros and quest videos
├── Scenes/                  One scene per chapter
├── Scripts/
│   └── Scripts/             All gameplay C#
│       └── Tutorial/        Tutorial logic
├── Settings/                URP configuration
├── TextMesh Pro/            Font assets and SDF data
└── [third-party packs]      Trees, shaders, jungle UI, animal models
```

### 2.1 Script organization

Gameplay C# is flat within `Scripts/Scripts/`. There is no nested per-system folder tree and no namespace hierarchy beyond what Unity generates.

Rationale: with a single engineer, additional folder depth adds navigation cost without reducing search time. Script responsibility is legible from the file name, which follows a consistent pattern:

| Prefix | Responsibility |
| --- | --- |
| `Journal*` | Journal state and page rendering |
| `Chapter*` | Chapter progression and results |
| `NPC*` | Character interaction |

Naming conventions carry the organizational load that folders would otherwise carry.

### 2.2 The Tutorial folder

`Scripts/Scripts/Tutorial/` is separated from the rest of the gameplay scripts.

Tutorial content has the highest revision volatility in the project and is the most likely to be cut or rewritten. Isolating it allows tutorial changes without modifying progression code.

## 3. Script Inventory

| File | Responsibility |
| --- | --- |
| `ChapterResult.cs` | End-of-chapter scoring, star rating, results screen |
| `JournalManager.cs` | Journal state and data; coordinates page scripts |
| `JournalAchievementsPage.cs` | Renders the achievements view |
| `JournalPlantGalleryPage.cs` | Renders the plant gallery; populates as plants are discovered |
| `NPCInteraction.cs` | Proximity detection, interaction prompt, dialogue hand-off |

## 4. Systems

### 4.1 Journal

Architecture: the journal is split into one state owner and N view scripts.

`JournalManager` owns the data layer. It tracks which plants have been discovered and which entries have been completed. It holds no knowledge of presentation.

Each page script owns exactly one screen:

- `JournalAchievementsPage.cs`
- `JournalPlantGalleryPage.cs`

Contract: page scripts read state from the manager and render it. The manager does not call into page rendering logic.

Extensibility: adding a journal page requires a new page script and a registration entry. It does not require modifying the manager. This holds the manager's size and complexity constant as page count grows; a conditional branch per page would make the manager the largest and least maintainable file in the project.

Rejected alternatives: ScriptableObject event channels and a formal MVC layer were both considered. Neither is implemented. At the current codebase size, both add indirection across every file that touches journal state while removing complexity from only one file.

### 4.2 Chapter Progression

Five chapters comprise the game. Each chapter has an intro cutscene, a set of objectives, and a result screen.

Progression state is held in one location and serves three functions:

- Content gating: a chapter 4 interaction is not available while the player is in chapter 2.
- Scene transitions: advancing a chapter loads the next chapter scene.
- Results input: the results screen reads final chapter state to compute the star rating.

Constraint: no scene-local progression flags. Scene-local flags are faster to author but produce no single authoritative answer to the question "what state is the game in?" by chapter five.

### 4.3 Results and Star Rating

`ChapterResult.cs` handles chapter termination. It:

- Computes the rating from recorded player performance.
- Displays the result before the player proceeds to the next chapter.

This constitutes the project's primary feedback loop for chapter performance.

### 4.4 Chop Minigame and Input Locking

The chop minigame runs its own input handling, independent of the first-person locomotion path.

Requirement: player rotation locks while the minigame is active. Without the lock, mouse-look continues to drive the camera during the minigame and the view rotates continuously.

Implementation: input suppression is mode-based. Entering the minigame suppresses the controller's look input; exiting restores it.

Rationale: mode-scoped input ownership keeps each suppression rule in one location and makes the mapping from mode to blocked input explicit. Per-frame conditional checks scattered through the controller achieve the same behavior with less predictable results.

The first-person controller uses the same mode-based mechanism.

### 4.5 NPC Interaction

`NPCInteraction.cs` covers the full interaction path:

- Detects player proximity.
- Displays the interaction prompt.
- Hands off to the dialogue system.
- Handles dialogue closure and returns control.

All four stages are implemented in one script, so NPC behavior is defined by a single linear sequence rather than distributed across character objects.

### 4.6 Controller and Footsteps

Locomotion is first-person with a walking head-bob.

Footstep audio is driven by movement state, not by a fixed-timer audio loop. The result is that footstep playback is synchronized with actual player movement state rather than running on an independent schedule.

### 4.7 UI as Prefabs

Shared UI is stored under `Assets/Assets/GENERAL prefabs/`:

- Pause panel and paused panel
- Dialogue UI and dialogue text
- Gameplay icon bar
- Contextual key prompts

All shared UI is instantiated from these prefabs. Nothing is duplicated per scene.

Scene file size: a Unity scene stores a reference to a prefab, not a copy of its hierarchy. Scene files remain small.

Global consistency: a styling or behavior change to a prefab applies to every instance. Per-scene UI copies diverge over time; prefab instantiation eliminates that class of defect.

Key prompts: prompts are labelled with the literal bound input (for example, `E to interact`) and use identical formatting across all instances. The player learns one prompt language for the whole game.

## 5. Scenes

| Scene | Purpose |
| --- | --- |
| `MainMenu` | Entry point, chapter select |
| `CH1_Intro` | Chapter 1 opening |
| `Chapter2` | Chapter 2 |
| `Chapter3A` | Chapter 3 |
| `Chapter4A` | Chapter 4 |
| `Chapter5` | Chapter 5 |
| `PekengScene` | Secondary gameplay scene |

Scene-to-chapter mapping is one-to-one: chapter N's content resides in chapter N's scene.

Consequence: shared systems must be reachable from every scene. This is the reason UI is prefab-based and progression state lives in a manager rather than in any individual scene.

## 6. Art Integration Pipeline

Models are produced by two collaborators in Blender and delivered in batches.

### 6.1 Import sequence

- Batch import: import delivered asset sets before assembling scenes.
- Scene assembly: build scenes once model revision has stopped.
- Prefab extraction: convert any asset used more than once into a prefab.

Rebuilding scenes on each individual model revision is not performed; it consumes disproportionate time for transient changes.

### 6.2 Prefabs

Environmental repeats and all UI are prefabs. A revised model updates every instance in a single step.

### 6.3 Project settings

| Setting | Value | Purpose |
| --- | --- | --- |
| Asset Serialization | Force Text | Asset files are diffable in Git rather than opaque binaries |
| Version Control Mode | Visible Meta Files | `.meta` files are tracked in the repository |

`.meta` files: these files hold the GUIDs that link scenes to assets. They must be committed. Losing them breaks every reference in the project.

## 7. Version Control

Git with Git LFS.

### 7.1 Git LFS

Cutscene video is tracked through Git LFS, declared in `.gitattributes`. Approximately 660 MB of video is stored via LFS. Direct Git tracking of this volume would make repository clones slow and working copies impractical.

### 7.2 `.gitignore`

The following are excluded:

- Generated folders: `Library/`, `Temp/`, `Logs/`, `Build/`, `UserSettings/`
- IDE files
- Compiled builds
- Local prototype archive

All excluded entries either regenerate automatically or are specific to one machine.

### 7.3 Licensing exclusions

One font asset is licensed for personal use only and is not redistributable. It is excluded from the repository.

Public history was rewritten so that the exclusion applies across the entire commit history, not only to the latest commit, because the font had been committed in earlier revisions.

## 8. Design Decisions and Constraints

### 8.1 Single confined play space

The play space is one location. This is a fixed scope decision.

Content density: with no traversal between areas, every square metre of the environment carries teaching content. Player attention remains on plants and remedies rather than on navigation.

Asset production feasibility: a single location is a realistic scope for two collaborators modelling in Blender alongside their own coursework.

No expansion in this build: no additional locations are planned or implemented.

### 8.2 Educational objective

The subject matter is plant identification and traditional remedies. The map is a container for that subject matter, not a subject in itself. Spatial and scope decisions follow from this.

## 9. Known Gaps

Incomplete or pending items. Review before extending the affected systems.

| Item | Status |
| --- | --- |
| Playable space | One location. No expansion in this build. |
| Font licensing | Two font licenses require verification for public release. One is excluded from the repository; the second requires confirmation. |
| Footstep audio | Placeholder assets. Final audio pass pending. |
| Platform | Windows only. No other platform builds are configured. |

## 10. Attribution

| Area | Author |
| --- | --- |
| Unity architecture, systems, gameplay code, UI, scene assembly, asset integration | Sole Unity developer (project author) |
| 3D models and environments | Two project collaborators, modelled in Blender |
