using System.IO;
using Doji.PackageAuthoring.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies generated companion project manifest output produced by the public package authoring API.
    /// </summary>
    internal sealed class PackageAuthoringApiProjectGenerationTests : PackageAuthoringApiTestBase {
        [Test]
        public void GeneratePackage_WritesCompanionManifestForPreferredEditorAndTests() {
            ProjectSettings projectSettings = CreateProjectSettings("Companion With Tests");
            projectSettings.PreferredEditor = PreferredEditor.Rider;

            PackageSettings packageSettings = CreatePackageSettings(
                authorUrl: string.Empty,
                documentationUrl: string.Empty);
            packageSettings.CreateTestsFolder = true;

            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                projectSettings,
                packageSettings,
                CreateRepoSettings(),
                openProjectAfterCreation: false);

            string companionManifestPath = Path.Combine(
                rootDirectory,
                "projects",
                projectSettings.ProductName,
                "Packages",
                "manifest.json");
            JObject companionManifest = JObject.Parse(File.ReadAllText(companionManifestPath));
            JObject dependencies = (JObject)companionManifest["dependencies"];

            Assert.That(
                dependencies?["com.doji.tests.tokenized"]?.Value<string>(),
                Is.EqualTo("file:../../../com.doji.tests.tokenized"));
            Assert.That(
                dependencies?["com.unity.ide.rider"]?.Value<string>(),
                Is.EqualTo("3.0.39"));
            Assert.That(companionManifest["testables"]?.Values<string>(), Does.Contain("com.doji.tests.tokenized"));
        }

        [Test]
        public void GeneratePackage_OmitsCompanionManifestTestablesWhenTestsFolderIsDisabled() {
            ProjectSettings projectSettings = CreateProjectSettings("Companion Without Tests");
            PackageSettings packageSettings = CreatePackageSettings(
                authorUrl: string.Empty,
                documentationUrl: string.Empty);
            packageSettings.CreateTestsFolder = false;

            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                projectSettings,
                packageSettings,
                CreateRepoSettings(),
                openProjectAfterCreation: false);

            string companionManifestPath = Path.Combine(
                rootDirectory,
                "projects",
                projectSettings.ProductName,
                "Packages",
                "manifest.json");
            JObject companionManifest = JObject.Parse(File.ReadAllText(companionManifestPath));

            Assert.That(companionManifest["testables"], Is.Null);
        }

        [Test]
        public void GeneratePackage_DoesNotCopyPackageAuthoringTemplateAssetsIntoCompanionProject() {
            ProjectSettings projectSettings = CreateProjectSettings("Companion Without Template Assets");

            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                projectSettings,
                CreatePackageSettings(authorUrl: string.Empty, documentationUrl: string.Empty),
                CreateRepoSettings(),
                openProjectAfterCreation: false);

            string companionProjectSettingsPath = Path.Combine(
                rootDirectory,
                "projects",
                projectSettings.ProductName,
                "ProjectSettings");

            Assert.That(
                File.Exists(Path.Combine(companionProjectSettingsPath, "ProjectSettings.asset")),
                Is.True);
            Assert.That(
                Directory.GetFiles(companionProjectSettingsPath, "PackageAuthoring*", SearchOption.AllDirectories),
                Is.Empty);
        }
    }
}
