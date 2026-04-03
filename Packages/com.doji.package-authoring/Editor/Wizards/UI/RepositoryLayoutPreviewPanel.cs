using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Doji.PackageAuthoring.Editor.Utilities;
using Doji.PackageAuthoring.Editor.Wizards.Presets;
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
        private const float TagRightMargin = 6f;
        private const float TagSpacing = 6f;
        private const float TagColumnWidth = 44f;
        private const string RuntimeAssemblyGuidPreview = "<runtime-assembly-guid>";
        private static readonly Color HoverColor = new(0.24f, 0.47f, 0.85f, 0.08f);
        private static readonly Color RelatedHoverColor = new(0.92f, 0.68f, 0.18f, 0.08f);
        private static readonly Color SelectionColor = new(0.24f, 0.47f, 0.85f, 0.18f);
        private static readonly Color RepoTint = new(0.92f, 0.86f, 1f);
        private static readonly Color DocsTint = new(0.78f, 0.90f, 1f);
        private static readonly Color PackageTint = new(0.84f, 1f, 0.84f);
        private static readonly Color ProjectTint = new(1f, 0.92f, 0.78f);

        private GUIStyle _treeRowStyle;
        private GUIStyle _treeContainerStyle;
        private GUIStyle _tagStyle;
        private RepositoryLayoutPreviewSelection _selectionPreview;
        private bool _hoverHighlightingEnabled = true;
        private readonly GUIContent _labelContent = new();
        private bool _hasCachedSnapshot;
        private PreviewSnapshotKey _cachedSnapshotKey;
        private PreviewSnapshot _cachedSnapshot;

        /// <summary>
        /// Draws the preview section and updates its scroll state.
        /// </summary>
        public void Draw(float windowWidth, float windowHeight, RepositoryLayoutPreviewData data, ref Vector2 scrollPosition) {
            PreviewSnapshot snapshot = GetOrBuildSnapshot(data);
            float previewWidth = Mathf.Clamp(windowWidth * 0.4f, MinWidth, MaxWidth);
            float previewHeight = Mathf.Max(MinHeight, windowHeight - 72f);

            // The preview tree is refreshed only when relevant wizard data changes, so the Inspector payload must be
            // refreshed against the cached snapshot instead of assuming a rebuild every frame.
            SyncInspectorSelection(snapshot, data);

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
            DrawContent(snapshot, previewWidth, previewHeight, data, ref scrollPosition);
            DrawHoverHighlightingToggle();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawContent(
            PreviewSnapshot snapshot,
            float panelWidth,
            float panelHeight,
            RepositoryLayoutPreviewData data,
            ref Vector2 scrollPosition) {
            EnsureStyles();

            float rowHeight = _treeRowStyle.lineHeight + RowPadding;
            float treeWidth = Mathf.Max(TreeColumnMinWidth, panelWidth - 12f);
            float treeHeight = Mathf.Max(MinHeight - 32f, panelHeight - 32f);

            RepositoryLayoutPreviewSelection previewSelection = GetOrCreateSelectionPreview();
            string selectedPath = Selection.activeObject == previewSelection ? previewSelection.RelativePath : string.Empty;
            IReadOnlyCollection<string> hoveredTargets = _hoverHighlightingEnabled
                ? RepositoryLayoutPreviewHoverContext.CurrentTargets
                : System.Array.Empty<string>();

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
                foreach (RepositoryLayoutEntry entry in snapshot.Entries) {
                    Rect rowRect = EditorGUILayout.GetControlRect(false, rowHeight, GUILayout.ExpandWidth(true));
                    bool isRelatedHover = hoveredTargets.Count > 0 &&
                                          IsHoverMatch(entry.Node, hoveredTargets, snapshot.Root.Name, data);
                    bool isSelected = entry.Node.CanPreview &&
                                      string.Equals(entry.Node.RelativePath, selectedPath, System.StringComparison.Ordinal);
                    bool isHovered = entry.Node.CanPreview &&
                                     rowRect.Contains(Event.current.mousePosition);
                    if (isRelatedHover) {
                        EditorGUI.DrawRect(rowRect, RelatedHoverColor);
                    }

                    if (isHovered && !isSelected) {
                        EditorGUI.DrawRect(rowRect, HoverColor);
                    }

                    if (isSelected) {
                        EditorGUI.DrawRect(rowRect, SelectionColor);
                    }

                    if (entry.Node.CanPreview) {
                        EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);
                        if (Event.current.type == EventType.MouseDown &&
                            Event.current.button == 0 &&
                            rowRect.Contains(Event.current.mousePosition)) {
                            SelectPreview(entry.Node, snapshot.Root.Name, data);
                            Event.current.Use();
                        }
                    }

                    DrawEntry(rowRect, entry);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawHoverHighlightingToggle() {
            EditorGUILayout.Space(4f);
            _hoverHighlightingEnabled = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Hover Highlights",
                    "Highlights affected preview rows and matching content in the selected file while hovering related settings fields."),
                _hoverHighlightingEnabled);
        }

        private PreviewSnapshot GetOrBuildSnapshot(RepositoryLayoutPreviewData data) {
            PreviewSnapshotKey snapshotKey = PreviewSnapshotKey.Create(data);
            if (_hasCachedSnapshot && _cachedSnapshotKey.Equals(snapshotKey) && _cachedSnapshot != null) {
                return _cachedSnapshot;
            }

            RepositoryLayoutNode root = BuildPreviewTree(data);
            List<RepositoryLayoutEntry> entries = new();
            AppendEntries(entries, root, string.Empty, isLast: true, isRoot: true);

            _cachedSnapshot = new PreviewSnapshot(root, entries);
            _cachedSnapshotKey = snapshotKey;
            _hasCachedSnapshot = true;
            return _cachedSnapshot;
        }

        private static bool IsHoverMatch(
            RepositoryLayoutNode node,
            IReadOnlyCollection<string> hoveredTargets,
            string rootDirectoryName,
            RepositoryLayoutPreviewData data) {
            foreach (string hoveredTarget in hoveredTargets) {
                if (MatchesTarget(node, hoveredTarget, rootDirectoryName, data)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Resolves one semantic hover target to the preview rows that should be softly highlighted.
        /// </summary>
        /// <remarks>
        /// This is intentionally kept next to the preview tree renderer instead of on
        /// <see cref="RepositoryLayoutPreviewHoverTargets"/>. The mapping needs access to both the generated node paths and
        /// the current scaffold data, and several fields expand to multiple rows rather than a single static file.
        ///
        /// Each switch case defines the matching rule for one hovered field. The rule can match a preview row in
        /// three different ways:
        /// exact relative path comparison for a specific generated file or folder,
        /// relative path prefix comparison for an entire generated subtree,
        /// or text comparison against the node name, output path, or generated preview content when the field's value
        /// is embedded into several files.
        ///
        /// Example:
        /// the package identifier highlights broadly because it is both the package folder name and a token that
        /// appears in multiple generated files, while dependencies only highlight <c>package.json</c> because that is
        /// the only generated file they affect directly.
        /// </remarks>
        /// <param name="node">Current preview row under evaluation.</param>
        /// <param name="hoveredTarget">Semantic hover identifier emitted by a settings field.</param>
        /// <param name="rootDirectoryName">Displayed repository root folder name.</param>
        /// <param name="data">Current preview data used to resolve generated paths and template content.</param>
        /// <returns><c>true</c> when the row should be highlighted for the hovered field.</returns>
        private static bool MatchesTarget(
            RepositoryLayoutNode node,
            string hoveredTarget,
            string rootDirectoryName,
            RepositoryLayoutPreviewData data) {
            switch (hoveredTarget) {
                case RepositoryLayoutPreviewHoverTargets.RepoCopyrightHolder:
                    return MatchesContent(node, data.Context.Repo.CopyrightHolder) || MatchesPath(node, "LICENSE");
                case RepositoryLayoutPreviewHoverTargets.RepoLicenseType:
                    return MatchesPath(node, "LICENSE");
                case RepositoryLayoutPreviewHoverTargets.IncludeRepositoryReadme:
                    return MatchesPath(node, "README.md");
                case RepositoryLayoutPreviewHoverTargets.PackageName:
                    return MatchesContent(node, data.Context.Package.PackageName) ||
                           MatchesPath(node, data.PackageName);
                case RepositoryLayoutPreviewHoverTargets.PackageDisplayName:
                    return MatchesContent(node, data.Context.Package.PackageDisplayName);
                case RepositoryLayoutPreviewHoverTargets.AssemblyName:
                    return MatchesContent(node, data.Context.Package.AssemblyName);
                case RepositoryLayoutPreviewHoverTargets.NamespaceName:
                    return MatchesContent(node, data.Context.Package.NamespaceName);
                case RepositoryLayoutPreviewHoverTargets.Description:
                    return MatchesContent(node, data.Context.Package.Description);
                case RepositoryLayoutPreviewHoverTargets.PackageCompanyName:
                    return MatchesContent(node, data.Context.Package.CompanyName);
                case RepositoryLayoutPreviewHoverTargets.IncludeAuthor:
                case RepositoryLayoutPreviewHoverTargets.AuthorUrl:
                case RepositoryLayoutPreviewHoverTargets.AuthorEmail:
                    return MatchesPath(node, $"{data.PackageName}/package.json");
                case RepositoryLayoutPreviewHoverTargets.DocumentationUrl:
                    return MatchesContent(node, data.Context.Package.DocumentationUrl) ||
                           MatchesPath(node, $"{data.PackageName}/package.json");
                case RepositoryLayoutPreviewHoverTargets.IncludeMinimumUnityVersion:
                case RepositoryLayoutPreviewHoverTargets.MinimumUnityVersion:
                    return MatchesPath(node, $"{data.PackageName}/package.json");
                case RepositoryLayoutPreviewHoverTargets.CreateDocsFolder:
                    return MatchesPath(node, "docs") || MatchesPrefix(node, "docs/");
                case RepositoryLayoutPreviewHoverTargets.IncludePackageReadme:
                    return MatchesPath(node, $"{data.PackageName}/README.md");
                case RepositoryLayoutPreviewHoverTargets.CreateSamplesFolder:
                    return MatchesPath(node, $"{data.PackageName}/Samples~") ||
                           MatchesPrefix(node, $"{data.PackageName}/Samples~/");
                case RepositoryLayoutPreviewHoverTargets.CreateEditorFolder:
                    return MatchesPath(node, $"{data.PackageName}/Editor") ||
                           MatchesPrefix(node, $"{data.PackageName}/Editor/");
                case RepositoryLayoutPreviewHoverTargets.CreateTestsFolder:
                    return MatchesPath(node, $"{data.PackageName}/Tests") ||
                           MatchesPrefix(node, $"{data.PackageName}/Tests/");
                case RepositoryLayoutPreviewHoverTargets.Dependencies:
                    return MatchesPath(node, $"{data.PackageName}/package.json");
                case RepositoryLayoutPreviewHoverTargets.ProjectCompanyName:
                    return MatchesContent(node, data.Context.Project.CompanyName);
                case RepositoryLayoutPreviewHoverTargets.ProductName:
                    return MatchesContent(node, data.Context.Project.ProductName) ||
                           MatchesPath(node, $"projects/{data.CompanionProjectName}");
                case RepositoryLayoutPreviewHoverTargets.Version:
                    return MatchesContent(node, data.Context.Project.Version);
                case RepositoryLayoutPreviewHoverTargets.ProjectManifest:
                    return MatchesPath(node, $"projects/{data.CompanionProjectName}/Packages/manifest.json");
                case RepositoryLayoutPreviewHoverTargets.TargetLocation:
                    return MatchesPath(node, rootDirectoryName);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Matches one concrete generated row by exact relative output path.
        /// </summary>
        private static bool MatchesPath(RepositoryLayoutNode node, string relativePath) {
            return !string.IsNullOrWhiteSpace(relativePath) &&
                   string.Equals(node.RelativePath, relativePath, System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Matches every row under a generated folder by checking whether the row path starts with the given prefix.
        /// </summary>
        private static bool MatchesPrefix(RepositoryLayoutNode node, string relativePathPrefix) {
            return !string.IsNullOrWhiteSpace(relativePathPrefix) &&
                   node.RelativePath.StartsWith(relativePathPrefix, System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Matches rows whose generated preview text contains the resolved field value.
        /// </summary>
        private static bool MatchesContent(RepositoryLayoutNode node, string value) {
            return !string.IsNullOrWhiteSpace(value) &&
                   !string.IsNullOrWhiteSpace(node.PreviewContent) &&
                   node.PreviewContent.Contains(value, System.StringComparison.OrdinalIgnoreCase);
        }

        private static string[] GetHoverHighlights(
            RepositoryLayoutNode node,
            IReadOnlyCollection<string> hoveredTargets,
            string rootDirectoryName,
            RepositoryLayoutPreviewData data) {
            if (node == null ||
                data == null ||
                hoveredTargets == null ||
                hoveredTargets.Count == 0 ||
                string.IsNullOrWhiteSpace(node.PreviewContent)) {
                return System.Array.Empty<string>();
            }

            HashSet<string> highlights = new(System.StringComparer.Ordinal);
            foreach (string hoveredTarget in hoveredTargets) {
                foreach (string highlight in GetHoverHighlightsForTarget(node, hoveredTarget, rootDirectoryName, data)) {
                    if (!string.IsNullOrWhiteSpace(highlight)) {
                        highlights.Add(highlight);
                    }
                }
            }

            if (highlights.Count == 0) {
                return System.Array.Empty<string>();
            }

            string[] highlightArray = new string[highlights.Count];
            highlights.CopyTo(highlightArray);
            return highlightArray;
        }

        private static IEnumerable<string> GetHoverHighlightsForTarget(
            RepositoryLayoutNode node,
            string hoveredTarget,
            string rootDirectoryName,
            RepositoryLayoutPreviewData data) {
            if (!MatchesTarget(node, hoveredTarget, rootDirectoryName, data)) {
                yield break;
            }

            switch (hoveredTarget) {
                case RepositoryLayoutPreviewHoverTargets.RepoCopyrightHolder:
                    yield return data.Context.Repo.CopyrightHolder;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.PackageName:
                    yield return data.Context.Package.PackageName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.PackageDisplayName:
                    yield return data.Context.Package.PackageDisplayName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.AssemblyName:
                    yield return data.Context.Package.AssemblyName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.NamespaceName:
                    yield return data.Context.Package.NamespaceName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.Description:
                    yield return data.Context.Package.Description;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.PackageCompanyName:
                    yield return data.Context.Package.CompanyName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.IncludeAuthor:
                    yield return data.Context.Package.CompanyName;
                    yield return data.Context.Package.AuthorUrl;
                    yield return data.Context.Package.AuthorEmail;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.AuthorUrl:
                    yield return data.Context.Package.AuthorUrl;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.AuthorEmail:
                    yield return data.Context.Package.AuthorEmail;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.DocumentationUrl:
                    yield return TemplateTokenResolver.Resolve(data.Context.Package.DocumentationUrl, data.Context);
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.MinimumUnityVersion:
                    yield return $"{data.Context.Package.MinimumUnityMajor}.{data.Context.Package.MinimumUnityMinor}";
                    yield return data.Context.Package.MinimumUnityRelease;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.ProjectCompanyName:
                    yield return data.Context.Project.CompanyName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.ProductName:
                    yield return data.Context.Project.ProductName;
                    yield break;
                case RepositoryLayoutPreviewHoverTargets.Version:
                    yield return data.Context.Project.Version;
                    yield return $"{data.Context.Project.Version}.0";
                    yield break;
            }
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

            UnityEngine.Object.DestroyImmediate(_selectionPreview);
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
                padding = new RectOffset(4, 4, 2, 2),
                clipping = TextClipping.Clip
            };

            _treeRowStyle.normal.textColor = _treeContainerStyle.normal.textColor;
            _treeRowStyle.hover.textColor = _treeContainerStyle.normal.textColor;
            _treeRowStyle.active.textColor = _treeContainerStyle.normal.textColor;
            _treeRowStyle.focused.textColor = _treeContainerStyle.normal.textColor;

            _tagStyle ??= new GUIStyle(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleRight,
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
                sourceFolderPath: string.Empty,
                hoverHighlights: null);
            return _selectionPreview;
        }

        private void SelectPreview(RepositoryLayoutNode node, string rootDirectoryName, RepositoryLayoutPreviewData data) {
            if (!node.CanPreview) {
                return;
            }

            RepositoryLayoutPreviewSelection preview = GetOrCreateSelectionPreview();
            ApplySelectionPayload(preview, node, rootDirectoryName, data);
            EditorUtility.SetDirty(preview);
            Selection.activeObject = preview;
            InspectorWindowUtility.TryRevealOpenInspector();
        }

        private void SyncInspectorSelection(PreviewSnapshot snapshot, RepositoryLayoutPreviewData data) {
            RepositoryLayoutPreviewSelection preview = GetOrCreateSelectionPreview();
            if (Selection.activeObject != preview || string.IsNullOrWhiteSpace(preview.RelativePath)) {
                return;
            }

            RepositoryLayoutNode matchingNode = snapshot.FindNodeByPath(preview.RelativePath);
            if (matchingNode == null || !matchingNode.CanPreview) {
                return;
            }

            ApplySelectionPayload(preview, matchingNode, snapshot.Root.Name, data);
            EditorUtility.SetDirty(preview);
        }

        /// <summary>
        /// Applies a node's resolved content and backing paths to the reusable inspector target.
        /// </summary>
        private static void ApplySelectionPayload(
            RepositoryLayoutPreviewSelection preview,
            RepositoryLayoutNode node,
            string rootDirectoryName,
            RepositoryLayoutPreviewData data) {
            ApplySelectionPayload(
                preview,
                node.Name,
                node.RelativePath,
                node.PreviewContent,
                node.SourceFilePath,
                node.SourceFolderPath,
                GetHoverHighlights(node, RepositoryLayoutPreviewHoverContext.CurrentTargets, rootDirectoryName, data));
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
            string sourceFolderPath,
            string[] hoverHighlights) {
            preview.UpdateContent(displayName, relativePath, previewContent, sourceFilePath, sourceFolderPath, hoverHighlights);
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

            if (data.Context.Repo.IncludeReadme) {
                root.Children.Add(CreateGeneratedFileNode(
                    "README.md",
                    "README.md",
                    data.Context.GetRepositoryReadme(),
                    RepositoryLayoutGroup.Repo));
            }
            root.Children.Add(BuildCompanionProjectNode(data));

            return root;
        }

        private static RepositoryLayoutNode BuildDocsNode(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode docs = CreateDirectoryNode("docs", "docs", RepositoryLayoutGroup.Docs);
            docs.Children.Add(CreateGeneratedFileNode(".gitignore", "docs/.gitignore", data.Context.GetDocsGitIgnore(), RepositoryLayoutGroup.Docs));
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
            api.Children.Add(CreateGeneratedFileNode(".gitignore", "docs/api/.gitignore", data.Context.GetDocsApiGitIgnore(), RepositoryLayoutGroup.Docs));
            api.Children.Add(CreateGeneratedFileNode("index.md", "docs/api/index.md", data.Context.GetDocsApiIndex(), RepositoryLayoutGroup.Docs));
            docs.Children.Add(api);

            DocsBrandingImageSettings brandingImages = DocsBrandingImageSettings.Instance;
            if (brandingImages.HasAnyImage) {
                RepositoryLayoutNode images = CreateDirectoryNode("images", "docs/images", RepositoryLayoutGroup.Docs);
                if (brandingImages.LogoTexture != null) {
                    string logoTexturePath = AssetDatabase.GetAssetPath(brandingImages.LogoTexture);
                    images.Children.Add(CreateGeneratedBinaryFileNode(
                        "logo.png",
                        "docs/images/logo.png",
                        $"Generated from documentation logo texture: {logoTexturePath}",
                        logoTexturePath,
                        RepositoryLayoutGroup.Docs));
                }

                if (brandingImages.FaviconTexture != null) {
                    string faviconTexturePath = AssetDatabase.GetAssetPath(brandingImages.FaviconTexture);
                    images.Children.Add(CreateGeneratedBinaryFileNode(
                        "favicon.ico",
                        "docs/images/favicon.ico",
                        $"Generated from documentation favicon texture: {faviconTexturePath}",
                        faviconTexturePath,
                        RepositoryLayoutGroup.Docs));
                }

                docs.Children.Add(images);
            }

            RepositoryLayoutNode manual = CreateDirectoryNode("manual", "docs/manual", RepositoryLayoutGroup.Docs);
            manual.Children.Add(CreateGeneratedFileNode("toc.yml", "docs/manual/toc.yml", data.Context.GetManualToc(), RepositoryLayoutGroup.Docs));
            docs.Children.Add(manual);

            RepositoryLayoutNode pdf = CreateDirectoryNode("pdf", "docs/pdf", RepositoryLayoutGroup.Docs);
            pdf.Children.Add(CreateGeneratedFileNode("toc.yml", "docs/pdf/toc.yml", data.Context.GetPdfToc(), RepositoryLayoutGroup.Docs));
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

            if (data.Context.Package.IncludeReadme) {
                package.Children.Add(CreateGeneratedFileNode(
                    "README.md",
                    $"{packageRoot}/README.md",
                    data.Context.GetPackageReadme(),
                    RepositoryLayoutGroup.Package));
            }

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

        private static RepositoryLayoutNode CreateGeneratedBinaryFileNode(
            string name,
            string relativePath,
            string previewContent,
            string sourceAssetPath,
            RepositoryLayoutGroup group) {
            return new RepositoryLayoutNode(
                name,
                relativePath,
                isDirectory: false,
                previewContent,
                sourceFilePath: GetExistingFilePath(sourceAssetPath),
                sourceFolderPath: GetExistingFolderPath(Path.GetDirectoryName(sourceAssetPath)),
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
            List<RepositoryLayoutNode> sortedChildren = GetSortedChildren(node.Children);
            for (int i = 0; i < sortedChildren.Count; i++) {
                AppendEntries(entries, sortedChildren[i], childIndent, i == sortedChildren.Count - 1);
            }
        }

        private static List<RepositoryLayoutNode> GetSortedChildren(List<RepositoryLayoutNode> children) {
            List<RepositoryLayoutNode> sortedChildren = new(children);
            sortedChildren.Sort((left, right) => {
                int ignoreCaseComparison = string.Compare(left.Name, right.Name, System.StringComparison.OrdinalIgnoreCase);
                return ignoreCaseComparison != 0
                    ? ignoreCaseComparison
                    : string.Compare(left.Name, right.Name, System.StringComparison.Ordinal);
            });
            return sortedChildren;
        }

        /// <summary>
        /// Represents one directory or file entry in the rendered repository tree.
        /// </summary>
        private sealed class RepositoryLayoutNode {
            public const string RepoGroupLabel = "REPO";
            public const string DocsGroupLabel = "DOCS";
            public const string PackageGroupLabel = "PKG";
            public const string ProjectGroupLabel = "PROJ";

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
                RepositoryLayoutGroup.Repo => RepoGroupLabel,
                RepositoryLayoutGroup.Docs => DocsGroupLabel,
                RepositoryLayoutGroup.Package => PackageGroupLabel,
                RepositoryLayoutGroup.Project => ProjectGroupLabel,
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
            Rect textRect = rowRect;
            Rect tagRect = default;
            bool hasTag = !string.IsNullOrEmpty(entry.Node.GroupLabel);
            if (hasTag) {
                tagRect = new Rect(
                    rowRect.xMax - TagColumnWidth - TagRightMargin,
                    rowRect.y + 1f,
                    TagColumnWidth,
                    Mathf.Max(0f, rowRect.height - 2f));
                textRect.xMax = Mathf.Max(textRect.xMin, tagRect.xMin - TagSpacing);
            }

            _labelContent.text = entry.DisplayText;
            _labelContent.tooltip = entry.DisplayText;
            GUI.Label(textRect, _labelContent, _treeRowStyle);

            if (!hasTag) {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = entry.Node.GroupColor;
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

        private sealed class PreviewSnapshot {
            private readonly Dictionary<string, RepositoryLayoutNode> _nodesByPath;

            public PreviewSnapshot(RepositoryLayoutNode root, List<RepositoryLayoutEntry> entries) {
                Root = root;
                Entries = entries;
                _nodesByPath = new Dictionary<string, RepositoryLayoutNode>(StringComparer.Ordinal);
                IndexNodes(root, _nodesByPath);
            }

            public RepositoryLayoutNode Root { get; }

            public IReadOnlyList<RepositoryLayoutEntry> Entries { get; }

            public RepositoryLayoutNode FindNodeByPath(string relativePath) {
                return !string.IsNullOrWhiteSpace(relativePath) &&
                       _nodesByPath.TryGetValue(relativePath, out RepositoryLayoutNode node)
                    ? node
                    : null;
            }

            private static void IndexNodes(
                RepositoryLayoutNode node,
                Dictionary<string, RepositoryLayoutNode> nodesByPath) {
                if (node == null || string.IsNullOrWhiteSpace(node.RelativePath)) {
                    return;
                }

                nodesByPath[node.RelativePath] = node;
                foreach (RepositoryLayoutNode child in node.Children) {
                    IndexNodes(child, nodesByPath);
                }
            }
        }

        private readonly struct PreviewSnapshotKey : IEquatable<PreviewSnapshotKey> {
            private PreviewSnapshotKey(string signature) {
                Signature = signature ?? string.Empty;
            }

            private string Signature { get; }

            public static PreviewSnapshotKey Create(RepositoryLayoutPreviewData data) {
                StringBuilder signature = new();
                AppendValue(signature, data.RootDirectoryName);
                AppendValue(signature, data.PackageName);
                AppendValue(signature, data.AssemblyName);
                AppendValue(signature, data.CompanionProjectName);
                AppendValue(signature, data.IncludeDocsFolder);
                AppendValue(signature, data.IncludeSamplesFolder);
                AppendValue(signature, data.IncludeEditorFolder);
                AppendValue(signature, data.IncludeTestsFolder);
                AppendValue(signature, data.IncludeRepositoryGitIgnore);
                AppendValue(signature, data.RepositoryGitIgnoreTemplate);
                AppendValue(signature, data.IncludePackagesLockFile);
                AppendContextValues(signature, data.Context);
                return new PreviewSnapshotKey(signature.ToString());
            }

            public bool Equals(PreviewSnapshotKey other) {
                return string.Equals(Signature, other.Signature, StringComparison.Ordinal);
            }

            public override bool Equals(object obj) {
                return obj is PreviewSnapshotKey other && Equals(other);
            }

            public override int GetHashCode() {
                return StringComparer.Ordinal.GetHashCode(Signature);
            }

            private static void AppendContextValues(StringBuilder signature, PackageContext context) {
                if (context == null) {
                    return;
                }

                AppendValue(signature, context.Project?.CompanyName);
                AppendValue(signature, context.Project?.ProductName);
                AppendValue(signature, context.Project?.Version);
                AppendValue(signature, context.Project?.TargetLocation);

                AppendValue(signature, context.Package?.PackageName);
                AppendValue(signature, context.Package?.PackageDisplayName);
                AppendValue(signature, context.Package?.AssemblyName);
                AppendValue(signature, context.Package?.NamespaceName);
                AppendValue(signature, context.Package?.Description);
                AppendValue(signature, context.Package?.CompanyName);
                AppendValue(signature, context.Package?.IncludeAuthor ?? false);
                AppendValue(signature, context.Package?.AuthorUrl);
                AppendValue(signature, context.Package?.AuthorEmail);
                AppendValue(signature, context.Package?.DocumentationUrl);
                AppendValue(signature, context.Package?.IncludeMinimumUnityVersion ?? false);
                AppendValue(signature, context.Package?.MinimumUnityMajor);
                AppendValue(signature, context.Package?.MinimumUnityMinor);
                AppendValue(signature, context.Package?.MinimumUnityRelease);
                AppendValue(signature, context.Package?.CreateDocsFolder ?? false);
                AppendValue(signature, context.Package?.CreateSamplesFolder ?? false);
                AppendValue(signature, context.Package?.CreateEditorFolder ?? false);
                AppendValue(signature, context.Package?.CreateTestsFolder ?? false);
                AppendDependencies(signature, context.Package?.Dependencies);

                AppendValue(signature, context.Repo?.CopyrightHolder);
                AppendValue(signature, (int?)context.Repo?.LicenseType ?? 0);
            }

            private static void AppendDependencies(StringBuilder signature, Doji.PackageAuthoring.Editor.Wizards.Models.PackageDependencyList dependencies) {
                if (dependencies?.Items == null) {
                    AppendValue(signature, 0);
                    return;
                }

                AppendValue(signature, dependencies.Items.Count);
                foreach (Doji.PackageAuthoring.Editor.Wizards.Models.PackageDependencyEntry dependency in dependencies.Items) {
                    AppendValue(signature, dependency?.PackageName);
                    AppendValue(signature, dependency?.Version);
                }
            }

            private static void AppendValue(StringBuilder signature, string value) {
                signature.Append(value ?? string.Empty);
                signature.Append('\u001F');
            }

            private static void AppendValue(StringBuilder signature, bool value) {
                signature.Append(value ? '1' : '0');
                signature.Append('\u001F');
            }

            private static void AppendValue(StringBuilder signature, int value) {
                signature.Append(value);
                signature.Append('\u001F');
            }
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
