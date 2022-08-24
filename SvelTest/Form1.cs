using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SvelTest
{
    public partial class Form1 : Form
    {
        private string lastWord = "";
        private char[] charsThatRemovesLastSpace = new char[] { ',', '.', '!', '?', ':' };
        private bool suggestedWordWasInserted = false;


        public Form1()
        {
            InitializeComponent();
            InitializeMenuStrip();
            InitializeTextBoxT9();
        }

        private void InitializeMenuStrip()
        {
            ToolStripMenuItem CreateWordDictionary = new ToolStripMenuItem("Создание словаря");
            CreateWordDictionary.Click += (object sender, EventArgs e) =>
            {
                WordDictionaryManager.CreateDictionary(OpenFile());
            };

            ToolStripMenuItem UpdateWordDictionary = new ToolStripMenuItem("Обновление словаря");
            UpdateWordDictionary.Click += (object sender, EventArgs e) =>
            {
                WordDictionaryManager.UpdateDictionary(OpenFile());
            };

            ToolStripMenuItem ClearWordDictionary = new ToolStripMenuItem("Очистка словаря");
            ClearWordDictionary.Click += (object sender, EventArgs e) =>
            {
                if (MessageBox.Show("Вы уверены?", "Очистьть словарь", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    WordDictionaryManager.ClearDictionary();
            };

            DictionaryToolStripMenuItem.DropDownItems.Add(CreateWordDictionary);
            DictionaryToolStripMenuItem.DropDownItems.Add(UpdateWordDictionary);
            DictionaryToolStripMenuItem.DropDownItems.Add(ClearWordDictionary);
        }

        private void InitializeTextBoxT9()
        {
            foreach (Control c in Controls)
            {
                TextBox textBox = c as TextBox;
                if (textBox != null)
                {
                    textBox.KeyDown += TextBox_KeyDown;
                    textBox.TextChanged += TextBox_TextChanged;
                }
            }
        }

        private string OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : "";
        }

        private void UpdateLastWord(TextBox textBox)
        {
            if (textBox.Text.Length >= 50)
            {
                var lastWords = textBox.Text.Substring(textBox.Text.Length - 50, 50).Split(' ', ',', '.', '!', '?', ':', '-', '>', '\r', '\n');
                lastWord = lastWords[lastWords.Length - 1];
            }
            else if (textBox.Text.Length > 0)
            {
                var lastWords = textBox.Text.Split(' ', ',', '.', '!', '?', ':', '-', '>', '\r', '\n');
                lastWord = lastWords[lastWords.Length - 1];
            }
            else
            {
                lastWord = "";
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox box = (sender as TextBox);
            if (suggestedWordWasInserted)
            {
                box.Text = box.Text.TrimEnd('\n').TrimEnd('\r');
                box.SelectionStart = box.Text.Length;
                suggestedWordWasInserted = false;
            }

            UpdateLastWord(box);
            var suggestedWords = WordDictionaryManager.GetSuggestedWords(lastWord);

            suggestedWordsList.Items.Clear();
            suggestedWordsList.Items.AddRange(suggestedWords);

            if (suggestedWordsList.Items.Count > 0)
            {
                suggestedWordsList.SelectedIndex = 0;
                suggestedWordsList.Visible = true;
                var position = box.GetPositionFromCharIndex(box.Text.Length - 1);
                position.Offset(25, 25);
                position.Offset(box.Location.X, box.Location.Y);
                suggestedWordsList.Location = position;
            }
            else
            {
                suggestedWordsList.Visible = false;
            }

            if (box.Text.Length > 0)
            {
                var lastLetter = box.Text[box.Text.Length - 1];
                if (box.Text.Length > 1 && charsThatRemovesLastSpace.Contains(lastLetter) && box.Text[box.Text.Length - 2] == ' ' && box.Text[box.Text.Length - 3] != ' ')
                {
                    string correctedText = box.Text.Remove(box.Text.Length - 2, 2);
                    correctedText += lastLetter;
                    correctedText += ' ';
                    box.Text = correctedText;
                    box.SelectionStart = box.Text.Length;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox box = (sender as TextBox);

            if (suggestedWordsList.Items.Count > 0)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    box.Text = box.Text.Remove(box.Text.Length - lastWord.Length, lastWord.Length) + (string)suggestedWordsList.SelectedItem + " ";
                    box.SelectionStart = box.Text.Length;
                    suggestedWordWasInserted = true;
                }
                if (e.KeyCode == Keys.Up)
                {
                    if (suggestedWordsList.SelectedIndex > 0)
                        suggestedWordsList.SelectedIndex--;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (suggestedWordsList.SelectedIndex < suggestedWordsList.Items.Count - 1)
                        suggestedWordsList.SelectedIndex++;
                }
            }
        }


    }
}
