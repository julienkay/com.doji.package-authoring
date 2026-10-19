using System;

namespace Doji.PackageAuthoring.Utilities {
    /// <summary>
    /// Centralizes stable GUID string formatting used when generated Unity assets need explicit meta files.
    /// </summary>
    internal static class GuidUtility {
        /// <summary>
        /// Produces a compact 32-character GUID string matching Unity meta file formatting.
        /// </summary>
        public static string NewMetaGuid() {
            return Guid.NewGuid().ToString("N");
        }
    }
}
