using System;
using System.Windows.Forms;

namespace ParcaAlma
{
    public partial class Form1 : Form
    {
        int x, y, z, ctr;

        #region FORM
        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            init();
        }
        #endregion

        #region pıck up from table
        private void button1_Click(object sender, EventArgs e)
        {

            x = x - 1;
            if (x == 0)
            {
                y = y - 1;
                if (y ==0 )
                {
                    y = 5;
                    z = z - 1;
                    if (z == 0 )
                    {
                        z = 3;
                    }
                }
                x = 4;
            }

            // her parca alma sonrasında toplam parcayi bir azalt
            ctr = ctr - 1;
            if (ctr < 0)
            {
                MessageBox.Show("Arabadaki Parcalar Bitti", "BİLGİ");
                ctr = 0;
            }
            textBox4.Text = Convert.ToString(ctr);

            textBox1.Text = Convert.ToString(x);
            textBox2.Text = Convert.ToString(y);
            textBox3.Text = Convert.ToString(z);
        }
#endregion

        #region reset all button
        private void button2_Click(object sender, EventArgs e)
        {
            init();
        }
        #endregion

        #region initial condition and reset
        void init()
        {
            textBox1.Text = "4";
            textBox2.Text = "5";
            textBox3.Text = "3";
            textBox4.Text = "60";
            x = Convert.ToInt16(textBox1.Text);
            y = Convert.ToInt16(textBox2.Text);
            z = Convert.ToInt16(textBox3.Text);
            ctr = Convert.ToInt16(textBox4.Text);
        }
        #endregion
    }
}
