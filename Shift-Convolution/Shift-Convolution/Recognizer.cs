using System.Collections.Generic;
using System.Linq;

namespace Shift_Convolution
{
    class Recognizer
    {
        private enum State { q, b }

        private Dictionary<string, KeyValuePair<string[], int[]>> _grammar = new Dictionary<string, KeyValuePair<string[], int[]>>();
        private string _chain;
        private State _state = State.q;
        private string _mainRule;


        private List<string> _log = new List<string>();

        public Dictionary<string, KeyValuePair<string[], int[]>> Grammar => _grammar;
        public List<string> Log => _log;

        public string SetChain(string chain) => _chain = chain;

        public string SetaMainRule(string rule) => _mainRule = rule;

        private string L2ToString(List<int> L2)
        {
            string str = "[";
            for(int i = 0; i < L2.Count; ++i) str += L2[i] + ",";
            str = str.Remove(str.Length - 1);
            return str += "]";
        }

        public List<int> StartRecognize()
        {
            Log.Clear();

            int i = 1;
            int step = 2;
            int n = _chain.Length - 1;
            string chain = "";
            List<int> L2 = new List<int>();
            _log.Add($"(q, 1, ~, ~)-2->");
            while (true)
            {
                switch (step)
                {
                    case 1:
                        string[] RulesName = _grammar.Keys.ToArray();

                        bool FindRule = false;
                        for(int ind = 0; ind < chain.Length; ++ind)
                        {
                            for (int ind2 = 0; ind2 < RulesName.Length; ++ind2)
                            {
                                for (int ind3 = 0; ind3 < _grammar[RulesName[ind2]].Key.Length; ++ind3)
                                {
                                    if (_grammar[RulesName[ind2]].Key[ind3] == chain.Substring(ind))
                                    {
                                        L2.Insert(0, _grammar[RulesName[ind2]].Value[ind3]);
                                        chain = chain.Remove(ind);
                                        chain += RulesName[ind2];
                                        step = 1;
                                        FindRule = true;
                                        break;
                                    }
                                }
                                if (FindRule) break;
                            }
                            if (FindRule) break;
                        }
                        if (!FindRule)
                            step = 2;

                        _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                        break;

                    case 2:
                        if(i < n + 1)
                        {
                            chain += _chain[i];
                            L2.Insert(0, 0);
                            i++;
                            step = 1;
                            _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                            break;
                        }
                        else if (i == n + 1)
                        {
                            step = 3;
                            _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                            break;
                        }
                        break;

                    case 3:
                        if(i == n + 1 && chain == _mainRule)
                        {
                            _log.Add("Завершена успешно");
                            return L2;
                        }
                        else
                        {
                            step = 4;
                            _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                        }
                        break;
                    case 4:
                        step = 5;
                        _state = State.b;
                        _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                        break;

                    case 5:
                        if (L2.Count == 0)
                        {
                            _log.Add("Завершена НЕ успешно");
                            return L2;
                        }

                        if (L2[0] > 0)
                        {
                            string[] RulesName2 = _grammar.Keys.ToArray();
                            bool FindState = false;
                            bool FindState2 = false;
                            string Termal = "";
                            int ReplaceTermal = 0;

                            for (int ind = 0; ind < RulesName2.Length; ++ind)
                            {
                                for (int ind2 = 0; ind2 < _grammar[RulesName2[ind]].Key.Length; ++ind2)
                                {
                                    if(!FindState && _grammar[RulesName2[ind]].Value[ind2] == L2[0])
                                    {
                                        Termal = _grammar[RulesName2[ind]].Key[ind2];
                                        FindState = true;
                                        break;
                                    }
                                    if (FindState && _grammar[RulesName2[ind]].Key[ind2] == Termal)
                                    {
                                        L2.RemoveAt(0);
                                        L2.Insert(0, _grammar[RulesName2[ind]].Value[ind2]);
                                        ReplaceTermal = ind;
                                        FindState2 = true;
                                        break;
                                    }
                                }
                                if (FindState2) break;
                            }

                            if (FindState2) //5.1.1
                            {
                                _state = State.q;
                                chain = chain.Remove(chain.Length - 1);
                                chain += RulesName2[ReplaceTermal];
                                step = 1;
                                _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                                break;
                            }
                            else
                            {
                                if(i == n + 1)//5.1.2
                                {
                                    string TermChain = "";
                                    bool FindState3 = false;
                                    for (int ind = 0; ind < RulesName2.Length; ++ind)
                                    {
                                        for (int ind2 = 0; ind2 < _grammar[RulesName2[ind]].Key.Length; ++ind2)
                                        {
                                            if(_grammar[RulesName2[ind]].Value[ind2] == L2[0])
                                            {
                                                TermChain = _grammar[RulesName2[ind]].Key[ind2];
                                                FindState3 = true;
                                            }
                                        }
                                        if (FindState3) break;
                                    }

                                    L2.RemoveAt(0);
                                    chain = chain.Remove(chain.Length - 1);
                                    chain += TermChain;
                                    _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                                    break;
                                }
                                else if(i != n + 1) //5.1.3
                                {
                                    string TermChain2 = "";
                                    bool FindState4 = false;
                                    for (int ind = 0; ind < RulesName2.Length; ++ind)
                                    {
                                        for (int ind2 = 0; ind2 < _grammar[RulesName2[ind]].Key.Length; ++ind2)
                                        {
                                            if (_grammar[RulesName2[ind]].Value[ind2] == L2[0])
                                            {
                                                TermChain2 = _grammar[RulesName2[ind]].Key[ind2];
                                                FindState4 = true;
                                            }
                                        }
                                        if (FindState4) break;
                                    }

                                    _state = State.q;
                                    L2.RemoveAt(0);
                                    L2.Insert(0, 0);
                                    chain = chain.Remove(chain.Length - 1);
                                    chain += TermChain2 + _chain[i];
                                    i++;
                                    step = 1;
                                    _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}->");
                                    break;
                                }
                                else
                                {
                                    _log.Add("Завершена НЕ успешно");
                                    return L2;
                                    //return null;
                                }
                            }
                        }
                        else //5.2
                        {
                            if(i > 1)
                            {
                                L2.RemoveAt(0);
                                chain = chain.Remove(chain.Length - 1);
                                i--;
                                _log.Add($"({_state},{i},{chain},{L2ToString(L2)})-{step}.2->");
                                break;
                            }
                            else
                            {
                                _log.Add("Завершена НЕ успешно");
                                return L2;
                                //return null;
                            }
                        }
                        //break;
                }
            }
        }
    }
}
