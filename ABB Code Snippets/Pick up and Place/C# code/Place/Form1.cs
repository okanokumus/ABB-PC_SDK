using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// (4,5,3) -> (X, Y, Z)
namespace ParcaYerlestirme
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "1";
            textBox2.Text = "1";
            textBox3.Text = "1";
        }
        double x = 1, y = 1, z = 1;

        private void button1_Click(object sender, EventArgs e)
        {

            x = x + 1;
            if (x%5 == 0)
            {
                y = y + 1;
                if(y % 6 == 0)
                {
                    y = 1;
                    z = z + 1;
                    if (z == 4)
                    {
                        z = 1;
                    }
                }
                x = 1;
            }
            textBox1.Text = Convert.ToString(x);
            textBox2.Text = Convert.ToString(y);
            textBox3.Text = Convert.ToString(z);
        }
    }
}
