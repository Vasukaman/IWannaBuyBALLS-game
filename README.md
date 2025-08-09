I Wanna Buy Your BALLS - A Unity Architecture Showcase
Play the Game on itch.io | Читать на русском (Russian README)

This project is a 2D incremental game developed in Unity. While the gameplay is simple, its primary purpose is to serve as a portfolio piece demonstrating a deep understanding of professional, scalable game architecture and modern Unity development patterns.

Architectural Features
SOLID Principles & Decoupled Design

1. Dependency Injection (DI) with the Reflex framework
2. Persistent Bootstrap Scene for a clean application entry point
3. Service Layer for abstracting core game logic
4. Model-View-Presenter (MVP) pattern for separating logic from MonoBehaviours
5. Data-Driven Design using ScriptableObjects for all configurations
6. Composite "Master" Profiles for a designer-friendly workflow
7. Strategy Pattern with [SerializeReference] for pluggable logic
8. ScriptableObject-based Event Bus for decoupled, cross-system communication
9. Game State Machine (FSM) to manage the application's lifecycle
10. GPU-Accelerated VFX with custom HLSL Shaders and GPU Instancing

Work in Progress:
1. Data Persistence (Save/Load System)
2. Refactor to an IAssetProvider service
3. Write Unit Tests for core services and models
4. Integrate Unity's Addressable Asset System


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

