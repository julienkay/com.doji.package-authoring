using Doji.PackageAuthoring.Editor.Wizards.Models;
using Doji.PackageAuthoring.Editor.Wizards.Presets;

namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Builds repository license text from the selected license preset.
    /// </summary>
    public static class LicenseTemplate {
        public static string GetLicense(PackageContext ctx) {
            switch (ctx.Repo.LicenseType) {
                case LicenseType.None:
                    return string.Empty;
                case LicenseType.Custom:
                    return GetCustomLicense(ctx);
                case LicenseType.Apache:
                    return GetApacheLicense(ctx);
                case LicenseType.Bsd:
                    return GetBsdLicense(ctx);
                case LicenseType.Mit:
                default:
                    return GetMitLicense(ctx);
            }
        }

        private static int CurrentYear => System.DateTime.Now.Year;

        private static string GetMitLicense(PackageContext ctx) {
            string copyrightHolder = GetCopyrightHolder(ctx);

            return $@"MIT License

Copyright (c) {CurrentYear} {copyrightHolder}

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
";
        }

        private static string GetCustomLicense(PackageContext ctx) {
            return CustomLicenseTemplateSettings.Instance.GetResolvedContent(ctx);
        }

        private static string GetApacheLicense(PackageContext ctx) {
            string copyrightHolder = GetCopyrightHolder(ctx);

            return $@"Apache License
Version 2.0, January 2004
https://www.apache.org/licenses/

Copyright {CurrentYear} {copyrightHolder}

Licensed under the Apache License, Version 2.0 (the ""License"");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an ""AS IS"" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
";
        }

        private static string GetBsdLicense(PackageContext ctx) {
            string copyrightHolder = GetCopyrightHolder(ctx);

            return $@"BSD 3-Clause License

Copyright (c) {CurrentYear}, {copyrightHolder}
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS IS""
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
";
        }

        private static string GetCopyrightHolder(PackageContext ctx) {
            return string.IsNullOrWhiteSpace(ctx.Repo.CopyrightHolder)
                ? "Your Name"
                : ctx.Repo.CopyrightHolder;
        }
    }
}
