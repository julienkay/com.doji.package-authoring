using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies generated package manifest output produced by the public package authoring API.
    /// </summary>
    internal sealed class PackageAuthoringApiManifestGenerationTests : PackageAuthoringApiTestBase {
        [Test]
        public void GeneratePackage_ResolvesTokenizedManifestUrls() {
            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                CreateProjectSettings("Tokenized Companion"),
                CreatePackageSettings(
                    authorUrl: "https://docs.doji-tech.com/{{PACKAGE_NAME}}",
                    documentationUrl: "https://docs.doji-tech.com/{{PACKAGE_NAME}}/manual"),
                CreateRepoSettings(),
                openProjectAfterCreation: false);

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
