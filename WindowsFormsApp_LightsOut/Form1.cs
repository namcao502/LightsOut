using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp_LightsOut
{
    public partial class LightsOut : Form
    {
        private static int n = 4;
        private Button[,] button = new Button[n,n];
        private Color normalColor = Color.Blue;
        private Color winningColor = Color.Red;
        public LightsOut()
        {
            InitializeComponent();
            PrepareUI();
        }
        public void PrepareUI()
        {
            this.SetBounds(400, 500, n*100 + 15, n*100 + 40);
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                {
                    int temp_i = i, temp_j = j;
                    Button temp = new Button();
                    temp.SetBounds(i * 100, j * 100, 100, 100);
                    temp.BackColor = Color.Blue;
                    button[i, j] = temp;
                    button[i, j].Click += new EventHandler(ButtonClick);
                    void ButtonClick(object sender, EventArgs e)
                    {
                        ClickPerform(button, temp_i, temp_j);
                        CheckWin();
                    }
                    this.Controls.Add(button[i,j]);
                }
        }

        public void ClickPerform(Button[,] button, int i, int j)
        {
            //Biến = ( điều kiện )? (Lệnh1 thực thi nếu đk đúng) : (lệnh 2 thực thi nếu đk sai);
            int[] row = new int[] { -1, 0, 1, 0, 0 };
            int[] col = new int[] { 0, 1, 0, -1, 0 };
            int length = row.Length;
            for (int k = 0; k < length; ++k)
                if (i + row[k] >= 0 && i + row[k] < n && j + col[k] >= 0 && j + col[k] < n)
                    button[i + row[k], j + col[k]].BackColor = (button[i + row[k], j + col[k]].BackColor == normalColor) ?
                                                               (button[i + row[k], j + col[k]].BackColor = winningColor) :
                                                               (button[i + row[k], j + col[k]].BackColor = normalColor);
        }

        public void CheckWin()
        {
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    if (button[i, j].BackColor == normalColor)
                        return;
            DialogResult message = MessageBox.Show("You win!!!", "Notification");
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    button[i, j].BackColor = normalColor;
        }
    }
}
