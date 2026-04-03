using Doji.PackageAuthoring.Editor.Wizards.Templates;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Captures the wizard state that shapes the repository layout preview without exposing the full editor window.
    /// </summary>
    internal sealed class RepositoryLayoutPreviewData {
        /// <summary>
        /// Current scaffold context used to resolve generated file contents.
        /// </summary>
        public PackageContext Context { get; set; }

        /// <summary>
        /// Display name of the repository root shown as the tree root.
        /// </summary>
        public string RootDirectoryName { get; set; }

        /// <summary>
        /// Package folder name under the repository root.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Assembly base name used by generated asmdef files.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Companion Unity project folder name under <c>projects/</c>.
        /// </summary>
        public string CompanionProjectName { get; set; }

        /// <summary>
        /// Whether the repository preview should include the docs scaffold.
        /// </summary>
        public bool IncludeDocsFolder { get; set; }

        /// <summary>
        /// Whether the package preview should include the <c>Samples~</c> scaffold.
        /// </summary>
        public bool IncludeSamplesFolder { get; set; }

        /// <summary>
        /// Whether the package preview should include the editor-only assembly scaffold.
        /// </summary>
        public bool IncludeEditorFolder { get; set; }

        /// <summary>
        /// Whether the package preview should include the tests folder.
        /// </summary>
        public bool IncludeTestsFolder { get; set; }

        /// <summary>
        /// Whether the companion project preview should include a generated <c>.gitignore</c>.
        /// </summary>
        public bool IncludeRepositoryGitIgnore { get; set; }

        /// <summary>
        /// Whether the repository preview should include a generated <c>AGENTS.md</c>.
        /// </summary>
        public bool IncludeAgentsFile { get; set; }

        /// <summary>
        /// Template content shown for the generated companion-project <c>.gitignore</c>.
        /// </summary>
        public string RepositoryGitIgnoreTemplate { get; set; }

        /// <summary>
        /// Whether the companion project preview should include the copied lock file from the template project.
        /// </summary>
        public bool IncludePackagesLockFile { get; set; }
    }
}
