using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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


        public Form1()
        {
            InitializeComponent();

            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();

            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            textBox1.TextChanged += TextBox1_TextChanged;
            // Подписка на события
            InitializeEventHandlers();
        }

        private void InitializeEventHandlers()
        {
            if (сохранитьToolStripButton != null)
                сохранитьToolStripButton.Click -= сохранитьToolStripButton_Click; // Отписываемся сначала
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void сохранитьToolStripButton_Click(object sender, EventArgs e)
        {
            СохранитьФайл();
        }

        private void открытьToolStripButton_Click(object sender, EventArgs e)
        {
            ОткрытьФайл();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            СоздатьНовыйФайл();
        }
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
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            СохранитьФайл();
        }

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
                    currentFileName = filename;
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
            {
                Clipboard.SetText(textBox1.SelectedText);

            }
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

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            СохранитьФайл();
        }

        private void создатьToolStripButton_Click(object sender, EventArgs e)
        {
            СоздатьНовыйФайл();
        }

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
            if (ignoreTextChanges) return;

            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                undoStack.Push(textBox1.Text);
                redoStack.Clear();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                string currentText = textBox1.Text;

                string previousText = undoStack.Pop();

                ignoreTextChanges = true;

                redoStack.Push(currentText);

                textBox1.Text = previousText;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;

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
                textBox1.SelectionLength = 0;

                ignoreTextChanges = false;
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ОткрытьФайл();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.SelectedText))
            {
                Clipboard.SetText(textBox1.SelectedText);

            }
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
            if (undoStack.Count > 0)
            {
                string currentText = textBox1.Text;

                string previousText = undoStack.Pop();

                ignoreTextChanges = true;

                redoStack.Push(currentText);

                textBox1.Text = previousText;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;

                ignoreTextChanges = false;
            }
        }

        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                string redoText = redoStack.Pop();

                undoStack.Push(textBox1.Text);

                ignoreTextChanges = true;

                textBox1.Text = redoText;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;

                ignoreTextChanges = false;
            }
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

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            УдалитьТекст();
        }

        private void выделитьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ВыделитьВесьТекст();
        }
        private void УдалитьТекст()
        {
            // Проверяем, есть ли выделенный текст
            if (textBox1.SelectionLength > 0)
            {
                // Сохраняем состояние для Undo
                if (!ignoreTextChanges)
                {
                    undoStack.Push(textBox1.Text);
                    redoStack.Clear();
                }

                // Удаляем выделенный текст
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
                "Реализованные функции:\n" +
                "• Создать - создание нового файла\n" +
                "• Открыть  - открытие существующего файла\n" +
                "• Сохранить - сохранение текущего файла\n" +
                "• Сохранить как - сохранение файла под новым именем\n" +
                "• Выход - завершение работы\n" +
                "• Отменить - отмена последнего действия\n" +
                "• Повторить - повтор отмененного действия\n" +
                "• Вырезать - вырезать выделенный текст\n" +
                "• Копировать - копировать выделенный текст\n" +
                "• Вставить - вставить текст из буфера\n" +
                "• Удалить  - удалить выделенный текст\n" +
                "• Выделить всё - выделить весь текст\n\n";

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
                "Работу сделал Марченко А.Е. АП-326",
                "Об авторе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}