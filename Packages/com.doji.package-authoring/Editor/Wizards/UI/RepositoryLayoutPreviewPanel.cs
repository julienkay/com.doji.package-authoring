using System.Collections.Generic;
using System.IO;
using Doji.PackageAuthoring.Editor.Wizards.Templates;
using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Draws the repository layout preview panel used by package-creation workflows.
    /// </summary>
    internal sealed class RepositoryLayoutPreviewPanel {
        private const float MinWidth = 360f;
        private const float MaxWidth = 760f;
        private const float MinHeight = 320f;
        private const float TreeColumnMinWidth = 180f;
        private const float RowPadding = 4f;
        private const string RuntimeAssemblyGuidPreview = "<runtime-assembly-guid>";
        private static readonly Color SelectionColor = new(0.24f, 0.47f, 0.85f, 0.18f);
        private static readonly Color RepoTint = new(0.92f, 0.86f, 1f);
        private static readonly Color DocsTint = new(0.78f, 0.90f, 1f);
        private static readonly Color PackageTint = new(0.84f, 1f, 0.84f);
        private static readonly Color ProjectTint = new(1f, 0.92f, 0.78f);

        private GUIStyle _treeRowStyle;
        private GUIStyle _treeContainerStyle;
        private GUIStyle _tagStyle;
        private RepositoryLayoutPreviewSelection _selectionPreview;

        /// <summary>
        /// Draws the preview section and updates its scroll state.
        /// </summary>
        public void Draw(float windowWidth, float windowHeight, RepositoryLayoutPreviewData data, ref Vector2 scrollPosition) {
            RepositoryLayoutNode root = BuildPreviewTree(data);
            float previewWidth = Mathf.Clamp(windowWidth * 0.4f, MinWidth, MaxWidth);
            float previewHeight = Mathf.Max(MinHeight, windowHeight - 72f);

            // The tree is rebuilt every frame from wizard state, so the hidden Inspector selection must be refreshed too.
            SyncInspectorSelection(root);

            // The parent wizard uses an outer scroll view, so the preview needs an explicit height instead of relying on ExpandHeight.
            EditorGUILayout.BeginVertical(
                GUILayout.Width(previewWidth),
                GUILayout.MinHeight(previewHeight),
                GUILayout.Height(previewHeight));
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox,
                GUILayout.MinHeight(previewHeight),
                GUILayout.Height(previewHeight));
            EditorGUILayout.LabelField("Repository Layout Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(3f);
            DrawContent(root, previewWidth, previewHeight, ref scrollPosition);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawContent(
            RepositoryLayoutNode root,
            float panelWidth,
            float panelHeight,
            ref Vector2 scrollPosition) {
            EnsureStyles();

            List<RepositoryLayoutEntry> entries = new();
            AppendEntries(entries, root, string.Empty, isLast: true, isRoot: true);

            float rowHeight = _treeRowStyle.lineHeight + RowPadding;
            float treeWidth = Mathf.Max(TreeColumnMinWidth, panelWidth - 12f);
            float treeHeight = Mathf.Max(MinHeight - 32f, panelHeight - 32f);

            RepositoryLayoutPreviewSelection previewSelection = GetOrCreateSelectionPreview();
            string selectedPath = Selection.activeObject == previewSelection ? previewSelection.RelativePath : string.Empty;

            EditorGUILayout.BeginVertical(
                _treeContainerStyle,
                GUILayout.Width(treeWidth),
                GUILayout.MinHeight(treeHeight),
                GUILayout.Height(treeHeight));

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(
                       scrollPosition,
                       GUILayout.ExpandWidth(true),
                       GUILayout.Height(treeHeight - 12f))) {
                scrollPosition = scrollView.scrollPosition;
                foreach (RepositoryLayoutEntry entry in entries) {
                    Rect rowRect = EditorGUILayout.GetControlRect(false, rowHeight, GUILayout.ExpandWidth(true));
                    bool isSelected = entry.Node.CanPreview &&
                                      string.Equals(entry.Node.RelativePath, selectedPath, System.StringComparison.Ordinal);
                    if (isSelected) {
                        EditorGUI.DrawRect(rowRect, SelectionColor);
                    }

                    if (entry.Node.CanPreview) {
                        EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);
                        if (Event.current.type == EventType.MouseDown &&
                            Event.current.button == 0 &&
                            rowRect.Contains(Event.current.mousePosition)) {
                            SelectPreview(entry.Node);
                            Event.current.Use();
                        }
                    }

                    DrawEntry(rowRect, entry);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Releases the hidden Inspector preview object created for selection-driven previews.
        /// </summary>
        public void Dispose() {
            if (_selectionPreview == null) {
                return;
            }

            if (Selection.activeObject == _selectionPreview) {
                Selection.activeObject = null;
            }

            Object.DestroyImmediate(_selectionPreview);
            _selectionPreview = null;
        }

        private void EnsureStyles() {
            _treeContainerStyle ??= new GUIStyle(EditorStyles.textArea) {
                padding = new RectOffset(6, 6, 6, 6),
                margin = new RectOffset(0, 0, 0, 0)
            };

            _treeRowStyle ??= new GUIStyle(EditorStyles.label) {
                richText = false,
                wordWrap = false,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(4, 4, 2, 2)
            };

            _treeRowStyle.normal.textColor = _treeContainerStyle.normal.textColor;
            _treeRowStyle.hover.textColor = _treeContainerStyle.normal.textColor;
            _treeRowStyle.active.textColor = _treeContainerStyle.normal.textColor;
            _treeRowStyle.focused.textColor = _treeContainerStyle.normal.textColor;

            _tagStyle ??= new GUIStyle(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(5, 5, 1, 1),
                margin = new RectOffset(0, 0, 0, 0)
            };
        }

        /// <summary>
        /// Lazily creates the hidden inspector target reused across selection changes.
        /// </summary>
        private RepositoryLayoutPreviewSelection GetOrCreateSelectionPreview() {
            if (_selectionPreview != null) {
                return _selectionPreview;
            }

            _selectionPreview = ScriptableObject.CreateInstance<RepositoryLayoutPreviewSelection>();
            _selectionPreview.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
            ApplySelectionPayload(
                _selectionPreview,
                displayName: "Repository Layout Preview",
                relativePath: string.Empty,
                previewContent: string.Empty,
                sourceFilePath: string.Empty,
                sourceFolderPath: string.Empty);
            return _selectionPreview;
        }

        private void SelectPreview(RepositoryLayoutNode node) {
            if (!node.CanPreview) {
                return;
            }

            RepositoryLayoutPreviewSelection preview = GetOrCreateSelectionPreview();
            ApplySelectionPayload(preview, node);
            EditorUtility.SetDirty(preview);
            Selection.activeObject = preview;
        }

        private void SyncInspectorSelection(RepositoryLayoutNode root) {
            RepositoryLayoutPreviewSelection preview = GetOrCreateSelectionPreview();
            if (Selection.activeObject != preview || string.IsNullOrWhiteSpace(preview.RelativePath)) {
                return;
            }

            RepositoryLayoutNode matchingNode = FindNodeByPath(root, preview.RelativePath);
            if (matchingNode == null || !matchingNode.CanPreview) {
                return;
            }

            ApplySelectionPayload(preview, matchingNode);
            EditorUtility.SetDirty(preview);
        }

        /// <summary>
        /// Applies a node's resolved content and backing paths to the reusable inspector target.
        /// </summary>
        private static void ApplySelectionPayload(RepositoryLayoutPreviewSelection preview, RepositoryLayoutNode node) {
            ApplySelectionPayload(
                preview,
                node.Name,
                node.RelativePath,
                node.PreviewContent,
                node.SourceFilePath,
                node.SourceFolderPath);
        }

        /// <summary>
        /// Centralizes selection payload assignment so the initialization and refresh paths stay in sync.
        /// </summary>
        private static void ApplySelectionPayload(
            RepositoryLayoutPreviewSelection preview,
            string displayName,
            string relativePath,
            string previewContent,
            string sourceFilePath,
            string sourceFolderPath) {
            preview.UpdateContent(displayName, relativePath, previewContent, sourceFilePath, sourceFolderPath);
        }

        private static RepositoryLayoutNode FindNodeByPath(RepositoryLayoutNode node, string relativePath) {
            if (node == null) {
                return null;
            }

            if (string.Equals(node.RelativePath, relativePath, System.StringComparison.Ordinal)) {
                return node;
            }

            foreach (RepositoryLayoutNode child in node.Children) {
                RepositoryLayoutNode match = FindNodeByPath(child, relativePath);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        private static RepositoryLayoutNode BuildPreviewTree(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode root = CreateDirectoryNode(data.RootDirectoryName, data.RootDirectoryName, RepositoryLayoutGroup.None);

            if (data.IncludeDocsFolder) {
                root.Children.Add(BuildDocsNode(data));
            }

            root.Children.Add(BuildPackageNode(data));
            string license = data.Context.GetLicense();
            if (!string.IsNullOrWhiteSpace(license)) {
                root.Children.Add(CreateGeneratedFileNode("LICENSE", "LICENSE", license, RepositoryLayoutGroup.Repo));
            }

            root.Children.Add(CreateGeneratedFileNode(
                "README.md",
                "README.md",
                data.Context.GetRepositoryReadme(),
                RepositoryLayoutGroup.Repo));
            root.Children.Add(BuildCompanionProjectNode(data));

            return root;
        }

        private static RepositoryLayoutNode BuildDocsNode(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode docs = CreateDirectoryNode("docs", "docs", RepositoryLayoutGroup.Docs);
            docs.Children.Add(CreateTemplateFileNode(".gitignore", "docs/.gitignore", "docs/.gitignore", RepositoryLayoutGroup.Docs));
            docs.Children.Add(CreateGeneratedFileNode("docfx.json", "docs/docfx.json", data.Context.GetDocfxJson(), RepositoryLayoutGroup.Docs));
            docs.Children.Add(CreateGeneratedFileNode(
                "docfx-pdf.json",
                "docs/docfx-pdf.json",
                data.Context.GetDocfxPdfJson(),
                RepositoryLayoutGroup.Docs));
            docs.Children.Add(CreateGeneratedFileNode(
                "filterConfig.yml",
                "docs/filterConfig.yml",
                data.Context.GetFilterConfig(),
                RepositoryLayoutGroup.Docs));
            docs.Children.Add(CreateGeneratedFileNode("index.md", "docs/index.md", data.Context.GetIndexMD(), RepositoryLayoutGroup.Docs));

            RepositoryLayoutNode api = CreateDirectoryNode("api", "docs/api", RepositoryLayoutGroup.Docs);
            api.Children.Add(CreateTemplateFileNode(".gitignore", "docs/api/.gitignore", "docs/api/.gitignore", RepositoryLayoutGroup.Docs));
            api.Children.Add(CreateTemplateFileNode("index.md", "docs/api/index.md", "docs/api/index.md", RepositoryLayoutGroup.Docs));
            docs.Children.Add(api);

            RepositoryLayoutNode images = CreateDirectoryNode("images", "docs/images", RepositoryLayoutGroup.Docs);
            images.Children.Add(CreateBinaryFileNode("doji.png", "docs/images/doji.png", RepositoryLayoutGroup.Docs));
            images.Children.Add(CreateBinaryFileNode("favicon.ico", "docs/images/favicon.ico", RepositoryLayoutGroup.Docs));
            docs.Children.Add(images);

            RepositoryLayoutNode manual = CreateDirectoryNode("manual", "docs/manual", RepositoryLayoutGroup.Docs);
            manual.Children.Add(CreateGeneratedFileNode("toc.yml", "docs/manual/toc.yml", data.Context.GetManualToc(), RepositoryLayoutGroup.Docs));
            docs.Children.Add(manual);

            RepositoryLayoutNode pdf = CreateDirectoryNode("pdf", "docs/pdf", RepositoryLayoutGroup.Docs);
            pdf.Children.Add(CreateTemplateFileNode("toc.yml", "docs/pdf/toc.yml", "docs/pdf/toc.yml", RepositoryLayoutGroup.Docs));
            docs.Children.Add(pdf);

            docs.Children.Add(CreateGeneratedFileNode("toc.yml", "docs/toc.yml", data.Context.GetRootToc(), RepositoryLayoutGroup.Docs));
            return docs;
        }

        private static RepositoryLayoutNode BuildPackageNode(RepositoryLayoutPreviewData data) {
            string packageRoot = data.PackageName;
            RepositoryLayoutNode package = CreateDirectoryNode(data.PackageName, packageRoot, RepositoryLayoutGroup.Package);
            package.Children.Add(CreateGeneratedFileNode(
                "CHANGELOG.md",
                $"{packageRoot}/CHANGELOG.md",
                data.Context.GetChangelog(),
                RepositoryLayoutGroup.Package));

            if (data.IncludeEditorFolder) {
                RepositoryLayoutNode editor = CreateDirectoryNode("Editor", $"{packageRoot}/Editor", RepositoryLayoutGroup.Package);
                editor.Children.Add(CreateGeneratedFileNode(
                    $"{data.AssemblyName}.Editor.asmdef",
                    $"{packageRoot}/Editor/{data.AssemblyName}.Editor.asmdef",
                    data.Context.GetEditorAsmDef(RuntimeAssemblyGuidPreview),
                    RepositoryLayoutGroup.Package));
                package.Children.Add(editor);
            }

            package.Children.Add(CreateGeneratedFileNode(
                "README.md",
                $"{packageRoot}/README.md",
                data.Context.GetPackageReadme(),
                RepositoryLayoutGroup.Package));

            RepositoryLayoutNode runtime = CreateDirectoryNode("Runtime", $"{packageRoot}/Runtime", RepositoryLayoutGroup.Package);
            runtime.Children.Add(CreateGeneratedFileNode(
                $"{data.AssemblyName}.asmdef",
                $"{packageRoot}/Runtime/{data.AssemblyName}.asmdef",
                data.Context.GetRuntimeAsmDef(),
                RepositoryLayoutGroup.Package));
            runtime.Children.Add(CreateGeneratedFileNode(
                "AssemblyInfo.cs",
                $"{packageRoot}/Runtime/AssemblyInfo.cs",
                data.Context.GetAssemblyInfo(),
                RepositoryLayoutGroup.Package));
            package.Children.Add(runtime);

            if (data.IncludeSamplesFolder) {
                RepositoryLayoutNode samples = CreateDirectoryNode("Samples~", $"{packageRoot}/Samples~", RepositoryLayoutGroup.Package);
                samples.Children.Add(CreateGeneratedFileNode(
                    $"{data.AssemblyName}.asmdef",
                    $"{packageRoot}/Samples~/{data.AssemblyName}.asmdef",
                    data.Context.GetSamplesAsmDef(RuntimeAssemblyGuidPreview),
                    RepositoryLayoutGroup.Package));
                samples.Children.Add(CreateDirectoryNode("00-SharedSampleAssets", $"{packageRoot}/Samples~/00-SharedSampleAssets", RepositoryLayoutGroup.Package));

                RepositoryLayoutNode basicSample = CreateDirectoryNode("01-BasicSample", $"{packageRoot}/Samples~/01-BasicSample", RepositoryLayoutGroup.Package);
                basicSample.Children.Add(CreateGeneratedFileNode(
                    "BasicSample.cs",
                    $"{packageRoot}/Samples~/01-BasicSample/BasicSample.cs",
                    data.Context.GetSampleScript(),
                    RepositoryLayoutGroup.Package));
                samples.Children.Add(basicSample);

                package.Children.Add(samples);
            }

            if (data.IncludeTestsFolder) {
                package.Children.Add(CreateDirectoryNode("Tests", $"{packageRoot}/Tests", RepositoryLayoutGroup.Package));
            }

            package.Children.Add(CreateGeneratedFileNode(
                "package.json",
                $"{packageRoot}/package.json",
                data.Context.GetPackageManifest(),
                RepositoryLayoutGroup.Package));
            return package;
        }

        private static RepositoryLayoutNode BuildCompanionProjectNode(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode projects = CreateDirectoryNode("projects", "projects", RepositoryLayoutGroup.Project);
            RepositoryLayoutNode project = CreateDirectoryNode(data.CompanionProjectName, $"projects/{data.CompanionProjectName}", RepositoryLayoutGroup.Project);
            if (data.IncludeRepositoryGitIgnore) {
                project.Children.Add(CreateGeneratedFileNode(
                    ".gitignore",
                    $"projects/{data.CompanionProjectName}/.gitignore",
                    data.RepositoryGitIgnoreTemplate,
                    RepositoryLayoutGroup.Project));
            }

            project.Children.Add(CreateDirectoryNode("Assets", $"projects/{data.CompanionProjectName}/Assets", RepositoryLayoutGroup.Project));

            RepositoryLayoutNode packages = CreateDirectoryNode("Packages", $"projects/{data.CompanionProjectName}/Packages", RepositoryLayoutGroup.Project);
            packages.Children.Add(CreateGeneratedFileNode(
                "manifest.json",
                $"projects/{data.CompanionProjectName}/Packages/manifest.json",
                data.Context.GetProjectManifest(),
                RepositoryLayoutGroup.Project));
            if (data.IncludePackagesLockFile) {
                packages.Children.Add(CreateTemplateFileNode(
                    "packages-lock.json",
                    $"projects/{data.CompanionProjectName}/Packages/packages-lock.json",
                    "Packages/packages-lock.json",
                    RepositoryLayoutGroup.Project));
            }

            project.Children.Add(packages);
            project.Children.Add(CreateDirectoryNode(
                "ProjectSettings",
                $"projects/{data.CompanionProjectName}/ProjectSettings",
                RepositoryLayoutGroup.Project));
            projects.Children.Add(project);
            return projects;
        }

        private static RepositoryLayoutNode CreateDirectoryNode(string name, string relativePath, RepositoryLayoutGroup group) {
            return new RepositoryLayoutNode(
                name,
                relativePath,
                isDirectory: true,
                previewContent: string.Empty,
                sourceFilePath: string.Empty,
                sourceFolderPath: GetExistingFolderPath(relativePath),
                group: group);
        }

        private static RepositoryLayoutNode CreateGeneratedFileNode(
            string name,
            string relativePath,
            string previewContent,
            RepositoryLayoutGroup group) {
            return new RepositoryLayoutNode(
                name,
                relativePath,
                isDirectory: false,
                previewContent,
                sourceFilePath: string.Empty,
                sourceFolderPath: string.Empty,
                group: group);
        }

        private static RepositoryLayoutNode CreateTemplateFileNode(
            string name,
            string relativePath,
            string sourcePath,
            RepositoryLayoutGroup group) {
            string previewContent = TryReadTemplateFile(sourcePath, out string fileContent)
                ? fileContent
                : $"Template source not found at {sourcePath}.";

            return new RepositoryLayoutNode(
                name,
                relativePath,
                isDirectory: false,
                previewContent,
                sourceFilePath: GetExistingFilePath(sourcePath),
                sourceFolderPath: GetExistingFolderPath(Path.GetDirectoryName(sourcePath)),
                group: group);
        }

        private static RepositoryLayoutNode CreateBinaryFileNode(string name, string sourcePath, RepositoryLayoutGroup group) {
            return new RepositoryLayoutNode(
                name,
                sourcePath,
                isDirectory: false,
                previewContent: $"Binary asset copied from template: {sourcePath}",
                sourceFilePath: GetExistingFilePath(sourcePath),
                sourceFolderPath: GetExistingFolderPath(Path.GetDirectoryName(sourcePath)),
                group: group);
        }

        private static bool TryReadTemplateFile(string relativePath, out string fileContent) {
            string absolutePath = GetAbsolutePath(relativePath);
            if (!File.Exists(absolutePath)) {
                fileContent = string.Empty;
                return false;
            }

            fileContent = File.ReadAllText(absolutePath);
            return true;
        }

        private static string GetExistingFilePath(string relativePath) {
            string absolutePath = GetAbsolutePath(relativePath);
            return File.Exists(absolutePath) ? absolutePath : string.Empty;
        }

        private static string GetExistingFolderPath(string relativePath) {
            string absolutePath = GetAbsolutePath(relativePath);
            return Directory.Exists(absolutePath) ? absolutePath : string.Empty;
        }

        private static string GetAbsolutePath(string relativePath) {
            return string.IsNullOrWhiteSpace(relativePath)
                ? string.Empty
                : Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        }

        private static void AppendEntries(
            List<RepositoryLayoutEntry> entries,
            RepositoryLayoutNode node,
            string indent,
            bool isLast,
            bool isRoot = false) {
            string displayText;
            if (isRoot) {
                displayText = node.Name;
            }
            else {
                displayText = $"{indent}{(isLast ? "└── " : "├── ")}{node.Name}";
            }

            entries.Add(new RepositoryLayoutEntry(displayText, node));

            string childIndent = isRoot ? string.Empty : indent + (isLast ? "    " : "│   ");
            for (int i = 0; i < node.Children.Count; i++) {
                AppendEntries(entries, node.Children[i], childIndent, i == node.Children.Count - 1);
            }
        }

        /// <summary>
        /// Represents one directory or file entry in the rendered repository tree.
        /// </summary>
        private sealed class RepositoryLayoutNode {
            /// <summary>
            /// Captures one rendered row and the preview metadata needed to drive Inspector selection.
            /// </summary>
            public RepositoryLayoutNode(
                string name,
                string relativePath,
                bool isDirectory,
                string previewContent,
                string sourceFilePath,
                string sourceFolderPath,
                RepositoryLayoutGroup group) {
                Name = string.IsNullOrWhiteSpace(name) ? "(unnamed)" : name;
                RelativePath = relativePath ?? string.Empty;
                IsDirectory = isDirectory;
                PreviewContent = previewContent;
                SourceFilePath = sourceFilePath ?? string.Empty;
                SourceFolderPath = sourceFolderPath ?? string.Empty;
                Group = group;
            }

            /// <summary>
            /// Display name shown in the tree.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Relative output path represented by this row.
            /// </summary>
            public string RelativePath { get; }

            /// <summary>
            /// Whether this row represents a directory rather than a file.
            /// </summary>
            public bool IsDirectory { get; }

            /// <summary>
            /// Resolved preview contents shown in the Inspector when selected.
            /// </summary>
            public string PreviewContent { get; }

            /// <summary>
            /// Existing source file path for template-backed entries, if one is available.
            /// </summary>
            public string SourceFilePath { get; }

            /// <summary>
            /// Existing source folder path associated with the entry, if one is available.
            /// </summary>
            public string SourceFolderPath { get; }

            public RepositoryLayoutGroup Group { get; }

            public string GroupLabel => Group switch {
                RepositoryLayoutGroup.Repo => "REPO",
                RepositoryLayoutGroup.Docs => "DOCS",
                RepositoryLayoutGroup.Package => "PKG",
                RepositoryLayoutGroup.Project => "PROJECT",
                _ => string.Empty
            };

            public Color GroupColor => Group switch {
                RepositoryLayoutGroup.Repo => RepoTint,
                RepositoryLayoutGroup.Docs => DocsTint,
                RepositoryLayoutGroup.Package => PackageTint,
                RepositoryLayoutGroup.Project => ProjectTint,
                _ => Color.white
            };

            /// <summary>
            /// Whether selecting this row should push content into the Inspector preview.
            /// </summary>
            public bool CanPreview => !IsDirectory && !string.IsNullOrWhiteSpace(PreviewContent);

            public List<RepositoryLayoutNode> Children { get; } = new();
        }

        private void DrawEntry(Rect rowRect, RepositoryLayoutEntry entry) {
            GUI.Label(rowRect, entry.DisplayText, _treeRowStyle);

            if (string.IsNullOrWhiteSpace(entry.Node.GroupLabel)) {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = entry.Node.GroupColor;

            Vector2 tagSize = _tagStyle.CalcSize(new GUIContent(entry.Node.GroupLabel));
            Rect tagRect = new Rect(
                rowRect.xMax - tagSize.x - 6f,
                rowRect.y + 1f,
                tagSize.x,
                Mathf.Min(rowRect.height - 2f, tagSize.y));
            GUI.Box(tagRect, entry.Node.GroupLabel, _tagStyle);

            GUI.color = previousColor;
        }

        private readonly struct RepositoryLayoutEntry {
            public RepositoryLayoutEntry(string displayText, RepositoryLayoutNode node) {
                DisplayText = displayText;
                Node = node;
            }

            public string DisplayText { get; }

            public RepositoryLayoutNode Node { get; }
        }

        private enum RepositoryLayoutGroup {
            None,
            Repo,
            Docs,
            Package,
            Project
        }
    }
}
