# Regroup - Turn-Based Tactical Strategy Game

![Gameplay Demo](link-to-your-gif-or-screenshot.gif)

## Overview
Regroup is a singleplayer turn-based tactical strategy game developed within the Unity Game Engine. It combines a grid-based tactical combat system with a node-based procedural exploration system, set against the historical backdrop of World War II in 1944 Occupied France. The player commands a squad of four United States paratroopers, navigating a procedurally generated map, managing supplies, and engaging in high-stakes tactical combat.

Download the compiled release directly from [here](https://gamahray.itch.io/regroup).

## Tech Stack
* **Engine:** Unity (Version 6.3 LTS - 6000.3.9f1)
* **Language:** C#

## Technical Highlights & Architecture
The primary objective of this project was not just creating a game, but the rigorous application of software engineering best practices. The codebase was strictly designed around SOLID principles and GRASP (General Responsibility Assignment Software Patterns) to ensure high cohesion, low coupling, and sustainable extensibility.

* **System Architecture (MVC & Observer Pattern):** The project relies on a strict separation of concerns inspired by the Model-View-Controller (MVC) paradigm. It utilizes an Event-Driven Architecture (C# events/delegates) to decouple the User Interface, audio managers, and cinematic camera systems from the core combat logic, entirely eliminating the need for expensive polling.
* **Custom A* Pathfinding:** Engineered a spatial 2D grid integrated with a custom A* (A-Star) search algorithm. It evaluates environmental obstacles, calculates variable movement costs without expensive floating-point operations, and employs strict tactical constraints to prevent "corner-cutting" through physical cover.
* **Utility-Based AI & FSM:** Developed a responsive Artificial Intelligence system governed by a Finite State Machine (FSM). Rather than relying on deterministic decision trees, the AI evaluates complex tactical heuristics (evaluating cover geometry via vector dot products, flanking vectors, and target health) to mathematically score and execute the most optimal actions.
* **Data Persistence & Serialization:** Solved Unity's scene-loading data destruction by abstracting unit profiles (health, procedural identities, and class roles) into lightweight, serializable C# data containers. A persistent Singleton manager synchronizes this data seamlessly between the overworld map and the tactical grid, maintaining the game's permadeath and resource economy.
* **Procedural Map Generation & Graph Math:** Implemented a non-linear graph structure for the campaign map generated at runtime. It utilizes polar coordinates for organic node distribution and a Disjoint-Set (Union-Find) data structure to calculate a Minimum Spanning Tree (MST), ensuring traversable paths connect without visually intersecting.

## Academic Context
This project was developed as my university thesis for my Informatics and Telecommunications degree. The full architectural design and system evaluation are documented in my thesis paper: *Design and Implementation of Regroup: Applying Software Engineering Principles to a Turn-Based Strategy Game*.
