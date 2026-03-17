namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Captures the wizard state that shapes the repository layout preview without exposing the full editor window.
    /// </summary>
    internal sealed class RepositoryLayoutPreviewData {
        public string RootDirectoryName { get; set; }

        public string PackageName { get; set; }

        public string AssemblyName { get; set; }

        public string CompanionProjectName { get; set; }

        public bool IncludeDocsFolder { get; set; }

        public bool IncludeSamplesFolder { get; set; }

        public bool IncludeEditorFolder { get; set; }

        public bool IncludeTestsFolder { get; set; }

        public bool IncludeRepositoryGitIgnore { get; set; }

        public bool IncludePackagesLockFile { get; set; }
    }
}
