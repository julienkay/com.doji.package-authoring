namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Provides the built-in default content for generated package and repository README templates.
    /// </summary>
    public static class ReadmeTemplate {
        public const string PackageReadmeDefaultContent = @"# {{PACKAGE_NAME}}

{{PACKAGE_DESCRIPTION}}
";

        public const string RepositoryReadmeDefaultContent = @"# {{PROJECT_NAME}}

{{PACKAGE_DESCRIPTION}}

## Installation

Add the package via UPM using:

`{{PACKAGE_NAME}}`

## Documentation

Document the public workflow, setup steps, and examples here.
";
    }
}
