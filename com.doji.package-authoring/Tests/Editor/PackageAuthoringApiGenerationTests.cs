using System;
using System.IO;
using Doji.PackageAuthoring.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies the public scripting API produces token-resolved scaffolding and stable companion-project metadata.
    /// </summary>
    internal sealed class PackageAuthoringApiGenerationTests {
        private readonly string _tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Doji.PackageAuthoring.Tests",
            Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown() {
            if (!Directory.Exists(_tempRoot)) {
                return;
            }

            Directory.Delete(_tempRoot, recursive: true);
        }

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

            Assert.That(rootDirectory, Is.EqualTo(Path.Combine(_tempRoot, "com.doji.tests.tokenized")));
            Assert.That(
                packageManifest.SelectToken("author.url")?.Value<string>(),
                Is.EqualTo("https://docs.doji-tech.com/com.doji.tests.tokenized"));
            Assert.That(
                packageManifest.Value<string>("documentationUrl"),
                Is.EqualTo("https://docs.doji-tech.com/com.doji.tests.tokenized/manual"));
        }

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

        private ProjectSettings CreateProjectSettings(string productName) {
            return new ProjectSettings {
                CompanyName = "Doji Technologies",
                ProductName = productName,
                Version = "2.3.4",
                TargetLocation = _tempRoot
            };
        }

        private static PackageSettings CreatePackageSettings(string authorUrl, string documentationUrl) {
            return new PackageSettings {
                PackageName = "com.doji.tests.tokenized",
                PackageDisplayName = "Tokenized Package",
                AssemblyName = "Doji.PackageAuthoring.Tokenized",
                NamespaceName = "Doji.PackageAuthoring.Tokenized",
                Description = "Token resolution test package.",
                CompanyName = "Doji Technologies",
                IncludeAuthor = true,
                AuthorUrl = authorUrl,
                AuthorEmail = "support@doji-tech.com",
                DocumentationUrl = documentationUrl,
                CreateDocsFolder = true,
                IncludeReadme = true
            };
        }

        private static RepoSettings CreateRepoSettings(bool includeReadme = false) {
            return new RepoSettings {
                CopyrightHolder = "Doji Technologies",
                IncludeReadme = includeReadme,
                InitializeGitRepository = false,
                RepositoryUrl = "https://github.com/doji/{{PACKAGE_NAME}}.git"
            };
        }
    }
}
