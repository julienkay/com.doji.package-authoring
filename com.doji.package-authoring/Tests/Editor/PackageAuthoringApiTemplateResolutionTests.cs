using System.IO;
using Doji.PackageAuthoring.Models;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies tokenized generated text files produced by the public package authoring API.
    /// </summary>
    internal sealed class PackageAuthoringApiTemplateResolutionTests : PackageAuthoringApiTestBase {
        [Test]
        public void GeneratePackage_ResolvesTokenizedTemplateFiles() {
            ProjectSettings projectSettings = CreateProjectSettings("Tokenized Companion");
            projectSettings.GenerateAgentsFile = true;

            string rootDirectory = PackageAuthoringApi.GeneratePackage(
                projectSettings,
                CreatePackageSettings(
                    authorUrl: "https://example.com/{{PACKAGE_NAME}}",
                    documentationUrl: "https://docs.doji-tech.com/{{PACKAGE_NAME}}/"),
                CreateRepoSettings(includeReadme: true),
                openProjectAfterCreation: false);

            string repositoryReadme = File.ReadAllText(Path.Combine(rootDirectory, "README.md"));
            string agentsFile = File.ReadAllText(Path.Combine(rootDirectory, "AGENTS.md"));
            string docsIndex = File.ReadAllText(Path.Combine(rootDirectory, "docs", "index.md"));
            string docfxJson = File.ReadAllText(Path.Combine(rootDirectory, "docs", "docfx.json"));

            Assert.That(repositoryReadme, Does.Contain("# Tokenized Companion"));
            Assert.That(repositoryReadme, Does.Contain("`com.doji.tests.tokenized`"));
            Assert.That(repositoryReadme, Does.Not.Contain("{{PACKAGE_NAME}}"));

            Assert.That(agentsFile, Does.Contain("`com.doji.tests.tokenized`"));
            Assert.That(agentsFile, Does.Contain("`Tokenized Companion`"));
            Assert.That(agentsFile, Does.Not.Contain("{{PROJECT_NAME}}"));

            Assert.That(docsIndex, Does.Contain("# Tokenized Package"));
            Assert.That(docsIndex, Does.Contain("Token resolution test package."));
            Assert.That(docsIndex, Does.Not.Contain("{{PACKAGE_DISPLAY_NAME}}"));

            Assert.That(docfxJson, Does.Contain("\"../com.doji.tests.tokenized\""));
            Assert.That(docfxJson, Does.Contain("https://docs.doji-tech.com/com.doji.tests.tokenized/"));
            Assert.That(docfxJson, Does.Not.Contain("{{DOCUMENTATION_URL}}"));
        }
    }
}
