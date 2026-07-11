using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LeVRn.Tutor
{
    /// <summary>
    /// Loads a <see cref="TutorScript"/> from the authored JSON content file.
    /// Kept separate from the engine so the engine core carries no I/O concern:
    /// any other loader (a Unity asset reader, a hand-built script) can produce
    /// the same <see cref="TutorScript"/> and the engine will not know the
    /// difference. Uses the in-house <see cref="Json"/> reader, so the library
    /// has zero third-party dependencies.
    /// </summary>
    public static class TutorScriptLoader
    {
        public static TutorScript LoadFromFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return LoadFromJson(File.ReadAllText(path));
        }

        public static TutorScript LoadFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var root = AsObject(Json.Parse(json), "root");

            string id = GetStringOrNull(root, "id");
            string title = GetStringOrNull(root, "title");
            string grounding = GetStringOrNull(root, "grounding");

            var guides = ParseGuides(AsObject(root["guides"], "guides"));
            var objectives = ParseObjectives(AsArray(root["objectives"], "objectives"));
            var nodes = ParseNodes(AsArray(root["script"], "script"));

            var extraClassify = new List<AskNode>();
            if (root.TryGetValue("extraClassify", out var extra) && extra != null)
            {
                foreach (var el in AsArray(extra, "extraClassify"))
                    extraClassify.Add(ParseAsk(AsObject(el, "extraClassify item")));
            }

            return new TutorScript(id, title, grounding, guides, objectives, nodes, extraClassify);
        }

        private static GuideSet ParseGuides(Dictionary<string, object> obj)
        {
            var list = new List<Guide>();
            foreach (var kvp in obj)
            {
                var g = AsObject(kvp.Value, "guide " + kvp.Key);
                list.Add(new Guide(
                    kvp.Key,
                    RequireString(g, "name"),
                    GetStringOrNull(g, "lens"),
                    GetStringOrNull(g, "color")));
            }
            return new GuideSet(list);
        }

        private static List<Objective> ParseObjectives(List<object> arr)
        {
            var list = new List<Objective>();
            foreach (var o in arr)
            {
                var obj = AsObject(o, "objective");
                list.Add(new Objective(RequireString(obj, "id"), RequireString(obj, "label")));
            }
            return list;
        }

        private static List<TutorNode> ParseNodes(List<object> arr)
        {
            var list = new List<TutorNode>();
            foreach (var n in arr)
                list.Add(ParseNode(AsObject(n, "node")));
            return list;
        }

        private static TutorNode ParseNode(Dictionary<string, object> el)
        {
            string t = RequireString(el, "t");
            switch (t)
            {
                case "say":
                    return new SayNode(RequireString(el, "g"), RequireString(el, "text"));
                case "ask":
                    return ParseAsk(el);
                case "scene":
                    return new SceneNode(RequireString(el, "fn"));
                case "end":
                    return new EndNode();
                default:
                    throw new FormatException("Unknown node type in content: " + t);
            }
        }

        private static AskNode ParseAsk(Dictionary<string, object> el)
        {
            var opts = new List<string>();
            foreach (var o in AsArray(el["opts"], "opts"))
                opts.Add(Convert.ToString(o, CultureInfo.InvariantCulture));

            var bank = new ResponseBank(
                RequireString(el, "affirm"),
                RequireString(el, "hint"),
                RequireString(el, "switchTo"),
                RequireString(el, "switchText"),
                RequireString(el, "walkthrough"));

            return new AskNode(
                RequireString(el, "g"),
                RequireString(el, "obj"),
                RequireString(el, "text"),
                opts,
                (int)Math.Round(AsDouble(el["correct"], "correct")),
                bank);
        }

        // ---- typed accessors ----

        private static Dictionary<string, object> AsObject(object value, string where)
        {
            if (value is Dictionary<string, object> d) return d;
            throw new FormatException("Expected an object for " + where + ".");
        }

        private static List<object> AsArray(object value, string where)
        {
            if (value is List<object> l) return l;
            throw new FormatException("Expected an array for " + where + ".");
        }

        private static double AsDouble(object value, string where)
        {
            if (value is double d) return d;
            throw new FormatException("Expected a number for " + where + ".");
        }

        private static string RequireString(Dictionary<string, object> obj, string name)
        {
            if (!obj.TryGetValue(name, out var v) || !(v is string s))
                throw new FormatException("Missing or non-string field '" + name + "'.");
            return s;
        }

        private static string GetStringOrNull(Dictionary<string, object> obj, string name)
        {
            return obj.TryGetValue(name, out var v) && v is string s ? s : null;
        }
    }
}
