namespace Doji.PackageAuthoring.Wizards.Templates {
    /// <summary>
    /// Builds Unity meta file content for generated assets so their GUIDs are stable before first import.
    /// </summary>
    internal static class AssetMetaTemplate {
        /// <summary>
        /// Builds meta file content for a generated folder.
        /// </summary>
        public static string GetFolderMeta(string guid) {
            return $@"fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
";
        }

        /// <summary>
        /// Builds meta file content for an assembly definition asset.
        /// </summary>
        public static string GetAsmDefMeta(string guid) {
            return GetContent(guid, "AssemblyDefinitionImporter");
        }

        /// <summary>
        /// Builds meta file content for a generated text asset such as <c>README.md</c> or <c>CHANGELOG.md</c>.
        /// </summary>
        public static string GetTextAssetMeta(string guid) {
            return GetContent(guid, "TextScriptImporter");
        }

        /// <summary>
        /// Builds meta file content for a generated <c>package.json</c> manifest.
        /// </summary>
        public static string GetPackageManifestMeta(string guid) {
            return GetContent(guid, "PackageManifestImporter");
        }

        /// <summary>
        /// Builds meta file content for a generated C# script.
        /// </summary>
        public static string GetMonoScriptMeta(string guid) {
            return $@"fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData:
  assetBundleName:
  assetBundleVariant:
";
        }

        public static string GetContent(string guid, string importerName) {
            return $@"fileFormatVersion: 2
guid: {guid}
{importerName}:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
";
        }
    }
}
