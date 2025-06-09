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
    public partial class Zapros1 : Form
    {
        private string connectionString = Properties.Settings.Default.SampleDatabaseConnectionString;

        public Zapros1()
        {
            InitializeComponent();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            // Загружаем факультеты в ComboBox
            LoadFakultety();
            // Загружаем все данные изначально
            LoadStudentsData();
        }

        private void LoadFakultety()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT fakultet_id, fakultet_name FROM Fakultety";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBoxFakultet.DisplayMember = "fakultet_name";
                    comboBoxFakultet.ValueMember = "fakultet_id";
                    comboBoxFakultet.DataSource = dt;
                    comboBoxFakultet.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки факультетов: " + ex.Message);
            }
        }

        private void LoadGruppy(int fakultetId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT grup_id, grup_name FROM Gruppy WHERE fakultet_id = @fakultetId";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@fakultetId", fakultetId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    checkedListBoxGruppy.DisplayMember = "grup_name";
                    checkedListBoxGruppy.ValueMember = "grup_id";
                    checkedListBoxGruppy.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки групп: " + ex.Message);
            }
        }

        private void comboBoxFakultet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFakultet.SelectedValue != null)
            {
                int fakultetId = (int)comboBoxFakultet.SelectedValue;
                LoadGruppy(fakultetId);
            }
        }

        private void buttonApplyFilter_Click(object sender, EventArgs e)
        {
            LoadStudentsData();
        }

        private void LoadStudentsData()
        {
            try
            {
                string query = BuildQuery();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridViewStudents.DataSource = dt;

                    // Обновляем счетчик
                    labelCount.Text = $"Общее количество студентов: {dt.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private string BuildQuery()
        {
            StringBuilder query = new StringBuilder();
            query.Append(@"SELECT s.student_id, s.fam + ' ' + s.name + ' ' + s.otch AS 'ФИО', 
                          f.fakultet_name AS 'Факультет', 
                          g.grup_name AS 'Группа', 
                          s.kurs AS 'Курс',
                          CASE WHEN s.pol = 'М' THEN 'Мужской' ELSE 'Женский' END AS 'Пол',
                          s.yob AS 'Год рождения', 
                          s.vozrast AS 'Возраст',
                          CASE WHEN s.deti = 1 THEN 'Есть' ELSE 'Нет' END AS 'Дети',
                          CASE WHEN s.stipendiya = 1 THEN 'Да' ELSE 'Нет' END AS 'Стипендия',
                          s.stipendiya_sum AS 'Размер стипендии'
                          FROM Students s 
                          INNER JOIN Fakultety f ON s.fakultet_id = f.fakultet_id 
                          INNER JOIN Gruppy g ON s.grup_id = g.grup_id 
                          WHERE 1=1");

            List<string> conditions = new List<string>();

            // Фильтр по факультету
            if (comboBoxFakultet.SelectedValue != null)
            {
                conditions.Add($"s.fakultet_id = {comboBoxFakultet.SelectedValue}");
            }

            // Фильтр по группам
            var selectedGroups = GetSelectedGroups();
            if (selectedGroups.Count > 0)
            {
                string groupIds = string.Join(",", selectedGroups);
                conditions.Add($"s.grup_id IN ({groupIds})");
            }

            // Фильтр по курсам
            var selectedCourses = GetSelectedCourses();
            if (selectedCourses.Count > 0)
            {
                string courses = string.Join(",", selectedCourses);
                conditions.Add($"s.kurs IN ({courses})");
            }

            // Фильтр по полу
            if (radioButtonMale.Checked)
                conditions.Add("s.pol = 'М'");
            else if (radioButtonFemale.Checked)
                conditions.Add("s.pol = 'Ж'");

            // Фильтр по году рождения
            if (!string.IsNullOrWhiteSpace(textBoxYearFrom.Text) && !string.IsNullOrWhiteSpace(textBoxYearTo.Text))
            {
                conditions.Add($"s.yob BETWEEN {textBoxYearFrom.Text} AND {textBoxYearTo.Text}");
            }

            // Фильтр по возрасту
            if (!string.IsNullOrWhiteSpace(textBoxAgeFrom.Text) && !string.IsNullOrWhiteSpace(textBoxAgeTo.Text))
            {
                conditions.Add($"s.vozrast BETWEEN {textBoxAgeFrom.Text} AND {textBoxAgeTo.Text}");
            }

            // Фильтр по наличию детей
            if (checkBoxHasChildren.Checked)
                conditions.Add("s.deti = 1");
            if (checkBoxNoChildren.Checked)
                conditions.Add("s.deti = 0");

            // Фильтр по стипендии
            if (checkBoxHasStipendiya.Checked)
                conditions.Add("s.stipendiya = 1");
            if (checkBoxNoStipendiya.Checked)
                conditions.Add("s.stipendiya = 0");

            // Фильтр по размеру стипендии
            if (!string.IsNullOrWhiteSpace(textBoxStipendiyaFrom.Text) && !string.IsNullOrWhiteSpace(textBoxStipendiyaTo.Text))
            {
                conditions.Add($"s.stipendiya_sum BETWEEN {textBoxStipendiyaFrom.Text} AND {textBoxStipendiyaTo.Text}");
            }

            // Добавляем условия к запросу
            if (conditions.Count > 0)
            {
                query.Append(" AND " + string.Join(" AND ", conditions));
            }

            return query.ToString();
        }

        private List<int> GetSelectedGroups()
        {
            List<int> selectedGroups = new List<int>();
            for (int i = 0; i < checkedListBoxGruppy.CheckedItems.Count; i++)
            {
                DataRowView item = (DataRowView)checkedListBoxGruppy.CheckedItems[i];
                selectedGroups.Add((int)item["grup_id"]);
            }
            return selectedGroups;
        }

        private List<int> GetSelectedCourses()
        {
            List<int> selectedCourses = new List<int>();
            if (checkBoxKurs1.Checked) selectedCourses.Add(1);
            if (checkBoxKurs2.Checked) selectedCourses.Add(2);
            if (checkBoxKurs3.Checked) selectedCourses.Add(3);
            if (checkBoxKurs4.Checked) selectedCourses.Add(4);
            return selectedCourses;
        }

        private void buttonClearFilters_Click(object sender, EventArgs e)
        {
            // Очищаем все фильтры
            comboBoxFakultet.SelectedIndex = -1;

            for (int i = 0; i < checkedListBoxGruppy.Items.Count; i++)
                checkedListBoxGruppy.SetItemChecked(i, false);

            checkBoxKurs1.Checked = false;
            checkBoxKurs2.Checked = false;
            checkBoxKurs3.Checked = false;
            checkBoxKurs4.Checked = false;

            radioButtonAll.Checked = true;

            textBoxYearFrom.Clear();
            textBoxYearTo.Clear();
            textBoxAgeFrom.Clear();
            textBoxAgeTo.Clear();

            checkBoxHasChildren.Checked = false;
            checkBoxNoChildren.Checked = false;

            checkBoxHasStipendiya.Checked = false;
            checkBoxNoStipendiya.Checked = false;

            textBoxStipendiyaFrom.Clear();
            textBoxStipendiyaTo.Clear();

            // Перезагружаем данные
            LoadStudentsData();
        }

        private void buttonClearFilters_Click_1(object sender, EventArgs e)
        {
            buttonClearFilters_Click(sender, e);
        }

        private void buttonApplyFilter_Click_1(object sender, EventArgs e)
        {
            buttonApplyFilter_Click(sender, e); 
        }
    }
}