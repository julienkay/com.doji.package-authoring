namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Provides the built-in fallback content for generated custom repository license files.
    /// </summary>
    internal static class CustomLicenseTemplate {
        public const string DefaultContent = @"Custom License

Package: {{PACKAGE_NAME}}
Version: {{PACKAGE_VERSION}}
Company: {{PACKAGE_COMPANY}}

Copyright (c) {{YEAR}} {{COPYRIGHT_HOLDER}}

Replace this text in Project Settings > Doji > Package Authoring > Templates.

Available tokens:
- {{YEAR}}
- {{COPYRIGHT_HOLDER}}
- {{PACKAGE_NAME}}
- {{PACKAGE_VERSION}}
- {{PACKAGE_COMPANY}}
- {{PROJECT_NAME}}
- {{PROJECT_COMPANY}}
- {{NAMESPACE_NAME}}
- {{ASSEMBLY_NAME}}";
    }
}
