# Car Goes Around - GMTK Game Jam 2025

A grid-based building and puzzle game where players construct tracks and guide cars through challenging levels.

** [Itch.io link](https://therealnovelist.itch.io/car-goes-around) **

## 🎮 Game Overview

This is a **3D grid-based building game** where players place track pieces and obstacles to create paths for cars. The game features a modular building system, car AI navigation, and level-based progression with win/lose conditions.

### Key Features
- **Grid-based building system** with snap-to-grid mechanics
- **Car spawning and pathfinding** with intelligent navigation
- **Layered building system** supporting multi-level construction
- **Inventory management** for building pieces
- **Level progression** with intro/outro sequences
- **Real-time preview system** for piece placement
- **Sound feedback** and visual effects

## 🛠️ Technical Specifications

### Unity Version
- **Unity 6000.0.52f1** (Unity 6)

### Rendering Pipeline
- **Universal Render Pipeline (URP)** with custom shaders and materials

### Key Dependencies
- **Input System** (1.14.1) - Modern input handling
- **Cinemachine** (2.10.4) - Camera management
- **PrimeTween** - Animation system
- **AI Navigation** (2.0.8) - Pathfinding for cars
- **Unified Universal Blur** - Post-processing effects

## 🏗️ System Architecture

### Core Systems

#### Building System (`Assets/_Project/_Scripts/BuildingSystem/`)
- **BuildingManager**: Orchestrates building operations and state management
- **GridManager**: Handles grid-based placement and collision detection
- **PreviewSystem**: Real-time visual feedback for piece placement
- **State Pattern**: Separate states for building and removing pieces

#### Car System (`Assets/_Project/_Scripts/Car/`)
- **CarSpawner**: Manages car instantiation and lifecycle
- **CarMovement**: Handles car physics and movement
- **CarBrain**: AI logic for pathfinding and decision making
- **CarWarehouse**: Car pooling and management

#### Grid System (`Assets/_Project/_Scripts/Grid/`)
- **GridXZ**: Core grid mathematics and coordinate conversion
- **GridCell**: Individual cell data and state management
- **GridBuildPiece**: Component for grid-placeable objects

#### Game Management (`Assets/_Project/_Scripts/Game/`)
- **LevelManager**: Level flow, intro/outro sequences
- **LevelIntro/Win/Lose**: UI and sequence management
- **Event System**: Decoupled communication via EventBus

#### Inventory System (`Assets/_Project/_Scripts/InventorySystem/`)
- **InventoryManager**: Resource management and availability
- **InventoryUI**: User interface for piece selection
- **InventoryPreset**: Predefined inventory configurations

### Data Architecture
- **JSON-based level data** (`GridData.json`, `Level1.json`)
- **ScriptableObject databases** for build pieces
- **Modular component system** for extensibility

## 🎨 Assets & Art

### 3D Models
- **PolygonPrototype**: Low-poly environment assets
- **Custom track pieces** and building components
- **Car models** with multiple variants

### Materials & Shaders
- **URP-compatible materials** with custom properties
- **Grid visualization shaders** for building feedback
- **Triplanar mapping** for seamless texturing

### Audio
- **Sound feedback system** for building actions
- **Modular audio architecture** for easy expansion

## 🚀 Getting Started

### Prerequisites
- **Unity 6000.0.52f1** or later
- **Git** for version control
- **Visual Studio** or **Rider** (recommended IDEs)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/GMTKGameJam2025/MainProject.git
   ```

2. Open the project in Unity Hub:
   - Click "Add project from disk"
   - Navigate to the cloned folder
   - Select the project folder

3. Wait for Unity to import all assets and compile scripts

4. Open the main scene:
   - Navigate to `Assets/_Project/Scenes/`
   - Open the main game scene

### First Run
1. Press **Play** in the Unity Editor
2. Use the **inventory UI** to select building pieces
3. **Left-click** to place pieces on the grid
4. **Right-click** to remove pieces
5. **R key** to rotate pieces during placement
6. **ESC** to exit building mode

## 🎯 Gameplay Mechanics

### Building System
- **Grid-based placement**: All pieces snap to a uniform grid
- **Layer system**: Build on multiple vertical layers
- **Rotation support**: 90-degree increments for piece orientation
- **Collision detection**: Prevents invalid placements
- **Preview system**: Visual feedback before placement

### Car Mechanics
- **Automatic spawning**: Cars appear at designated spawn points
- **Pathfinding**: AI navigation through built tracks
- **Physics-based movement**: Realistic car behavior
- **Obstacle interaction**: Cars react to barriers and track pieces

### Level Progression
- **Objective-based gameplay**: Complete specific goals to win
- **Resource management**: Limited building pieces per level
- **Time-based challenges**: Some levels include time constraints
- **Progressive difficulty**: Increasing complexity across levels

## 🔧 Development

### Code Style
- **C# conventions**: PascalCase for public members, camelCase for private
- **Component-based architecture**: Modular, reusable systems
- **Event-driven communication**: Loose coupling via EventBus
- **SOLID principles**: Clean, maintainable code structure

### Key Design Patterns
- **State Pattern**: Building system states (Build/Remove)
- **Observer Pattern**: Event system for decoupled communication
- **Object Pooling**: Efficient car and effect management
- **Command Pattern**: Undo/redo functionality for building
- **Singleton Pattern**: Global managers (with MonoBehaviourSingleton)

### Testing
- **Unity Test Framework**: Unit and integration tests
- **Play mode testing**: Automated gameplay validation
- **Performance profiling**: Regular optimization checks

### Build Configuration
- **Development builds**: Include debug symbols and logging
- **Release builds**: Optimized for performance
- **Platform-specific settings**: Configured for target platforms

## 📁 Project Structure

```
Assets/
├── _Project/                    # Main project assets
│   ├── _Scripts/               # All C# scripts
│   │   ├── BuildingSystem/     # Building and placement logic
│   │   ├── Car/               # Car behavior and AI
│   │   ├── Game/              # Level management and flow
│   │   ├── Grid/              # Grid system and mathematics
│   │   ├── InventorySystem/   # Resource management
│   │   └── UI/                # User interface
│   ├── Scenes/                # Game scenes
│   ├── Prefabs/               # Reusable game objects
│   ├── Materials/             # URP materials
│   ├── Textures/              # Texture assets
│   └── Settings/              # Project configuration
├── PolygonPrototype/          # Third-party art assets
├── StarterAssets/             # Unity starter templates
└── Plugins/                   # External plugins
    └── PrimeTween/            # Animation system
```

## 🏆 Game Jam Context

This project was created for **GMTK Game Jam 2025**, focusing on innovative gameplay mechanics within the constraints of a short development timeline. The game explores themes of:
- **Spatial reasoning** through grid-based building
- **Cause and effect** via car behavior
- **Resource management** through limited inventory
- **Progressive complexity** across multiple levels

## 🤝 Contributing

### Development Workflow
1. **Create feature branches** from main
2. **Follow coding conventions** outlined above
3. **Test thoroughly** before submitting PRs
4. **Document new systems** and public APIs
5. **Update README** for significant changes

### Reporting Issues
- Use GitHub Issues for bug reports
- Include Unity version and platform information
- Provide steps to reproduce the issue
- Attach relevant log files or screenshots

## 📄 License

This project was created for GMTK Game Jam 2025. Please refer to the game jam rules and Unity's licensing terms for usage guidelines.

## 🙏 Credits

### Development Team
- **GMTK Game Jam 2025 Team** - Game design and implementation

### Third-Party Assets
- **PolygonPrototype** - Low-poly 3D assets
- **Unity Technologies** - StarterAssets and core engine
- **PrimeTween** - Animation system
- **Unified Universal Blur** - Post-processing effects

### Tools & Technologies
- **Unity 6** - Game engine
- **Visual Studio/Rider** - Development environment
- **Git** - Version control
- **GitHub** - Repository hosting

---

*Built with ❤️ for GMTK Game Jam 2025*
