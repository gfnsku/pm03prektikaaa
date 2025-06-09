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

namespace SampleDatabaseWalkthrough
{
    public partial class RegForm : Form
    {
        public RegForm()
        {
            InitializeComponent();
        }

        private void RegButton_Click(object sender, EventArgs e)
        {
            // Проверяем, что все поля заполнены
            if (string.IsNullOrWhiteSpace(IdTxt.Text))
            {
                MessageBox.Show("Введите ID!");
                return;
            }

            if (string.IsNullOrWhiteSpace(FamTxt.Text))
            {
                MessageBox.Show("Введите фамилию!");
                return;
            }

            if (string.IsNullOrWhiteSpace(NameTxt.Text))
            {
                MessageBox.Show("Введите имя!");
                return;
            }

            if (string.IsNullOrWhiteSpace(OtchTxt.Text))
            {
                MessageBox.Show("Введите отчество!");
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginTxt.Text))
            {
                MessageBox.Show("Введите логин!");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordTxt.Text))
            {
                MessageBox.Show("Введите пароль!");
                return;
            }

            if (string.IsNullOrWhiteSpace(RoleTxt.Text))
            {
                MessageBox.Show("Введите роль (1 или 2)!");
                return;
            }

            // Проверяем, что ID и роль - это числа
            int userId, roleId;
            if (!int.TryParse(IdTxt.Text, out userId))
            {
                MessageBox.Show("ID должен быть числом!");
                return;
            }

            if (!int.TryParse(RoleTxt.Text, out roleId))
            {
                MessageBox.Show("Роль должна быть числом (1 или 2)!");
                return;
            }

            // Создаём соединение с базой данных
            SqlConnection con = new SqlConnection(Properties.Settings.Default.SampleDatabaseConnectionString);

            try
            {
                // Сначала проверяем, не существует ли уже пользователь с таким логином
                SqlDataAdapter checkAdap = new SqlDataAdapter("Select * from individ where login='" + LoginTxt.Text + "'", con);
                DataSet checkDs = new DataSet();
                checkAdap.Fill(checkDs, "individ");

                if (checkDs.Tables["individ"].Rows.Count > 0)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }

                // Открываем соединение
                con.Open();

                // Создаём команду для вставки нового пользователя со всеми полями включая ID
                SqlCommand cmd = new SqlCommand("INSERT INTO individ (id_individ, fam, name, otch, login, password, id_role) VALUES (" + userId + ", '" + FamTxt.Text + "', '" + NameTxt.Text + "', '" + OtchTxt.Text + "', '" + LoginTxt.Text + "', '" + PasswordTxt.Text + "', " + roleId + ")", con);

                // Выполняем команду
                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Пользователь успешно зарегистрирован!");

                    // Очищаем поля после успешной регистрации
                    IdTxt.Text = "";
                    FamTxt.Text = "";
                    NameTxt.Text = "";
                    OtchTxt.Text = "";
                    LoginTxt.Text = "";
                    PasswordTxt.Text = "";
                    RoleTxt.Text = "";
                }
                else
                {
                    MessageBox.Show("Ошибка при регистрации пользователя!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message);
            }
            finally
            {
                // Закрываем соединение
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }
}