﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class Chain
    {
        private Chain()
        {
            m_elements = new List<string>();
            m_startPos = m_endPos = -1;
        }
        private List<string> m_elements;
        public List<string> Elements
        {
            get { return m_elements; }
            set { m_elements = value; }
        }



        private int m_startPos;
        public int StartPos
        {
            get { return m_startPos; }
            private set { m_startPos = value; }
        }
        private int m_endPos;
        public int EndPos
        {
            get { return m_endPos; }
            private set { m_endPos = value; }
        }

        private Function m_lastFunction;
        public Function LastFunction
        {
            get { return m_lastFunction; }
            private set { m_lastFunction = value; }
        }
        public Type getType(VariableManager variables)
        {
            if (Elements.Count == 0) return null;
            string word = Elements[0];
            Type t = null;
            Variable var = variables.getVariable(word);
            if (var != null)
            {
                t = var.Type;
            }
            else
            {
                Function func = variables.getFunction(word);
                if (func != null)
                {
                    LastFunction = func;
                    t = func.ReturnType;
                }
            }
            if (t == null) return null;

            if (Elements.Count == 1) return t;

            for (int i = 1; i < Elements.Count - 1; i++)
            {
                string name = Elements[i];
                if (t.Members.ContainsKey(name))
                {
                    t = t.Members[name].Type;
                }
                else if (t.Methods.ContainsKey(name))
                {
                    t = t.Methods[name].ReturnType;
                }
                else return null;
            }
            //last
            string last = getLastElement();
            if (t.Members.ContainsKey(last))
            {
                return t.Members[last].Type;
            }
            else if (t.Methods.ContainsKey(last))
            {
                LastFunction = t.Methods[last];
                return t.Methods[last].ReturnType;
            }
            else return t;
        }

        public string getLastElement()
        {
            return Elements[Elements.Count - 1];
        }

        public override string ToString()
        {
            string rst = "";
            for (int i = 0; i < Elements.Count; i++)
            {
                rst += Elements[i];
                if (i != Elements.Count - 1)
                {
                    rst += "\n";
                }
            }

            return rst;
        }


        enum PaserState
        {
            searchWordEnd,
            searchWordStart,
            searchSeperator,
            searchBracket
        };
        public static Chain ParseBackward(ScintillaNET.Scintilla scintilla, int pos = -1)
        {
            const string seperator = ".:";
            const string lbracket = "([{";
            const string rbracket = ")]}";

            string str = scintilla.Text;
            if (pos < 0)
            {
                pos = scintilla.CurrentPos - 1;
            }
            PaserState state = PaserState.searchWordEnd;

            Chain rst = new Chain();
            int wordStart = pos;
            int wordEnd = pos;

            int bracketLevel = 0;



            while (pos >= 0)
            {
                char c = str[pos];
                bool isComment = Parser.isComment(scintilla, pos);
                bool isString = Parser.isString(scintilla, pos);



                switch (state)
                {
                    case PaserState.searchWordStart:
                        if (isString) return rst;
                        if (!char.IsLetterOrDigit(c) || isComment || pos == 0)
                        {
                            wordStart = pos;
                            string word;
                            if (pos != 0) word = str.Substring(wordStart + 1, wordEnd - wordStart);
                            else word = str.Substring(wordStart, wordEnd - wordStart + 1);
                            word.Trim();
                            {
                                rst.Elements.Insert(0, word);
                                rst.StartPos = pos;
                            }
                            state = PaserState.searchSeperator;
                        }
                        else
                        {
                            pos--;
                        }

                        break;

                    case PaserState.searchWordEnd:
                        if (isString) return rst;
                        if (isComment)
                        {
                            pos--;
                            break;
                        }
                        if (rbracket.Contains(c))
                        {
                            state = PaserState.searchBracket;
                            break;
                        }
                        if (char.IsLetterOrDigit(c))
                        {
                            wordEnd = pos;
                            if (rst.EndPos < 0) rst.EndPos = pos;
                            state = PaserState.searchWordStart;
                        }
                        else
                        {
                            pos--;
                        }
                        break;
                    case PaserState.searchSeperator:
                        if (isString) return rst;
                        if (seperator.Contains(c))
                        {
                            state = PaserState.searchWordEnd;
                            pos--;
                        }
                        else if (char.IsWhiteSpace(c) || isComment)
                        {
                            pos--;
                        }
                        else
                        {
                            //end
                            return rst;
                        }
                        break;
                    case PaserState.searchBracket:
                        if (!isComment && !isString)
                        {
                            if (rbracket.Contains(c)) bracketLevel++;
                            else if (lbracket.Contains(c))
                            {
                                bracketLevel--;
                                if (bracketLevel == 0)
                                {
                                    state = PaserState.searchWordEnd;
                                }
                            }
                        }
                        pos--;
                        break;
                }
            }


            return rst;
        }

        public static Chain ParseFoward(ScintillaNET.Scintilla scintilla, int pos)
        {
            const string seperator = ".:";
            const string lbracket = "([{";
            const string rbracket = ")]}";

            string str = scintilla.Text;

            PaserState state = PaserState.searchWordStart;

            Chain rst = new Chain();
            int wordStart = pos;
            int wordEnd = pos;

            int bracketLevel = 0;



            while (pos < str.Length)
            {
                char c = str[pos];

                bool isComment = Parser.isComment(scintilla, pos);
                bool isString = Parser.isString(scintilla, pos);

                switch (state)
                {
                    case PaserState.searchWordEnd:
                        if (isString) return rst;
                        if (!char.IsLetterOrDigit(c) || isComment || pos == str.Length - 1)
                        {
                            wordEnd = pos;
                            string word;
                            if (pos == str.Length - 1)
                            {
                                word = str.Substring(wordStart, wordEnd - wordStart + 1);
                            }
                            else
                            {
                                word = str.Substring(wordStart, wordEnd - wordStart);
                            }
                            word.Trim();
                            {
                                rst.Elements.Add(word);
                                rst.EndPos = pos;
                            }
                            state = PaserState.searchSeperator;
                        }
                        else
                        {
                            pos++;
                        }

                        break;

                    case PaserState.searchWordStart:
                        if (isString) return rst;
                        if (isComment)
                        {
                            pos++;
                            break;
                        }

                        if (char.IsLetterOrDigit(c))
                        {
                            wordStart = pos;
                            if (rst.StartPos < 0) rst.StartPos = pos;
                            state = PaserState.searchWordEnd;
                        }
                        else
                        {
                            pos++;
                        }
                        break;
                    case PaserState.searchSeperator:
                        if (isString) return rst;
                        if (lbracket.Contains(c))
                        {
                            state = PaserState.searchBracket;
                            break;
                        }
                        if (seperator.Contains(c))
                        {
                            state = PaserState.searchWordStart;
                            pos++;
                        }
                        else if (char.IsWhiteSpace(c) || isComment)
                        {
                            pos++;
                        }
                        else
                        {
                            //end
                            return rst;
                        }
                        break;
                    case PaserState.searchBracket:
                        if (!isComment && !isString)
                        {
                            if (lbracket.Contains(c)) bracketLevel++;
                            else if (rbracket.Contains(c))
                            {
                                bracketLevel--;
                                if (bracketLevel == 0)
                                {
                                    state = PaserState.searchWordStart;
                                }
                            }
                        }
                        pos++;
                        break;
                }
            }


            return rst;
        }
    }
}