# Unity 2D Incremental/Clicker Game

This is a 2D incremental game developed in Unity, focusing on creating complex visual effects and building a scalable, clean architecture.

This project is currently a work in progress. I am actively refactoring and polishing the codebase to adhere to the highest standards of software design and architecture.

## Key Features & Technical Highlights

This project serves as a portfolio piece to demonstrate a deep understanding of professional game development practices.

* **SOLID Architecture & Clean Code:** The project is structured with a strong emphasis on SOLID principles. Responsibilities are separated into distinct, decoupled components (e.g., `Ball`, `BallMerger`, `BallScaler`, `BallVisualController`). While I am still working to perfect it, the foundation is built for scalability and maintainability.

* **Dependency Injection with Reflex:** The project leverages the **Reflex** framework for Dependency Injection. This allows for a decoupled architecture where dependencies like services (`IMoneyService`) and factories (`IBallFactory`) are injected at runtime, making the code cleaner and more testable.

* **Scriptable Objects for Configuration:** Game design data (like gadget stats, visual styles, and libraries) is managed using Scriptable Objects. This decouples configuration from the scene, allowing designers to create and tweak new content without touching any code.

* **GPU-Accelerated Visuals:** Many visual effects are offloaded to the GPU for maximum performance, using techniques like **GPU Instancing** for particle-like systems to render thousands of objects with minimal CPU overhead.

* **Custom HLSL Shaders:** All core visuals are built on custom shaders written in HLSL to achieve unique and performant effects that are not possible with standard Unity materials.

### Shader Effects Showcase

#### Ball Merging (Metaball Effect)
*A smooth, liquid-like merge effect when two balls combine.*
*(place your Ball Merging GIF here)*

#### Wobbly Gates
*A dynamic distortion effect on the gates, making them feel alive.*
https://imgur.com/a/0mG6fTW

#### Gadget Sell Zone
*A custom shader that creates a visual field for selling gadgets.*
*(place your Sell Zone GIF here)*

#### Ball Appearance
*The core ball shader uses multiple layers of procedural effects, including internal orbiting orbs that represent the ball's value.*
*(place your Ball Appearance GIF here)*

## Future Improvements

* **Complete Decoupling:** Address all `TODO`s in the code to fully decouple components, such as removing the `Ball`'s knowledge of its factory.
* **Performance Optimization:** Replace `Update()` loops for logic like neighbor-finding with timed coroutines or the C# Job System for better performance.
* **UI Refinement:** Build out a more robust UI system, potentially using the MVVM pattern.
