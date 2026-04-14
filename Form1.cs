using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace comp
{
    public partial class Form1 : Form
    {
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private string currentFileName = "";
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private bool ignoreTextChanges = false;
        private ToolStripComboBox comboBoxSearchType;
        private ToolStripLabel toolStripLabelCount;

        public Form1()
        {
            InitializeComponent();
            ConfigureDataGridView();
            AddSearchControlsToToolStrip();

            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            textBox1.TextChanged += TextBox1_TextChanged;
            InitializeEventHandlers();

        }

        private void ConfigureDataGridView()
        {
            dataGridViewResults.Columns.Clear();
            dataGridViewResults.Columns.Add("Substring", "Найденная подстрока");
            dataGridViewResults.Columns.Add("Position", "Начальная позиция (строка, столбец)");
            dataGridViewResults.Columns.Add("Length", "Длина");
            dataGridViewResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewResults.RowHeadersVisible = false;
            dataGridViewResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void AddSearchControlsToToolStrip()
        {
            comboBoxSearchType = new ToolStripComboBox();
            comboBoxSearchType.Items.AddRange(new string[] { "Числа", "Идентификаторы", "Время" });
            comboBoxSearchType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSearchType.SelectedIndex = 0;
            comboBoxSearchType.ToolTipText = "Выберите тип искомых подстрок";

            var separator = new ToolStripSeparator();
            toolStripLabelCount = new ToolStripLabel("Найдено: 0");

            int insertIndex = toolStrip1.Items.IndexOf(запуск);
            if (insertIndex >= 0)
            {
                toolStrip1.Items.Insert(insertIndex, comboBoxSearchType);
                toolStrip1.Items.Insert(insertIndex + 1, separator);
                toolStrip1.Items.Insert(insertIndex + 2, toolStripLabelCount);
            }
            else
            {
                toolStrip1.Items.Add(comboBoxSearchType);
                toolStrip1.Items.Add(separator);
                toolStrip1.Items.Add(toolStripLabelCount);
            }
        }

        private Regex GetRegexForSelectedType()
        {
            string selected = comboBoxSearchType.SelectedItem?.ToString();
            switch (selected)
            {
                case "Числа":
                    return new Regex(@"-?\d+(?:\.\d+)?", RegexOptions.Compiled);
                case "Идентификаторы":
                    return new Regex(@"(?<!\S)[A-Za-z_$][A-Za-z0-9]*(?!\S)", RegexOptions.Compiled);
                case "Время":
                    return new Regex(@"\b(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]\b", RegexOptions.Compiled);
                default:
                    return null;
            }
        }

        private (int line, int column) GetLineAndColumn(int index, string text)
        {
            int line = 1, column = 1;
            for (int i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
            return (line, column);
        }

        private void SearchAndDisplay()
        {
            dataGridViewResults.Rows.Clear();
            toolStripLabelCount.Text = "Найдено: 0";
            dataGridViewResults.Tag = null;

            string inputText = textBox1.Text;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                MessageBox.Show("Нет данных для поиска. Введите текст в редакторе.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Regex regex = GetRegexForSelectedType();
            if (regex == null) return;

            MatchCollection matches = regex.Matches(inputText);
            if (matches.Count == 0)
            {
                toolStripLabelCount.Text = "Найдено: 0";
                MessageBox.Show("Совпадений не найдено.", "Результат поиска",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var matchInfoList = new List<(int index, int length)>();
            foreach (Match match in matches)
            {
                string value = match.Value;
                int startIndex = match.Index;
                int length = match.Length;
                var (line, column) = GetLineAndColumn(startIndex, inputText);
                string position = $"строка {line}, {column}";
                dataGridViewResults.Rows.Add(value, position, length);
                matchInfoList.Add((startIndex, length));
            }
            dataGridViewResults.Tag = matchInfoList;
            toolStripLabelCount.Text = $"Найдено: {matches.Count}";
        }

        private void InitializeEventHandlers()
        {
            this.запуск.Click += запуск_Click;
            this.dataGridViewResults.CellClick += dataGridViewResults_CellClick;
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void сохранитьToolStripButton_Click(object sender, EventArgs e) => СохранитьФайл();
        private void открытьToolStripButton_Click(object sender, EventArgs e) => ОткрытьФайл();
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e) => СоздатьНовыйФайл();
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e) => СохранитьФайл();
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e) => СохранитьКак();
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) Application.Exit();
        }

        private void создатьToolStripButton_Click(object sender, EventArgs e) => СоздатьНовыйФайл();
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e) => ОткрытьФайл();

        private void копироватьToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
                Clipboard.SetText(textBox1.SelectedText);
        }

        private void вырезатьToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
            {
                Clipboard.SetText(textBox1.SelectedText);
                int selStart = textBox1.SelectionStart;
                string before = textBox1.Text.Substring(0, selStart);
                string after = textBox1.Text.Substring(selStart + textBox1.SelectionLength);
                textBox1.Text = before + after;
                textBox1.SelectionStart = selStart;
            }
        }

        private void вставкаToolStripButton_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clip = Clipboard.GetText();
                int selStart = textBox1.SelectionStart;
                string before = textBox1.Text.Substring(0, selStart);
                string after = textBox1.Text.Substring(selStart + textBox1.SelectionLength);
                textBox1.Text = before + clip + after;
                textBox1.SelectionStart = selStart + clip.Length;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e) // Отмена
        {
            if (undoStack.Count > 0)
            {
                string current = textBox1.Text;
                string prev = undoStack.Pop();
                ignoreTextChanges = true;
                redoStack.Push(current);
                textBox1.Text = prev;
                textBox1.SelectionStart = textBox1.Text.Length;
                ignoreTextChanges = false;
            }
        }

        private void Повтор_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                string redoText = redoStack.Pop();
                undoStack.Push(textBox1.Text);
                ignoreTextChanges = true;
                textBox1.Text = redoText;
                textBox1.SelectionStart = textBox1.Text.Length;
                ignoreTextChanges = false;
            }
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e) => toolStripButton2_Click(sender, e);
        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e) => Повтор_Click(sender, e);
        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e) => копироватьToolStripButton_Click(sender, e);
        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e) => вырезатьToolStripButton_Click(sender, e);
        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e) => вставкаToolStripButton_Click(sender, e);
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e) => УдалитьТекст();
        private void выделитьВсёToolStripMenuItem_Click(object sender, EventArgs e) => ВыделитьВесьТекст();

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Лабораторная работа №4\n\n" +
                "Поиск подстрок с помощью регулярных выражений.\n\n" +
                "1. Числа: целые и с плавающей точкой (разделитель точка).\n" +
                "2. Идентификаторы: начинаются с буквы, $ или _, далее буквы или цифры.\n" +
                "3. Время: ЧЧ:ММ:СС (24-часовой формат, ведущий ноль обязателен).\n\n" +
                "Клик по строке таблицы выделяет найденную подстроку в редакторе.",
                "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Поиск с регулярными выражениями\nВерсия 2.0\n\nМарченко А.Е. АП-326",
                "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e) => вызовСправкиToolStripMenuItem_Click(sender, e);
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Марченко А.Е. АП-326", "Об авторе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void запуск_Click(object sender, EventArgs e)
        {
            SearchAndDisplay();
        }

        private void dataGridViewResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var matchInfoList = dataGridViewResults.Tag as List<(int index, int length)>;
            if (matchInfoList == null || e.RowIndex >= matchInfoList.Count) return;

            var (startIndex, length) = matchInfoList[e.RowIndex];

            if (startIndex < 0 || startIndex + length > textBox1.Text.Length) return;

            textBox1.Focus();
            textBox1.SelectionStart = startIndex;
            textBox1.SelectionLength = length;
            textBox1.ScrollToCaret();
            textBox1.Refresh();
        }

        private void СоздатьНовыйФайл()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult res = MessageBox.Show("Сохранить изменения?", "Новый файл",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes) СохранитьФайл();
                else if (res == DialogResult.Cancel) return;
            }
            textBox1.Clear();
            currentFileName = "";
            dataGridViewResults.Rows.Clear();
            toolStripLabelCount.Text = "Найдено: 0";
        }

        private void СохранитьФайл()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFileName))
                {
                    File.WriteAllText(currentFileName, textBox1.Text);
                    MessageBox.Show("Файл сохранён", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    СохранитьКак();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void СохранитьКак()
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog1.FileName;
                File.WriteAllText(filename, textBox1.Text);
                currentFileName = filename;
                MessageBox.Show("Файл сохранён", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ОткрытьФайл()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                textBox1.Text = File.ReadAllText(filename);
                currentFileName = filename;
                dataGridViewResults.Rows.Clear();
                toolStripLabelCount.Text = "Найдено: 0";
                MessageBox.Show("Файл открыт", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void УдалитьТекст()
        {
            if (textBox1.SelectionLength > 0)
            {
                if (!ignoreTextChanges) undoStack.Push(textBox1.Text);
                int selStart = textBox1.SelectionStart;
                string before = textBox1.Text.Substring(0, selStart);
                string after = textBox1.Text.Substring(selStart + textBox1.SelectionLength);
                ignoreTextChanges = true;
                textBox1.Text = before + after;
                textBox1.SelectionStart = selStart;
                ignoreTextChanges = false;
                redoStack.Clear();
            }
        }

        private void ВыделитьВесьТекст()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.SelectAll();
                textBox1.Focus();
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!ignoreTextChanges && !string.IsNullOrEmpty(textBox1.Text))
            {
                undoStack.Push(textBox1.Text);
                redoStack.Clear();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult result = MessageBox.Show("Сохранить изменения перед выходом?",
                    "Выход", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes) СохранитьФайл();
                else if (result == DialogResult.Cancel) e.Cancel = true;
            }
        }
    }
}