# Botika ni Lolo — List of Revisions

**Prepared for:** Competition Submission
**Date:** June 15, 2026

This document formally declares the gameplay, UI, and feedback revisions made to *Botika ni Lolo* in preparation for the competition build.

---

## 1. UI Key "(E)" Prompt for Interactable Zones
On-screen contextual prompt that displays the exact key (e.g. "(E)") the player must press to interact with an object (pick up items, use the sink, chopping board, claypot, etc.). The prompt appears only when the player is within range of an interactable and automatically hides once they move away or the interaction is no longer valid, improving clarity on what actions are available at any moment.

## 2. Environment FX and Animations
Added ambient environmental animation (e.g. rotating ceiling fan blades) to make the kitchen/house environments feel more alive and lived-in.

## 3. Walking Effects
Improved the player's walking head-bob for a more natural first-person movement feel, and added footstep sound effect placeholders that play in sync with the player's movement.

## 4. Boundaries Around the Playable Zone
Added invisible boundary colliders around the playable area of each chapter to prevent the player from wandering outside the intended map space.

## 5. Decision Panel — Star Rating System
The end-of-chapter decision panel now displays a 1-3 star rating reflecting the player's performance (based on how well the chopping and cooking steps were completed), along with a corresponding Tagalog feedback message:
- 3 stars — "Perpekto! Eksakto ang bawat hakbang ng paghahanda mo."
- 2 stars — "Magaling! Maayos ang gamot, pero may dapat pang pagbutihin sa susunod."
- 1 star (undercooked) — "Kulang pa sa luto, apo."
- 1 star (overcooked) — "Sobra sa luto, apo."

A rating of 1 star requires the player to retry the chapter; 2-3 stars allow progression to the next chapter.

## 6. Chopping Effects and Camera POV
Enhanced the chopping minigame with real-time feedback: a squash/recoil animation on the ingredient with each chop, a camera "kick" effect for impact, smoke particle bursts, and a progressive reveal from whole to chopped ingredient as the player chops. The player's camera/rotation is locked during the chopping minigame to prevent unintended camera spinning, and automatically restored once the minigame ends.

## 7. Gameplay Icon Bar
Added an on-screen icon bar showing the player which keys control the Journal, How-To-Play/Tutorial panel, and Pause menu, with customizable key labels. The bar automatically hides while the Tutorial/How-To-Play panel is open and reappears once it is closed.

## 8. Pause Menu
Added a pause system (Escape key by default) that freezes gameplay and displays a pause panel with options to Resume, return to the Chapter Select menu, or Exit to the Main Menu.

---

*All revisions listed above were implemented across Chapters 1-5 (PekengScene, Chapter2, Chapter3A, Chapter4A, Chapter5).*
