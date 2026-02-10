# Features

Complete list of features in the Lights Out application.

---

## Gameplay

### 1. Configurable N x N Light Grid

The board is an N x N grid of clickable buttons (default 4x4, range 2–10). Each button represents a light that can be either ON (blue) or OFF (red).

- The player can choose the grid size at any time via **Game > New Game** (`Ctrl+N`), which opens a dialog with a numeric spinner.
- Button size scales automatically so the board fits comfortably on screen (100px per cell at small sizes, down to 40px at larger sizes).

### 2. Cross-Pattern Toggle

Clicking a cell toggles **five** lights at once: the clicked cell and its four orthogonal neighbors (up, down, left, right). Cells on edges or corners correctly skip out-of-bounds neighbors.

- **Tested by:** `ToggleCell_Center_TogglesItselfAndNeighbors`, `ToggleCell_Corner_OnlyTogglesValidNeighbors`

### 3. Toggle Idempotency

Clicking the same cell twice restores the board to its previous state. This is a core property of the Lights Out puzzle that ensures every move is reversible.

- **Tested by:** `ToggleCell_Twice_RestoresOriginalState`

### 4. Win Detection

The game checks the board after every click. The player wins when all lights are OFF (all buttons are red).

- **Tested by:** `NewGame_AllLightsOff_HasWonReturnsTrue`, `SetAll_Off_HasWonReturnsTrue`

### 5. Win Dialog with Replay Option

When the player wins, a dialog appears with the message "You win! Play again?" and Yes/No buttons:

- **Yes** opens the grid-size dialog so the player can pick a new size (or keep the same).
- **No** closes the application.

### 6. Random Solvable Puzzles

Each new game generates a random puzzle that is **guaranteed to be solvable**. The algorithm works by starting from the solved state (all lights off) and applying a random number of clicks (5-15), which can always be reversed by the player.

- **Tested by:** `Randomize_ProducesNonSolvedBoard`, `Randomize_IsSolvable_ByReversingClicks`

### 7. Bulk Light Control

The `SetAll` method sets every light to ON or OFF at once. Used internally by the randomization algorithm.

- **Tested by:** `SetAll_On_NoLightIsOff`, `SetAll_Off_HasWonReturnsTrue`

---

## UI / UX

### 8. Color-Coded Light States

| State | Color |
|-------|-------|
| ON    | Blue  |
| OFF   | Red   |

Colors are defined as named constants (`LightOnColor`, `LightOffColor`) for easy customization.

### 9. Auto-Sized Window

The form's `ClientSize` is calculated from the grid dimensions (`gridSize * buttonSize`), so the window always fits the board exactly with no wasted space. The window resizes automatically when the grid size changes.

### 9a. Adaptive Button Scaling

Buttons are 100px at small grid sizes and scale down proportionally (minimum 40px) for larger grids, keeping the board within 600px.

### 10. Centered Window

The form opens at the center of the screen (`StartPosition = CenterScreen`).

### 11. Flat Button Style

Buttons use `FlatStyle.Flat` for a clean, modern look without 3D borders.

### 12. Game Menu

A **Game** menu in the menu bar provides:

- **New Game** (`Ctrl+N`) — opens the grid-size dialog and starts a fresh puzzle.
- **Exit** (`Alt+F4`) — closes the application.

### 13. Dynamic Title Bar

The title bar displays the current grid size (e.g. "Lights Out (4x4)"), updating whenever a new game starts.

### 14. Custom Window Icon

The application uses a custom icon embedded in the form's `.resx` resource file.

---

## Architecture

### 15. Separated Game Logic (Model / View Split)

Game rules live in `LightsOutGame` (the model) and are completely independent of WinForms. The form (`LightsOut`) only handles UI concerns: building buttons, reading model state to set colors, and forwarding clicks to the model.

### 16. Input Validation

The `LightsOutGame` constructor rejects invalid grid sizes (zero or negative) with a descriptive `ArgumentOutOfRangeException`.

- **Tested by:** `Constructor_ZeroGridSize_ThrowsException`, `Constructor_NegativeGridSize_ThrowsException`

### 17. GridSize Property

The game exposes its grid size as a read-only property, allowing the UI and tests to query it without hard-coding values.

- **Tested by:** `GridSize_ReturnsCorrectValue`

---

## Testing

### 18. Unit Test Suite (11 tests)

The `LightsOutGame.Tests` project covers all core game logic with MSTest:

| # | Test | What it verifies |
|---|------|------------------|
| 1 | `NewGame_AllLightsOff_HasWonReturnsTrue` | Default board is all-off (solved) |
| 2 | `SetAll_On_NoLightIsOff` | `SetAll(true)` turns every light on |
| 3 | `SetAll_Off_HasWonReturnsTrue` | `SetAll(false)` returns to solved state |
| 4 | `ToggleCell_Center_TogglesItselfAndNeighbors` | Center click toggles 5 cells |
| 5 | `ToggleCell_Corner_OnlyTogglesValidNeighbors` | Corner click toggles 3 cells safely |
| 6 | `ToggleCell_Twice_RestoresOriginalState` | Double-click is a no-op |
| 7 | `Randomize_ProducesNonSolvedBoard` | New puzzle is not already solved |
| 8 | `Randomize_IsSolvable_ByReversingClicks` | Same seed produces same board |
| 9 | `Constructor_ZeroGridSize_ThrowsException` | Rejects grid size 0 |
| 10 | `Constructor_NegativeGridSize_ThrowsException` | Rejects negative grid size |
| 11 | `GridSize_ReturnsCorrectValue` | Property returns correct value |
