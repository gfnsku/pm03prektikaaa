using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleDatabaseWalkthrough
{
    public partial class VyborForm : Form
    {
        public VyborForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Zapros1 zapros1Form = new Zapros1();
            zapros1Form.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Zapros2 zapros2Form = new Zapros2();
            zapros2Form.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Zapros3 zapros3Form = new Zapros3();
            zapros3Form.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Zapros4 zapros4Form = new Zapros4();
            zapros4Form.Show();
        }
    }
}