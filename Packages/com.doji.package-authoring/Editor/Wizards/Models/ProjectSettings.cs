using System;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Models {
    /// <summary>
    /// Selects which Unity IDE integration package should be installed into generated projects.
    /// </summary>
    public enum PreferredEditor {
        None,
        VisualStudio,
        VisualStudioCode,
        Rider
    }

    /// <summary>
    /// Shared project-facing settings used by both the standalone and companion project wizards.
    /// </summary>
    [Serializable]
    public class ProjectSettings {
        /// <summary>
        /// Company or organization name written into generated Unity project settings.
        /// </summary>
        [field: SerializeField]
        public string CompanyName { get; set; } = "Your Company";

        /// <summary>
        /// User-facing project name for generated standalone or companion projects.
        /// </summary>
        [field: SerializeField]
        public string ProductName { get; set; } = "My Project";

        /// <summary>
        /// Version applied to generated project and package metadata.
        /// </summary>
        [field: SerializeField]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// IDE integration package added to generated project manifests for script-opening and project sync.
        /// </summary>
        [field: SerializeField]
        public PreferredEditor PreferredEditor { get; set; } = PreferredEditor.None;

        /// <summary>
        /// Additional packages merged into generated project manifests on top of the copied baseline manifest.
        /// </summary>
        [field: SerializeField]
        public PackageDependencyList IncludedPackages { get; set; } = new();

        /// <summary>
        /// Whether generated repositories should include an <c>AGENTS.md</c> instructions file.
        /// </summary>
        [field: SerializeField]
        public bool GenerateAgentsFile { get; set; }

        /// <summary>
        /// Base output folder used when scaffolding projects from the editor windows.
        /// </summary>
        [field: SerializeField]
        public string TargetLocation { get; set; } = "../";

        /// <summary>
        /// Copies all project-facing values from another settings instance.
        /// </summary>
        /// <param name="other">The source settings to copy from.</param>
        public void CopyFrom(ProjectSettings other) {
            if (other == null) {
                return;
            }

            CompanyName = other.CompanyName;
            ProductName = other.ProductName;
            Version = other.Version;
            PreferredEditor = other.PreferredEditor;
            IncludedPackages ??= new PackageDependencyList();
            IncludedPackages.CopyFrom(other.IncludedPackages);
            GenerateAgentsFile = other.GenerateAgentsFile;
            TargetLocation = other.TargetLocation;
        }
    }
}
