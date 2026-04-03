using System;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Carries generated repository file content into the Inspector when a preview entry is selected.
    /// </summary>
    internal sealed class RepositoryLayoutPreviewSelection : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private string _relativePath;
        [SerializeField] private string _content;
        [SerializeField] private string _sourceFilePath;
        [SerializeField] private string _sourceFolderPath;
        [NonSerialized] private string[] _hoverHighlights = Array.Empty<string>();

        /// <summary>
        /// File or entry name shown in the Inspector header row.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Relative output path represented by the selected preview entry.
        /// </summary>
        public string RelativePath => _relativePath;

        /// <summary>
        /// Resolved file contents shown in the Inspector preview area.
        /// </summary>
        public string Content => _content;

        /// <summary>
        /// Optional template-backed file path that can be opened directly from the Inspector.
        /// </summary>
        public string SourceFilePath => _sourceFilePath;

        /// <summary>
        /// Optional directory path associated with the selected preview entry.
        /// </summary>
        public string SourceFolderPath => _sourceFolderPath;

        /// <summary>
        /// Literal values that should be accent-highlighted in the Inspector preview while a related field is hovered.
        /// </summary>
        public string[] HoverHighlights => _hoverHighlights ?? Array.Empty<string>();

        /// <summary>
        /// Replaces the selected preview payload without creating a new inspector target.
        /// </summary>
        public void UpdateContent(
            string displayName,
            string relativePath,
            string content,
            string sourceFilePath,
            string sourceFolderPath,
            string[] hoverHighlights) {
            _displayName = displayName;
            _relativePath = relativePath;
            _content = content;
            _sourceFilePath = sourceFilePath;
            _sourceFolderPath = sourceFolderPath;
            _hoverHighlights = hoverHighlights ?? Array.Empty<string>();
            name = string.IsNullOrWhiteSpace(displayName) ? "Repository Layout Preview" : displayName;
        }
    }
}
