using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Models {
    /// <summary>
    /// Supported generated license templates for new packages.
    /// </summary>
    public enum LicenseType {
        /// <summary>
        /// Does not generate a repository license file.
        /// </summary>
        None,

        /// <summary>
        /// Generates a license file from the project-scoped custom license template.
        /// </summary>
        Custom,

        /// <summary>
        /// Generates a license file for the SPDX <c>MIT</c> identifier.
        /// </summary>
        [InspectorName("MIT")]
        Mit,

        /// <summary>
        /// Generates a license file for the SPDX <c>Apache-2.0</c> identifier.
        /// </summary>
        [InspectorName("Apache-2.0")]
        Apache,

        /// <summary>
        /// Generates a license file for the SPDX <c>BSD-3-Clause</c> identifier.
        /// </summary>
        [InspectorName("BSD-3-Clause")]
        Bsd
    }
}
