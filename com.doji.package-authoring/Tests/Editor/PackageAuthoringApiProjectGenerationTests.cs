using System.IO;
using System.Linq;
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
        public void GeneratePackage_SortsCompanionManifestDependenciesToMatchUnityPackageManagerOrder() {
            ProjectSettings projectSettings = CreateProjectSettings("Companion Dependency Ordering");
            projectSettings.PreferredEditor = PreferredEditor.Rider;
            projectSettings.IncludedPackages.Items.Add(new PackageDependencyEntry {
                PackageName = "com.unity.test-framework",
                Version = "1.5.1"
            });
            projectSettings.IncludedPackages.Items.Add(new PackageDependencyEntry {
                PackageName = "com.unity.2d.sprite",
                Version = "1.0.0"
            });

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
                dependencies?.Properties().Select(property => property.Name).ToArray(),
                Is.EqualTo(new[] {
                    "com.doji.tests.tokenized",
                    "com.unity.2d.sprite",
                    "com.unity.ide.rider",
                    "com.unity.inputsystem",
                    "com.unity.modules.adaptiveperformance",
                    "com.unity.modules.ai",
                    "com.unity.modules.androidjni",
                    "com.unity.modules.animation",
                    "com.unity.modules.assetbundle",
                    "com.unity.modules.audio",
                    "com.unity.modules.imageconversion",
                    "com.unity.modules.imgui",
                    "com.unity.modules.jsonserialize",
                    "com.unity.modules.particlesystem",
                    "com.unity.modules.physics",
                    "com.unity.modules.physics2d",
                    "com.unity.modules.screencapture",
                    "com.unity.modules.tilemap",
                    "com.unity.modules.ui",
                    "com.unity.modules.uielements",
                    "com.unity.modules.unitywebrequest",
                    "com.unity.modules.unitywebrequestassetbundle",
                    "com.unity.modules.unitywebrequestaudio",
                    "com.unity.modules.unitywebrequesttexture",
                    "com.unity.modules.unitywebrequestwww",
                    "com.unity.modules.video",
                    "com.unity.modules.vr",
                    "com.unity.modules.xr",
                    "com.unity.nuget.newtonsoft-json",
                    "com.unity.render-pipelines.universal",
                    "com.unity.test-framework",
                    "com.unity.ugui"
                }));
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
