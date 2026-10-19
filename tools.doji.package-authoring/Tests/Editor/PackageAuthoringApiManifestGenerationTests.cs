using System.IO;
using Doji.PackageAuthoring.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies generated package manifest output produced by the public package authoring API.
    /// </summary>
    internal sealed class PackageAuthoringApiManifestGenerationTests : PackageAuthoringApiTestBase {
        [Test]
        public void GeneratePackage_OmitsUnityReleaseWhenMinimumVersionReleaseIsUnset() {
            ProjectSettings projectSettings = CreateProjectSettings("Minimum Unity Default");
            PackageSettings packageSettings = CreatePackageSettings(
                "https://docs.doji-tech.com/{{PACKAGE_NAME}}",
                "https://docs.doji-tech.com/{{PACKAGE_NAME}}/manual");
            packageSettings.IncludeMinimumUnityVersion = true;
            packageSettings.MinimumUnityMajor = "6000";
            packageSettings.MinimumUnityMinor = "3";
            packageSettings.MinimumUnityRelease = " ";

            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                projectSettings,
                packageSettings,
                CreateRepoSettings());

            string packageManifestPath = Path.Combine(
                rootDirectory,
                "com.doji.tests.tokenized",
                "package.json");
            JObject packageManifest = JObject.Parse(File.ReadAllText(packageManifestPath));

            Assert.That(packageManifest.Value<string>("unity"), Is.EqualTo("6000.3"));
            Assert.That(packageManifest.ContainsKey("unityRelease"), Is.False);
        }

        [Test]
        public void GeneratePackage_ResolvesTokenizedManifestUrls() {
            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                CreateProjectSettings("Tokenized Companion"),
                CreatePackageSettings(
                    "https://docs.doji-tech.com/{{PACKAGE_NAME}}",
                    "https://docs.doji-tech.com/{{PACKAGE_NAME}}/manual"),
                CreateRepoSettings());

            string packageManifestPath = Path.Combine(
                rootDirectory,
                "com.doji.tests.tokenized",
                "package.json");
            JObject packageManifest = JObject.Parse(File.ReadAllText(packageManifestPath));

            Assert.That(rootDirectory, Is.EqualTo(Path.Combine(TempRoot, "com.doji.tests.tokenized")));
            Assert.That(
                packageManifest.SelectToken("author.url")?.Value<string>(),
                Is.EqualTo("https://docs.doji-tech.com/com.doji.tests.tokenized"));
            Assert.That(
                packageManifest.Value<string>("documentationUrl"),
                Is.EqualTo("https://docs.doji-tech.com/com.doji.tests.tokenized/manual"));
        }
    }
}
