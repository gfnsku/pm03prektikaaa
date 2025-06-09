using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace SampleDatabaseWalkthrough
{
    public partial class Avtoriz : Form
    {
        public Avtoriz()
        {
            InitializeComponent();
        }
        private void Avtoriz_Load(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Создаём новое соединение с базой данных, исползуя строку подключения из настроек приложения
            SqlConnection con = new
                SqlConnection(Properties.Settings.Default.SampleDatabaseConnectionString);

            // Создаём SqlDataAdapter для выполнения SQL-запроса на выборку данных пользователя по логину и паролю
            SqlDataAdapter adap = new SqlDataAdapter("Select * from individ where login='" + textBox1.Text + "'" + " and password ='" + textBox2.Text + "'", con);

            // Создаём DataSet для хранения результатов запроса
            DataSet ds = new DataSet();

            // Заполняем DataSet данными из базы данных

            adap.Fill(ds, "individ");

            // Проверяем есть ли текст кнопки "Войти"

            if (button1.Text == "Войти")
            {
                if (ds.Tables["individ"].Rows.Count != 0)
                {
                    for (int i = 0; i < ds.Tables["individ"].Rows.Count; i++)
                    {

                        // Проверяем есть ли роль у пользователя

                        if (ds.Tables["individ"].Rows[i]["id_role"].ToString() != "")
                        {

                            //Меняем текст кнопки на "Выйти" и скрываем элементы управления для ввода логина и пароля

                            button1.Text = "Выйти";
                            label1.Visible = false;
                            label2.Visible = false;
                            textBox1.Visible = false;
                            textBox2.Visible = false;

                            // Открываем форму VyborForm для всех пользователей независимо от роли
                            Form frm = new VyborForm();
                            frm.Show();
                            frm.Left = this.Left;
                            frm.Top = this.Top;
                            this.Hide(); // Скрываем текущую форму

                            // Получаем информацию о роли пользователя из таблицы "role" 

                            SqlDataAdapter adap2 = new
            SqlDataAdapter("Select * from role where id_role='" + ds.Tables["individ"].Rows[i]["id_role"].ToString().Trim() + "'", con);
                            DataSet ds2 = new DataSet();

                            // Заполняем новый DataSet данными о роли

                            adap2.Fill(ds2, "role");
                            // Отображаем роль пользователя
                            label5.Text = ds2.Tables["role"].Rows[i]["role"].ToString().Trim();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Логин и пароль не верны!");
                }
            }
            else
            {

                // Если текст кнопки не "Войти", возвращаем интерфейс к состоянию входа

                button1.Text = "Войти";
                label1.Visible = true;
                label2.Visible = true;
                textBox1.Visible = true;
                textBox2.Visible = true;
                label5.Text = "";
                textBox1.Text = "";
                textBox2.Text = "";
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            RegForm regform = new RegForm();
            regform.ShowDialog();
        }
    }
}