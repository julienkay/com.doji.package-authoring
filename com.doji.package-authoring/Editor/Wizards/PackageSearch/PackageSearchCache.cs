using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.PackageSearch {
    /// <summary>
    /// Session-scoped package index shared by package authoring UI across editor windows, inspectors, and
    /// domain reloads.
    /// The cache merges Unity and scoped registry results into one lookup table, then restores that merged
    /// snapshot from <see cref="SessionState"/> until <c>Packages/manifest.json</c> changes.
    /// </summary>
    internal sealed class PackageSearchCache : IDisposable {
        private const string SessionStateKey =
            "Doji.PackageAuthoring.Wizards.PackageSearch.PackageSearchCache";

        private readonly List<IPackageSearchSource> _sources = new();
        private readonly List<PackageSearchEntry> _entries = new();

        /// <summary>
        /// Shared cache instance used by the package authoring tooling so all IMGUI hosts converge on the same
        /// merged package index and reuse the same session snapshot.
        /// </summary>
        public static PackageSearchCache Shared { get; } = new();

        public event Action Changed;

        public bool IsLoading {
            get {
                foreach (IPackageSearchSource source in _sources) {
                    if (source.IsLoading) {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool HasPackages => _entries.Count > 0;

        public string StatusMessage {
            get {
                if (IsLoading) {
                    return "Loading package indexes...";
                }

                int sourceCount = _sources.Count;
                return sourceCount == 0
                    ? "No package sources configured."
                    : $"Loaded {_entries.Count} packages from {sourceCount} source{(sourceCount == 1 ? string.Empty : "s")}.";
            }
        }

        [Serializable]
        private sealed class SessionCacheSnapshot {
            public string ManifestHash;
            public long CachedAtUtcTicks;
            public SessionCacheEntry[] Entries;
        }

        [Serializable]
        private sealed class SessionCacheEntry {
            public string PackageName;
            public string Version;
            public string DisplayName;
            public string Description;
            public string[] Keywords;
            public string SourceName;
        }

        private PackageSearchCache() {
        }

        /// <summary>
        /// Ensures package metadata is available either by restoring the current session snapshot or by starting
        /// fresh source queries when no compatible snapshot exists.
        /// </summary>
        public void EnsureLoaded() {
            if (_sources.Count == 0) {
                BuildSources();
                if (TryRestoreSnapshot()) {
                    return;
                }

                RefreshSources();
                return;
            }

            if (!HasPackages && !IsLoading) {
                RefreshSources();
            }
        }

        private void RefreshSources() {
            foreach (IPackageSearchSource source in _sources) {
                source.Refresh();
            }

            RebuildEntries();
            Changed?.Invoke();
        }

        public PackageSearchEntry? FindExact(string packageName) {
            foreach (PackageSearchEntry entry in _entries.Where(entry =>
                         string.Equals(entry.PackageName, packageName, StringComparison.OrdinalIgnoreCase))) {
                return entry;
            }

            return null;
        }

        public List<PackageSearchEntry> GetMatches(string query, int maxResults) {
            List<PackageSearchEntry> matches = new();
            string trimmedQuery = query?.Trim();

            foreach (PackageSearchEntry entry in _entries.Where(entry => PackageMatchesQuery(entry, trimmedQuery))) {
                matches.Add(entry);
                if (matches.Count >= maxResults) {
                    break;
                }
            }

            return matches;
        }

        public bool HasMoreMatches(string query, int currentMatchCount) {
            string trimmedQuery = query?.Trim();
            int matchCount = 0;

            foreach (PackageSearchEntry _ in _entries.Where(entry => PackageMatchesQuery(entry, trimmedQuery))) {
                matchCount++;
                if (matchCount > currentMatchCount) {
                    return true;
                }
            }

            return false;
        }

        public void Dispose() {
            DisposeSources();
        }

        private void BuildSources() {
            foreach (ScopedRegistryManifestReader.ScopedRegistryDefinition registry in ScopedRegistryManifestReader
                         .ReadFromProjectManifest(Path.Combine("Packages",
                             "manifest.json"))) {
                ScopedRegistryPackageSearchSource source = new ScopedRegistryPackageSearchSource(registry);
                source.Changed += HandleSourceChanged;
                _sources.Add(source);
            }

            UnityPackageSearchSource unitySource = new UnityPackageSearchSource();
            unitySource.Changed += HandleSourceChanged;
            _sources.Add(unitySource);
        }

        private void DisposeSources() {
            foreach (IPackageSearchSource source in _sources) {
                source.Changed -= HandleSourceChanged;
                source.Dispose();
            }

            _sources.Clear();
            _entries.Clear();
        }

        private void HandleSourceChanged() {
            RebuildEntries();
            SaveSnapshotIfReady();
            Changed?.Invoke();
        }

        private void RebuildEntries() {
            _entries.Clear();
            HashSet<string> seenPackageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (IPackageSearchSource source in _sources) {
                foreach (PackageSearchEntry entry in source.Entries) {
                    if (!seenPackageNames.Add(entry.PackageName)) {
                        continue;
                    }

                    _entries.Add(entry);
                }
            }

            _entries.Sort((left, right) =>
                string.Compare(left.PackageName, right.PackageName, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryRestoreSnapshot() {
            string snapshotJson = SessionState.GetString(SessionStateKey, string.Empty);
            if (string.IsNullOrWhiteSpace(snapshotJson)) {
                return false;
            }

            SessionCacheSnapshot snapshot = JsonUtility.FromJson<SessionCacheSnapshot>(snapshotJson);
            if (snapshot == null || !string.Equals(snapshot.ManifestHash, ComputeManifestHash(), StringComparison.Ordinal)) {
                SessionState.EraseString(SessionStateKey);
                return false;
            }

            _entries.Clear();
            if (snapshot.Entries != null) {
                foreach (SessionCacheEntry entry in snapshot.Entries) {
                    if (string.IsNullOrWhiteSpace(entry?.PackageName)) {
                        continue;
                    }

                    _entries.Add(new PackageSearchEntry(
                        entry.PackageName,
                        entry.Version,
                        entry.DisplayName,
                        entry.Description,
                        entry.Keywords ?? Array.Empty<string>(),
                        entry.SourceName));
                }
            }

            _entries.Sort((left, right) =>
                string.Compare(left.PackageName, right.PackageName, StringComparison.OrdinalIgnoreCase));
            return _entries.Count > 0;
        }

        private void SaveSnapshotIfReady() {
            if (IsLoading || _entries.Count == 0) {
                return;
            }

            SessionCacheSnapshot snapshot = new() {
                ManifestHash = ComputeManifestHash(),
                CachedAtUtcTicks = DateTime.UtcNow.Ticks,
                Entries = _entries
                    .Select(entry => new SessionCacheEntry {
                        PackageName = entry.PackageName,
                        Version = entry.Version,
                        DisplayName = entry.DisplayName,
                        Description = entry.Description,
                        Keywords = entry.Keywords ?? Array.Empty<string>(),
                        SourceName = entry.SourceName
                    })
                    .ToArray()
            };

            SessionState.SetString(SessionStateKey, JsonUtility.ToJson(snapshot));
        }

        private static string ComputeManifestHash() {
            string manifestPath = Path.Combine("Packages", "manifest.json");
            if (!File.Exists(manifestPath)) {
                return string.Empty;
            }

            string manifestJson = File.ReadAllText(manifestPath);
            unchecked {
                const uint fnvOffsetBasis = 2166136261u;
                const uint fnvPrime = 16777619u;

                uint hash = fnvOffsetBasis;
                foreach (char character in manifestJson) {
                    hash ^= character;
                    hash *= fnvPrime;
                }

                return hash.ToString("X8");
            }
        }

        private static bool PackageMatchesQuery(PackageSearchEntry entry, string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                return true;
            }

            // Matching stays focused on the fields users are most likely to search intentionally.
            return ContainsIgnoreCase(entry.PackageName, query)
                   || ContainsIgnoreCase(entry.DisplayName, query)
                   || ContainsIgnoreCase(entry.Keywords, query)
                   || ContainsIgnoreCase(entry.SourceName, query);
        }

        private static bool ContainsIgnoreCase(string value, string query) {
            return !string.IsNullOrEmpty(value)
                   && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsIgnoreCase(IEnumerable<string> values, string query) {
            if (values == null) {
                return false;
            }

            foreach (string value in values) {
                if (ContainsIgnoreCase(value, query)) {
                    return true;
                }
            }

            return false;
        }
    }
}
