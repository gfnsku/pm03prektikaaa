using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleDatabaseWalkthrough
{
    public partial class Role : Form
    {
        public Role()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
   
        private void ShowDB()
        {
            // Создание нового соединения
            SqlConnection con = new
                SqlConnection(Properties.Settings.Default.SampleDatabaseConnectionString);
            con.Open();
            SqlCommand command = con.CreateCommand();
            command.CommandText = "SELECT * FROM role ORDER BY id_role";
            SqlDataReader dataReader = command.ExecuteReader(); // Выполнение команды и получение объекта для чтения данных
            int ItemIndex = 0; // Инициализация индекса для отслеживания позиции элемента в ListView
            listView1.Items.Clear();
            // Чтение данных построчно
            while (dataReader.Read())
            {
                // FieldCount - возвращает количество столбцов в строке
                for (int i=0;i<dataReader.FieldCount;i++)
                {
                    string st = dataReader.GetValue(i).ToString();
                    switch(i)
                    {
                        case 0: // поле id_role
                            listView1.Items.Add(st); // Item - получает значение столбца в нативном формате
                            break;
                        case 1: // поле role
                            listView1.Items[ItemIndex].SubItems.Add(st);
                            break;
                    }
                }
                ItemIndex++; 
            }
            con.Close();
        }

        private void Role_Load(object sender, EventArgs e)
        {
            ShowDB();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection con = new
       SqlConnection(Properties.Settings.Default.SampleDatabaseConnectionString);
            con.Open();
            SqlCommand command = con.CreateCommand();
            command.CommandText = "INSERT INTO role(id_role,name) VALUES(@id_role,@name)";
            command.Parameters.AddWithValue("@id_role", Convert.ToInt32(textBox1.Text));
            command.Parameters.AddWithValue("@name", textBox2.Text.ToString());
            command.ExecuteNonQuery();
            con.Close();
            textBox1.Clear();
            textBox2.Clear();

            ShowDB();
            textBox1.Focus();
        }
    }
}
