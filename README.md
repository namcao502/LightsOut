# Blackout

A classic **Lights Out** puzzle game built with Windows Forms (.NET Framework 4.7.2).

Click a light to toggle it and its neighbors. Turn all lights off to win.

## Project Structure

```
Blackout/
├── Blackout/                        # Main application
│   ├── BlackoutForm.cs              # Form / UI layer
│   ├── BlackoutForm.Designer.cs     # Designer-generated layout
│   ├── BlackoutForm.resx            # Form resources (icon)
│   ├── BlackoutGame.cs              # Core game logic (model)
│   ├── BlackoutSolver.cs            # GF(2) Gaussian elimination solver
│   ├── TogglePattern.cs             # Toggle pattern definitions
│   ├── GameState.cs                 # Save/load serialization
│   ├── HighScoreManager.cs          # Local high score persistence
│   ├── AchievementManager.cs        # Achievement tracking
│   ├── Program.cs                   # Application entry point
│   └── Blackout.csproj
├── Blackout.Tests/                  # Unit tests (MSTest)
│   ├── BlackoutGameTests.cs         # Game logic tests
│   ├── BlackoutSolverTests.cs       # Solver tests
│   └── Blackout.Tests.csproj
├── FEATURES.md                      # Complete feature list
└── README.md
```

## How to Build

### Visual Studio

1. Open `Blackout/Blackout.sln`.
2. Build the solution (`Ctrl+Shift+B`).
3. Run with `F5`.

### Command Line

```bash
# Build the main project
msbuild Blackout/Blackout.csproj /p:Configuration=Debug

# Build and run tests
dotnet test Blackout.Tests/Blackout.Tests.csproj
```

## How to Play

1. The game starts with a random solvable puzzle on a 4x4 grid (default).
2. **Blue** = light is ON, **Red** = light is OFF (Classic theme).
3. Click any cell to toggle it and its neighbors.
4. Turn all lights off to win.
5. After winning, see your stats and choose to play again or exit.

### Menus

| Menu | Items |
|------|-------|
| **Game** | Difficulty presets (Easy/Medium/Hard/Custom), Undo, Hint, Show Solution, Save/Load, Puzzle Editor, High Scores, Exit |
| **Theme** | Classic, Dark, Neon, Pastel |
| **Pattern** | Cross (+), Diagonal (X), All 8 (*), Plus-3 (++) |
| **Settings** | Enable/disable animation |
| **Help** | Tutorial, Achievements, About |

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Ctrl+N` | New game (custom size) |
| `Ctrl+Z` | Undo last move |
| `Ctrl+H` | Show hint |
| `Ctrl+S` | Save game |
| `Ctrl+O` | Load game |
| Arrow keys | Navigate grid |
| Enter / Space | Toggle selected cell |
| Escape | Deselect |

## Architecture

The codebase separates concerns into model and view layers:

| Layer | Class | Responsibility |
|-------|-------|----------------|
| **Model** | `BlackoutGame` | Board state, toggle logic, undo, move counting, patterns |
| **Model** | `BlackoutSolver` | GF(2) solver, hints, difficulty-rated generation |
| **Model** | `TogglePattern` | Toggle pattern offset definitions |
| **Model** | `GameState` | Serializable game snapshot |
| **Model** | `HighScoreManager` | Local JSON high score persistence |
| **Model** | `AchievementManager` | Achievement tracking and persistence |
| **View** | `BlackoutForm` (Form) | UI: grid, menus, status bar, dialogs, animation, tutorial |

All game logic is fully testable without WinForms.

## Tests

~37 unit tests cover the game engine and solver:

```bash
dotnet test Blackout.Tests/Blackout.Tests.csproj
```

See [FEATURES.md](FEATURES.md) for details on every feature and its test coverage.

## Requirements

- .NET Framework 4.7.2
- Windows
- Visual Studio 2019+ (or MSBuild + .NET SDK for command-line builds)
