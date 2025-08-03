# Unity 2D Incremental/Clicker Game

Unity 2D Incremental Game
This is a 2D incremental game developed in Unity. The project serves as a portfolio piece to demonstrate a deep understanding of professional game development architecture, clean code practices, and advanced engine features.

## Key Features & Technical Highlights

* **SOLID, Decoupled Architecture:** The project is built on a foundation of SOLID principles, with a clear separation between different layers of the application.
* **Dependency Injection (DI):** Utilizes the **Reflex** framework to manage dependencies, creating a flexible and testable codebase where systems are decoupled from each other.
* **Service Layer:** Core game systems (like the economy, ball spawning, and store logic) are abstracted into a pure C# **Service Layer**, separating logic from the Unity scene.
* **Model-View-Presenter (MVP):** Employs the MVP pattern to separate pure C# data and logic (**Models**) from their `MonoBehaviour` representations in the scene (**Views** and **Presenters**).
* **Event-Driven Communication:** Uses a **ScriptableObject-based Event Bus** for communication between systems, allowing for maximum decoupling (e.g., the `BallSellerZone` announces a sale, and the `MoneyService` listens, with neither knowing about the other).
* **Data-Driven Design:** Leverages **Scriptable Objects** for all configuration (gadget stats, visual styles, event channels), allowing for easy iteration and designer-friendly workflows.
* **Component-Based Prefabs:** Game objects are built by composing small, single-responsibility `MonoBehaviour` "behaviours" (`ForceZone`, `Draggable`, `RotatorOnActivate`), creating a library of reusable tools.
* **GPU-Accelerated VFX:** Core visuals are driven by custom **HLSL shaders** and leverage **GPU Instancing** for high-performance particle-like effects.




### Shader Effects Showcase

#### Ball Merging (Metaball Effect)
*A smooth, liquid-like merge effect when two balls combine.*

![Merging_git](https://github.com/user-attachments/assets/65438e40-9194-475e-a5e2-e824c688f529)

#### Wobbly Gates

![wibbly gates](https://github.com/user-attachments/assets/86a4d903-0a17-48f6-9e3a-c1fc16c1dd1c)


#### Gadget Sell Zone
*A custom shader that creates a visual field for selling gadgets.*

![Seller_git](https://github.com/user-attachments/assets/740ba7d8-9697-402e-becb-d03a6635ea59)

#### Seller Suck In
![Sellet_git](https://github.com/user-attachments/assets/75fb8afe-e02b-458c-ac3b-9866911c0830)

## Development Roadmap

This project is an active work in progress, following a deliberate path of prototyping and professional refactoring.

1.  ✅ **Initial Prototype:** Build the core gameplay loop with a focus on functionality.
2.  ✅ **Dependency Injection:** Integrate the Reflex framework to manage dependencies.
3.  ✅ **Refactor to Model-View & SOLID:** Separate core entities (`Ball`, `Gadget`, `Store`) into pure C# Models and `MonoBehaviour` Views/Presenters.
4.  ✅ **Implement Service Layer:** Abstract core logic into decoupled, testable services (`IMoneyService`, `IBallService`, `IGadgetService`).
5.  ✅ **Decouple with Event Bus:** Implement a ScriptableObject-based event system to handle cross-system communication.
6.  ✅ **Data-Driven Configuration:** Move all hardcoded settings into `ScriptableObject` assets for a designer-friendly workflow.
7.  ✅ **Bootstrap Scene & Persistent Systems:** Create a professional startup flow with a persistent `ProjectScope` to manage the game's lifecycle.
8.  ✅ **Data-driven Architecture**
9.  **More Events using Event Channels + SOUNDS** Adding special events on things like BallMerge and ButGadget and using them for playing sounds for example. 
10.  **Implement a Game State Machine (FSM):** Add states(menu, pause) and FSM for them **<-- WE ARE HERE**
11. **Write Unit Tests:** Create EditMode tests for the pure C# services to verify their logic and ensure stability.
12. **Modernize Asset Management:** Convert the prefab loading system to use Unity's Addressables for production-level asset management.
