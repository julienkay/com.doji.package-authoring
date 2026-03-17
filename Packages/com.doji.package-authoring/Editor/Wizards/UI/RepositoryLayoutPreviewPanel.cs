using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Draws the repository layout preview panel used by package-creation workflows.
    /// </summary>
    internal sealed class RepositoryLayoutPreviewPanel {
        private const float MinWidth = 360f;
        private const float MaxWidth = 760f;
        private const float MinHeight = 220f;
        private const float MaxHeight = 680f;

        private GUIStyle _previewStyle;

        /// <summary>
        /// Draws the preview section and updates its scroll state.
        /// </summary>
        public void Draw(float windowWidth, RepositoryLayoutPreviewData data, ref Vector2 scrollPosition) {
            string previewText = BuildPreviewText(data);
            float previewWidth = Mathf.Clamp(windowWidth * 0.4f, MinWidth, MaxWidth);
            EditorGUILayout.BeginVertical(GUILayout.Width(previewWidth), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Repository Layout Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(3f);
            DrawContent(previewText, ref scrollPosition);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private static string BuildPreviewText(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode root = new RepositoryLayoutNode(data.RootDirectoryName);

            if (data.IncludeDocsFolder) {
                root.Children.Add(BuildDocsNode());
            }

            root.Children.Add(BuildPackageNode(data));
            root.Children.Add(new RepositoryLayoutNode("LICENSE"));
            root.Children.Add(new RepositoryLayoutNode("README.md"));
            root.Children.Add(BuildCompanionProjectNode(data));

            StringBuilder builder = new StringBuilder();
            AppendTree(builder, root, string.Empty, isLast: true, isRoot: true);
            return builder.ToString();
        }

        private void DrawContent(string previewText, ref Vector2 scrollPosition) {
            _previewStyle ??= new GUIStyle(EditorStyles.textArea) {
                wordWrap = false,
                richText = false
            };

            float contentHeight = _previewStyle.CalcHeight(
                new GUIContent(previewText),
                Mathf.Max(1f, EditorGUIUtility.currentViewWidth));
            float previewHeight = Mathf.Clamp(contentHeight + 10f, MinHeight, MaxHeight);

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(
                       scrollPosition,
                       GUILayout.ExpandWidth(true),
                       GUILayout.Height(previewHeight))) {
                scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.SelectableLabel(
                    previewText,
                    _previewStyle,
                    GUILayout.MinHeight(contentHeight),
                    GUILayout.ExpandWidth(true));
            }
        }

        private static RepositoryLayoutNode BuildDocsNode() {
            RepositoryLayoutNode docs = new RepositoryLayoutNode("docs");
            docs.Children.Add(new RepositoryLayoutNode(".gitignore"));
            docs.Children.Add(new RepositoryLayoutNode("docfx.json"));
            docs.Children.Add(new RepositoryLayoutNode("docfx-pdf.json"));
            docs.Children.Add(new RepositoryLayoutNode("filterConfig.yml"));
            docs.Children.Add(new RepositoryLayoutNode("index.md"));

            RepositoryLayoutNode api = new RepositoryLayoutNode("api");
            api.Children.Add(new RepositoryLayoutNode(".gitignore"));
            api.Children.Add(new RepositoryLayoutNode("index.md"));
            docs.Children.Add(api);

            RepositoryLayoutNode images = new RepositoryLayoutNode("images");
            images.Children.Add(new RepositoryLayoutNode("doji.png"));
            images.Children.Add(new RepositoryLayoutNode("favicon.ico"));
            docs.Children.Add(images);

            RepositoryLayoutNode manual = new RepositoryLayoutNode("manual");
            manual.Children.Add(new RepositoryLayoutNode("toc.yml"));
            docs.Children.Add(manual);

            RepositoryLayoutNode pdf = new RepositoryLayoutNode("pdf");
            pdf.Children.Add(new RepositoryLayoutNode("toc.yml"));
            docs.Children.Add(pdf);

            docs.Children.Add(new RepositoryLayoutNode("toc.yml"));
            return docs;
        }

        private static RepositoryLayoutNode BuildPackageNode(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode package = new RepositoryLayoutNode(data.PackageName);
            package.Children.Add(new RepositoryLayoutNode("CHANGELOG.md"));

            if (data.IncludeEditorFolder) {
                RepositoryLayoutNode editor = new RepositoryLayoutNode("Editor");
                editor.Children.Add(new RepositoryLayoutNode($"{data.AssemblyName}.Editor.asmdef"));
                package.Children.Add(editor);
            }

            package.Children.Add(new RepositoryLayoutNode("README.md"));

            RepositoryLayoutNode runtime = new RepositoryLayoutNode("Runtime");
            runtime.Children.Add(new RepositoryLayoutNode($"{data.AssemblyName}.asmdef"));
            runtime.Children.Add(new RepositoryLayoutNode("AssemblyInfo.cs"));
            package.Children.Add(runtime);

            if (data.IncludeSamplesFolder) {
                RepositoryLayoutNode samples = new RepositoryLayoutNode("Samples~");
                samples.Children.Add(new RepositoryLayoutNode($"{data.AssemblyName}.asmdef"));
                samples.Children.Add(new RepositoryLayoutNode("00-SharedSampleAssets"));

                RepositoryLayoutNode basicSample = new RepositoryLayoutNode("01-BasicSample");
                basicSample.Children.Add(new RepositoryLayoutNode("BasicSample.cs"));
                samples.Children.Add(basicSample);

                package.Children.Add(samples);
            }

            if (data.IncludeTestsFolder) {
                package.Children.Add(new RepositoryLayoutNode("Tests"));
            }

            package.Children.Add(new RepositoryLayoutNode("package.json"));
            return package;
        }

        private static RepositoryLayoutNode BuildCompanionProjectNode(RepositoryLayoutPreviewData data) {
            RepositoryLayoutNode projects = new RepositoryLayoutNode("projects");
            RepositoryLayoutNode project = new RepositoryLayoutNode(data.CompanionProjectName);
            if (data.IncludeRepositoryGitIgnore) {
                project.Children.Add(new RepositoryLayoutNode(".gitignore"));
            }

            project.Children.Add(new RepositoryLayoutNode("Assets"));

            RepositoryLayoutNode packages = new RepositoryLayoutNode("Packages");
            packages.Children.Add(new RepositoryLayoutNode("manifest.json"));
            if (data.IncludePackagesLockFile) {
                packages.Children.Add(new RepositoryLayoutNode("packages-lock.json"));
            }

            project.Children.Add(packages);
            project.Children.Add(new RepositoryLayoutNode("ProjectSettings"));
            projects.Children.Add(project);
            return projects;
        }

        private static void AppendTree(StringBuilder builder, RepositoryLayoutNode node, string indent, bool isLast,
            bool isRoot = false) {
            if (isRoot) {
                builder.AppendLine(node.Name);
            }
            else {
                builder.Append(indent);
                builder.Append(isLast ? "└── " : "├── ");
                builder.AppendLine(node.Name);
            }

            string childIndent = isRoot ? string.Empty : indent + (isLast ? "    " : "│   ");
            for (int i = 0; i < node.Children.Count; i++) {
                AppendTree(builder, node.Children[i], childIndent, i == node.Children.Count - 1);
            }
        }

        /// <summary>
        /// Represents one directory or file entry in the rendered repository tree.
        /// </summary>
        private sealed class RepositoryLayoutNode {
            public RepositoryLayoutNode(string name) {
                Name = string.IsNullOrWhiteSpace(name) ? "(unnamed)" : name;
            }

            public string Name { get; }

            public List<RepositoryLayoutNode> Children { get; } = new();
        }
    }
}
