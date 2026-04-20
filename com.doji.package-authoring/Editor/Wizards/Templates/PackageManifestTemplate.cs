using System.Linq;
using Doji.PackageAuthoring.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Doji.PackageAuthoring.Utilities.JsonBuilder;

namespace Doji.PackageAuthoring.Wizards.Templates {
    /// <summary>
    /// Builds the generated package manifest.
    /// </summary>
    internal static class PackageManifestTemplate {
        public static string GetPackageManifest(PackageContext ctx) {
            string documentationUrl = TemplateTokenResolver.Resolve(ctx.Package.DocumentationUrl, ctx);

            JObject json = Obj(
                Prop("name", ctx.Package.PackageName),
                Prop("version", ctx.Project.Version),
                Prop("displayName", ctx.Package.PackageDisplayName),
                Prop("description", ctx.Package.Description),
                Prop("author", ctx.Package.IncludeAuthor ? GetAuthorMetadata(ctx) : null),
                PropIf(
                    ctx.Package.IncludeMinimumUnityVersion,
                    "unity",
                    $"{ctx.Package.MinimumUnityMajor}.{ctx.Package.MinimumUnityMinor}"),
                PropIf(
                    ctx.Package.IncludeMinimumUnityVersion &&
                    !string.IsNullOrWhiteSpace(ctx.Package.MinimumUnityRelease),
                    "unityRelease",
                    ctx.Package.MinimumUnityRelease),
                PropIf(!string.IsNullOrWhiteSpace(documentationUrl), "documentationUrl", documentationUrl),
                PropIf(ctx.Package.CreateSamplesFolder, "samples", GetSamples(ctx)),
                PropIf(ctx.Package.Dependencies is { Count: > 0 }, "dependencies", GetDependencies(ctx))
            );

            return json.ToString(Formatting.Indented);
        }

        private static JObject GetAuthorMetadata(PackageContext ctx) {
            string authorUrl = TemplateTokenResolver.Resolve(ctx.Package.AuthorUrl, ctx);
            JObject author = Obj(
                Prop("name", ctx.Package.CompanyName),
                Prop("url", authorUrl),
                Prop("email", ctx.Package.AuthorEmail)
            );

            return author.HasValues ? author : null;
        }

        private static JArray GetSamples(PackageContext ctx) {
            return Arr(
                Obj(
                    Prop("displayName", "Shared Sample Assets (Required)"),
                    Prop("description",
                        "Shared resources for samples. All other samples depend on this being imported."),
                    Prop("path", "Samples~/00-SharedSampleAssets")
                ),
                Obj(
                    Prop("displayName", "Basic Sample"),
                    Prop("description", $"Basic example on how to use {ctx.Package.PackageDisplayName}."),
                    Prop("path", "Samples~/01-BasicSample")
                )
            );
        }

        private static JObject GetDependencies(PackageContext ctx) {
            JObject obj = new JObject();

            foreach (PackageDependencyEntry dep in (ctx.Package.Dependencies?.Items ??
                                                    Enumerable.Empty<PackageDependencyEntry>())
                     .OrderBy(d => d.PackageName, System.StringComparer.Ordinal)) {
                obj[dep.PackageName] = dep.Version;
            }

            return obj;
        }
    }
}
