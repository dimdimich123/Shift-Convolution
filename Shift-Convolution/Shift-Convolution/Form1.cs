using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace Shift_Convolution
{
    public partial class Form1 : Form
    {
        private Recognizer _rec = new Recognizer();
        private Grammar _gramm = new Grammar();
        private List<int> L = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Recognize()
        {
            List<int> L2 = _rec.StartRecognize();
            //int count = 1;
            listBox1.Items.AddRange(_rec.Log.ToArray());
            //foreach (string str in _rec.Log)
            //{
            //    listBox1.Items.Add(count++ + ". " + str);
            //}

            if (_rec.Log.Last().Contains("НЕ"))
            {
                labelResult.Text = "Результат: НЕ принадлежит языку";
                btnOutput.Enabled = false;
                btnSaveToFile.Enabled = false;
                L.Clear();
            }
            else
            {
                labelResult.Text = "Результат: Принадлежит языку";
                btnOutput.Enabled = true;
                btnSaveToFile.Enabled = true;
                L = L2;
            }
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Length == 0 || textBox1.Text == " ")
            {
                MessageBox.Show("Поле 1 должно быть заполнено");
                return;
            }

            if (textBox2.Text.Length == 0 || textBox2.Text == " " || textBox2.Text == "  ")
            {
                MessageBox.Show("Поле 2 должно быть заполнено");
                return;
            }

            if (textBox2.Text.Length == 1 && ((textBox2.Text[0] >= 'A' && textBox2.Text[0] <= 'Z') || 
                (textBox2.Text[0] >= 'А' && textBox2.Text[0] <= 'Я')))
            {
                MessageBox.Show("Терминальный символ не должен быть в верхнем регистре");
                return;
            }

            if(textBox2.Text.Length == 2)
            {
                for (int i = 0; i < textBox2.Text.Length; ++i)
                {
                    if ((textBox2.Text[i] < 'A' || textBox2.Text[i] > 'Z') && (textBox2.Text[i] < 'А' || textBox2.Text[i] > 'Я'))
                    {
                        MessageBox.Show("Нетерминальные символы должны быть в верхнем регистре");
                        return;
                    }
                }
            }


            if (!_gramm.States.ContainsKey(textBox1.Text))
            {
                _gramm.States[textBox1.Text] = new List<string>() { textBox2.Text };
            }
            else
            {
                if (!_gramm.States[textBox1.Text].Contains(textBox2.Text))
                {
                    _gramm.States[textBox1.Text].Add(textBox2.Text);
                }
            }
            _gramm.RulesName = _gramm.States.Keys.ToList();

            
            UpdateGrammarListBox();
            textBox1.Clear();
            textBox2.Clear();
        }

        private void UpdateGrammarListBox()
        {
            listBox2.Items.Clear();

            string[] Keys = _gramm.States.Keys.ToArray();
            
            for (int i = 0; i < Keys.Length; ++i)
            {
                string Rule = Keys[i] + "->";
                foreach(string str in _gramm.States[Keys[i]])
                {
                    Rule += str + "|";
                }
                Rule = Rule.Remove(Rule.Length - 1);
                listBox2.Items.Add(Rule);
            }
        }

        private bool CheckGrammar()
        {
            Dictionary<string, int> CalledRules = new Dictionary<string, int>();
            string[] Rules = _gramm.RulesName.ToArray();
            for (int i = 0; i < Rules.Length; ++i)
            {
                CalledRules[Rules[i]] = 0;
                if(Rules[i] == _gramm.MainRule)
                {
                    CalledRules[Rules[i]]++;
                }
            }

            for (int i = 0; i < Rules.Length; ++i)
            {
                foreach(string str in _gramm.States[Rules[i]])
                {
                    for(int j = 0; j < str.Length; ++j)
                    {
                        if (CalledRules.ContainsKey(str[j].ToString()))
                        {
                            CalledRules[str[j].ToString()]++;
                        }
                    }
                }
            }

            for (int i = 0; i < Rules.Length; ++i)
            {
                if(CalledRules[Rules[i]] == 0)
                {
                    MessageBox.Show($"Граматика не должна содержать невызываемые правила ({Rules[i]})");
                    return false;
                }
            }
            return true;
        }

        private void buttonSelectStartState_Click(object sender, EventArgs e)
        {
            if(listBox2.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите правило из списка");
                return;
            }

            _gramm.MainRule = listBox2.Items[listBox2.SelectedIndex].ToString()[0].ToString();
            labelStartState.Text = "Начальное правило: " + _gramm.MainRule;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = numericUpDown2.Value;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!CheckGrammar())
            {
                return;
            }

            if(_gramm.MainRule == "")
            {
                MessageBox.Show("Необходимо выбрать начальное состояние");
                return;
            }

            _gramm.Chains.Clear();
            listBox3.Items.Clear();

            _gramm.initialLength = (int)numericUpDown1.Value;
            _gramm.finalLength = (int)numericUpDown2.Value;
            if (radioButton1.Checked)
                _gramm.OutPut = Grammar.TypeOutput.Left;
            else
                _gramm.OutPut = Grammar.TypeOutput.Right;

            _gramm.StartGenerate();

            List<string> WithOutStep = new List<string>();
            foreach (string str in _gramm.Chains)
            {
                string str2 = str.Remove(str.Length - 2);
                for (int i = str2.Length - 1; i > 0; --i)
                {
                    if (str2[i] == '>')
                    {
                        WithOutStep.Add(str2.Remove(0, i + 1));
                        break;
                    }
                }
            }

            List<string> DistinctList = WithOutStep.Distinct().ToList();

            listBox3.Items.AddRange(DistinctList.ToArray());
            //foreach (string chain in DistinctList)
            //{
            //    listBox3.Items.Add(chain);
            //}

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            int tabIndex = 0;
            for (int i = 0; i < groupBox3.Controls.Count; ++i)
            {
                if (groupBox3.Controls[i].TabIndex != 9)
                {
                    RadioButton btn = (RadioButton)groupBox3.Controls[i];
                    if (btn.Checked)
                    {
                        tabIndex = btn.TabIndex;
                        break;
                    }
                }
            }

            labelStartState.Text = "Начальное правило:";
            listBox2.Items.Clear();
            _gramm.Chains.Clear();
            _gramm.MainRule = "";
            _gramm.RulesName.Clear();
            _gramm.States.Clear();
            _rec.Grammar.Clear();
            _rec.SetChain("");
            _rec.Log.Clear();

            switch (tabIndex)
            {
                case 1:
                    _gramm.States["S"] = new List<string>() { "S+T", "S-T", "T*E", "T/E", "(S)", "a", "b" };
                    _gramm.States["T"] = new List<string>() { "T*E", "T/E", "(S)", "a", "b" };
                    _gramm.States["E"] = new List<string>() { "(S)", "a", "b" };

                    _gramm.MainRule = "S";
                    labelStartState.Text = "Начальное правило: " + _gramm.MainRule;
                    _gramm.RulesName = _gramm.States.Keys.ToList();

                    UpdateGrammarListBox();
                    break;
                case 2:
                    _gramm.States["S"] = new List<string>() { "AE"};
                    _gramm.States["X"] = new List<string>() { "AF", "BF" };
                    _gramm.States["F"] = new List<string>() { "AF", "BF", "CC" };
                    _gramm.States["E"] = new List<string>() { "XH", "YX", "y" };
                    _gramm.States["H"] = new List<string>() { "YX", "y" };
                    _gramm.States["A"] = new List<string>() { "a" };
                    _gramm.States["Y"] = new List<string>() { "y" };
                    _gramm.States["B"] = new List<string>() { "b" };
                    _gramm.States["C"] = new List<string>() { "c" };

                    _gramm.MainRule = "S";
                    labelStartState.Text = "Начальное правило: " + _gramm.MainRule;
                    _gramm.RulesName = _gramm.States.Keys.ToList();

                    UpdateGrammarListBox();
                    break;
            }

            btnOutput.Enabled = false;



        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0 || textBox1.Text == " ")
            {
                MessageBox.Show("Поле 1 должно быть заполнено");
                return;
            }

            if (textBox2.Text.Length == 0 || textBox2.Text == " " || textBox2.Text == "  ")
            {
                MessageBox.Show("Поле 2 должно быть заполнено");
                return;
            }

            if (textBox2.Text.Length == 1 && ((textBox2.Text[0] >= 'A' && textBox2.Text[0] <= 'Z') ||
                (textBox2.Text[0] >= 'А' && textBox2.Text[0] <= 'Я')))
            {
                MessageBox.Show("Терминальный символ не должен быть в верхнем регистре");
                return;
            }

            if (textBox2.Text.Length == 2)
            {
                for (int i = 0; i < textBox2.Text.Length; ++i)
                {
                    if ((textBox2.Text[i] < 'A' || textBox2.Text[i] > 'Z') && (textBox2.Text[i] < 'А' || textBox2.Text[i] > 'Я'))
                    {
                        MessageBox.Show("Нетерминальные символы должны быть в верхнем регистре");
                        return;
                    }
                }
            }


            if (_gramm.States.ContainsKey(textBox1.Text))
            {
                if(_gramm.States[textBox1.Text] != null && _gramm.States[textBox1.Text].Contains(textBox2.Text))
                {
                    _gramm.States[textBox1.Text].Remove(textBox2.Text);
                    if(_gramm.States[textBox1.Text].Count == 0)
                    {
                        _gramm.States.Remove(textBox1.Text);
                    }
                }
            }

            _gramm.RulesName = _gramm.States.Keys.ToList();


            UpdateGrammarListBox();
            textBox1.Clear();
            textBox2.Clear();
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (!CheckGrammar())
            {
                return;
            }

            if (_gramm.States.Count == 0)
            {
                MessageBox.Show("Необходимо ввести граматику");
                return;
            }

            if(textBox3.Text.Length < 1)
            {
                MessageBox.Show("Длина цепочки не может быть меньше 1");
                return;
            }

            if(_rec.Grammar.Count == 0)
            {
                string[] Rules = _gramm.States.Keys.ToArray();
                int count = 1;
                for(int i = 0; i < Rules.Length; ++i)
                {
                    string[] states = _gramm.States[Rules[i]].ToArray();
                    int[] values = new int[states.Length];
                    for(int j = 0; j < values.Length; ++j)
                    {
                        values[j] = count++;
                    }
                    _rec.Grammar[Rules[i]] = new KeyValuePair<string[], int[]>(states, values);
                }
            }

            listBox1.Items.Clear();

            _rec.SetChain("@" + textBox3.Text);
            _rec.SetaMainRule(_gramm.MainRule);
            Recognize();
        }

        private void btnTakeChain_Click(object sender, EventArgs e)
        {
            if(listBox3.SelectedIndex > -1)
            {
                textBox3.Text = listBox3.SelectedItem.ToString();
            }
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            string description = "Порядок правил для правостороннего вывода: [";
            for(int i = 0; i < L.Count; ++i)
            {
                if(L[i] > 0)
                {
                    description += L[i] + ",";
                }
            }
            description = description.Remove(description.Length - 1);
            description += "]";

            string description2 = "Номера правил:\n";
            string[] Rules = _rec.Grammar.Keys.ToArray();
            for(int i = 0; i < Rules.Length; ++i)
            {
                description2 += Rules[i] + "->";
                for(int j = 0; j < _rec.Grammar[Rules[i]].Key.Length; ++j)
                {
                    description2 += _rec.Grammar[Rules[i]].Key[j] + $" [{_rec.Grammar[Rules[i]].Value[j]}] | ";
                }
                description2 = description2.Remove(description2.Length - 1);
                description2 += "\n";
            }
            string chain = _gramm.MainRule;
            string description3 = "Вывод цепочки: \n" + chain;

            for (int i = 0; i < L.Count; ++i)
            {
                if (L[i] > 0)
                {
                    for(int j = chain.Length - 1; j >= 0; --j)
                    {
                        if (Rules.Contains(chain[j].ToString()))
                        {
                            string NotTerm = chain[j].ToString();
                            chain = chain.Remove(j, 1);

                            int index = 0;
                            for(int k = 0; k < _rec.Grammar[NotTerm].Key.Length; ++k)
                            {
                                if(_rec.Grammar[NotTerm].Value[k] == L[i])
                                {
                                    index = k;
                                    break;
                                }
                            }

                            chain = chain.Insert(j, _rec.Grammar[NotTerm].Key[index]);
                            description3 += "->" + chain;
                            break;
                        }
                    }
                }
            }

            MessageBox.Show(description + "\n" + description2 + description3);
        }

        private void btnRule_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Переходы правил граматики вводятся в номальной форме Хомского" +
                            " без порождения пустых строк.\n Например:\n" +
                            "S->AB\nA->B|a\nB->A|b");


        }

        private void btnSaveToFile_Click(object sender, EventArgs e)
        {
            //FileStream stream = new FileStream("File" + ".doc", FileMode.OpenOrCreate);
            //StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            //writer.WriteLine()


            //writer.Close();

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "DOC|*.doc";
            saveFileDialog1.Title = "Save an Doc File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                FileStream stream = (FileStream)saveFileDialog1.OpenFile();
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                //FileStream fs = (FileStream)saveFileDialog1.OpenFile();

                string description = "Порядок правил для правостороннего вывода: [";
                for (int i = 0; i < L.Count; ++i)
                {
                    if (L[i] > 0)
                    {
                        description += L[i] + ",";
                    }
                }
                description = description.Remove(description.Length - 1);
                description += "]";

                string description2 = "Номера правил:\n";
                string[] Rules = _rec.Grammar.Keys.ToArray();
                for (int i = 0; i < Rules.Length; ++i)
                {
                    description2 += Rules[i] + "->";
                    for (int j = 0; j < _rec.Grammar[Rules[i]].Key.Length; ++j)
                    {
                        description2 += _rec.Grammar[Rules[i]].Key[j] + $" [{_rec.Grammar[Rules[i]].Value[j]}] | ";
                    }
                    description2 = description2.Remove(description2.Length - 1);
                    description2 += "\n";
                }
                string chain = _gramm.MainRule;
                string description3 = "Вывод цепочки: \n" + chain;

                for (int i = 0; i < L.Count; ++i)
                {
                    if (L[i] > 0)
                    {
                        for (int j = chain.Length - 1; j >= 0; --j)
                        {
                            if (Rules.Contains(chain[j].ToString()))
                            {
                                string NotTerm = chain[j].ToString();
                                chain = chain.Remove(j, 1);

                                int index = 0;
                                for (int k = 0; k < _rec.Grammar[NotTerm].Key.Length; ++k)
                                {
                                    if (_rec.Grammar[NotTerm].Value[k] == L[i])
                                    {
                                        index = k;
                                        break;
                                    }
                                }

                                chain = chain.Insert(j, _rec.Grammar[NotTerm].Key[index]);
                                description3 += "->" + chain;
                                break;
                            }
                        }
                    }
                }

                writer.WriteLine(description + "\n" + description2 + description3 + "\n\nПроверка выводимости:");


                foreach (object obj in listBox1.Items)
                {
                    writer.WriteLine(obj.ToString());
                }


                writer.Close();
            }
        }

        private void авторToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Чепурко К.Р.\nГруппа: ИП-812");
        }

        private void темаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Написать программу, которая для языка, заданного контекстносвободной грамматикой в требуемой форме (проверить корректность" +
            "задания и при отрицательном результате выдать соответствующее сообщение), построит детерминированный распознаватель с магазинной" +
            "памятью, используя алгоритм восходящего анализа с возвратами" + 
            "(«сдвиг - свертка»).Программа должна сгенерировать по исходной" +
            "грамматике несколько цепочек в указанном диапазоне длин и проверить их допустимость построенным ДМПА.Процессы построения цепочек" +
            "и проверки их выводимости отображать на экране(по требованию)." +
            "Предусмотреть возможность проверки цепочки, введённой пользователем.");
        }
    }
}
