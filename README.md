Of course. Here is the complete `README.md` in Markdown format, ready to be copied.

-----

# I Wanna Buy Your BALLS - A Unity Architecture Showcase

**[Play the Game on itch.io](https://vasuka.itch.io/i-want-to-buy-your-balls)** | **[Читать на русском (Russian README)](  )**

This project is a 2D incremental game developed in Unity. While the gameplay is simple, its primary purpose is to serve as a portfolio piece demonstrating a deep understanding of professional, scalable game architecture and modern Unity development patterns.

-----

### **Architectural Features**

  * **SOLID Principles & Decoupled Design**
  * **Dependency Injection (DI)** with the Reflex framework
  * **Persistent Bootstrap Scene** for a clean application entry point
  * **Service Layer** for abstracting core game logic
  * **Model-View-Presenter (MVP)** pattern for separating logic from `MonoBehaviour`s
  * **Data-Driven Design** using `ScriptableObjects` for all configuration
  * **Composite "Master" Profiles** for a designer-friendly workflow
  * **Strategy Pattern** with `[SerializeReference]` for pluggable logic
  * **ScriptableObject-based Event Bus** for decoupled, cross-system communication
  * **Game State Machine (FSM)** to manage the application's lifecycle
  * **GPU-Accelerated VFX** with custom **HLSL Shaders** and **GPU Instancing**

-----

### **Work in Progress**

  * Data Persistence (Save/Load System)
  * Refactor to an `IAssetProvider` service
  * Write Unit Tests for core services and models
  * Integrate Unity's Addressable Asset System

-----

### **Visual Effects Showcase**

**Ball Merging (Metaball Effect)**
![Merging_git](https://github.com/user-attachments/assets/65438e40-9194-475e-a5e2-e824c688f529)

**Wobbly Gates**
![wibbly gates](https://github.com/user-attachments/assets/86a4d903-0a17-48f6-9e3a-c1fc16c1dd1c)

**Gadget Sell Zone**
![Seller_git](https://github.com/user-attachments/assets/740ba7d8-9697-402e-becb-d03a6635ea59)

**Connection Trail Effect**
![Sellet_git](https://github.com/user-attachments/assets/75fb8afe-e02b-458c-ac3b-9866911c0830)

-----

### **Architectural Deep Dive**

This project was intentionally refactored from a simple prototype into a robust application to showcase professional development practices.

  * **Foundation (DI & Bootstrap):** The game starts from a persistent `Bootstrap` scene containing a `ProjectScope`, which establishes the global Dependency Injection container. A `ProjectInstaller` then "wires up" all the game's core systems, ensuring a clean and predictable initialization order.

  * **Service Layer:** Core logic is handled by pure C\# services (e.g., `IMoneyService`, `IBallService`, `IStoreService`). These services are registered with the DI container and injected into the `MonoBehaviour`s that need them. This decouples the game's "brain" from its "body" and makes the core logic fully testable.

  * **Model-View-Presenter (MVP):** Gameplay entities are split into their logical (`Model`) and engine (`View`/`Presenter`) components. For example, a `BallView` in the scene holds a plain C\# `BallData` object. The `BallView` handles physics and lifecycle events, while the `BallData` handles the price and value logic.

  * **Data-Driven with ScriptableObjects:** All "magic numbers" and configuration settings have been extracted into `ScriptableObject` assets. Complex objects, like the `Ball`, use a "Composite Profile" (`BallProfile.asset`) which holds references to smaller, specialized profiles (`BallScalingProfile`, `BallMergingProfile`). This creates a single, easy-to-manage configuration point for designers.

  * **Strategy Pattern & `[SerializeReference]`:** For components with multiple behaviors, like the `GateLogic`, the Strategy Pattern is used. A `GateProfile` `ScriptableObject` contains a `[SerializeReference]` field for an `IGateEffect` interface. This allows different logic classes (e.g., `AddValueEffect`, `MultiplyValueEffect`) to be selected and configured directly in the Inspector, making the system incredibly flexible and extensible.

  * **Event Bus:** To break dependencies between major systems (e.g., `Gameplay` and `UI`), the project uses a `ScriptableObject`-based event bus. When a ball is sold, the `BallSellerZone` raises a global `OnBallSold` event asset. The `MoneyService` and `AudioService` listen to this asset, but the `BallSellerZone` has no direct reference to them, achieving maximum decoupling.




#### Seller Suck In


## Development Roadmap

