# Lights Out

A classic **Lights Out** puzzle game built with Windows Forms (.NET Framework 4.7.2).

Click a light to toggle it and its orthogonal neighbors. Turn all lights off to win.

## Project Structure

```
LightsOut/
├── WindowsFormsApp_LightsOut/       # Main application
│   ├── LightsOut.cs                 # Form / UI layer
│   ├── LightsOut.Designer.cs        # Designer-generated layout
│   ├── LightsOut.resx               # Form resources (icon)
│   ├── LightsOutGame.cs             # Core game logic (model)
│   ├── Program.cs                   # Application entry point
│   └── WindowsFormsApp_LightsOut.csproj
├── LightsOutGame.Tests/             # Unit tests (MSTest)
│   ├── LightsOutGameTests.cs
│   └── LightsOutGame.Tests.csproj
└── README.md
```

## How to Build

### Visual Studio

1. Open `WindowsFormsApp_LightsOut/WindowsFormsApp_LightsOut.sln`.
2. Build the solution (`Ctrl+Shift+B`).
3. Run with `F5`.

### Command Line

```bash
# Build the main project
msbuild WindowsFormsApp_LightsOut/WindowsFormsApp_LightsOut.csproj /p:Configuration=Debug

# Build and run tests
dotnet test LightsOutGame.Tests/LightsOutGame.Tests.csproj
```

## How to Play

1. The game starts with a random solvable puzzle on a 4x4 grid (default).
2. **Blue** = light is ON, **Red** = light is OFF.
3. Click any cell to toggle it and its up/down/left/right neighbors.
4. Turn all lights off (all red) to win.
5. After winning, choose to play again with a new grid size, or exit.
6. Use **Game > New Game** (or `Ctrl+N`) at any time to start a new game and pick a grid size from 2x2 to 10x10.

## Architecture

The codebase separates concerns into two layers:

| Layer | Class | Responsibility |
|-------|-------|----------------|
| **Model** | `LightsOutGame` | Board state, toggle logic, win detection, puzzle generation |
| **View**  | `LightsOut` (Form) | Button grid, colors, click handling, win dialog |

The UI reads from and writes to the model, keeping game rules fully testable without WinForms.

## Tests

11 unit tests cover the game engine:

```bash
dotnet test LightsOutGame.Tests/LightsOutGame.Tests.csproj
```

See [FEATURES.md](FEATURES.md) for details on every feature and its test coverage.

## Requirements

- .NET Framework 4.7.2
- Windows
- Visual Studio 2019+ (or MSBuild + .NET SDK for command-line builds)
