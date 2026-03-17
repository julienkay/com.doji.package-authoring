using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Utilities;
using Doji.PackageAuthoring.Editor.Wizards.Models;
using Doji.PackageAuthoring.Editor.Wizards.Presets;
using Doji.PackageAuthoring.Editor.Wizards.Templates;
using Doji.PackageAuthoring.Editor.Wizards.UI;

namespace Doji.PackageAuthoring.Editor.Wizards {
    /// <summary>
    /// Editor window that scaffolds a package repository and a companion Unity test project.
    /// </summary>
    public class PackageCreationWizard : EditorWindow {
        private const string PackageSectionPresetTooltip = "Apply package defaults or a package preset asset.";

        private const string CompanionProjectPresetTooltip =
            "Apply project defaults or a preset asset to the companion project.";

        private static readonly string PackageNameField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.PackageSettings.PackageName)}>k__BackingField";

        private static readonly string AssemblyNameField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.PackageSettings.AssemblyName)}>k__BackingField";

        private static readonly string CreateDocsFolderField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.PackageSettings.CreateDocsFolder)}>k__BackingField";

        private static readonly string CreateSamplesFolderField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.PackageSettings.CreateSamplesFolder)}>k__BackingField";

        private static readonly string CreateEditorFolderField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.PackageSettings.CreateEditorFolder)}>k__BackingField";

        private static readonly string CreateTestsFolderField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.PackageSettings.CreateTestsFolder)}>k__BackingField";

        private static readonly string ProductNameField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.ProjectSettings.ProductName)}>k__BackingField";

        private static readonly string TargetLocationField =
            $"<{nameof(Doji.PackageAuthoring.Editor.Wizards.Models.ProjectSettings.TargetLocation)}>k__BackingField";

        [SerializeField] private bool _initializedFromDefaults;
        [SerializeField] private Vector2 _contentScrollPosition;
        [SerializeField] private Vector2 _repositoryLayoutPreviewScrollPosition;
        [SerializeField] private bool _autoOpenAfterCreation = true;

        private string RootDirectory => Path.Combine(ProjectSettings.TargetLocation, PackageSettings.PackageName);
        private string PackageDirectory => Path.Combine(RootDirectory, PackageSettings.PackageName);
        private string ProjectDirectory => Path.Combine(RootDirectory, "projects", ProjectSettings.ProductName);
        private PackageContext Ctx => new(ProjectSettings, PackageSettings, RepoSettings);
        private string _runtimeAssemblyGuid;

        private PackageAuthoringProfile _defaults;
        private SerializedObject _defaultsSerializedObject;
        private SerializedObject _windowSerializedObject;
        private SerializedProperty _autoOpenAfterCreationProperty;
        private RepositoryLayoutPreviewPanel _repositoryLayoutPreviewPanel;

        private PackageAuthoringProfile Defaults => _defaults ??= CreateTemporaryProfile();
        private ProjectSettings ProjectSettings => Defaults.ProjectDefaults;
        private PackageSettings PackageSettings => Defaults.PackageDefaults;
        private RepoSettings RepoSettings => Defaults.RepoDefaults;

        /// <summary>
        /// Opens the package creation wizard.
        /// </summary>
        [MenuItem("Tools/Package Creation Wizard")]
        public static void ShowWindow() {
            PackageCreationWizard window = GetWindow<PackageCreationWizard>();
            window.titleContent = new GUIContent("Package Creation");
            window.minSize = new Vector2(1000f, 600f);
        }

        /// <summary>
        /// Initializes the window title, transient state, and dependency editor UI.
        /// </summary>
        private void OnEnable() {
            titleContent = new GUIContent("Package Creation");
            minSize = new Vector2(1000f, 600f);

            if (!_initializedFromDefaults) {
                ApplyProjectDefaults();
                _initializedFromDefaults = true;
            }

            InitializeSerializedState();
        }

        private void OnDisable() {
            _repositoryLayoutPreviewPanel?.Dispose();

            if (_defaults != null) {
                DestroyImmediate(_defaults);
                _defaults = null;
            }

            _defaultsSerializedObject = null;
            _windowSerializedObject = null;
            _autoOpenAfterCreationProperty = null;
            _repositoryLayoutPreviewPanel = null;
        }

        /// <summary>
        /// Resets both the package and companion-project portions of the current window state from project defaults.
        /// </summary>
        private void ApplyProjectDefaults() {
            Defaults.CopyFrom(PackageAuthoringProjectSettings.Instance);
        }

        /// <summary>
        /// Applies only the shared project-facing portion of a preset to the companion-project section.
        /// </summary>
        private void ApplyProjectPreset(PackageAuthoringProfile preset) {
            if (preset == null) {
                ApplyProjectDefaultsToCompanionProject();
                return;
            }

            ProjectSettings.CopyFrom(preset.ProjectDefaults);
        }

        /// <summary>
        /// Applies only the package-facing portion of a preset to the package-definition section.
        /// </summary>
        private void ApplyPackagePreset(PackageAuthoringProfile preset) {
            if (preset == null) {
                ApplyProjectDefaultsToPackageDefinition();
                return;
            }

            PackageSettings.CopyFrom(preset.PackageDefaults);
            RepoSettings.CopyFrom(preset.RepoDefaults);
        }

        /// <summary>
        /// Rebuilds cached serialized properties after domain reloads or window recreation.
        /// </summary>
        private void InitializeSerializedState() {
            _defaultsSerializedObject = new SerializedObject(Defaults);
            _windowSerializedObject = new SerializedObject(this);
            _autoOpenAfterCreationProperty = _windowSerializedObject.FindProperty(nameof(_autoOpenAfterCreation));
        }

        /// <summary>
        /// Draws the package creation wizard UI.
        /// </summary>
        private void OnGUI() {
            if (_defaultsSerializedObject == null || _windowSerializedObject == null) {
                InitializeSerializedState();
            }

            _defaultsSerializedObject.Update();
            _windowSerializedObject.Update();
            GUILayout.Space(10f);

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(
                       _contentScrollPosition,
                       GUILayout.ExpandHeight(true))) {
                _contentScrollPosition = scrollView.scrollPosition;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                DrawPackageDefinitionSection();

                GUILayout.Space(8f);
                DrawRepoSettingsSection();

                GUILayout.Space(8f);
                DrawCompanionProjectSection();

                GUILayout.Space(8f);
                PackageAuthoringGui.DrawSection("Output", DrawOutputSection);
                EditorGUILayout.EndVertical();

                GUILayout.Space(10f);
                _defaultsSerializedObject.ApplyModifiedProperties();
                _windowSerializedObject.ApplyModifiedProperties();
                DrawRepositoryLayoutPreviewPanel();
                EditorGUILayout.EndHorizontal();
            }

            _defaultsSerializedObject.ApplyModifiedProperties();
            _windowSerializedObject.ApplyModifiedProperties();

            GUILayout.Space(10f);
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Create Package", GUILayout.Height(24f), GUILayout.Width(140f))) {
                    CreatePackageScaffolding();
                }
            }
        }

        /// <summary>
        /// Restores only the package-definition portion of the window state from project defaults.
        /// </summary>
        private void ApplyProjectDefaultsToPackageDefinition() {
            PackageSettings.CopyFrom(PackageAuthoringProjectSettings.Instance.PackageDefaults);
            RepoSettings.CopyFrom(PackageAuthoringProjectSettings.Instance.RepoDefaults);
        }

        /// <summary>
        /// Restores only the companion-project portion of the window state from project defaults.
        /// </summary>
        private void ApplyProjectDefaultsToCompanionProject() {
            ProjectSettings.CopyFrom(PackageAuthoringProjectSettings.Instance.ProjectDefaults);
        }

        /// <summary>
        /// Restores package defaults and refreshes the window immediately.
        /// </summary>
        private void ApplyPackageDefaultsAndRefresh() {
            ApplyProjectDefaultsToPackageDefinition();
            GUI.FocusControl(null);
            _defaultsSerializedObject?.Update();
            _windowSerializedObject?.Update();
            Repaint();
        }

        /// <summary>
        /// Restores companion-project defaults and refreshes the window immediately.
        /// </summary>
        private void ApplyCompanionProjectDefaultsAndRefresh() {
            ApplyProjectDefaultsToCompanionProject();
            GUI.FocusControl(null);
            _defaultsSerializedObject?.Update();
            _windowSerializedObject?.Update();
            Repaint();
        }

        /// <summary>
        /// Applies the package portion of a selected preset and refreshes the window.
        /// </summary>
        private void ApplyPackagePresetAndRefresh(PackageAuthoringProfile preset) {
            ApplyPackagePreset(preset);
            GUI.FocusControl(null);
            _defaultsSerializedObject?.Update();
            _windowSerializedObject?.Update();
            Repaint();
        }

        /// <summary>
        /// Applies the project portion of a selected preset and refreshes the window.
        /// </summary>
        private void ApplyProjectPresetAndRefresh(PackageAuthoringProfile preset) {
            ApplyProjectPreset(preset);
            GUI.FocusControl(null);
            _defaultsSerializedObject?.Update();
            _windowSerializedObject?.Update();
            Repaint();
        }

        /// <summary>
        /// Opens the preset menu scoped to package-definition defaults.
        /// </summary>
        private void ShowPackagePresetMenu(Rect buttonRect) {
            PackageAuthoringPresetMenu.Show(
                buttonRect,
                ApplyPackageDefaultsAndRefresh,
                ApplyPackagePresetAndRefresh);
        }

        /// <summary>
        /// Opens the preset menu scoped to companion-project defaults.
        /// </summary>
        private void ShowCompanionProjectPresetMenu(Rect buttonRect) {
            PackageAuthoringPresetMenu.Show(
                buttonRect,
                ApplyCompanionProjectDefaultsAndRefresh,
                ApplyProjectPresetAndRefresh);
        }

        /// <summary>
        /// Draws the package-definition section, including optional content toggles and dependencies.
        /// </summary>
        private void DrawPackageDefinitionSection() {
            PackageAuthoringGui.DrawPackageSettingsSection(
                _defaultsSerializedObject,
                "Package Definition",
                drawHeaderAction: () => PackageAuthoringGui.DrawSectionHeaderPresetButton(
                    PackageSectionPresetTooltip,
                    ShowPackagePresetMenu));
        }

        /// <summary>
        /// Draws the repository-level section that controls generated root metadata such as the license file.
        /// </summary>
        private void DrawRepoSettingsSection() {
            PackageAuthoringGui.DrawRepoSettingsSection(_defaultsSerializedObject, "Repo Settings");
        }

        /// <summary>
        /// Draws the companion-project section that controls the generated sample or test project metadata.
        /// </summary>
        private void DrawCompanionProjectSection() {
            PackageAuthoringGui.DrawProjectSettingsSection(
                _defaultsSerializedObject,
                "Companion Project",
                productLabel: "Project Name",
                includeTargetLocation: false,
                drawHeaderAction: () => PackageAuthoringGui.DrawSectionHeaderPresetButton(
                    CompanionProjectPresetTooltip,
                    ShowCompanionProjectPresetMenu),
                drawFooter: DrawCompanionProjectFooter);
        }

        private void DrawCompanionProjectFooter() {
            EditorGUILayout.PropertyField(
                _autoOpenAfterCreationProperty,
                new GUIContent("Auto-Open After Creation"));
            EditorGUILayout.HelpBox(
                "These values are applied to the generated companion Unity project and shared where the package uses the same product metadata.",
                MessageType.None);
        }

        /// <summary>
        /// Draws the output paths resolved from the current package and project settings.
        /// </summary>
        private void DrawOutputSection() {
            PackageAuthoringGui.DrawProjectOutputField(_defaultsSerializedObject);
            EditorGUILayout.LabelField("Repository Root", PreviewRootDirectory, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Package Folder", PreviewPackageDirectory, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Companion Project", PreviewProjectDirectory, EditorStyles.miniLabel);
        }

        /// <summary>
        /// Draws a live repository layout preview beside the editable fields.
        /// </summary>
        private void DrawRepositoryLayoutPreviewPanel() {
            _repositoryLayoutPreviewPanel ??= new RepositoryLayoutPreviewPanel();
            _repositoryLayoutPreviewPanel.Draw(
                position.width,
                position.height,
                BuildRepositoryLayoutPreviewData(),
                ref _repositoryLayoutPreviewScrollPosition);
        }

        private RepositoryLayoutPreviewData BuildRepositoryLayoutPreviewData() {
            return new RepositoryLayoutPreviewData {
                Context = Ctx,
                RootDirectoryName = GetDirectoryName(PreviewRootDirectory, CurrentPackageName),
                PackageName = CurrentPackageName,
                AssemblyName = CurrentAssemblyName,
                CompanionProjectName = CurrentProductName,
                IncludeDocsFolder = CurrentCreateDocsFolder,
                IncludeSamplesFolder = CurrentCreateSamplesFolder,
                IncludeEditorFolder = CurrentCreateEditorFolder,
                IncludeTestsFolder = CurrentCreateTestsFolder,
                IncludeRepositoryGitIgnore = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".gitignore")),
                IncludePackagesLockFile = File.Exists(
                    Path.Combine(Directory.GetCurrentDirectory(), "Packages", "packages-lock.json"))
            };
        }

        private string PreviewRootDirectory => Path.Combine(CurrentTargetLocation, CurrentPackageName);
        private string PreviewPackageDirectory => Path.Combine(PreviewRootDirectory, CurrentPackageName);
        private string PreviewProjectDirectory => Path.Combine(PreviewRootDirectory, "projects", CurrentProductName);

        private string CurrentPackageName => GetSerializedString(
            PackageAuthoringGui.FindPackageDefaultsProperty(_defaultsSerializedObject),
            PackageNameField,
            PackageSettings.PackageName);

        private string CurrentAssemblyName => GetSerializedString(
            PackageAuthoringGui.FindPackageDefaultsProperty(_defaultsSerializedObject),
            AssemblyNameField,
            PackageSettings.AssemblyName);

        private string CurrentProductName => GetSerializedString(
            PackageAuthoringGui.FindProjectDefaultsProperty(_defaultsSerializedObject),
            ProductNameField,
            ProjectSettings.ProductName);

        private string CurrentTargetLocation => GetSerializedString(
            PackageAuthoringGui.FindProjectDefaultsProperty(_defaultsSerializedObject),
            TargetLocationField,
            ProjectSettings.TargetLocation);

        private bool CurrentCreateDocsFolder => GetSerializedBool(
            PackageAuthoringGui.FindPackageDefaultsProperty(_defaultsSerializedObject),
            CreateDocsFolderField,
            PackageSettings.CreateDocsFolder);

        private bool CurrentCreateSamplesFolder => GetSerializedBool(
            PackageAuthoringGui.FindPackageDefaultsProperty(_defaultsSerializedObject),
            CreateSamplesFolderField,
            PackageSettings.CreateSamplesFolder);

        private bool CurrentCreateEditorFolder => GetSerializedBool(
            PackageAuthoringGui.FindPackageDefaultsProperty(_defaultsSerializedObject),
            CreateEditorFolderField,
            PackageSettings.CreateEditorFolder);

        private bool CurrentCreateTestsFolder => GetSerializedBool(
            PackageAuthoringGui.FindPackageDefaultsProperty(_defaultsSerializedObject),
            CreateTestsFolderField,
            PackageSettings.CreateTestsFolder);

        private static string GetSerializedString(SerializedProperty property, string relativePath, string fallback) {
            return property?.FindPropertyRelative(relativePath)?.stringValue ?? fallback;
        }

        private static bool GetSerializedBool(SerializedProperty property, string relativePath, bool fallback) {
            return property?.FindPropertyRelative(relativePath)?.boolValue ?? fallback;
        }

        private static PackageAuthoringProfile CreateTemporaryProfile() {
            PackageAuthoringProfile profile = CreateInstance<PackageAuthoringProfile>();
            profile.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
            profile.ProjectDefaults = new ProjectSettings();
            profile.PackageDefaults = new PackageSettings();
            profile.RepoDefaults = new RepoSettings();
            return profile;
        }

        private static string GetDirectoryName(string path, string fallback) {
            if (string.IsNullOrWhiteSpace(path)) {
                return fallback;
            }

            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.IsNullOrWhiteSpace(path) ? fallback : Path.GetFileName(path);
        }

        /// <summary>
        /// Creates the package repository skeleton, optional folders, and the companion Unity project.
        /// </summary>
        private void CreatePackageScaffolding() {
            Directory.CreateDirectory(RootDirectory);
            Directory.CreateDirectory(PackageDirectory);
            Directory.CreateDirectory(ProjectDirectory);

            if (PackageSettings.CreateDocsFolder) {
                CreateDocsFolder(RootDirectory);
            }

            CreatePackageFolders();
            CreateProjectStructure();
            CreateRootFiles();

            GitUtility.InitializeRepository(RootDirectory, PackageSettings.PackageName);

            Debug.Log($"Package scaffolding created successfully at {RootDirectory}");

            if (_autoOpenAfterCreation) {
                UnityEditorLauncherUtility.TryOpenProjectInCurrentEditor(ProjectDirectory);
            }
        }

        /// <summary>
        /// Creates the package folders that live inside the generated package root.
        /// </summary>
        private void CreatePackageFolders() {
            CreateRuntimeFolder();

            if (PackageSettings.CreateSamplesFolder) {
                CreateSamplesFolder();
            }

            if (PackageSettings.CreateEditorFolder) {
                CreateEditorFolder();
            }

            if (PackageSettings.CreateTestsFolder) {
                Directory.CreateDirectory(Path.Combine(PackageDirectory, "Tests"));
            }

            // Package metadata is written after the folder layout is in place so optional folders can influence it.
            CreatePackageFiles(PackageDirectory);
        }

        /// <summary>
        /// Copies the template Unity project and points its manifest back to the generated local package.
        /// </summary>
        private void CreateProjectStructure() {
            UpdateProjectSettings();

            try {
                Directory.CreateDirectory(Path.Combine(ProjectDirectory, "Assets"));

                CopyDirectory("Packages", Path.Combine(ProjectDirectory, "Packages"));
                CopyDirectory("ProjectSettings", Path.Combine(ProjectDirectory, "ProjectSettings"));

                string gitignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");
                if (File.Exists(gitignorePath)) {
                    string targetPath = Path.Combine(ProjectDirectory, ".gitignore");
                    CopyFile(gitignorePath, targetPath);
                }

                string projectManifestPath = Path.Combine(ProjectDirectory, "Packages", "manifest.json");
                CreateFile(projectManifestPath, Ctx.GetProjectManifest(), overwrite: true);
            }
            finally {
                // The generator temporarily mutates project-level settings so copied files contain the new package metadata.
                RevertProjectSettings();
            }
        }

        private static string _originalCompanyName;
        private static string _originalProductName;
        private static string _originalVersion;
        private static readonly List<string> OriginalIdentifiers = new();

        private static readonly List<NamedBuildTarget> NamedTargets = new() {
            NamedBuildTarget.Standalone,
            NamedBuildTarget.Android
        };

        private static string _originalRootNamespace;

        /// <summary>
        /// Temporarily applies the wizard values to <see cref="PlayerSettings"/> so copied project files inherit them.
        /// </summary>
        private void UpdateProjectSettings() {
            _originalCompanyName = PlayerSettings.companyName;
            _originalProductName = PlayerSettings.productName;
            _originalVersion = PlayerSettings.bundleVersion;
            OriginalIdentifiers.Clear();
            foreach (NamedBuildTarget target in NamedTargets) {
                OriginalIdentifiers.Add(PlayerSettings.GetApplicationIdentifier(target));
            }

            _originalRootNamespace = EditorSettings.projectGenerationRootNamespace;

            PlayerSettings.companyName = ProjectSettings.CompanyName;
            PlayerSettings.productName = ProjectSettings.ProductName;
            PlayerSettings.bundleVersion = ProjectSettings.Version;
            foreach (NamedBuildTarget target in NamedTargets) {
                PlayerSettings.SetApplicationIdentifier(target, PackageSettings.PackageName);
            }

            EditorSettings.projectGenerationRootNamespace = PackageSettings.NamespaceName;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Restores the user's original project settings after the template project has been copied.
        /// </summary>
        private static void RevertProjectSettings() {
            PlayerSettings.companyName = _originalCompanyName;
            PlayerSettings.productName = _originalProductName;
            PlayerSettings.bundleVersion = _originalVersion;
            for (int i = 0; i < OriginalIdentifiers.Count; i++) {
                PlayerSettings.SetApplicationIdentifier(NamedTargets[i], OriginalIdentifiers[i]);
            }

            EditorSettings.projectGenerationRootNamespace = _originalRootNamespace;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateRootFiles() {
            string license = Ctx.GetLicense();
            if (!string.IsNullOrWhiteSpace(license)) {
                string licensePath = Path.Combine(RootDirectory, "LICENSE");
                CreateFile(licensePath, license);
            }

            string readmePath = Path.Combine(RootDirectory, "README.md");
            CreateFile(readmePath, Ctx.GetRepositoryReadme());
        }

        private void CreatePackageFiles(string path) {
            string packageManifestPath = Path.Combine(path, "package.json");
            // `package.json` is regenerated because optional samples and dependencies directly affect its contents.
            CreateFile(packageManifestPath, Ctx.GetPackageManifest(), overwrite: true);

            string readmePath = Path.Combine(path, "README.md");
            CreateFile(readmePath, Ctx.GetPackageReadme());

            string changelogPath = Path.Combine(path, "CHANGELOG.md");
            CreateFile(changelogPath, Ctx.GetChangelog());
        }

        /// <summary>
        /// Recursively copies a directory into the generated output, preserving any existing destination files.
        /// </summary>
        private void CopyDirectory(string sourceDir, string destinationDir) {
            if (!Directory.Exists(sourceDir)) {
                Debug.LogWarning($"Source directory {sourceDir} does not exist. Skipping copy.");
                return;
            }

            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir)) {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                CopyFile(file, destFile);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir)) {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        /// <summary>
        /// Creates the runtime assembly definition in the provided folder.
        /// </summary>
        /// <returns>GUID written into the runtime asmdef meta file.</returns>
        private string CreateRuntimeAsmDef(string path) {
            string asmDefPath = Path.Combine(path, $"{PackageSettings.AssemblyName}.asmdef");
            return CreateAsmDefWithMeta(asmDefPath, Ctx.GetRuntimeAsmDef());
        }

        /// <summary>
        /// Creates the samples assembly definition in the provided folder.
        /// </summary>
        /// <param name="runtimeAssemblyGuid">GUID of the generated runtime asmdef used for stable references.</param>
        private void CreateSamplesAsmDef(string path, string runtimeAssemblyGuid) {
            string asmDefPath = Path.Combine(path, $"{PackageSettings.AssemblyName}.asmdef");
            CreateAsmDefWithMeta(asmDefPath, Ctx.GetSamplesAsmDef(runtimeAssemblyGuid));
        }

        /// <summary>
        /// Creates the editor-only assembly definition in the provided folder.
        /// </summary>
        /// <param name="runtimeAssemblyGuid">GUID of the generated runtime asmdef used for stable references.</param>
        private void CreateEditorAsmDef(string path, string runtimeAssemblyGuid) {
            string asmDefPath = Path.Combine(path, $"{PackageSettings.AssemblyName}.Editor.asmdef");
            CreateAsmDefWithMeta(asmDefPath, Ctx.GetEditorAsmDef(runtimeAssemblyGuid));
        }

        /// <summary>
        /// Creates the runtime assembly info file in the provided folder.
        /// </summary>
        private void CreateAssemblyInfo(string path) {
            string assemblyInfoPath = Path.Combine(path, "AssemblyInfo.cs");
            CreateFile(assemblyInfoPath, Ctx.GetAssemblyInfo());
        }

        /// <summary>
        /// Creates the runtime folder and its baseline assembly files.
        /// </summary>
        private void CreateRuntimeFolder() {
            string runtimePath = Path.Combine(PackageDirectory, "Runtime");
            Directory.CreateDirectory(runtimePath);
            string runtimeAssemblyGuid = CreateRuntimeAsmDef(runtimePath);
            _runtimeAssemblyGuid = runtimeAssemblyGuid;
            CreateAssemblyInfo(runtimePath);
        }

        /// <summary>
        /// Creates the editor folder and its editor-only assembly definition.
        /// </summary>
        private void CreateEditorFolder() {
            string editorPath = Path.Combine(PackageDirectory, "Editor");
            Directory.CreateDirectory(editorPath);
            CreateEditorAsmDef(editorPath, _runtimeAssemblyGuid);
        }

        /// <summary>
        /// Creates the samples root, assembly definition, and starter sample script.
        /// </summary>
        private void CreateSamplesFolder() {
            string samplesPath = Path.Combine(PackageDirectory, "Samples~");
            Directory.CreateDirectory(samplesPath);

            Directory.CreateDirectory(Path.Combine(samplesPath, "00-SharedSampleAssets"));
            Directory.CreateDirectory(Path.Combine(samplesPath, "01-BasicSample"));
            CreateSamplesAsmDef(samplesPath, _runtimeAssemblyGuid);

            // Keep the starter sample in a numbered folder so package manager ordering is predictable.
            CreateFile(Path.Combine(samplesPath, "01-BasicSample", "BasicSample.cs"), Ctx.GetSampleScript());
        }

        /// <summary>
        /// Creates the documentation folder and the generated docfx configuration files.
        /// </summary>
        private void CreateDocsFolder(string path) {
            path = Path.Combine(path, "docs");
            Directory.CreateDirectory(path);
            CreateDocfxFolders(path);
            CreateDocfxFiles(path);
        }

        /// <summary>
        /// Copies any repository-level docs template content into the generated docs folder.
        /// </summary>
        private void CreateDocfxFolders(string path) {
            CopyDirectory("docs", path);
        }

        /// <summary>
        /// Writes the generated docfx configuration and table-of-contents files.
        /// </summary>
        private void CreateDocfxFiles(string path) {
            string docfxConfigPath = Path.Combine(path, "docfx.json");
            CreateFile(docfxConfigPath, Ctx.GetDocfxJson());

            string docfxPdfConfigPath = Path.Combine(path, "docfx-pdf.json");
            CreateFile(docfxPdfConfigPath, Ctx.GetDocfxPdfJson());

            string filterConfigPath = Path.Combine(path, "filterConfig.yml");
            CreateFile(filterConfigPath, Ctx.GetFilterConfig());

            string indexPath = Path.Combine(path, "index.md");
            CreateFile(indexPath, Ctx.GetIndexMD());

            string tocPath = Path.Combine(path, "toc.yml");
            CreateFile(tocPath, Ctx.GetRootToc());

            string manualTocPath = Path.Combine(path, "manual", "toc.yml");
            CreateFile(manualTocPath, Ctx.GetManualToc());
        }

        /// <summary>
        /// Writes scaffolded content to disk and ensures the target directory exists first.
        /// </summary>
        private void CreateFile(string path, string content, bool overwrite = false) {
            if (File.Exists(path) && !overwrite) {
                return;
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Creates an asmdef asset together with a matching meta file so dependent assemblies can reference its GUID.
        /// </summary>
        /// <returns>GUID written into the generated asmdef meta file.</returns>
        private string CreateAsmDefWithMeta(string asmDefPath, string content) {
            string guid = Guid.NewGuid().ToString("N");
            CreateFile(asmDefPath, content, overwrite: true);
            CreateFile($"{asmDefPath}.meta", AssetMetaTemplate.GetAsmDefMeta(guid), overwrite: true);
            return guid;
        }

        private void CopyFile(string sourceFileName, string destFileName) {
            if (!File.Exists(destFileName)) {
                File.Copy(sourceFileName, destFileName, overwrite: false);
            }
        }
    }
}
