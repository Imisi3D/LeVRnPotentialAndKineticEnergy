using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LeVRn.Tutor
{
    /// <summary>
    /// A minimal, dependency-free JSON reader. Just enough to parse the authored
    /// content file into object graphs (Dictionary&lt;string, object&gt;,
    /// List&lt;object&gt;, string, double, bool, null). Kept in-house so the
    /// engine library ships with zero third-party dependencies and stays clean
    /// for Unity integration and licensing review.
    ///
    /// Supports the full JSON value grammar (objects, arrays, strings with
    /// escapes including \uXXXX, numbers, true, false, null). It is a reader
    /// only; it does not write JSON.
    /// </summary>
    internal static class Json
    {
        public static object Parse(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var p = new Parser(text);
            p.SkipWhitespace();
            object value = p.ParseValue();
            p.SkipWhitespace();
            if (!p.AtEnd)
                throw p.Error("Unexpected trailing characters");
            return value;
        }

        private sealed class Parser
        {
            private readonly string _s;
            private int _pos;

            public Parser(string s) { _s = s; _pos = 0; }

            public bool AtEnd => _pos >= _s.Length;

            public FormatException Error(string message)
            {
                return new FormatException(message + " at position " + _pos.ToString(CultureInfo.InvariantCulture) + ".");
            }

            public void SkipWhitespace()
            {
                while (_pos < _s.Length)
                {
                    char c = _s[_pos];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r') _pos++;
                    else break;
                }
            }

            private char Peek()
            {
                if (_pos >= _s.Length) throw Error("Unexpected end of input");
                return _s[_pos];
            }

            public object ParseValue()
            {
                SkipWhitespace();
                char c = Peek();
                switch (c)
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return ParseString();
                    case 't':
                    case 'f': return ParseBool();
                    case 'n': ParseLiteral("null"); return null;
                    default: return ParseNumber();
                }
            }

            private Dictionary<string, object> ParseObject()
            {
                var obj = new Dictionary<string, object>(StringComparer.Ordinal);
                Expect('{');
                SkipWhitespace();
                if (Peek() == '}') { _pos++; return obj; }
                while (true)
                {
                    SkipWhitespace();
                    if (Peek() != '"') throw Error("Expected property name string");
                    string key = ParseString();
                    SkipWhitespace();
                    Expect(':');
                    object value = ParseValue();
                    obj[key] = value;
                    SkipWhitespace();
                    char c = Peek();
                    if (c == ',') { _pos++; continue; }
                    if (c == '}') { _pos++; break; }
                    throw Error("Expected ',' or '}'");
                }
                return obj;
            }

            private List<object> ParseArray()
            {
                var arr = new List<object>();
                Expect('[');
                SkipWhitespace();
                if (Peek() == ']') { _pos++; return arr; }
                while (true)
                {
                    object value = ParseValue();
                    arr.Add(value);
                    SkipWhitespace();
                    char c = Peek();
                    if (c == ',') { _pos++; continue; }
                    if (c == ']') { _pos++; break; }
                    throw Error("Expected ',' or ']'");
                }
                return arr;
            }

            private string ParseString()
            {
                Expect('"');
                var sb = new StringBuilder();
                while (true)
                {
                    if (_pos >= _s.Length) throw Error("Unterminated string");
                    char c = _s[_pos++];
                    if (c == '"') break;
                    if (c == '\\')
                    {
                        if (_pos >= _s.Length) throw Error("Unterminated escape");
                        char e = _s[_pos++];
                        switch (e)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                if (_pos + 4 > _s.Length) throw Error("Bad unicode escape");
                                string hex = _s.Substring(_pos, 4);
                                _pos += 4;
                                sb.Append((char)ushort.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                                break;
                            default: throw Error("Invalid escape character");
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }

            private bool ParseBool()
            {
                if (Peek() == 't') { ParseLiteral("true"); return true; }
                ParseLiteral("false"); return false;
            }

            private void ParseLiteral(string literal)
            {
                if (_pos + literal.Length > _s.Length ||
                    string.CompareOrdinal(_s, _pos, literal, 0, literal.Length) != 0)
                    throw Error("Expected literal '" + literal + "'");
                _pos += literal.Length;
            }

            private double ParseNumber()
            {
                int start = _pos;
                if (Peek() == '-') _pos++;
                while (_pos < _s.Length)
                {
                    char c = _s[_pos];
                    if ((c >= '0' && c <= '9') || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-')
                        _pos++;
                    else break;
                }
                string num = _s.Substring(start, _pos - start);
                if (num.Length == 0) throw Error("Invalid number");
                return double.Parse(num, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            private void Expect(char c)
            {
                if (_pos >= _s.Length || _s[_pos] != c)
                    throw Error("Expected '" + c + "'");
                _pos++;
            }
        }
    }
}
