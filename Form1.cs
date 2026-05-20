using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

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
        private string lastTextState = "";
        private LexicalAnalyzer analyzer = new LexicalAnalyzer();

        public Form1()
        {
            InitializeComponent();

            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();

            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            textBox1.TextChanged += TextBox1_TextChanged;

            InitializeEventHandlers();
            ConfigureSemanticErrorsGrid();

            dataGridViewSyntaxErrors.CellClick += DataGridViewSyntaxErrors_CellClick;

            CreateTextMenu();
        }
        private void CreateTextMenu()
        {
            ToolStripMenuItem текстToolStripMenuItem = new ToolStripMenuItem();
            текстToolStripMenuItem.Name = "текстToolStripMenuItem";
            текстToolStripMenuItem.Text = "Текст";

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Постановка задачи", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.b9f515lw7ahn")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Грамматика", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.f1khvv1vfj5j")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Классификация грамматики", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.tfyplo3nuu5i")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Метод анализа", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.w5nko5s1rdka")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Диагностика и нейтрализация ошибок", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.cio90abaqrj9")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Тестовый пример", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.kffipbewqu7x")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Список литературы", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.xbcgfyq7fdyh")
            );

            текстToolStripMenuItem.DropDownItems.Add(
                CreateGoogleDocMenuItem("Исходный код программы", "https://docs.google.com/document/d/1OGdS2wuu9XFbennB3lYzXE29VhGk_S1jTFkbgTgGAAw/edit?hl=ru&pli=1&tab=t.0#heading=h.eftxcd90bxqm")
            );

            menuStrip1.Items.Add(текстToolStripMenuItem);
        }
        private ToolStripMenuItem CreateGoogleDocMenuItem(string title, string url)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = title;
            item.Tag = url;
            item.Click += GoogleDocMenuItem_Click;

            return item;
        }
        private void GoogleDocMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;

                if (item == null || item.Tag == null)
                    return;

                string url = item.Tag.ToString();

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось открыть Google Документ:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void InitializeEventHandlers()
        {
            if (сохранитьToolStripButton != null)
                сохранитьToolStripButton.Click -= сохранитьToolStripButton_Click;
            сохранитьToolStripButton.Click += сохранитьToolStripButton_Click;

            if (открытьToolStripButton != null)
                открытьToolStripButton.Click -= открытьToolStripButton_Click;
            открытьToolStripButton.Click += открытьToolStripButton_Click;

            if (сохранитьToolStripMenuItem != null)
                сохранитьToolStripMenuItem.Click -= сохранитьToolStripMenuItem_Click;
            сохранитьToolStripMenuItem.Click += сохранитьToolStripMenuItem_Click;

            if (создатьToolStripMenuItem != null)
                создатьToolStripMenuItem.Click -= создатьToolStripMenuItem_Click;
            создатьToolStripMenuItem.Click += создатьToolStripMenuItem_Click;
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void сохранитьToolStripButton_Click(object sender, EventArgs e) => СохранитьФайл();
        private void открытьToolStripButton_Click(object sender, EventArgs e) => ОткрытьФайл();
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e) => СоздатьНовыйФайл();

        private void СоздатьНовыйФайл()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult result = MessageBox.Show(
                    "Сохранить изменения перед созданием нового файла?",
                    "Создание нового файла",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (!string.IsNullOrEmpty(currentFileName))
                    {
                        СохранитьФайл();
                    }
                    else
                    {
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            string filename = saveFileDialog1.FileName;
                            File.WriteAllText(filename, textBox1.Text);
                            currentFileName = filename;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            saveFileDialog1.FileName = "Новый документ.txt";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string newFileName = saveFileDialog1.FileName;
                    File.WriteAllText(newFileName, "");

                    textBox1.Clear();
                    ResetUndoRedoHistory();
                    dataGridViewResults.Rows.Clear();
                    dataGridViewSyntaxErrors.Rows.Clear();
                    labelErrorCount.Text = "Общее количество ошибок: 0";
                    currentFileName = newFileName;

                    MessageBox.Show($"Новый файл создан и сохранен как:\n{newFileName}",
                        "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e) => СохранитьФайл();

        private void СохранитьФайл()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFileName))
                {
                    File.WriteAllText(currentFileName, textBox1.Text);
                    MessageBox.Show("Файл сохранен", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    СохранитьКак();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void СохранитьКак()
        {
            saveFileDialog1.FileName = currentFileName;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filename = saveFileDialog1.FileName;
                    File.WriteAllText(filename, textBox1.Text);
                    currentFileName = filename;
                    MessageBox.Show("Файл сохранен", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ОткрытьФайл()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filename = openFileDialog1.FileName;
                    string fileText = File.ReadAllText(filename);
                    textBox1.Text = fileText;
                    ResetUndoRedoHistory();
                    currentFileName = filename;
                    dataGridViewResults.Rows.Clear();
                    dataGridViewSyntaxErrors.Rows.Clear();
                    labelErrorCount.Text = "Общее количество ошибок: 0";
                    MessageBox.Show("Файл открыт", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult result = MessageBox.Show(
                    "Сохранить изменения перед выходом?",
                    "Выход из программы",
                    MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    СохранитьФайл();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void копироватьToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
                Clipboard.SetText(textBox1.SelectedText);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите выйти из программы?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    DialogResult saveResult = MessageBox.Show(
                        "У вас есть несохраненные изменения. Сохранить перед выходом?",
                        "Несохраненные изменения",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);

                    if (saveResult == DialogResult.Yes)
                    {
                        СохранитьФайл();
                        Application.Exit();
                    }
                    else if (saveResult == DialogResult.No)
                    {
                        Application.Exit();
                    }
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e) => СохранитьКак();
        private void создатьToolStripButton_Click(object sender, EventArgs e) => СоздатьНовыйФайл();

        private void вставкаToolStripButton_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (textBox1.SelectionLength > 0)
                {
                    int selectionStart = textBox1.SelectionStart;
                    string textBefore = textBox1.Text.Substring(0, selectionStart);
                    string textAfter = textBox1.Text.Substring(selectionStart + textBox1.SelectionLength);
                    textBox1.Text = textBefore + clipboardText + textAfter;
                    textBox1.SelectionStart = selectionStart + clipboardText.Length;
                    textBox1.SelectionLength = 0;
                }
                else
                {
                    int cursorPosition = textBox1.SelectionStart;
                    string textBefore = textBox1.Text.Substring(0, cursorPosition);
                    string textAfter = textBox1.Text.Substring(cursorPosition);
                    textBox1.Text = textBefore + clipboardText + textAfter;
                    textBox1.SelectionStart = cursorPosition + clipboardText.Length;
                    textBox1.SelectionLength = 0;
                }
            }
        }

        private void вырезатьToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
            {
                Clipboard.SetText(textBox1.SelectedText);
                int selectionStart = textBox1.SelectionStart;
                string textBefore = textBox1.Text.Substring(0, selectionStart);
                string textAfter = textBox1.Text.Substring(selectionStart + textBox1.SelectionLength);
                textBox1.Text = textBefore + textAfter;
                textBox1.SelectionStart = selectionStart;
                textBox1.SelectionLength = 0;
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignoreTextChanges)
                return;

            undoStack.Push(lastTextState);
            lastTextState = textBox1.Text;

            redoStack.Clear();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            UndoText();
        }

        private void Повтор_Click(object sender, EventArgs e)
        {
            RedoText();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e) => ОткрытьФайл();

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
                Clipboard.SetText(textBox1.SelectedText);
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
            {
                Clipboard.SetText(textBox1.SelectedText);
                int selectionStart = textBox1.SelectionStart;
                string textBefore = textBox1.Text.Substring(0, selectionStart);
                string textAfter = textBox1.Text.Substring(selectionStart + textBox1.SelectionLength);
                textBox1.Text = textBefore + textAfter;
                textBox1.SelectionStart = selectionStart;
                textBox1.SelectionLength = 0;
            }
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoText();
        }

        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RedoText();
        }
        private void UndoText()
        {
            if (undoStack.Count == 0)
                return;

            string currentText = textBox1.Text;
            string previousText = undoStack.Pop();

            redoStack.Push(currentText);

            ignoreTextChanges = true;
            textBox1.Text = previousText;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            ignoreTextChanges = false;

            lastTextState = previousText;
        }

        private void RedoText()
        {
            if (redoStack.Count == 0)
                return;

            string currentText = textBox1.Text;
            string redoText = redoStack.Pop();

            undoStack.Push(currentText);

            ignoreTextChanges = true;
            textBox1.Text = redoText;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            ignoreTextChanges = false;

            lastTextState = redoText;
        }
        private void ResetUndoRedoHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
            lastTextState = textBox1.Text;
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (textBox1.SelectionLength > 0)
                {
                    int selectionStart = textBox1.SelectionStart;
                    string textBefore = textBox1.Text.Substring(0, selectionStart);
                    string textAfter = textBox1.Text.Substring(selectionStart + textBox1.SelectionLength);
                    textBox1.Text = textBefore + clipboardText + textAfter;
                    textBox1.SelectionStart = selectionStart + clipboardText.Length;
                    textBox1.SelectionLength = 0;
                }
                else
                {
                    int cursorPosition = textBox1.SelectionStart;
                    string textBefore = textBox1.Text.Substring(0, cursorPosition);
                    string textAfter = textBox1.Text.Substring(cursorPosition);
                    textBox1.Text = textBefore + clipboardText + textAfter;
                    textBox1.SelectionStart = cursorPosition + clipboardText.Length;
                    textBox1.SelectionLength = 0;
                }
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e) => УдалитьТекст();
        private void выделитьВсёToolStripMenuItem_Click(object sender, EventArgs e) => ВыделитьВесьТекст();

        private void УдалитьТекст()
        {
            if (textBox1.SelectionLength > 0)
            {
                if (!ignoreTextChanges)
                {
                    undoStack.Push(textBox1.Text);
                    redoStack.Clear();
                }
                int selectionStart = textBox1.SelectionStart;
                string textBefore = textBox1.Text.Substring(0, selectionStart);
                string textAfter = textBox1.Text.Substring(selectionStart + textBox1.SelectionLength);
                ignoreTextChanges = true;
                textBox1.Text = textBefore + textAfter;
                textBox1.SelectionStart = selectionStart;
                textBox1.SelectionLength = 0;
                ignoreTextChanges = false;
            }
        }

        private void ВыделитьВесьТекст()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.SelectionStart = 0;
                textBox1.SelectionLength = textBox1.Text.Length;
                textBox1.Focus();
            }
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string helpMessage =
                "Лексический анализатор\n\n" +
                "Реализованные функции:\n" +
                "• Создать - создание нового файла\n" +
                "• Открыть - открытие существующего файла\n" +
                "• Сохранить - сохранение текущего файла\n" +
                "• Сохранить как - сохранение файла под новым именем\n" +
                "• Выход - завершение работы\n" +
                "• Отменить - отмена последнего действия\n" +
                "• Повторить - повтор отмененного действия\n" +
                "• Вырезать - вырезать выделенный текст\n" +
                "• Копировать - копировать выделенный текст\n" +
                "• Вставить - вставить текст из буфера\n" +
                "• Удалить - удалить выделенный текст\n" +
                "• Выделить всё - выделить весь текст\n" +
                "• Пуск - запуск лексического и синтаксического анализа\n\n";

            MessageBox.Show(helpMessage, "Справка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Работу сделал Марченко А.Е. АП-326",
                "Об авторе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Лексический анализатор\nВерсия 2.0 (с синтаксическим анализом)\n\nРаботу сделал Марченко А.Е. АП-326",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e) => вызовСправкиToolStripMenuItem_Click(sender, e);

        private void запуск_Click(object sender, EventArgs e) => RunAnalysis();

        private void RunAnalysis()
        {
            dataGridViewResults.Rows.Clear();
            dataGridViewSyntaxErrors.Rows.Clear();

            labelErrorCount.Text = "Общее количество ошибок: 0";

            string inputText = textBox1.Text;

            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show(
                    "Введите текст для анализа",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            var tokens = analyzer.Analyze(inputText);

            var syntaxAnalyzer = new SyntaxAnalyzer(tokens);
            var syntaxErrors = syntaxAnalyzer.Parse();

            foreach (var err in syntaxErrors)
            {
                dataGridViewSyntaxErrors.Rows.Add(
                    err.Fragment,
                    err.Location,
                    err.Description
                );
            }

            if (syntaxErrors.Count > 0)
            {
                labelErrorCount.Text =
                    $"Общее количество ошибок: {syntaxErrors.Count}";

                MessageBox.Show(
                    "Обнаружены синтаксические ошибки. Семантический анализ не выполнялся.",
                    "Результат анализа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            var semanticAnalyzer = new SemanticAnalyzer(tokens);
            semanticAnalyzer.Analyze();

            foreach (var err in semanticAnalyzer.Errors)
            {
                int rowIndex = dataGridViewResults.Rows.Add(
                    err.Fragment,
                    err.Location,
                    err.Message
                );

                dataGridViewResults.Rows[rowIndex].DefaultCellStyle.BackColor =
                    System.Drawing.Color.LightCoral;
            }

            labelErrorCount.Text =
                $"Общее количество ошибок: {semanticAnalyzer.Errors.Count}";

            MessageBox.Show(
                semanticAnalyzer.GetAstText(),
                "Абстрактное синтаксическое дерево AST",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            if (semanticAnalyzer.Errors.Count == 0)
            {
                MessageBox.Show(
                    "Семантических ошибок не обнаружено.",
                    "Результат семантического анализа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void dataGridViewResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            try
            {
                var row = dataGridViewResults.Rows[e.RowIndex];

                string fragment = row.Cells[0].Value?.ToString();
                string location = row.Cells[1].Value?.ToString();

                if (string.IsNullOrWhiteSpace(location))
                    return;

                SelectSemanticErrorFragment(location, fragment);
            }
            catch
            {
            }
        }
        private void SelectSemanticErrorFragment(string location, string fragment)
        {
            if (string.IsNullOrWhiteSpace(location))
                return;

            string cleaned = location
                .Replace("строка", "")
                .Replace("символ", "")
                .Trim();

            string[] parts = cleaned.Split(',');

            if (parts.Length < 2)
                return;

            if (!int.TryParse(parts[1].Trim(), out int start))
                return;

            int absoluteStart = start - 1;

            if (absoluteStart < 0 || absoluteStart >= textBox1.Text.Length)
                return;

            int length = 1;

            if (!string.IsNullOrEmpty(fragment))
                length = fragment.Length;

            if (absoluteStart + length > textBox1.Text.Length)
                length = textBox1.Text.Length - absoluteStart;

            textBox1.Focus();
            textBox1.SelectionStart = absoluteStart;
            textBox1.SelectionLength = length;
            textBox1.ScrollToCaret();
        }

        private void DataGridViewSyntaxErrors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            try
            {
                var row = dataGridViewSyntaxErrors.Rows[e.RowIndex];

                string fragment = row.Cells[0].Value?.ToString();
                string location = row.Cells[1].Value?.ToString();

                if (string.IsNullOrWhiteSpace(location))
                    return;

                SelectSyntaxErrorFragment(location, fragment);
            }
            catch
            {
            }
        }
        private void SelectSyntaxErrorFragment(string location, string fragment)
        {
            if (string.IsNullOrWhiteSpace(location))
                return;

            string cleaned = location
                .Replace("строка", "")
                .Replace("позиция", "")
                .Trim();

            string[] parts = cleaned.Split(',');

            if (parts.Length < 2)
                return;

            if (!int.TryParse(parts[1].Trim(), out int start))
                return;

            int absoluteStart = start - 1;

            if (absoluteStart < 0 || absoluteStart >= textBox1.Text.Length)
                return;

            int length = 1;

            if (!string.IsNullOrEmpty(fragment) && fragment != "<конец строки>")
                length = fragment.Length;

            if (absoluteStart + length > textBox1.Text.Length)
                length = textBox1.Text.Length - absoluteStart;

            textBox1.Focus();
            textBox1.SelectionStart = absoluteStart;
            textBox1.SelectionLength = length;
            textBox1.ScrollToCaret();
        }
        private void SelectFragmentByLocation(string location, string fragment = null)
        {
            if (string.IsNullOrWhiteSpace(location))
                return;

            string cleaned = location
                .Replace("строка", "")
                .Replace("позиция", "")
                .Trim();

            string[] parts = cleaned.Split(',');

            if (parts.Length < 2)
                return;

            string posPart = parts[1].Trim();

            int start;
            int end;

            if (posPart.Contains("-"))
            {
                string[] positions = posPart.Split('-');

                if (!int.TryParse(positions[0].Trim(), out start))
                    return;

                if (!int.TryParse(positions[1].Trim(), out end))
                    return;
            }
            else
            {
                if (!int.TryParse(posPart.Trim(), out start))
                    return;

                if (!string.IsNullOrEmpty(fragment) && fragment != "<конец строки>")
                    end = start + fragment.Length - 1;
                else
                    end = start;
            }

            int absoluteStart = start - 1;
            int absoluteEnd = end - 1;

            if (absoluteStart < 0 || absoluteStart >= textBox1.Text.Length)
                return;

            if (absoluteEnd < absoluteStart)
                absoluteEnd = absoluteStart;

            if (absoluteEnd >= textBox1.Text.Length)
                absoluteEnd = textBox1.Text.Length - 1;

            int selectionLength = absoluteEnd - absoluteStart + 1;

            textBox1.Focus();
            textBox1.SelectionStart = absoluteStart;
            textBox1.SelectionLength = selectionLength;
            textBox1.ScrollToCaret();
        }
        private void ConfigureSemanticErrorsGrid()
        {
            dataGridViewResults.Columns.Clear();

            dataGridViewResults.Columns.Add("ColumnSemanticFragment", "Фрагмент");
            dataGridViewResults.Columns.Add("ColumnSemanticLocation", "Позиция");
            dataGridViewResults.Columns.Add("ColumnSemanticMessage", "Сообщение");

            dataGridViewResults.Columns[0].Width = 180;
            dataGridViewResults.Columns[1].Width = 180;
            dataGridViewResults.Columns[2].Width = 700;

            dataGridViewResults.MultiSelect = false;
            dataGridViewResults.RowHeadersVisible = false;
            dataGridViewResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
    }
}