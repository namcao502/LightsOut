using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp_LightsOut
{
    public partial class LightsOut : Form
    {
        // Grid size limits and default
        private const int DefaultGridSize = 4;
        private const int MinGridSize = 2;
        private const int MaxGridSize = 10;
        private const int MaxBoardPixels = 600;

        // Colors representing light states
        private static readonly Color LightOnColor = Color.Blue;
        private static readonly Color LightOffColor = Color.Red;

        // Mutable game state
        private int gridSize;
        private int buttonSize;
        private LightsOutGame game;
        private Button[,] gridButtons;
        private readonly Random random = new Random();

        // Menu
        private MenuStrip menuStrip;

        public LightsOut()
        {
            InitializeComponent();
            BuildMenu();
            StartNewGame(DefaultGridSize);
        }

        // ── Menu ────────────────────────────────────────────────

        /// <summary>
        /// Creates the Game menu with New Game and Exit items.
        /// </summary>
        private void BuildMenu()
        {
            menuStrip = new MenuStrip();
            var gameMenu = new ToolStripMenuItem("&Game");

            var newGameItem = new ToolStripMenuItem("&New Game...", null, (s, e) => PromptAndStartNewGame())
            {
                ShortcutKeys = Keys.Control | Keys.N
            };

            var exitItem = new ToolStripMenuItem("E&xit", null, (s, e) => Close())
            {
                ShortcutKeys = Keys.Alt | Keys.F4
            };

            gameMenu.DropDownItems.Add(newGameItem);
            gameMenu.DropDownItems.Add(new ToolStripSeparator());
            gameMenu.DropDownItems.Add(exitItem);

            menuStrip.Items.Add(gameMenu);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }

        // ── New Game Dialog ─────────────────────────────────────

        /// <summary>
        /// Shows a dialog asking for grid size, then starts a new game.
        /// </summary>
        private void PromptAndStartNewGame()
        {
            int? size = ShowGridSizeDialog();
            if (size.HasValue)
                StartNewGame(size.Value);
        }

        /// <summary>
        /// Displays a small dialog with a NumericUpDown for choosing grid size.
        /// Returns the chosen value, or null if the user cancels.
        /// </summary>
        private int? ShowGridSizeDialog()
        {
            using (var dialog = new Form())
            {
                dialog.Text = "New Game";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(260, 110);

                var label = new Label
                {
                    Text = $"Grid size ({MinGridSize}–{MaxGridSize}):",
                    Location = new Point(15, 22),
                    AutoSize = true
                };

                var numeric = new NumericUpDown
                {
                    Minimum = MinGridSize,
                    Maximum = MaxGridSize,
                    Value = gridSize,
                    Location = new Point(175, 19),
                    Width = 60
                };

                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(55, 65),
                    Width = 70
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(135, 65),
                    Width = 70
                };

                dialog.Controls.AddRange(new Control[] { label, numeric, okButton, cancelButton });
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                    return (int)numeric.Value;

                return null;
            }
        }

        // ── Grid ────────────────────────────────────────────────

        /// <summary>
        /// Removes old buttons, creates a new grid, and resizes the form.
        /// </summary>
        private void BuildGrid()
        {
            // Remove previous buttons if any
            if (gridButtons != null)
            {
                for (int row = 0; row < gridButtons.GetLength(0); row++)
                    for (int col = 0; col < gridButtons.GetLength(1); col++)
                    {
                        Controls.Remove(gridButtons[row, col]);
                        gridButtons[row, col].Dispose();
                    }
            }

            // Scale button size so the board fits comfortably on screen
            buttonSize = Math.Max(40, Math.Min(100, MaxBoardPixels / gridSize));

            gridButtons = new Button[gridSize, gridSize];
            int menuHeight = menuStrip.Height;
            int boardPixels = gridSize * buttonSize;

            ClientSize = new Size(boardPixels, boardPixels + menuHeight);

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    int r = row, c = col; // Capture for closure
                    var button = new Button
                    {
                        Bounds = new Rectangle(
                            col * buttonSize,
                            row * buttonSize + menuHeight,
                            buttonSize,
                            buttonSize),
                        FlatStyle = FlatStyle.Flat
                    };
                    button.Click += (sender, e) => OnGridButtonClick(r, c);
                    gridButtons[row, col] = button;
                    Controls.Add(button);
                }
            }
        }

        // ── Game Flow ───────────────────────────────────────────

        /// <summary>
        /// Starts a new game with the given grid size.
        /// </summary>
        private void StartNewGame(int newGridSize)
        {
            gridSize = newGridSize;
            game = new LightsOutGame(gridSize);
            BuildGrid();
            game.Randomize(random);
            RefreshGrid();
            Text = $"Lights Out ({gridSize}x{gridSize})";
        }

        /// <summary>
        /// Handles a grid button click: toggles the cell and checks for a win.
        /// </summary>
        private void OnGridButtonClick(int row, int col)
        {
            game.ToggleCell(row, col);
            RefreshGrid();

            if (game.HasWon())
            {
                var result = MessageBox.Show(
                    "You win! Play again?",
                    "Congratulations",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    PromptAndStartNewGame();
                else
                    Close();
            }
        }

        /// <summary>
        /// Syncs button colors with the game model.
        /// </summary>
        private void RefreshGrid()
        {
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    gridButtons[row, col].BackColor =
                        game.IsLightOn(row, col) ? LightOnColor : LightOffColor;
        }
    }
}
