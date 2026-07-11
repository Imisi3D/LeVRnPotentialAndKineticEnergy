using System;
using System.IO;
using System.Text;

namespace LeVRn.Progress
{
    /// <summary>
    /// Storage abstraction so the core stays free of any platform I/O. The Unity
    /// layer supplies an implementation backed by Application.persistentDataPath;
    /// tests use <see cref="FileStorage"/> against a temp directory. Keys are
    /// opaque slot identifiers, not file paths.
    /// </summary>
    public interface IStorage
    {
        bool Exists(string key);
        /// <summary>Read the stored text for a key. Returns false if absent.</summary>
        bool TryRead(string key, out string contents);
        void Write(string key, string contents);
        void Delete(string key);
    }

    /// <summary>File-backed <see cref="IStorage"/>. One UTF-8 file per key under a
    /// root directory. Suitable for Application.persistentDataPath in Unity.</summary>
    public sealed class FileStorage : IStorage
    {
        private readonly string _root;

        public FileStorage(string rootDirectory)
        {
            _root = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            Directory.CreateDirectory(_root);
        }

        private string PathFor(string key) => Path.Combine(_root, key + ".json");

        public bool Exists(string key) => File.Exists(PathFor(key));

        public bool TryRead(string key, out string contents)
        {
            string path = PathFor(key);
            if (!File.Exists(path)) { contents = null; return false; }
            contents = File.ReadAllText(path, Encoding.UTF8);
            return true;
        }

        public void Write(string key, string contents)
        {
            // Write to a temp file then move into place, so a crash mid-write does
            // not leave a half-written progress file.
            string path = PathFor(key);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, contents, new UTF8Encoding(false));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        public void Delete(string key)
        {
            string path = PathFor(key);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>
    /// Loads and saves <see cref="LearnerProgress"/> through an <see cref="IStorage"/>.
    /// Loading never throws on a missing, corrupt, or unreadable file: it logs the
    /// problem through the supplied sink and returns a fresh session instead.
    /// </summary>
    public sealed class ProgressStore
    {
        private readonly IStorage _storage;
        private readonly Action<string> _log;

        public ProgressStore(IStorage storage, Action<string> log = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _log = log ?? (_ => { });
        }

        /// <summary>Persist the record under its slot id. Call after every scored event.</summary>
        public void Save(LearnerProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            _storage.Write(progress.LearnerSlotId, progress.ToJson(indent: true));
        }

        /// <summary>
        /// Load the record for a slot, or return a fresh session. A corrupt or
        /// unreadable file is logged and replaced by a fresh session (the corrupt
        /// bytes are left on disk untouched so they can be inspected later); it
        /// never crashes and never silently discards a valid file.
        /// </summary>
        public LearnerProgress LoadOrNew(string slotId, System.Collections.Generic.IEnumerable<string> objectiveIds, DateTime nowUtc)
        {
            if (!_storage.TryRead(slotId, out string text) || string.IsNullOrWhiteSpace(text))
                return LearnerProgress.NewSession(slotId, objectiveIds, nowUtc);

            try
            {
                var loaded = LearnerProgress.FromJson(text);
                if (loaded == null || string.IsNullOrEmpty(loaded.LearnerSlotId))
                {
                    _log("Progress file for slot '" + slotId + "' parsed but was structurally invalid; starting a fresh session.");
                    return LearnerProgress.NewSession(slotId, objectiveIds, nowUtc);
                }
                if (loaded.SchemaVersion > LearnerProgress.CurrentSchemaVersion)
                    _log("Progress file for slot '" + slotId + "' has newer schema v" + loaded.SchemaVersion +
                         "; reading known fields and ignoring the rest.");
                return loaded;
            }
            catch (Exception ex)
            {
                _log("Progress file for slot '" + slotId + "' is corrupt (" + ex.GetType().Name + ": " +
                     ex.Message + "); starting a fresh session.");
                return LearnerProgress.NewSession(slotId, objectiveIds, nowUtc);
            }
        }
    }
}
