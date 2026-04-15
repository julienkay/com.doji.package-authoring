using System;
using System.IO;
using Doji.PackageAuthoring.Models;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Provides shared temp-output and settings helpers for package authoring API regression tests.
    /// </summary>
    internal abstract class PackageAuthoringApiTestBase {
        private readonly string _tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Doji.PackageAuthoring.Tests",
            Guid.NewGuid().ToString("N"));

        protected string TempRoot => _tempRoot;

        [TearDown]
        public void TearDown() {
            if (!Directory.Exists(_tempRoot)) {
                return;
            }

            Directory.Delete(_tempRoot, recursive: true);
        }

        protected ProjectSettings CreateProjectSettings(string productName) {
            return new ProjectSettings {
                CompanyName = "Doji Technologies",
                ProductName = productName,
                Version = "2.3.4",
                TargetLocation = _tempRoot
            };
        }

        protected static PackageSettings CreatePackageSettings(string authorUrl, string documentationUrl) {
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

        protected static RepoSettings CreateRepoSettings(bool includeReadme = false) {
            return new RepoSettings {
                CopyrightHolder = "Doji Technologies",
                IncludeReadme = includeReadme,
                InitializeGitRepository = false,
                RepositoryUrl = "https://github.com/doji/{{PACKAGE_NAME}}.git"
            };
        }
    }
}
