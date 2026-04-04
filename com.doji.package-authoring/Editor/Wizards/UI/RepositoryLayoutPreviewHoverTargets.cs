namespace Doji.PackageAuthoring.Wizards.UI {
    /// <summary>
    /// Stable hover identifiers that link wizard fields to preview highlight rules.
    /// </summary>
    /// <remarks>
    /// The identifiers live here so drawers can remain lightweight and only publish semantic hover intents.
    /// The actual mapping from each intent to one or more preview rows is resolved in
    /// <see cref="RepositoryLayoutPreviewPanel"/> via its <c>MatchesTarget</c> logic, where the current preview tree
    /// and wizard state are available.
    /// </remarks>
    internal static class RepositoryLayoutPreviewHoverTargets {
        public const string RepoCopyrightHolder = nameof(RepoCopyrightHolder);
        public const string RepoLicenseType = nameof(RepoLicenseType);
        public const string IncludeRepositoryReadme = nameof(IncludeRepositoryReadme);
        public const string PackageName = nameof(PackageName);
        public const string PackageDisplayName = nameof(PackageDisplayName);
        public const string AssemblyName = nameof(AssemblyName);
        public const string NamespaceName = nameof(NamespaceName);
        public const string Description = nameof(Description);
        public const string PackageCompanyName = nameof(PackageCompanyName);
        public const string IncludeAuthor = nameof(IncludeAuthor);
        public const string AuthorUrl = nameof(AuthorUrl);
        public const string AuthorEmail = nameof(AuthorEmail);
        public const string DocumentationUrl = nameof(DocumentationUrl);
        public const string IncludeMinimumUnityVersion = nameof(IncludeMinimumUnityVersion);
        public const string MinimumUnityVersion = nameof(MinimumUnityVersion);
        public const string CreateDocsFolder = nameof(CreateDocsFolder);
        public const string IncludePackageReadme = nameof(IncludePackageReadme);
        public const string CreateSamplesFolder = nameof(CreateSamplesFolder);
        public const string CreateEditorFolder = nameof(CreateEditorFolder);
        public const string CreateTestsFolder = nameof(CreateTestsFolder);
        public const string Dependencies = nameof(Dependencies);
        public const string ProjectCompanyName = nameof(ProjectCompanyName);
        public const string ProductName = nameof(ProductName);
        public const string Version = nameof(Version);
        public const string ProjectManifest = nameof(ProjectManifest);
        public const string GenerateAgentsFile = nameof(GenerateAgentsFile);
        public const string TargetLocation = nameof(TargetLocation);
    }
}
