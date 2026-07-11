using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LeVRn.Progress
{
    /// <summary>
    /// A tiny, dependency-free JSON reader/writer. It exists so this assembly
    /// carries no third-party package (matching the rest of the offline core) and
    /// stays friendly to the Unity import pipeline. It parses into a plain object
    /// graph (<see cref="Dictionary{TKey,TValue}"/> with string keys,
    /// <see cref="List{T}"/> of object, string, double, bool, or null) and
    /// serialises the same shapes back. Numbers always parse to double; callers
    /// convert to int where needed.
    /// </summary>
    public static class Json
    {
        /// <summary>Thrown when the input is not well-formed JSON.</summary>
        public sealed class ParseException : Exception
        {
            public ParseException(string message) : base(message) { }
        }

        // ---------------- Serialize ----------------

        public static string Serialize(object value, bool indent = true)
        {
            var sb = new StringBuilder();
            WriteValue(sb, value, indent, 0);
            return sb.ToString();
        }

        private static void WriteValue(StringBuilder sb, object value, bool indent, int depth)
        {
            switch (value)
            {
                case null:
                    sb.Append("null");
                    break;
                case bool b:
                    sb.Append(b ? "true" : "false");
                    break;
                case string s:
                    WriteString(sb, s);
                    break;
                case int i:
                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                    break;
                case long l:
                    sb.Append(l.ToString(CultureInfo.InvariantCulture));
                    break;
                case double d:
                    sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
                    break;
                case IDictionary<string, object> map:
                    WriteObject(sb, map, indent, depth);
                    break;
                case IEnumerable<object> list:
                    WriteArray(sb, list, indent, depth);
                    break;
                default:
                    throw new ArgumentException("Json.Serialize cannot handle type " + value.GetType());
            }
        }

        private static void WriteObject(StringBuilder sb, IDictionary<string, object> map, bool indent, int depth)
        {
            if (map.Count == 0) { sb.Append("{}"); return; }
            sb.Append('{');
            bool first = true;
            foreach (var kv in map)
            {
                if (!first) sb.Append(',');
                first = false;
                NewlineIndent(sb, indent, depth + 1);
                WriteString(sb, kv.Key);
                sb.Append(indent ? ": " : ":");
                WriteValue(sb, kv.Value, indent, depth + 1);
            }
            NewlineIndent(sb, indent, depth);
            sb.Append('}');
        }

        private static void WriteArray(StringBuilder sb, IEnumerable<object> list, bool indent, int depth)
        {
            bool any = false;
            var inner = new StringBuilder();
            bool first = true;
            foreach (var item in list)
            {
                any = true;
                if (!first) inner.Append(',');
                first = false;
                NewlineIndent(inner, indent, depth + 1);
                WriteValue(inner, item, indent, depth + 1);
            }
            if (!any) { sb.Append("[]"); return; }
            sb.Append('[');
            sb.Append(inner);
            NewlineIndent(sb, indent, depth);
            sb.Append(']');
        }

        private static void NewlineIndent(StringBuilder sb, bool indent, int depth)
        {
            if (!indent) return;
            sb.Append('\n');
            sb.Append(' ', depth * 2);
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        // ---------------- Parse ----------------

        public static object Parse(string text)
        {
            if (text == null) throw new ParseException("Input is null.");
            var p = new Parser(text);
            p.SkipWhitespace();
            object value = p.ParseValue();
            p.SkipWhitespace();
            if (!p.AtEnd) throw new ParseException("Unexpected trailing content at position " + p.Position + ".");
            return value;
        }

        private sealed class Parser
        {
            private readonly string _s;
            private int _pos;

            public Parser(string s) { _s = s; _pos = 0; }
            public bool AtEnd => _pos >= _s.Length;
            public int Position => _pos;

            public void SkipWhitespace()
            {
                while (_pos < _s.Length)
                {
                    char c = _s[_pos];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r') _pos++;
                    else break;
                }
            }

            public object ParseValue()
            {
                if (AtEnd) throw new ParseException("Unexpected end of input.");
                char c = _s[_pos];
                switch (c)
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return ParseString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default: return ParseNumber();
                }
            }

            private Dictionary<string, object> ParseObject()
            {
                var map = new Dictionary<string, object>(StringComparer.Ordinal);
                _pos++; // {
                SkipWhitespace();
                if (!AtEnd && _s[_pos] == '}') { _pos++; return map; }
                while (true)
                {
                    SkipWhitespace();
                    if (AtEnd || _s[_pos] != '"') throw new ParseException("Expected string key at position " + _pos + ".");
                    string key = ParseString();
                    SkipWhitespace();
                    if (AtEnd || _s[_pos] != ':') throw new ParseException("Expected ':' at position " + _pos + ".");
                    _pos++;
                    SkipWhitespace();
                    map[key] = ParseValue();
                    SkipWhitespace();
                    if (AtEnd) throw new ParseException("Unterminated object.");
                    char c = _s[_pos++];
                    if (c == '}') break;
                    if (c != ',') throw new ParseException("Expected ',' or '}' at position " + (_pos - 1) + ".");
                }
                return map;
            }

            private List<object> ParseArray()
            {
                var list = new List<object>();
                _pos++; // [
                SkipWhitespace();
                if (!AtEnd && _s[_pos] == ']') { _pos++; return list; }
                while (true)
                {
                    SkipWhitespace();
                    list.Add(ParseValue());
                    SkipWhitespace();
                    if (AtEnd) throw new ParseException("Unterminated array.");
                    char c = _s[_pos++];
                    if (c == ']') break;
                    if (c != ',') throw new ParseException("Expected ',' or ']' at position " + (_pos - 1) + ".");
                }
                return list;
            }

            private string ParseString()
            {
                var sb = new StringBuilder();
                _pos++; // opening quote
                while (true)
                {
                    if (AtEnd) throw new ParseException("Unterminated string.");
                    char c = _s[_pos++];
                    if (c == '"') break;
                    if (c == '\\')
                    {
                        if (AtEnd) throw new ParseException("Unterminated escape.");
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
                                if (_pos + 4 > _s.Length) throw new ParseException("Bad unicode escape.");
                                string hex = _s.Substring(_pos, 4);
                                _pos += 4;
                                sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                                break;
                            default: throw new ParseException("Bad escape \\" + e + ".");
                        }
                    }
                    else sb.Append(c);
                }
                return sb.ToString();
            }

            private double ParseNumber()
            {
                int start = _pos;
                while (_pos < _s.Length)
                {
                    char c = _s[_pos];
                    if ((c >= '0' && c <= '9') || c == '-' || c == '+' || c == '.' || c == 'e' || c == 'E') _pos++;
                    else break;
                }
                if (_pos == start) throw new ParseException("Invalid value at position " + start + ".");
                string token = _s.Substring(start, _pos - start);
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    throw new ParseException("Invalid number '" + token + "'.");
                return d;
            }

            private void Expect(string literal)
            {
                if (_pos + literal.Length > _s.Length || _s.Substring(_pos, literal.Length) != literal)
                    throw new ParseException("Expected '" + literal + "' at position " + _pos + ".");
                _pos += literal.Length;
            }
        }
    }
}
