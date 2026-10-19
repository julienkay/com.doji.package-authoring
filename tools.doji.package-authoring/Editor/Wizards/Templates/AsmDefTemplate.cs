namespace Doji.PackageAuthoring.Wizards.Templates {
    /// <summary>
    /// Builds asmdef files for generated package assemblies.
    /// </summary>
    internal static class AsmDefTemplate {
        /// <summary>
        /// Builds the runtime asmdef JSON for the generated package.
        /// </summary>
        public static string GetRuntimeAsmDef(PackageContext ctx) {
            return $@"{{
    ""name"": ""{ctx.Package.AssemblyName}"",
    ""rootNamespace"": ""{ctx.Package.NamespaceName}"",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        }

        /// <summary>
        /// Builds the samples asmdef JSON for the generated package.
        /// </summary>
        /// <param name="runtimeAssemblyGuid">GUID of the generated runtime asmdef used for stable references.</param>
        public static string GetSamplesAsmDef(PackageContext ctx, string runtimeAssemblyGuid) {
            return $@"{{
    ""name"": ""{ctx.Package.AssemblyName}.Samples"",
    ""rootNamespace"": ""{ctx.Package.NamespaceName}.Samples"",
    ""references"": [
        ""GUID:{runtimeAssemblyGuid}""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        }

        /// <summary>
        /// Builds the editor-only asmdef JSON for the generated package.
        /// </summary>
        /// <param name="runtimeAssemblyGuid">GUID of the generated runtime asmdef used for stable references.</param>
        public static string GetEditorAsmDef(PackageContext ctx, string runtimeAssemblyGuid) {
            return $@"{{
    ""name"": ""{ctx.Package.AssemblyName}.Editor"",
    ""rootNamespace"": ""{ctx.Package.NamespaceName}.Editor"",
    ""references"": [
        ""GUID:{runtimeAssemblyGuid}""
    ],
    ""includePlatforms"": [
        ""Editor""
    ],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        }

        /// <summary>
        /// Builds the Editor test asmdef JSON for the generated package.
        /// </summary>
        /// <param name="runtimeAssemblyGuid">GUID of the generated runtime asmdef.</param>
        /// <param name="editorAssemblyGuid">Optional GUID of the generated editor asmdef.</param>
        public static string GetEditorTestsAsmDef(
            PackageContext ctx,
            string runtimeAssemblyGuid,
            string editorAssemblyGuid) {
            string editorReference = string.IsNullOrWhiteSpace(editorAssemblyGuid)
                ? string.Empty
                : $"        \"GUID:{editorAssemblyGuid}\",\n";

            return $@"{{
    ""name"": ""{ctx.Package.AssemblyName}.Editor.Tests"",
    ""rootNamespace"": ""{ctx.Package.NamespaceName}.Tests"",
    ""references"": [
{editorReference}        ""GUID:{runtimeAssemblyGuid}""
    ],
    ""optionalUnityReferences"": [
        ""TestAssemblies""
    ],
    ""includePlatforms"": [
        ""Editor""
    ],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": false,
    ""defineConstraints"": [
        ""UNITY_INCLUDE_TESTS""
    ],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        }
    }
}
