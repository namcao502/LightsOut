# Features

Complete list of features in the Blackout application.

---

## Gameplay

### 1. Configurable N x M Light Grid

The board supports both square (N x N) and rectangular (N x M) grids (range 2-10 per dimension). Default is 4x4.

- The player can choose grid dimensions via **Game > Difficulty > Custom** (`Ctrl+N`), which opens a dialog with separate Rows/Cols spinners and a "Square" checkbox.
- Button size scales automatically so the board fits within 600px.
- **Tested by:** `RectangularGrid_DifferentRowsCols`, `RectangularGrid_HasWon`

### 2. Cross-Pattern Toggle (default)

Clicking a cell toggles the clicked cell and its four orthogonal neighbors (up, down, left, right). Cells on edges or corners correctly skip out-of-bounds neighbors.

- **Tested by:** `ToggleCell_Center_TogglesItselfAndNeighbors`, `ToggleCell_Corner_OnlyTogglesValidNeighbors`

### 3. Multiple Toggle Patterns

Four toggle patterns are available via the **Pattern** menu:

| Pattern | Behavior |
|---------|----------|
| Cross (+) | Self + 4 orthogonal neighbors (classic) |
| Diagonal (X) | Self + 4 diagonal neighbors |
| All 8 (*) | Self + all 8 surrounding neighbors |
| Plus-3 (++) | Self + 2 cells in each cardinal direction |

Changing pattern starts a new game. The current pattern is shown in the status bar.

- **Tested by:** `ToggleCell_DiagonalPattern_TogglesDiagonals`, `ToggleCell_XShapePattern_TogglesAll8Neighbors`

### 4. Toggle Idempotency

Clicking the same cell twice restores the board to its previous state, ensuring every move is reversible.

- **Tested by:** `ToggleCell_Twice_RestoresOriginalState`

### 5. Win Detection

The game checks the board after every click. The player wins when all lights are OFF.

- **Tested by:** `NewGame_AllLightsOff_HasWonReturnsTrue`, `SetAll_Off_HasWonReturnsTrue`

### 6. Win Dialog with Stats

When the player wins, a dialog shows:
- Total moves and elapsed time
- Optimal solution move count
- Whether a new high score was set
- Option to play again or exit

### 7. Random Solvable Puzzles

Each new game generates a puzzle guaranteed to be solvable. The algorithm starts from the solved state (all off) and applies random clicks.

- **Tested by:** `Randomize_ProducesNonSolvedBoard`, `Randomize_IsSolvable_ByReversingClicks`

### 8. Difficulty Presets

The **Game > Difficulty** submenu provides quick starts:
- Easy (3x3)
- Medium (5x5)
- Hard (7x7)
- Custom (opens the grid-size dialog)

### 9. Difficulty-Rated Puzzle Generator

The solver generates puzzles targeting a specific optimal move count, ensuring difficulty matches the preset chosen.

- **Tested by:** `GenerateWithDifficulty_ProducesValidPuzzle`

### 10. Move Counter

The status bar shows the number of moves (clicks) made. Resets on new game.

- **Tested by:** `MoveCount_IncrementedOnToggle`, `MoveCount_ResetOnRandomize`

### 11. Timer

The status bar shows elapsed time (M:SS). Timer starts on the first click and stops on win. Resets on new game.

### 12. Undo

**Game > Undo** (`Ctrl+Z`) reverts the last move. Multiple undo is supported (full move history). Toggle is self-inverse, so undo simply replays the same cell.

- **Tested by:** `Undo_RevertsLastMove`, `Undo_DecrementsCount`, `CanUndo_FalseOnNewGame`, `CanUndo_TrueAfterToggle`, `Undo_EmptyHistory_ThrowsException`

### 13. Keyboard Navigation

Arrow keys navigate a yellow-highlighted selection cursor across the grid. Enter/Space toggles the selected cell. Escape deselects. Selection wraps around edges.

### 14. Hint System

**Game > Hint** (`Ctrl+H`) highlights the next optimal cell to click with a flashing yellow background (4 flashes over ~2 seconds). Based on the GF(2) solver.

- **Tested by:** `GetHint_ReturnsFirstMove`, `GetHint_SolvedBoard_ReturnsNull`

### 15. Step-by-Step Solver

**Game > Show Solution** displays numbered overlays on all cells that need clicking, in order. Shows total move count. Uses Gaussian elimination over GF(2).

- **Tested by:** `Solve_SimpleGrid_ReturnsValidSolution`, `Solve_AlreadySolved_ReturnsEmpty`, `GetStepByStep_SolvesWhenApplied`

---

## UI / UX

