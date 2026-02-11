using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Blackout
{
    public partial class BlackoutForm : Form
    {
        // ── Constants ────────────────────────────────────────────

        private const int DefaultGridSize = 4;
        private const int MinGridSize = 2;
        private const int MaxGridSize = 10;
        private const int MaxBoardPixels = 600;
        private const int MinButtonSize = 40;
        private const int MaxButtonSize = 100;

        // ── Color themes ─────────────────────────────────────────

        private static readonly (string name, Color on, Color off, Color bg)[] Themes =
        {
            ("Classic",  Color.Blue,           Color.Red,          SystemColors.Control),
            ("Dark",     Color.DarkCyan,       Color.DimGray,      Color.FromArgb(30, 30, 30)),
            ("Neon",     Color.Lime,           Color.Magenta,      Color.Black),
            ("Pastel",   Color.CornflowerBlue, Color.LightCoral,   Color.WhiteSmoke),
        };

        // ── Game state ───────────────────────────────────────────

        private int gridRows, gridCols;
        private int buttonSize;
        private BlackoutGame game;
        private Button[,] gridButtons;
        private readonly Random random = new Random();

        // Current theme colors
        private int currentThemeIndex;
        private Color lightOnColor = Themes[0].on;
        private Color lightOffColor = Themes[0].off;

        // Current toggle pattern
        private TogglePatternType currentPattern = TogglePatternType.Cross;

        // ── UI controls ──────────────────────────────────────────

        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel movesLabel;
        private ToolStripStatusLabel timerLabel;
        private ToolStripStatusLabel patternLabel;
        private ToolStripStatusLabel optimalLabel;
        private ToolStripMenuItem undoMenuItem;

        // ── Timer ────────────────────────────────────────────────

        private Timer gameTimer;
        private int elapsedSeconds;
        private bool timerStarted;

        // ── Keyboard navigation ──────────────────────────────────

        private int selectedRow, selectedCol;
        private bool hasSelection;

        // ── Undo tracking ────────────────────────────────────────

        private bool usedUndo;

        // ── Editor mode ──────────────────────────────────────────

        private bool editorMode;
        private ToolStripMenuItem editorMenuItem;
        private ToolStripMenuItem editorPlayItem;

        // ── Animation ────────────────────────────────────────────

        private Timer animationTimer;
        private Dictionary<(int r, int c), (Color from, Color to, int step)> animations
            = new Dictionary<(int, int), (Color, Color, int)>();
        private const int AnimationSteps = 6;
        private bool animationEnabled = true;
        private ToolStripMenuItem animationToggleItem;

        // ── Hint / Solution ──────────────────────────────────────

        private Timer hintTimer;
        private (int row, int col)? hintCell;
        private int hintFlashCount;
        private List<(int row, int col)> solutionOverlay;
        private int? optimalMoveCount;

        // ── Tutorial ─────────────────────────────────────────────

        private bool tutorialActive;
        private int tutorialStep;
        private Label tutorialLabel;
        private static readonly string[] TutorialSteps =
        {
            "Welcome! Click the highlighted cell to toggle it.",
            "Notice the neighbors toggled too! Click the highlighted cell.",
            "Goal: turn ALL lights off (red). Click the highlighted cell.",
            "Use Ctrl+Z to undo mistakes. Click the highlighted cell.",
            "Great job! You completed the tutorial!",
        };
        private static readonly (int row, int col)[] TutorialTargets =
        {
            (1, 1), (0, 0), (2, 2), (1, 0), (-1, -1)
        };

        // ── Persistence ──────────────────────────────────────────

        private readonly HighScoreManager highScores = new HighScoreManager();
        private readonly AchievementManager achievements = new AchievementManager();

        // ── Toast notification ───────────────────────────────────

        private Label toastLabel;
        private Timer toastTimer;

        // ══════════════════════════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════════════════════════

        public BlackoutForm()
        {
            InitializeComponent();
            BuildMenu();
            BuildStatusBar();
            BuildToast();
            BuildTutorialLabel();

            achievements.AchievementUnlocked += OnAchievementUnlocked;

            gameTimer = new Timer { Interval = 1000 };
            gameTimer.Tick += (s, e) => { elapsedSeconds++; UpdateTimerLabel(); };

            animationTimer = new Timer { Interval = 50 };
            animationTimer.Tick += OnAnimationTick;

            hintTimer = new Timer { Interval = 300 };
            hintTimer.Tick += OnHintTick;

            toastTimer = new Timer { Interval = 3000 };
            toastTimer.Tick += (s, e) => { toastTimer.Stop(); toastLabel.Visible = false; };

            StartNewGame(DefaultGridSize, DefaultGridSize, currentPattern);
        }

        // ══════════════════════════════════════════════════════════
        // Menu
        // ══════════════════════════════════════════════════════════

        private void BuildMenu()
        {
            menuStrip = new MenuStrip();

            // ── Game menu ──
            var gameMenu = new ToolStripMenuItem("&Game");

            // Difficulty presets
            var diffMenu = new ToolStripMenuItem("&Difficulty");
            diffMenu.DropDownItems.Add(new ToolStripMenuItem("Easy (3x3)", null,
                (s, e) => StartNewGame(3, 3, currentPattern)));
            diffMenu.DropDownItems.Add(new ToolStripMenuItem("Medium (5x5)", null,
                (s, e) => StartNewGame(5, 5, currentPattern)));
            diffMenu.DropDownItems.Add(new ToolStripMenuItem("Hard (7x7)", null,
                (s, e) => StartNewGame(7, 7, currentPattern)));
            diffMenu.DropDownItems.Add(new ToolStripSeparator());
            diffMenu.DropDownItems.Add(new ToolStripMenuItem("&Custom...", null,
                (s, e) => PromptAndStartNewGame()) { ShortcutKeys = Keys.Control | Keys.N });
            gameMenu.DropDownItems.Add(diffMenu);

            gameMenu.DropDownItems.Add(new ToolStripSeparator());

            // Undo
            undoMenuItem = new ToolStripMenuItem("&Undo", null, (s, e) => DoUndo())
            {
                ShortcutKeys = Keys.Control | Keys.Z,
                Enabled = false
            };
            gameMenu.DropDownItems.Add(undoMenuItem);

            // Hint
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("&Hint", null,
                (s, e) => ShowHint()) { ShortcutKeys = Keys.Control | Keys.H });

            // Show Solution
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("Show &Solution", null,
                (s, e) => ShowSolution()));

            gameMenu.DropDownItems.Add(new ToolStripSeparator());

            // Save / Load
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("&Save Game", null,
                (s, e) => SaveGame()) { ShortcutKeys = Keys.Control | Keys.S });
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("&Load Game", null,
                (s, e) => LoadGame()) { ShortcutKeys = Keys.Control | Keys.O });

            gameMenu.DropDownItems.Add(new ToolStripSeparator());

            // Puzzle Editor
            editorMenuItem = new ToolStripMenuItem("Puzzle &Editor", null,
                (s, e) => ToggleEditorMode());
            editorPlayItem = new ToolStripMenuItem("&Play from Editor", null,
                (s, e) => PlayFromEditor()) { Visible = false };
            gameMenu.DropDownItems.Add(editorMenuItem);
            gameMenu.DropDownItems.Add(editorPlayItem);

            gameMenu.DropDownItems.Add(new ToolStripSeparator());

            // High Scores
            gameMenu.DropDownItems.Add(new ToolStripMenuItem("High S&cores...", null,
                (s, e) => ShowHighScores()));

            gameMenu.DropDownItems.Add(new ToolStripSeparator());

            gameMenu.DropDownItems.Add(new ToolStripMenuItem("E&xit", null,
                (s, e) => Close()) { ShortcutKeys = Keys.Alt | Keys.F4 });

            // ── Theme menu ──
            var themeMenu = new ToolStripMenuItem("&Theme");
            for (int i = 0; i < Themes.Length; i++)
            {
                int idx = i;
                var item = new ToolStripMenuItem(Themes[i].name, null,
                    (s, e) => ApplyTheme(idx));
                if (i == 0) item.Checked = true;
                themeMenu.DropDownItems.Add(item);
            }

            // ── Pattern menu ──
            var patternMenu = new ToolStripMenuItem("&Pattern");
            foreach (TogglePatternType pt in Enum.GetValues(typeof(TogglePatternType)))
            {
                var p = pt;
                var item = new ToolStripMenuItem(TogglePattern.GetDisplayName(p), null,
                    (s, e) => ApplyPattern(p));
                if (p == TogglePatternType.Cross) item.Checked = true;
                patternMenu.DropDownItems.Add(item);
            }

            // ── Settings menu ──
            var settingsMenu = new ToolStripMenuItem("&Settings");
            animationToggleItem = new ToolStripMenuItem("Enable &Animation", null,
                (s, e) => { animationEnabled = !animationEnabled; animationToggleItem.Checked = animationEnabled; })
            {
                Checked = true
            };
            settingsMenu.DropDownItems.Add(animationToggleItem);

            // ── Help menu ──
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("&Tutorial", null,
                (s, e) => StartTutorial()));
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("&Achievements...", null,
                (s, e) => ShowAchievements()));
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("&About", null,
                (s, e) => MessageBox.Show("Blackout\nA classic puzzle game.",
                    "About", MessageBoxButtons.OK, MessageBoxIcon.Information)));

            menuStrip.Items.Add(gameMenu);
            menuStrip.Items.Add(themeMenu);
            menuStrip.Items.Add(patternMenu);
            menuStrip.Items.Add(settingsMenu);
            menuStrip.Items.Add(helpMenu);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }

        // ══════════════════════════════════════════════════════════
        // Status bar
        // ══════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            statusStrip = new StatusStrip();
            movesLabel = new ToolStripStatusLabel("Moves: 0") { Spring = false };
            timerLabel = new ToolStripStatusLabel("Time: 0:00") { Spring = false };
            patternLabel = new ToolStripStatusLabel("Pattern: Cross (+)") { Spring = false };
            optimalLabel = new ToolStripStatusLabel("") { Spring = true };

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                movesLabel,
                new ToolStripStatusLabel(" | "),
                timerLabel,
                new ToolStripStatusLabel(" | "),
                patternLabel,
                optimalLabel
            });
            Controls.Add(statusStrip);
        }

        private void UpdateMovesLabel()
        {
            movesLabel.Text = $"Moves: {game.MoveCount}";
            undoMenuItem.Enabled = game.CanUndo;
        }

        private void UpdateTimerLabel()
        {
            timerLabel.Text = $"Time: {elapsedSeconds / 60}:{elapsedSeconds % 60:D2}";
        }

        // ══════════════════════════════════════════════════════════
        // Toast notification (for achievements)
        // ══════════════════════════════════════════════════════════

        private void BuildToast()
        {
            toastLabel = new Label
            {
                Visible = false,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Gold,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 30,
                Dock = DockStyle.Top
            };
            Controls.Add(toastLabel);
            toastLabel.BringToFront();
        }

        private void ShowToast(string message)
        {
            toastLabel.Text = message;
            toastLabel.Visible = true;
            toastLabel.BringToFront();
            toastTimer.Stop();
            toastTimer.Start();
        }

        // ══════════════════════════════════════════════════════════
        // Tutorial label
        // ══════════════════════════════════════════════════════════

        private void BuildTutorialLabel()
        {
            tutorialLabel = new Label
            {
                Visible = false,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                Height = 35,
                Dock = DockStyle.Top
            };
            Controls.Add(tutorialLabel);
        }

        // ══════════════════════════════════════════════════════════
        // New Game Dialog (supports rectangular grids)
        // ══════════════════════════════════════════════════════════

        private void PromptAndStartNewGame()
        {
            var result = ShowGridSizeDialog();
            if (result.HasValue)
                StartNewGame(result.Value.rows, result.Value.cols, currentPattern);
        }

        private (int rows, int cols)? ShowGridSizeDialog()
        {
            using (var dialog = new Form())
            {
                dialog.Text = "New Game";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(300, 150);

                var lblRows = new Label { Text = "Rows:", Location = new Point(15, 22), AutoSize = true };
                var numRows = new NumericUpDown
                {
                    Minimum = MinGridSize, Maximum = MaxGridSize,
                    Value = gridRows, Location = new Point(70, 19), Width = 60
                };

                var lblCols = new Label { Text = "Cols:", Location = new Point(150, 22), AutoSize = true };
                var numCols = new NumericUpDown
                {
                    Minimum = MinGridSize, Maximum = MaxGridSize,
                    Value = gridCols, Location = new Point(200, 19), Width = 60
                };

                var chkSquare = new CheckBox
                {
                    Text = "Square", Location = new Point(15, 55),
                    Checked = (gridRows == gridCols), AutoSize = true
                };
                chkSquare.CheckedChanged += (s, e) =>
                {
                    if (chkSquare.Checked) numCols.Value = numRows.Value;
                    numCols.Enabled = !chkSquare.Checked;
                };
                numRows.ValueChanged += (s, e) =>
                {
                    if (chkSquare.Checked) numCols.Value = numRows.Value;
                };

                numCols.Enabled = !chkSquare.Checked;

                var okBtn = new Button
                {
                    Text = "OK", DialogResult = DialogResult.OK,
                    Location = new Point(70, 100), Width = 70
                };
                var cancelBtn = new Button
                {
                    Text = "Cancel", DialogResult = DialogResult.Cancel,
                    Location = new Point(160, 100), Width = 70
                };

                dialog.Controls.AddRange(new Control[]
                    { lblRows, numRows, lblCols, numCols, chkSquare, okBtn, cancelBtn });
                dialog.AcceptButton = okBtn;
                dialog.CancelButton = cancelBtn;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                    return ((int)numRows.Value, (int)numCols.Value);
                return null;
            }
        }

        // ══════════════════════════════════════════════════════════
        // Grid building
        // ══════════════════════════════════════════════════════════

        private void BuildGrid()
        {
            // Remove previous buttons
            if (gridButtons != null)
            {
                for (int row = 0; row < gridButtons.GetLength(0); row++)
                    for (int col = 0; col < gridButtons.GetLength(1); col++)
                    {
                        Controls.Remove(gridButtons[row, col]);
                        gridButtons[row, col].Dispose();
                    }
            }

            // Scale button size to fit on screen
            int maxDim = Math.Max(gridRows, gridCols);
            buttonSize = Math.Max(MinButtonSize, Math.Min(MaxButtonSize, MaxBoardPixels / maxDim));

            gridButtons = new Button[gridRows, gridCols];
            int menuHeight = menuStrip.Height;
            int tutorialHeight = tutorialLabel.Visible ? tutorialLabel.Height : 0;
            int topOffset = menuHeight + tutorialHeight;
            int boardWidth = gridCols * buttonSize;
            int boardHeight = gridRows * buttonSize;
            int statusHeight = statusStrip.Height;

            ClientSize = new Size(
                Math.Max(boardWidth, 300),
                topOffset + boardHeight + statusHeight);

            for (int row = 0; row < gridRows; row++)
            {
                for (int col = 0; col < gridCols; col++)
                {
                    int r = row, c = col;
                    var button = new Button
                    {
                        Bounds = new Rectangle(
                            col * buttonSize,
                            row * buttonSize + topOffset,
                            buttonSize,
                            buttonSize),
                        FlatStyle = FlatStyle.Flat,
                        TabStop = false
                    };
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = Color.Black;
                    button.Click += (sender, e) => OnGridButtonClick(r, c);
                    gridButtons[row, col] = button;
                    Controls.Add(button);
                }
            }
        }

        // ══════════════════════════════════════════════════════════
        // Game flow
        // ══════════════════════════════════════════════════════════

        private void StartNewGame(int rows, int cols, TogglePatternType pattern)
        {
            // Clamp
            rows = Math.Max(MinGridSize, Math.Min(MaxGridSize, rows));
            cols = Math.Max(MinGridSize, Math.Min(MaxGridSize, cols));

            gridRows = rows;
            gridCols = cols;
            currentPattern = pattern;

            game = new BlackoutGame(rows, cols, pattern);
            BuildGrid();
            game.Randomize(random);

            // Reset state
            gameTimer.Stop();
            elapsedSeconds = 0;
            timerStarted = false;
            usedUndo = false;
            hasSelection = false;
            selectedRow = 0;
            selectedCol = 0;
            solutionOverlay = null;
            ClearHint();
            animations.Clear();

            // Calculate optimal moves for this puzzle
            var solution = BlackoutSolver.Solve(game);
            optimalMoveCount = solution?.Count;

            RefreshGrid();
            UpdateMovesLabel();
            UpdateTimerLabel();
            patternLabel.Text = $"Pattern: {TogglePattern.GetDisplayName(pattern)}";
            optimalLabel.Text = optimalMoveCount.HasValue
                ? $"  Optimal: {optimalMoveCount.Value} moves"
                : "";

            string sizeText = (rows == cols) ? $"{rows}x{cols}" : $"{rows}x{cols}";
            Text = editorMode ? $"Blackout ({sizeText}) - Editor" : $"Blackout ({sizeText})";
        }

        private void OnGridButtonClick(int row, int col)
        {
            if (tutorialActive)
            {
                HandleTutorialClick(row, col);
                return;
            }

            if (editorMode)
            {
                // Toggle individual light in editor
                bool current = game.IsLightOn(row, col);
                game.SetLight(row, col, !current);
                RefreshGrid();
                return;
            }

            // Start timer on first click
            if (!timerStarted)
            {
                timerStarted = true;
                gameTimer.Start();
            }

            ClearHint();
            solutionOverlay = null;

            // Capture old colors for animation
            var affectedCells = GetAffectedCells(row, col);
            var oldColors = new Dictionary<(int, int), Color>();
            foreach (var cell in affectedCells)
                oldColors[cell] = gridButtons[cell.r, cell.c].BackColor;

            game.ToggleCell(row, col);

            // Start animations
            if (animationEnabled)
            {
                foreach (var cell in affectedCells)
                {
                    Color newColor = game.IsLightOn(cell.r, cell.c) ? lightOnColor : lightOffColor;
                    if (oldColors[cell] != newColor)
                        animations[cell] = (oldColors[cell], newColor, 0);
                }
                if (animations.Count > 0)
                    animationTimer.Start();
            }

            RefreshGrid();
            UpdateMovesLabel();

            if (game.HasWon())
            {
                gameTimer.Stop();
                animations.Clear();
                animationTimer.Stop();
                RefreshGrid();
                HandleWin();
            }
        }

        private List<(int r, int c)> GetAffectedCells(int row, int col)
        {
            var (rowOff, colOff) = TogglePattern.GetOffsets(currentPattern);
            var cells = new List<(int r, int c)>();
            for (int k = 0; k < rowOff.Length; k++)
            {
                int nr = row + rowOff[k];
                int nc = col + colOff[k];
                if (nr >= 0 && nr < gridRows && nc >= 0 && nc < gridCols)
                    cells.Add((nr, nc));
            }
            return cells;
        }

        private void HandleWin()
        {
            // Record high score
            bool isNewRecord = highScores.RecordScore(
                gridRows, gridCols, game.MoveCount, elapsedSeconds);

            // Check achievements
            achievements.CheckAndUnlock(game, elapsedSeconds, usedUndo, optimalMoveCount);

            string msg = $"You win!\n\nMoves: {game.MoveCount}\nTime: {elapsedSeconds / 60}:{elapsedSeconds % 60:D2}";
            if (optimalMoveCount.HasValue)
                msg += $"\nOptimal: {optimalMoveCount.Value} moves";
            if (isNewRecord)
                msg += "\n\nNew high score!";
            msg += "\n\nPlay again?";

            var result = MessageBox.Show(msg, "Congratulations",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
                PromptAndStartNewGame();
            else
                Close();
        }

        // ══════════════════════════════════════════════════════════
        // Refresh grid
        // ══════════════════════════════════════════════════════════

        private void RefreshGrid()
        {
            for (int row = 0; row < gridRows; row++)
            {
                for (int col = 0; col < gridCols; col++)
                {
                    var btn = gridButtons[row, col];

                    // Animation takes priority
                    if (animations.ContainsKey((row, col)))
                    {
                        var anim = animations[(row, col)];
                        btn.BackColor = LerpColor(anim.from, anim.to, anim.step, AnimationSteps);
                    }
                    else
                    {
                        btn.BackColor = game.IsLightOn(row, col) ? lightOnColor : lightOffColor;
                    }

                    // Keyboard selection highlight
                    if (hasSelection && row == selectedRow && col == selectedCol)
                    {
                        btn.FlatAppearance.BorderColor = Color.Yellow;
                        btn.FlatAppearance.BorderSize = 3;
                    }
                    else
                    {
                        btn.FlatAppearance.BorderColor = Color.Black;
                        btn.FlatAppearance.BorderSize = 1;
                    }

                    // Hint highlight
                    if (hintCell.HasValue && hintCell.Value.row == row && hintCell.Value.col == col
                        && hintFlashCount % 2 == 0)
                    {
                        btn.BackColor = Color.Yellow;
                    }

                    // Solution overlay numbers
                    if (solutionOverlay != null)
                    {
                        int idx = solutionOverlay.FindIndex(s => s.row == row && s.col == col);
                        btn.Text = idx >= 0 ? (idx + 1).ToString() : "";
                        btn.ForeColor = Color.White;
                        btn.Font = new Font("Segoe UI", Math.Max(8, buttonSize / 4), FontStyle.Bold);
                    }
                    else if (!tutorialActive)
                    {
                        btn.Text = "";
                    }

                    // Tutorial highlight
                    if (tutorialActive && tutorialStep < TutorialTargets.Length)
                    {
                        var target = TutorialTargets[tutorialStep];
                        if (target.row == row && target.col == col)
                        {
                            btn.FlatAppearance.BorderColor = Color.Gold;
                            btn.FlatAppearance.BorderSize = 3;
                        }
                    }
                }
            }
        }

        // ══════════════════════════════════════════════════════════
        // Animation
        // ══════════════════════════════════════════════════════════

        private void OnAnimationTick(object sender, EventArgs e)
        {
            var finished = new List<(int, int)>();
            var keys = animations.Keys.ToList();

            foreach (var key in keys)
            {
                var anim = animations[key];
                anim.step++;
                if (anim.step >= AnimationSteps)
                {
                    finished.Add(key);
                    gridButtons[key.r, key.c].BackColor = anim.to;
                }
                else
                {
                    animations[key] = anim;
                    gridButtons[key.r, key.c].BackColor =
                        LerpColor(anim.from, anim.to, anim.step, AnimationSteps);
                }
            }

            foreach (var key in finished)
                animations.Remove(key);

            if (animations.Count == 0)
                animationTimer.Stop();
        }

        private static Color LerpColor(Color from, Color to, int step, int totalSteps)
        {
            float t = (float)step / totalSteps;
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }

        // ══════════════════════════════════════════════════════════
        // Keyboard navigation
        // ══════════════════════════════════════════════════════════

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    MoveSelection(-1, 0);
                    return true;
                case Keys.Down:
                    MoveSelection(1, 0);
                    return true;
                case Keys.Left:
                    MoveSelection(0, -1);
                    return true;
                case Keys.Right:
                    MoveSelection(0, 1);
                    return true;
                case Keys.Enter:
                case Keys.Space:
                    if (hasSelection)
                        OnGridButtonClick(selectedRow, selectedCol);
                    return true;
                case Keys.Escape:
                    hasSelection = false;
                    RefreshGrid();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MoveSelection(int dRow, int dCol)
        {
            if (!hasSelection)
            {
                hasSelection = true;
                selectedRow = 0;
                selectedCol = 0;
            }
            else
            {
                selectedRow = (selectedRow + dRow + gridRows) % gridRows;
                selectedCol = (selectedCol + dCol + gridCols) % gridCols;
            }
            RefreshGrid();
        }

        // ══════════════════════════════════════════════════════════
        // Undo
        // ══════════════════════════════════════════════════════════

        private void DoUndo()
        {
            if (!game.CanUndo) return;
            usedUndo = true;
            game.Undo();
            RefreshGrid();
            UpdateMovesLabel();
        }

        // ══════════════════════════════════════════════════════════
        // Themes
        // ══════════════════════════════════════════════════════════

        private void ApplyTheme(int index)
        {
            currentThemeIndex = index;
            lightOnColor = Themes[index].on;
            lightOffColor = Themes[index].off;
            BackColor = Themes[index].bg;

            // Update radio checks
            var themeMenu = menuStrip.Items[1] as ToolStripMenuItem;
            for (int i = 0; i < themeMenu.DropDownItems.Count; i++)
            {
                if (themeMenu.DropDownItems[i] is ToolStripMenuItem item)
                    item.Checked = (i == index);
            }

            RefreshGrid();
        }

        // ══════════════════════════════════════════════════════════
        // Toggle pattern switching
        // ══════════════════════════════════════════════════════════

        private void ApplyPattern(TogglePatternType pattern)
        {
            currentPattern = pattern;

            // Update radio checks
            var patternMenu = menuStrip.Items[2] as ToolStripMenuItem;
            int idx = 0;
            foreach (TogglePatternType pt in Enum.GetValues(typeof(TogglePatternType)))
            {
                if (patternMenu.DropDownItems[idx] is ToolStripMenuItem item)
                    item.Checked = (pt == pattern);
                idx++;
            }

            StartNewGame(gridRows, gridCols, pattern);
        }

        // ══════════════════════════════════════════════════════════
        // Hints
        // ══════════════════════════════════════════════════════════

        private void ShowHint()
        {
            var hint = BlackoutSolver.GetHint(game);
            if (!hint.HasValue)
            {
                MessageBox.Show("No hint available.", "Hint",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            hintCell = hint;
            hintFlashCount = 0;
            hintTimer.Start();
            RefreshGrid();
        }

        private void OnHintTick(object sender, EventArgs e)
        {
            hintFlashCount++;
            if (hintFlashCount >= 8)
            {
                ClearHint();
            }
            RefreshGrid();
        }

        private void ClearHint()
        {
            hintTimer.Stop();
            hintCell = null;
            hintFlashCount = 0;
        }

        // ══════════════════════════════════════════════════════════
        // Solution display
        // ══════════════════════════════════════════════════════════

        private void ShowSolution()
        {
            var solution = BlackoutSolver.GetStepByStepSolution(game);
            if (solution.Count == 0)
            {
                MessageBox.Show("Already solved or no solution found.", "Solution",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            solutionOverlay = solution;
            RefreshGrid();
            MessageBox.Show($"Solution requires {solution.Count} moves.\n" +
                "Numbers on the grid show the click order.",
                "Solution", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ══════════════════════════════════════════════════════════
        // Save / Load
        // ══════════════════════════════════════════════════════════

        private void SaveGame()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Blackout Save|*.bosave";
                dlg.DefaultExt = "bosave";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var state = GameState.FromGame(game, elapsedSeconds);
                    state.Save(dlg.FileName);
                }
            }
        }

        private void LoadGame()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Blackout Save|*.bosave|Legacy Save|*.losave";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var state = GameState.Load(dlg.FileName);

                        gridRows = state.Rows;
                        gridCols = state.Cols;
                        currentPattern = state.Pattern;

                        game = new BlackoutGame(state.Rows, state.Cols, state.Pattern);
                        game.LoadBoard(state.GetBoard());

                        BuildGrid();
                        elapsedSeconds = state.ElapsedSeconds;
                        timerStarted = false;
                        usedUndo = false;
                        hasSelection = false;
                        solutionOverlay = null;
                        ClearHint();
                        animations.Clear();

                        var solution = BlackoutSolver.Solve(game);
                        optimalMoveCount = solution?.Count;

                        RefreshGrid();
                        UpdateMovesLabel();
                        UpdateTimerLabel();
                        patternLabel.Text = $"Pattern: {TogglePattern.GetDisplayName(currentPattern)}";
                        optimalLabel.Text = optimalMoveCount.HasValue
                            ? $"  Optimal: {optimalMoveCount.Value} moves"
                            : "";

                        string sizeText = $"{gridRows}x{gridCols}";
                        Text = $"Blackout ({sizeText})";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ══════════════════════════════════════════════════════════
        // Puzzle Editor
        // ══════════════════════════════════════════════════════════

        private void ToggleEditorMode()
        {
            editorMode = !editorMode;
            editorMenuItem.Checked = editorMode;
            editorPlayItem.Visible = editorMode;

            if (editorMode)
            {
                gameTimer.Stop();
                game.SetAll(false);
                RefreshGrid();
                UpdateMovesLabel();

                string sizeText = $"{gridRows}x{gridCols}";
                Text = $"Blackout ({sizeText}) - Editor";
            }
            else
            {
                string sizeText = $"{gridRows}x{gridCols}";
                Text = $"Blackout ({sizeText})";
                // Keep current board, let the user start playing
                PlayFromEditor();
            }
        }

        private void PlayFromEditor()
        {
            editorMode = false;
            editorMenuItem.Checked = false;
            editorPlayItem.Visible = false;

            // Reset tracking but keep the board
            usedUndo = false;
            timerStarted = false;
            elapsedSeconds = 0;
            game.LoadBoard(game.GetBoardSnapshot()); // Resets MoveCount/history

            var solution = BlackoutSolver.Solve(game);
            optimalMoveCount = solution?.Count;

            UpdateMovesLabel();
            UpdateTimerLabel();
            optimalLabel.Text = optimalMoveCount.HasValue
                ? $"  Optimal: {optimalMoveCount.Value} moves"
                : "";

            string sizeText = $"{gridRows}x{gridCols}";
            Text = $"Blackout ({sizeText})";
            RefreshGrid();
        }

        // ══════════════════════════════════════════════════════════
        // Tutorial
        // ══════════════════════════════════════════════════════════

        private void StartTutorial()
        {
            tutorialActive = true;
            tutorialStep = 0;

            // Set up a small 3x3 grid with a known state
            currentPattern = TogglePatternType.Cross;
            gridRows = 3;
            gridCols = 3;
            game = new BlackoutGame(3, 3, TogglePatternType.Cross);
            BuildGrid();

            // Set a simple starting pattern
            game.SetAll(false);
            game.SetLight(0, 1, true);
            game.SetLight(1, 0, true);
            game.SetLight(1, 1, true);
            game.SetLight(1, 2, true);
            game.SetLight(2, 1, true);

            tutorialLabel.Visible = true;
            tutorialLabel.Text = TutorialSteps[0];
            BuildGrid(); // Rebuild to account for tutorial label height

            gameTimer.Stop();
            elapsedSeconds = 0;
            timerStarted = false;

            RefreshGrid();
            UpdateMovesLabel();
            UpdateTimerLabel();
            Text = "Blackout - Tutorial";
        }

        private void HandleTutorialClick(int row, int col)
        {
            if (tutorialStep >= TutorialTargets.Length)
                return;

            var target = TutorialTargets[tutorialStep];

            // If no specific target, any click advances
            if (target.row == -1 || (row == target.row && col == target.col))
            {
                if (target.row >= 0)
                    game.ToggleCell(row, col);

                tutorialStep++;

                if (tutorialStep >= TutorialSteps.Length)
                {
                    // Tutorial complete
                    tutorialActive = false;
                    tutorialLabel.Visible = false;
                    StartNewGame(DefaultGridSize, DefaultGridSize, TogglePatternType.Cross);
                    return;
                }

                tutorialLabel.Text = TutorialSteps[tutorialStep];
                RefreshGrid();
            }
        }

        // ══════════════════════════════════════════════════════════
        // Achievements
        // ══════════════════════════════════════════════════════════

        private void OnAchievementUnlocked(Achievement achievement)
        {
            ShowToast($"Achievement Unlocked: {achievement.Name}!");
        }

        private void ShowAchievements()
        {
            var all = achievements.GetAll();
            using (var dialog = new Form())
            {
                dialog.Text = "Achievements";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(400, 300);

                var listView = new ListView
                {
                    Dock = DockStyle.Fill,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true
                };
                listView.Columns.Add("Status", 60);
                listView.Columns.Add("Achievement", 140);
                listView.Columns.Add("Description", 180);

                foreach (var a in all)
                {
                    var item = new ListViewItem(a.Unlocked ? "Unlocked" : "Locked");
                    item.SubItems.Add(a.Name);
                    item.SubItems.Add(a.Description);
                    if (a.Unlocked)
                        item.ForeColor = Color.Green;
                    else
                        item.ForeColor = Color.Gray;
                    listView.Items.Add(item);
                }

                dialog.Controls.Add(listView);
                dialog.ShowDialog(this);
            }
        }

        // ══════════════════════════════════════════════════════════
        // High Scores
        // ══════════════════════════════════════════════════════════

        private void ShowHighScores()
        {
            var scores = highScores.GetAllScores();
            using (var dialog = new Form())
            {
                dialog.Text = "High Scores";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(400, 300);

                var listView = new ListView
                {
                    Dock = DockStyle.Fill,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true
                };
                listView.Columns.Add("Grid", 80);
                listView.Columns.Add("Moves", 80);
                listView.Columns.Add("Time", 80);
                listView.Columns.Add("Date", 140);

                foreach (var s in scores)
                {
                    var item = new ListViewItem($"{s.Rows}x{s.Cols}");
                    item.SubItems.Add(s.Moves.ToString());
                    item.SubItems.Add($"{s.Seconds / 60}:{s.Seconds % 60:D2}");
                    item.SubItems.Add(s.Date);
                    listView.Items.Add(item);
                }

                if (scores.Count == 0)
                {
                    dialog.Controls.Add(new Label
                    {
                        Text = "No high scores yet. Win a game!",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 11)
                    });
                }
                else
                {
                    dialog.Controls.Add(listView);
                }

                dialog.ShowDialog(this);
            }
        }
    }
}
