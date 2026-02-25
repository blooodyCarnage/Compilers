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

        public Form1()
        {
            InitializeComponent();

            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();

            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";

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

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            СохранитьФайл();
        }

        private void СоздатьНовыйФайл()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult result = MessageBox.Show(
                    "Хотите сохранить изменения?",
                    "Создание нового файла",
                    MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    СохранитьФайл();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            textBox1.Clear();
            currentFileName = "";
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
    }
}