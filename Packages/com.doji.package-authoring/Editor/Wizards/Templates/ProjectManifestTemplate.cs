using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Doji.PackageAuthoring.Editor.Wizards.Models;
using static Doji.PackageAuthoring.Editor.Utilities.JsonBuilder;

namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Builds the companion project's Unity manifest.
    /// </summary>
    public static class ProjectManifestTemplate {
        public static string GetProjectManifest(PackageContext ctx) {
            JObject json = Obj(
                Prop("dependencies", GetProjectDependencies(ctx)),
                PropIf(ctx.Package.CreateTestsFolder, "testables", Arr(ctx.Package.PackageName))
            );

            return json.ToString(Formatting.Indented);
        }

        private static JObject GetProjectDependencies(PackageContext ctx) {
            JObject deps = new JObject {
                [ctx.Package.PackageName] = $"file:../../../{ctx.Package.PackageName}",
                ["com.unity.ugui"] = "2.0.0",
                ["com.unity.modules.ai"] = "1.0.0",
                ["com.unity.modules.androidjni"] = "1.0.0",
                ["com.unity.modules.animation"] = "1.0.0",
                ["com.unity.modules.assetbundle"] = "1.0.0",
                ["com.unity.modules.audio"] = "1.0.0",
                ["com.unity.modules.imageconversion"] = "1.0.0",
                ["com.unity.modules.imgui"] = "1.0.0",
                ["com.unity.modules.jsonserialize"] = "1.0.0",
                ["com.unity.modules.particlesystem"] = "1.0.0",
                ["com.unity.modules.physics"] = "1.0.0",
                ["com.unity.modules.physics2d"] = "1.0.0",
                ["com.unity.modules.screencapture"] = "1.0.0",
                ["com.unity.modules.tilemap"] = "1.0.0",
                ["com.unity.modules.ui"] = "1.0.0",
                ["com.unity.modules.uielements"] = "1.0.0",
                ["com.unity.modules.unitywebrequest"] = "1.0.0",
                ["com.unity.modules.unitywebrequestassetbundle"] = "1.0.0",
                ["com.unity.modules.unitywebrequestaudio"] = "1.0.0",
                ["com.unity.modules.unitywebrequesttexture"] = "1.0.0",
                ["com.unity.modules.unitywebrequestwww"] = "1.0.0",
                ["com.unity.modules.video"] = "1.0.0",
                ["com.unity.modules.vr"] = "1.0.0",
                ["com.unity.modules.xr"] = "1.0.0"
            };

            AddPreferredEditorDependency(deps, ctx.Project.PreferredEditor);
            return new JObject(deps.Properties());
        }

        private static void AddPreferredEditorDependency(JObject deps, PreferredEditor preferredEditor) {
            switch (preferredEditor) {
                case PreferredEditor.VisualStudio:
                    deps["com.unity.ide.visualstudio"] = "2.0.27";
                    break;
                case PreferredEditor.VisualStudioCode:
                    deps["com.unity.ide.vscode"] = "1.2.5";
                    break;
                case PreferredEditor.Rider:
                    deps["com.unity.ide.rider"] = "3.0.39";
                    break;
            }
        }
    }
}
