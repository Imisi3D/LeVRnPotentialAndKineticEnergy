using System;
using System.Collections.Generic;

namespace LeVRn.Tutor
{
    /// <summary>
    /// A peer guide identity and its teaching lens. One of Niyat, Morenike or
    /// Ejike. Identities are authored data loaded from content, never hardcoded.
    /// </summary>
    public sealed class Guide
    {
        public string Id { get; }
        public string Name { get; }
        public string Lens { get; }

        /// <summary>Presentation colour from the reference. Not used by the
        /// engine logic; carried through for UI layers.</summary>
        public string Color { get; }

        public Guide(string id, string name, string lens, string color)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Lens = lens ?? string.Empty;
            Color = color ?? string.Empty;
        }
    }

    /// <summary>The three guides, keyed by id.</summary>
    public sealed class GuideSet
    {
        private readonly Dictionary<string, Guide> _byId;

        public GuideSet(IEnumerable<Guide> guides)
        {
            if (guides == null) throw new ArgumentNullException(nameof(guides));
            _byId = new Dictionary<string, Guide>(StringComparer.Ordinal);
            foreach (var g in guides) _byId[g.Id] = g;
        }

        public Guide this[string id]
        {
            get
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                if (!_byId.TryGetValue(id, out var g))
                    throw new KeyNotFoundException("Unknown guide id: " + id);
                return g;
            }
        }

        public bool Contains(string id) => id != null && _byId.ContainsKey(id);
        public int Count => _byId.Count;
    }
}
