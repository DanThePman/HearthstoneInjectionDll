using System;
using System.Text;
using UnityEngine;

public class Tokenizer
{
    private char[] m_chars;
    private int m_index;
    private const char NULLCHAR = '\0';

    public Tokenizer(string str)
    {
        this.m_chars = str.ToCharArray();
        this.m_index = 0;
    }

    public void ClearWhiteSpace()
    {
        while (this.IsWhiteSpace(this.CurrentChar()))
        {
            this.NextChar();
        }
    }

    private char CurrentChar()
    {
        if (this.m_index >= this.m_chars.Length)
        {
            return '\0';
        }
        return this.m_chars[this.m_index];
    }

    private bool IsDecimal(char c)
    {
        return ((c >= '0') && (c <= '9'));
    }

    private bool IsEOF()
    {
        return (this.m_index >= this.m_chars.Length);
    }

    private bool IsWhiteSpace(char c)
    {
        return ((c != '\0') && (c <= ' '));
    }

    private char NextChar()
    {
        if (this.m_index >= this.m_chars.Length)
        {
            return '\0';
        }
        char ch = this.m_chars[this.m_index];
        this.m_index++;
        return ch;
    }

    private bool NextCharIsWhiteSpace()
    {
        if ((this.m_index + 1) >= this.m_chars.Length)
        {
            return false;
        }
        return this.IsWhiteSpace(this.m_chars[this.m_index + 1]);
    }

    public float NextFloat()
    {
        char ch;
        this.ClearWhiteSpace();
        float num = 1f;
        float num2 = 0f;
        if (this.CurrentChar() == '-')
        {
            num = -1f;
            this.NextChar();
        }
        bool flag = false;
        float p = 1f;
    Label_0034:
        ch = this.CurrentChar();
        if ((ch != '\0') && !this.IsWhiteSpace(ch))
        {
            if (ch == 'f')
            {
                this.NextChar();
            }
            else
            {
                if (ch == '.')
                {
                    flag = true;
                    this.NextChar();
                }
                else
                {
                    if (!this.IsDecimal(ch))
                    {
                        throw new Exception(string.Format("Found a non-numeric value while parsing an int: {0}", ch));
                    }
                    float num4 = ch - '0';
                    if (!flag)
                    {
                        num2 *= 10f;
                        num2 += num4;
                    }
                    else
                    {
                        float num5 = num4 * Mathf.Pow(0.1f, p);
                        num2 += num5;
                        p++;
                    }
                    this.NextChar();
                }
                goto Label_0034;
            }
        }
        return (num * num2);
    }

    public void NextOpenBracket()
    {
        this.ClearWhiteSpace();
        if (this.CurrentChar() != '{')
        {
            throw new Exception("Expected open bracket.");
        }
        this.NextChar();
    }

    public string NextQuotedString()
    {
        this.ClearWhiteSpace();
        if (this.IsEOF())
        {
            return null;
        }
        char ch = this.NextChar();
        if (ch != '"')
        {
            throw new Exception(string.Format("Expected quoted string.  Found {0} instead of quote.", ch));
        }
        StringBuilder builder = new StringBuilder();
        while (true)
        {
            char ch2 = this.CurrentChar();
            switch (ch2)
            {
                case '"':
                    this.NextChar();
                    return builder.ToString();

                case '\0':
                    throw new Exception("Parsing ended before quoted string was completed.");
            }
            builder.Append(ch2);
            this.NextChar();
        }
    }

    public string NextString()
    {
        this.ClearWhiteSpace();
        if (this.IsEOF())
        {
            return null;
        }
        StringBuilder builder = new StringBuilder();
        while (true)
        {
            char c = this.CurrentChar();
            if ((c == '\0') || this.IsWhiteSpace(c))
            {
                break;
            }
            builder.Append(c);
            this.NextChar();
        }
        return builder.ToString();
    }

    public uint NextUInt32()
    {
        this.ClearWhiteSpace();
        uint num = 0;
        while (true)
        {
            char c = this.CurrentChar();
            if (c == '\0')
            {
                return num;
            }
            if (this.IsWhiteSpace(c))
            {
                return num;
            }
            if (!this.IsDecimal(c))
            {
                throw new Exception(string.Format("Found a non-numeric value while parsing an int: {0}", c));
            }
            uint num2 = c - '0';
            num *= 10;
            num += num2;
            this.NextChar();
        }
    }

    private void PrevChar()
    {
        if (this.m_index > 0)
        {
            this.m_index--;
        }
    }

    public void SkipUnknownToken()
    {
        this.ClearWhiteSpace();
        if (!this.IsEOF())
        {
            if (this.CurrentChar() == '"')
            {
                this.NextQuotedString();
            }
            else
            {
                this.NextString();
            }
        }
    }
}

