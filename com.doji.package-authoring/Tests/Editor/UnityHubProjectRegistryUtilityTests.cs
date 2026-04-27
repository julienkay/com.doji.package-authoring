using System.IO;
using Doji.PackageAuthoring.Models;
using Doji.PackageAuthoring.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies best-effort Unity Hub registry updates for generated projects.
    /// </summary>
    internal sealed class UnityHubProjectRegistryUtilityTests : PackageAuthoringApiTestBase {
        [TearDown]
        public void ResetHubOverride() {
            UnityHubProjectRegistryUtility.HubDataDirectoryOverride = null;
        }

        [Test]
        public void TryRegisterProject_CreatesProjectsRegistryEntryForGeneratedProject() {
            ProjectSettings projectSettings = CreateProjectSettings("Hub Registration");
            string projectDirectory = PackageAuthoringApi.GenerateProject(projectSettings, openProjectAfterCreation: false);
            string hubDirectory = Path.Combine(TempRoot, "UnityHub");
            string registryPath = Path.Combine(hubDirectory, "projects-v1.json");

            UnityHubProjectRegistryUtility.HubDataDirectoryOverride = hubDirectory;

            bool result = UnityHubProjectRegistryUtility.TryRegisterProject(projectDirectory);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(registryPath), Is.True);

            JObject root = JObject.Parse(File.ReadAllText(registryPath));
            JObject entry = (JObject)root["data"]?[projectDirectory];

            Assert.That(root["schema_version"]?.Value<string>(), Is.EqualTo("v1"));
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry["title"]?.Value<string>(), Is.EqualTo(projectSettings.ProductName));
            Assert.That(entry["path"]?.Value<string>(), Is.EqualTo(projectDirectory));
            Assert.That(entry["containingFolderPath"]?.Value<string>(), Is.EqualTo(projectSettings.TargetLocation));
            Assert.That(entry["version"]?.Value<string>(), Is.Not.Empty);
            Assert.That(entry["architecture"]?.Value<string>(), Is.Not.Empty);
            Assert.That(entry["isFavorite"]?.Value<bool>(), Is.False);
        }

        [Test]
        public void TryRegisterProject_UpdatesExistingRegistryEntryForProjectPath() {
            ProjectSettings projectSettings = CreateProjectSettings("Hub Registration Existing");
            string projectDirectory = PackageAuthoringApi.GenerateProject(projectSettings, openProjectAfterCreation: false);
            string hubDirectory = Path.Combine(TempRoot, "UnityHub");
            string registryPath = Path.Combine(hubDirectory, "projects-v1.json");
            Directory.CreateDirectory(hubDirectory);
            File.WriteAllText(
                registryPath,
                "{\"schema_version\":\"v1\",\"data\":{\"" +
                projectDirectory.Replace("\\", "\\\\") +
                "\":{\"title\":\"Old Title\",\"path\":\"" +
                projectDirectory.Replace("\\", "\\\\") +
                "\",\"isFavorite\":true}}}");

            UnityHubProjectRegistryUtility.HubDataDirectoryOverride = hubDirectory;

            bool result = UnityHubProjectRegistryUtility.TryRegisterProject(projectDirectory);

            Assert.That(result, Is.True);

            JObject root = JObject.Parse(File.ReadAllText(registryPath));
            JObject entry = (JObject)root["data"]?[projectDirectory];
            Assert.That(entry["title"]?.Value<string>(), Is.EqualTo(projectSettings.ProductName));
            Assert.That(entry["isFavorite"]?.Value<bool>(), Is.False);
            Assert.That(entry["version"]?.Value<string>(), Is.Not.Empty);
        }
    }
}
