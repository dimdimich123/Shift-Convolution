using System.Collections.Generic;

namespace Shift_Convolution
{
    class Grammar
    {
        public enum TypeOutput : int
        {
            Left,
            Right
        }

        public List<string> RulesName = new List<string>();
        public string MainRule = "";
        public Dictionary<string, List<string>> States = new Dictionary<string, List<string>>();
        public List<string> Chains = new List<string>();
        public TypeOutput OutPut = TypeOutput.Left;
        public int initialLength;
        public int finalLength;


        public void StartGenerate()
        {
            string result = MainRule + "->";
            string Before = "";
            string After = "";
            Generate(Before, After, MainRule, result);
        }

        private int TakeLengthVT(string str)
        {
            int countTerminalChars = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == '~') continue;
                if ((str[i] < 'A' || str[i] > 'Z') && (str[i] < 'А' || str[i] > 'Я'))
                {
                    countTerminalChars++;
                }
            }
            return countTerminalChars;
        }

        private int TakeLengthVN(string str)
        {
            int countNotTerminalChars = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == '~') continue;
                if ((str[i] >= 'A' && str[i] <= 'Z') || (str[i] >= 'А' && str[i] <= 'Я'))
                {
                    countNotTerminalChars++;
                }
            }
            return countNotTerminalChars;
        }


        private int Generate(string Befor, string After, string Rule, string result)
        {
            Dictionary<string, int> StateIndex = new Dictionary<string, int>();
            Dictionary<string, bool> StateBool = new Dictionary<string, bool>();
            for (int i = 0; i < RulesName.Count; ++i)
            {
                StateIndex[RulesName[i]] = 0;
            }
            string stringRule = "";
            string substringAfter = "";
            string substringBefor = "";
            while (true)
            {
                string Chain, newResult = result;
                int indexState = StateIndex[Rule];
                string insertString = States[Rule][indexState];
                Chain = Befor + insertString + After;
                newResult += Chain + "->";

                if (OutPut == TypeOutput.Left)
                {
                    bool isHaveRule = false;
                    for (int i = 0; i < Chain.Length; ++i) // обход слево на право
                    {
                        if ((Chain[i] >= 'A' && Chain[i] <= 'Z') || (Chain[i] >= 'А' && Chain[i] <= 'Я'))  // Ищем нетерминальный символ
                        {
                            bool isRulesName = false;
                            for (int j = 0; j < RulesName.Count; ++j)
                            {
                                if (RulesName[j][0] == Chain[i])
                                {
                                    isRulesName = true;
                                    isHaveRule = true;
                                    break;
                                }
                            }
                            if (isRulesName)    // вставляем правило вместо нетерминального символа
                            {
                                stringRule = Chain[i].ToString();
                                substringAfter = Chain.Remove(0, i + 1);
                                substringBefor = Chain.Remove(i);
                                break;
                            }
                        }
                    }
                    if (!isHaveRule)
                    {
                        if (TakeLengthVT(Chain) <= finalLength && TakeLengthVT(Chain) >= initialLength)
                        {

                            Chains.Add(newResult);
                            if (StateIndex[Rule] == States[Rule].Count - 1)
                            {
                                StateIndex[Rule] = 0;
                                break;
                            }
                            else
                            {
                                StateIndex[Rule]++;
                            }
                            continue;
                        }
                        else
                        {
                            if (StateIndex[Rule] == States[Rule].Count - 1)
                            {
                                StateIndex[Rule] = 0;
                                break;
                            }
                            else
                            {
                                StateIndex[Rule]++;
                            }
                            continue;
                        }
                    }
                }
                else
                {
                    bool isHaveRule = false;
                    for (int i = Chain.Length - 1; i >= 0; --i) // обход с право на слево 
                    {
                        if ((Chain[i] >= 'A' && Chain[i] <= 'Z') || (Chain[i] >= 'А' && Chain[i] <= 'Я') && !StateBool[Rule])  // Ищем нетерминальный символ
                        {
                            bool isRulesName = false;
                            for (int j = 0; j < RulesName.Count; ++j)
                            {
                                if (RulesName[j][0] == Chain[i])
                                {
                                    isRulesName = true;
                                    isHaveRule = true;
                                    break;
                                }
                            }
                            if (isRulesName)    // вставляем правило вместо нетерминального символа
                            {
                                stringRule = Chain[i].ToString();
                                substringAfter = Chain.Remove(0, i + 1);
                                substringBefor = Chain.Remove(i);
                                break;
                            }
                        }
                    }
                    if (!isHaveRule)
                    {
                        if (TakeLengthVT(Chain) <= finalLength && TakeLengthVT(Chain) >= initialLength)
                        {

                            Chains.Add(newResult);
                            if (StateIndex[Rule] == States[Rule].Count - 1)
                            {
                                StateIndex[Rule] = 0;
                                break;
                            }
                            else
                            {
                                StateIndex[Rule]++;
                            }
                            continue;
                        }
                        else
                        {
                            if (StateIndex[Rule] == States[Rule].Count - 1)
                            {
                                StateIndex[Rule] = 0;
                                break;
                            }
                            else
                            {
                                StateIndex[Rule]++;
                            }
                            continue;
                        }
                    }
                }
                if (TakeLengthVT(substringBefor + stringRule + substringAfter) > finalLength)
                {
                    if (StateIndex[Rule] == States[Rule].Count - 1)
                    {
                        StateIndex[Rule] = 0;
                        break;
                    }
                    else{
                        StateIndex[Rule]++;
                    }
                    continue;
                }
                else
                {
                    string Check1 = ("->" + substringBefor + stringRule + substringAfter + "->").Replace("~", "");
                    string Check2 = result.Replace("~", "");
                    if (Check2.IndexOf(Check1) != -1)
                    {
                        if (StateIndex[Rule] == States[Rule].Count - 1)
                        {
                            StateIndex[Rule] = 0;
                            break;
                        }
                        else
                        {
                            StateIndex[Rule]++;
                        }
                        continue;
                    }
                    else
                    {
                        if (TakeLengthVN(substringBefor + stringRule + substringAfter) > finalLength)
                        {
                            if (StateIndex[Rule] == States[Rule].Count - 1)
                            {
                                StateIndex[Rule] = 0;
                                break;
                            }
                            else
                            {
                                StateIndex[Rule]++;
                            }
                            continue;
                        }
                        else
                        {
                            Generate(substringBefor, substringAfter, stringRule, newResult);
                            if (StateIndex[Rule] == States[Rule].Count - 1)
                            {
                                StateIndex[Rule] = 0;
                                StateBool[Rule] = true;
                                break;
                            }
                            else
                            {
                                StateIndex[Rule]++;
                            }
                            continue;
                        }          
                    }
                    
                    
                }
            }

            return 4;               
        }

    }
}
