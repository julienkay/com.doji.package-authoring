using System;
using System.IO;
using UnityEditor;
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
        private const string SessionStateKey = "Doji.PackageAuthoring.PackageCreationWizard.SessionState";
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
        private string ScopedSessionStateKey => WizardSessionStateUtility.GetProjectScopedKey(SessionStateKey);
        private ProjectSettings ProjectSettings => Defaults.ProjectDefaults;
        private PackageSettings PackageSettings => Defaults.PackageDefaults;
        private RepoSettings RepoSettings => Defaults.RepoDefaults;
        private string CurrentGitIgnoreTemplate => GitIgnoreTemplateSettings.Instance.GetResolvedContent(Ctx);

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
            WizardStateUtility.InitializeWindow(
                this,
                "Package Creation",
                RestoreSessionState,
                ApplyProjectDefaults,
                InitializeSerializedState,
                minSize: new Vector2(1000f, 600f),
                wantsMouseMove: true);
        }

        private void OnDisable() {
            WizardStateUtility.DisposeWindow(
                SaveSessionState,
                ref _defaults,
                ref _defaultsSerializedObject,
                ref _windowSerializedObject,
                ref _autoOpenAfterCreationProperty,
                () => _repositoryLayoutPreviewPanel?.Dispose());
            _repositoryLayoutPreviewPanel = null;
        }

        /// <summary>
        /// Resets both the package and companion-project portions of the current window state from project defaults.
        /// </summary>
        private void ApplyProjectDefaults() {
            Defaults.CopyFrom(PackageAuthoringProjectSettings.Instance);
        }

        /// <summary>
        /// Restores unsaved wizard input captured before the last domain reload within the current editor session.
        /// </summary>
        /// <returns><c>true</c> when prior session state existed.</returns>
        private bool RestoreSessionState() {
            return WizardSessionStateUtility.TryRestoreProfile(ScopedSessionStateKey, Defaults);
        }

        /// <summary>
        /// Saves the current ad hoc wizard input so script recompiles do not reset the open form.
        /// </summary>
        private void SaveSessionState() {
            WizardSessionStateUtility.SaveProfile(ScopedSessionStateKey, _defaults);
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
        /// Rebuilds the serialized wrappers and cached window properties around the current wizard state.
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

            RepositoryLayoutPreviewHoverContext.BeginFrame();
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

            if (Event.current.type == EventType.MouseMove) {
                Repaint();
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
            WizardStateUtility.RefreshWindow(_defaultsSerializedObject, _windowSerializedObject, this);
        }

        /// <summary>
        /// Restores companion-project defaults and refreshes the window immediately.
        /// </summary>
        private void ApplyCompanionProjectDefaultsAndRefresh() {
            ApplyProjectDefaultsToCompanionProject();
            WizardStateUtility.RefreshWindow(_defaultsSerializedObject, _windowSerializedObject, this);
        }

        /// <summary>
        /// Applies the package portion of a selected preset and refreshes the window.
        /// </summary>
        private void ApplyPackagePresetAndRefresh(PackageAuthoringProfile preset) {
            ApplyPackagePreset(preset);
            WizardStateUtility.RefreshWindow(_defaultsSerializedObject, _windowSerializedObject, this);
        }

        /// <summary>
        /// Applies the project portion of a selected preset and refreshes the window.
        /// </summary>
        private void ApplyProjectPresetAndRefresh(PackageAuthoringProfile preset) {
            ApplyProjectPreset(preset);
            WizardStateUtility.RefreshWindow(_defaultsSerializedObject, _windowSerializedObject, this);
        }

        /// <summary>
        /// Opens the preset menu scoped to package-definition defaults.
        /// </summary>
        private void ShowPackagePresetMenu(Rect buttonRect) {
            WizardStateUtility.ShowPresetMenu(
                buttonRect,
                ApplyPackageDefaultsAndRefresh,
                ApplyPackagePresetAndRefresh);
        }

        /// <summary>
        /// Opens the preset menu scoped to companion-project defaults.
        /// </summary>
        private void ShowCompanionProjectPresetMenu(Rect buttonRect) {
            WizardStateUtility.ShowPresetMenu(
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
            PackageAuthoringGui.DrawGeneratedProjectSettingsSection(
                _defaultsSerializedObject,
                _autoOpenAfterCreationProperty,
                "Companion Project",
                "The companion project",
                CompanionProjectPresetTooltip,
                ShowCompanionProjectPresetMenu);
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
                IncludeRepositoryGitIgnore = !string.IsNullOrWhiteSpace(CurrentGitIgnoreTemplate),
                RepositoryGitIgnoreTemplate = CurrentGitIgnoreTemplate,
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
            return WizardStateUtility.GetSerializedString(property, relativePath, fallback);
        }

        private static bool GetSerializedBool(SerializedProperty property, string relativePath, bool fallback) {
            return WizardStateUtility.GetSerializedBool(property, relativePath, fallback);
        }

        private static PackageAuthoringProfile CreateTemporaryProfile() {
            return WizardStateUtility.CreateTemporaryProfile();
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
            using IDisposable _ = GeneratedProjectScaffoldingUtility.ApplyTemporaryProjectSettings(
                ProjectSettings,
                _ => PackageSettings.PackageName,
                PackageSettings.NamespaceName);
            GeneratedProjectScaffoldingUtility.CopyTemplateProjectBaseline(
                ProjectDirectory,
                CurrentGitIgnoreTemplate,
                Ctx.GetProjectManifest());
        }

        private void CreateRootFiles() {
            string license = Ctx.GetLicense();
            if (!string.IsNullOrWhiteSpace(license)) {
                string licensePath = Path.Combine(RootDirectory, "LICENSE");
                GeneratedProjectScaffoldingUtility.CreateFile(licensePath, license);
            }

            if (RepoSettings.IncludeReadme) {
                string readmePath = Path.Combine(RootDirectory, "README.md");
                GeneratedProjectScaffoldingUtility.CreateFile(readmePath, Ctx.GetRepositoryReadme());
            }
        }

        private void CreatePackageFiles(string path) {
            string packageManifestPath = Path.Combine(path, "package.json");
            // `package.json` is regenerated because optional samples and dependencies directly affect its contents.
            GeneratedProjectScaffoldingUtility.CreateFile(packageManifestPath, Ctx.GetPackageManifest(), overwrite: true);

            if (PackageSettings.IncludeReadme) {
                string readmePath = Path.Combine(path, "README.md");
                GeneratedProjectScaffoldingUtility.CreateFile(readmePath, Ctx.GetPackageReadme());
            }

            string changelogPath = Path.Combine(path, "CHANGELOG.md");
            GeneratedProjectScaffoldingUtility.CreateFile(changelogPath, Ctx.GetChangelog());
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
            GeneratedProjectScaffoldingUtility.CreateFile(assemblyInfoPath, Ctx.GetAssemblyInfo());
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
            GeneratedProjectScaffoldingUtility.CreateFile(
                Path.Combine(samplesPath, "01-BasicSample", "BasicSample.cs"),
                Ctx.GetSampleScript());
        }

        /// <summary>
        /// Creates the documentation folder and the generated docfx configuration files.
        /// </summary>
        private void CreateDocsFolder(string path) {
            path = Path.Combine(path, "docs");
            Directory.CreateDirectory(path);
            CreateDocfxFolders(path);
            CreateDocfxFiles(path);
            CreateDocfxImages(path);
        }

        /// <summary>
        /// Creates the required documentation subfolders before any generated content is written.
        /// </summary>
        private void CreateDocfxFolders(string path) {
            Directory.CreateDirectory(Path.Combine(path, "api"));
            Directory.CreateDirectory(Path.Combine(path, "manual"));
            Directory.CreateDirectory(Path.Combine(path, "pdf"));
        }

        /// <summary>
        /// Writes the generated docfx configuration and table-of-contents files.
        /// </summary>
        private void CreateDocfxFiles(string path) {
            string docsGitIgnorePath = Path.Combine(path, ".gitignore");
            GeneratedProjectScaffoldingUtility.CreateFile(docsGitIgnorePath, Ctx.GetDocsGitIgnore());

            string docfxConfigPath = Path.Combine(path, "docfx.json");
            GeneratedProjectScaffoldingUtility.CreateFile(docfxConfigPath, Ctx.GetDocfxJson());

            string docfxPdfConfigPath = Path.Combine(path, "docfx-pdf.json");
            GeneratedProjectScaffoldingUtility.CreateFile(docfxPdfConfigPath, Ctx.GetDocfxPdfJson());

            string filterConfigPath = Path.Combine(path, "filterConfig.yml");
            GeneratedProjectScaffoldingUtility.CreateFile(filterConfigPath, Ctx.GetFilterConfig());

            string indexPath = Path.Combine(path, "index.md");
            GeneratedProjectScaffoldingUtility.CreateFile(indexPath, Ctx.GetIndexMD());

            string apiGitIgnorePath = Path.Combine(path, "api", ".gitignore");
            GeneratedProjectScaffoldingUtility.CreateFile(apiGitIgnorePath, Ctx.GetDocsApiGitIgnore());

            string apiIndexPath = Path.Combine(path, "api", "index.md");
            GeneratedProjectScaffoldingUtility.CreateFile(apiIndexPath, Ctx.GetDocsApiIndex());

            string tocPath = Path.Combine(path, "toc.yml");
            GeneratedProjectScaffoldingUtility.CreateFile(tocPath, Ctx.GetRootToc());

            string manualTocPath = Path.Combine(path, "manual", "toc.yml");
            GeneratedProjectScaffoldingUtility.CreateFile(manualTocPath, Ctx.GetManualToc());

            string pdfTocPath = Path.Combine(path, "pdf", "toc.yml");
            GeneratedProjectScaffoldingUtility.CreateFile(pdfTocPath, Ctx.GetPdfToc());
        }

        /// <summary>
        /// Generates optional documentation image outputs from the configured branding texture.
        /// </summary>
        private void CreateDocfxImages(string path) {
            DocsBrandingImageSettings brandingImages = DocsBrandingImageSettings.Instance;
            if (!brandingImages.HasAnyImage) {
                return;
            }

            string imagesPath = Path.Combine(path, "images");
            if (!DocumentationImageUtility.TryWriteDocumentationImages(
                    brandingImages.LogoTexture,
                    brandingImages.FaviconTexture,
                    imagesPath)) {
                Debug.LogWarning("Failed to generate one or more documentation branding images from the configured source textures.");
            }
        }

        /// <summary>
        /// Creates an asmdef asset together with a matching meta file so dependent assemblies can reference its GUID.
        /// </summary>
        /// <returns>GUID written into the generated asmdef meta file.</returns>
        private string CreateAsmDefWithMeta(string asmDefPath, string content) {
            string guid = Guid.NewGuid().ToString("N");
            GeneratedProjectScaffoldingUtility.CreateFile(asmDefPath, content, overwrite: true);
            GeneratedProjectScaffoldingUtility.CreateFile(
                $"{asmDefPath}.meta",
                AssetMetaTemplate.GetAsmDefMeta(guid),
                overwrite: true);
            return guid;
        }
    }
}
