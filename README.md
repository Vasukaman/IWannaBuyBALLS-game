# I Wanna To Buy Your BALLS - A Unity Architecture Showcase

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

### **Future Improvements**

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

**Ball Seller**

![Sellet_git](https://github.com/user-attachments/assets/75fb8afe-e02b-458c-ac3b-9866911c0830)

