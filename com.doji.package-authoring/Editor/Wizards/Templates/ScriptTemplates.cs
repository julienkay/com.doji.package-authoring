namespace Doji.PackageAuthoring.Wizards.Templates {
    /// <summary>
    /// Builds starter scripts included in generated samples.
    /// </summary>
    internal static class ScriptTemplates {
        public static string GetSampleScript(PackageContext ctx) {
            return $@"using UnityEngine;

namespace {ctx.Package.NamespaceName}.Samples {{

    public class {ctx.Project.ProductName.Replace(" ", string.Empty)}_BasicSample : MonoBehaviour {{

    }}
}}";
        }
    }
}