### 16. Color Themes

Four themes available via the **Theme** menu:

| Theme | Light ON | Light OFF | Background |
|-------|----------|-----------|------------|
| Classic | Blue | Red | Default |
| Dark | DarkCyan | DimGray | Near-black |
| Neon | Lime | Magenta | Black |
| Pastel | CornflowerBlue | LightCoral | WhiteSmoke |

### 17. Smooth Animation

When toggling cells, colors transition smoothly over ~300ms (6 animation frames at 50ms). Can be disabled via **Settings > Enable Animation**.

### 18. Auto-Sized Window

The form's `ClientSize` is calculated from grid dimensions, menu, status bar, and optional tutorial overlay. The window always fits the board with no wasted space.

### 19. Centered Window

The form opens at the center of the screen.

### 20. Flat Button Style

Buttons use `FlatStyle.Flat` for a clean, modern look.

### 21. Dynamic Title Bar

The title bar shows the current grid size (e.g. "Blackout (4x4)") and "(Editor)" when in editor mode.

### 22. Status Bar

A `StatusStrip` at the bottom shows: Moves count, Timer, Current pattern, and Optimal solution moves.

### 23. Custom Window Icon

The application uses a custom icon embedded in the form's `.resx` resource file.

---

## Save / Load / Persistence

### 24. Save Game

**Game > Save** (`Ctrl+S`) saves the current game state (board, pattern, moves, timer) to a `.bosave` file using JSON serialization.

### 25. Load Game

**Game > Load** (`Ctrl+O`) restores a saved game from a `.bosave` file (also supports legacy `.losave` files), rebuilding the grid and resuming where you left off.

### 26. Puzzle Editor

**Game > Puzzle Editor** toggles editor mode:
- Click toggles individual lights (no neighbor toggling)
- Timer and move counter are paused
- **Play from Editor** starts a game from the current board layout

- **Tested by:** `SetLight_SetsIndividualCell`

### 27. Board Serialization

`GetBoardSnapshot()` and `LoadBoard()` support copying and restoring board state.

- **Tested by:** `GetBoardSnapshot_ReturnsCorrectCopy`, `LoadBoard_RestoresState`

---

## High Scores and Achievements

### 28. High Score Tracking

Best (fewest moves, fastest time) per grid size is saved to `%AppData%/Blackout/highscores.json`. Viewable via **Game > High Scores**.

### 29. Achievement System

Seven achievements tracked across sessions:

| Achievement | Description |
|-------------|-------------|
| First Win | Solve any puzzle |
| Speed Demon | Solve in under 10 seconds |
| Minimalist | Solve in minimum possible moves |
| No Undo | Solve without using undo |
| Size Master | Solve on every grid size 2 through 10 |
| Pattern Explorer | Solve with each toggle pattern |
| Perfectionist | Solve 5 different puzzles with minimum moves |

Viewable via **Help > Achievements**. A gold toast notification appears when an achievement unlocks.

---

## Tutorial

### 30. Interactive Tutorial

**Help > Tutorial** runs a guided walkthrough on a 3x3 grid:
1. "Click the highlighted cell to toggle it"
2. "Notice the neighbors toggled too"
3. "Goal: turn ALL lights off"
4. "Use Ctrl+Z to undo mistakes"
5. Completion message

Each step highlights the target cell and advances when clicked.

---

## Architecture

### 31. Model / View Separation

Game rules live in `BlackoutGame` (model) with no WinForms dependency. The solver, patterns, persistence, and achievements are all in separate model classes. The form only handles UI.

### 32. Input Validation

The model validates all inputs: grid size, cell coordinates (`IsLightOn`, `SetLight`), `Randomize` parameters, and board dimensions on `LoadBoard`.

- **Tested by:** `IsLightOn_OutOfBounds_ThrowsException`, `Randomize_NullRandom_ThrowsException`, `Randomize_InvalidMinMax_ThrowsException`, `Constructor_ZeroGridSize_ThrowsException`, `Constructor_NegativeGridSize_ThrowsException`

### 33. GF(2) Solver

`BlackoutSolver` uses Gaussian elimination over the Galois field GF(2) to find optimal solutions. Supports all toggle patterns and rectangular grids.

- **Tested by:** `Solve_SimpleGrid_ReturnsValidSolution`, `Solve_WithDiagonalPattern_Works`

---

## Testing

### 34. Unit Test Suite (~37 tests)

Two test files cover all model-layer logic:

| Category | Test count | File |
|----------|-----------|------|
| Core game logic | ~30 | `BlackoutGameTests.cs` |
| Solver | ~7 | `BlackoutSolverTests.cs` |
