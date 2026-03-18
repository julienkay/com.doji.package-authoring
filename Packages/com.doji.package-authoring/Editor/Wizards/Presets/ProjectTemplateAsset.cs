using Doji.PackageAuthoring.Editor.Wizards.Models;
using Doji.PackageAuthoring.Editor.Wizards.Templates;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Base asset for project-scoped editable templates that resolve shared package authoring tokens.
    /// </summary>
    internal abstract class ProjectTemplateAsset : ScriptableObject {
        /// <summary>
        /// Built-in fallback content used when the project has not customized the template yet.
        /// </summary>
        protected abstract string DefaultContent { get; }

        /// <summary>
        /// Raw template content edited in Project Settings.
        /// </summary>
        [field: SerializeField]
        public string Content { get; set; }

        /// <summary>
        /// Returns the template content with shared tokens resolved against the current scaffold context.
        /// </summary>
        public virtual string GetResolvedContent(PackageContext ctx) {
            return TemplateTokenResolver.Resolve(Content, ctx);
        }

        /// <summary>
        /// Returns the template content with shared tokens resolved from the provided settings objects.
        /// </summary>
        public virtual string GetResolvedContent(
            ProjectSettings project,
            PackageSettings package = null,
            RepoSettings repo = null) {
            return TemplateTokenResolver.Resolve(Content, project, package, repo);
        }

        /// <summary>
        /// Ensures the template content is initialized to the built-in default when absent.
        /// </summary>
        protected void EnsureDefaultContent() {
            if (Content == null) {
                Content = DefaultContent;
            }
        }
    }
}
