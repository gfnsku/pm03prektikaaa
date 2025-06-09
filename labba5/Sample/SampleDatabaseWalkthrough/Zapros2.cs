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
    public partial class Zapros2 : Form
    {
        private string connectionString = Properties.Settings.Default.SampleDatabaseConnectionString;

        public Zapros2()
        {
            InitializeComponent();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            // Загружаем факультеты в ComboBox
            LoadFakultety();
            // Загружаем все данные изначально
            LoadPrepodavateliData();
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

        private void LoadKafedry(int fakultetId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT kafedra_id, kafedra_name FROM Kafedry WHERE fakultet_id = @fakultetId";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@fakultetId", fakultetId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    checkedListBoxKafedry.DisplayMember = "kafedra_name";
                    checkedListBoxKafedry.ValueMember = "kafedra_id";
                    checkedListBoxKafedry.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки кафедр: " + ex.Message);
            }
        }

        private void LoadAllKafedry()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT kafedra_id, kafedra_name FROM Kafedry";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    checkedListBoxKafedry.DisplayMember = "kafedra_name";
                    checkedListBoxKafedry.ValueMember = "kafedra_id";
                    checkedListBoxKafedry.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки кафедр: " + ex.Message);
            }
        }

        private void comboBoxFakultet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFakultet.SelectedValue != null)
            {
                int fakultetId = (int)comboBoxFakultet.SelectedValue;
                LoadKafedry(fakultetId);
            }
            else
            {
                LoadAllKafedry();
            }
        }

        private void buttonApplyFilter_Click(object sender, EventArgs e)
        {
            LoadPrepodavateliData();
        }

        private void LoadPrepodavateliData()
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

                    dataGridViewPrepodavateli.DataSource = dt;

                    // Обновляем счетчик
                    labelCount.Text = $"Общее количество преподавателей: {dt.Rows.Count}";
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
            query.Append(@"SELECT p.prepod_id, p.fam + ' ' + p.name + ' ' + p.otch AS 'ФИО', 
                          f.fakultet_name AS 'Факультет', 
                          k.kafedra_name AS 'Кафедра', 
                          p.kategoriya AS 'Категория',
                          CASE WHEN p.pol = 'М' THEN 'Мужской' ELSE 'Женский' END AS 'Пол',
                          p.yob AS 'Год рождения', 
                          p.vozrast AS 'Возраст',
                          CASE WHEN p.deti = 1 THEN 'Есть' ELSE 'Нет' END AS 'Дети',
                          p.zarplata AS 'Заработная плата',
                          CASE WHEN p.aspirant = 1 THEN 'Да' ELSE 'Нет' END AS 'Аспирант',
                          CASE WHEN p.kand_data IS NOT NULL THEN 'Да' ELSE 'Нет' END AS 'Кандидат наук',
                          p.kand_data AS 'Дата защиты канд.',
                          CASE WHEN p.doktor_data IS NOT NULL THEN 'Да' ELSE 'Нет' END AS 'Доктор наук',
                          p.doktor_data AS 'Дата защиты докт.'
                          FROM Prepodavateli p 
                          INNER JOIN Fakultety f ON p.fakultet_id = f.fakultet_id 
                          INNER JOIN Kafedry k ON p.kafedra_id = k.kafedra_id 
                          WHERE 1=1");

            List<string> conditions = new List<string>();

            // Фильтр по факультету
            if (comboBoxFakultet.SelectedValue != null)
            {
                conditions.Add($"p.fakultet_id = {comboBoxFakultet.SelectedValue}");
            }

            // Фильтр по кафедрам
            var selectedKafedry = GetSelectedKafedry();
            if (selectedKafedry.Count > 0)
            {
                string kafedraIds = string.Join(",", selectedKafedry);
                conditions.Add($"p.kafedra_id IN ({kafedraIds})");
            }

            // Фильтр по категориям
            var selectedKategorii = GetSelectedKategorii();
            if (selectedKategorii.Count > 0)
            {
                string kategoriiStr = string.Join("','", selectedKategorii);
                conditions.Add($"p.kategoriya IN ('{kategoriiStr}')");
            }

            // Фильтр по полу
            if (radioButtonMale.Checked)
                conditions.Add("p.pol = 'М'");
            else if (radioButtonFemale.Checked)
                conditions.Add("p.pol = 'Ж'");

            // Фильтр по году рождения
            if (!string.IsNullOrWhiteSpace(textBoxYearFrom.Text) && !string.IsNullOrWhiteSpace(textBoxYearTo.Text))
            {
                conditions.Add($"p.yob BETWEEN {textBoxYearFrom.Text} AND {textBoxYearTo.Text}");
            }

            // Фильтр по возрасту
            if (!string.IsNullOrWhiteSpace(textBoxAgeFrom.Text) && !string.IsNullOrWhiteSpace(textBoxAgeTo.Text))
            {
                conditions.Add($"p.vozrast BETWEEN {textBoxAgeFrom.Text} AND {textBoxAgeTo.Text}");
            }

            // Фильтр по наличию детей
            if (checkBoxHasChildren.Checked && !checkBoxNoChildren.Checked)
                conditions.Add("p.deti = 1");
            else if (checkBoxNoChildren.Checked && !checkBoxHasChildren.Checked)
                conditions.Add("p.deti = 0");

            // Фильтр по зарплате
            if (!string.IsNullOrWhiteSpace(textBoxZarplataFrom.Text) && !string.IsNullOrWhiteSpace(textBoxZarplataTo.Text))
            {
                conditions.Add($"p.zarplata BETWEEN {textBoxZarplataFrom.Text} AND {textBoxZarplataTo.Text}");
            }

            // Фильтр по аспирантам
            if (checkBoxAspirant.Checked)
                conditions.Add("p.aspirant = 1");

            // Фильтр по кандидатам наук
            if (checkBoxKandNauk.Checked)
                conditions.Add("p.kand_data IS NOT NULL");

            // Фильтр по докторам наук
            if (checkBoxDoktorNauk.Checked)
                conditions.Add("p.doktor_data IS NOT NULL");

            // Фильтр по периоду защиты кандидатской
            if (!string.IsNullOrWhiteSpace(textBoxKandFrom.Text) && !string.IsNullOrWhiteSpace(textBoxKandTo.Text))
            {
                conditions.Add($"p.kand_data BETWEEN '{textBoxKandFrom.Text}' AND '{textBoxKandTo.Text}'");
            }

            // Фильтр по периоду защиты докторской
            if (!string.IsNullOrWhiteSpace(textBoxDoktorFrom.Text) && !string.IsNullOrWhiteSpace(textBoxDoktorTo.Text))
            {
                conditions.Add($"p.doktor_data BETWEEN '{textBoxDoktorFrom.Text}' AND '{textBoxDoktorTo.Text}'");
            }

            // Добавляем условия к запросу
            if (conditions.Count > 0)
            {
                query.Append(" AND " + string.Join(" AND ", conditions));
            }

            return query.ToString();
        }

        private List<int> GetSelectedKafedry()
        {
            List<int> selectedKafedry = new List<int>();
            for (int i = 0; i < checkedListBoxKafedry.CheckedItems.Count; i++)
            {
                DataRowView item = (DataRowView)checkedListBoxKafedry.CheckedItems[i];
                selectedKafedry.Add((int)item["kafedra_id"]);
            }
            return selectedKafedry;
        }

        private List<string> GetSelectedKategorii()
        {
            List<string> selectedKategorii = new List<string>();
            if (checkBoxKategoriyaAssistent.Checked) selectedKategorii.Add("Ассистент");
            if (checkBoxKategoriyaDocent.Checked) selectedKategorii.Add("Доцент");
            if (checkBoxKategoriyaProfessor.Checked) selectedKategorii.Add("Профессор");
            return selectedKategorii;
        }

        private void buttonClearFilters_Click(object sender, EventArgs e)
        {
            // Очищаем все фильтры
            comboBoxFakultet.SelectedIndex = -1;

            for (int i = 0; i < checkedListBoxKafedry.Items.Count; i++)
                checkedListBoxKafedry.SetItemChecked(i, false);

            checkBoxKategoriyaAssistent.Checked = false;
            checkBoxKategoriyaDocent.Checked = false;
            checkBoxKategoriyaProfessor.Checked = false;

            radioButtonAll.Checked = true;

            textBoxYearFrom.Clear();
            textBoxYearTo.Clear();
            textBoxAgeFrom.Clear();
            textBoxAgeTo.Clear();

            checkBoxHasChildren.Checked = false;
            checkBoxNoChildren.Checked = false;

            textBoxZarplataFrom.Clear();
            textBoxZarplataTo.Clear();

            checkBoxAspirant.Checked = false;
            checkBoxKandNauk.Checked = false;
            checkBoxDoktorNauk.Checked = false;

            textBoxKandFrom.Clear();
            textBoxKandTo.Clear();
            textBoxDoktorFrom.Clear();
            textBoxDoktorTo.Clear();

            // Перезагружаем данные
            LoadPrepodavateliData();
        }
    }
}