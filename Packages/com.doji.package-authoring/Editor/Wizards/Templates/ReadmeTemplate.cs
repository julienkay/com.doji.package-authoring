namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Builds README files for the generated package repository.
    /// </summary>
    public static class ReadmeTemplate {
        public const string RepositoryReadmeDefaultContent = @"# {{PROJECT_NAME}}

{{PACKAGE_DESCRIPTION}}

## Installation

Add the package via UPM using:

`{{PACKAGE_NAME}}`

## Documentation

Document the public workflow, setup steps, and examples here.
";

        public static string GetPackageReadme(PackageContext ctx) {
            return $"# {ctx.Package.PackageName}\n\n{ctx.Package.Description}";
        }
    }
}
