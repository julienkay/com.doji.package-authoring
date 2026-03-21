namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Provides the built-in fallback content for generated documentation scaffold files.
    /// </summary>
    public static class DocfxTemplates {
        public const string DocsGitIgnoreDefaultContent = @"###############
#    folder   #
###############
/**/DROP/
/**/TEMP/
/**/packages/
/**/bin/
/**/obj/
_site";

        public const string DocsApiGitIgnoreDefaultContent = @"###############
#  temp file  #
###############
*.yml
.manifest";

        public const string DocsApiIndexDefaultContent = @"# Scripting API
This is the documentation for the Scripting APIs of this package.";

        public const string DocfxJsonDefaultContent = @"{
  ""metadata"": [
    {
      ""src"": [
        {
          ""files"": [
            ""**/*.cs""
          ],
          ""src"": ""../{{PACKAGE_NAME}}""
        }
      ],
      ""dest"": ""api"",
      ""includePrivateMembers"": false,
      ""disableGitFeatures"": false,
      ""disableDefaultFilter"": false,
      ""noRestore"": false,
      ""namespaceLayout"": ""flattened"",
      ""memberLayout"": ""samePage"",
      ""EnumSortOrder"": ""declaringOrder"",
      ""allowCompilationErrors"": true,
      ""globalNamespaceId"": ""Global"",
      ""filter"": ""filterConfig.yml""
    }
  ],
  ""build"": {
    ""content"": [
      {
        ""files"": [
          ""api/**.yml"",
          ""api/index.md""
        ]
      },
      {
        ""files"": [
          ""manual/**.md"",
          ""manual/**/toc.yml"",
          ""toc.yml"",
          ""*.md""
        ]
      }
    ],
    ""resource"": [
      {
        ""files"": [
          ""images/**""
        ]
      }
    ],
    ""output"": ""_site"",
    ""globalMetadataFiles"": [],
    ""globalMetadata"": {
      ""_disableContribution"": true
    },
    ""fileMetadataFiles"": [],
    ""template"": [
      ""default""
    ],
    ""postProcessors"": [],
    ""keepFileLink"": false,
    ""disableGitFeatures"": false
  }
}";

        public const string DocfxPdfJsonDefaultContent = @"{
  ""metadata"": [
    {
      ""src"": [
        {
          ""files"": [
            ""**/*.cs""
          ],
          ""src"": ""../{{PACKAGE_NAME}}""
        }
      ],
      ""dest"": ""api"",
      ""includePrivateMembers"": false,
      ""disableGitFeatures"": false,
      ""disableDefaultFilter"": false,
      ""noRestore"": false,
      ""namespaceLayout"": ""flattened"",
      ""memberLayout"": ""samePage"",
      ""EnumSortOrder"": ""declaringOrder"",
      ""allowCompilationErrors"": true,
      ""globalNamespaceId"": ""Global"",
      ""filter"": ""filterConfig.yml""
    }
  ],
  ""build"": {
    ""content"": [
      {
        ""files"": [
          ""api/**.yml"",
          ""api/index.md""
        ]
      },
      {
        ""files"": [
          ""manual/**.md"",
          ""manual/**/toc.yml"",
          ""toc.yml"",
          ""*.md""
        ]
      },
      {
        ""files"": [
          ""pdf/toc.yml""
        ]
      }
    ],
    ""resource"": [
      {
        ""files"": [
          ""images/**""
        ]
      }
    ],
    ""output"": ""_site"",
    ""globalMetadataFiles"": [],
    ""globalMetadata"": {
      ""_disableContribution"": true,
      ""pdf"": true,
      ""pdfTocPage"": true,
      ""pdfFileName"": ""{{PACKAGE_NAME}}.pdf""
    },
    ""fileMetadataFiles"": [],
    ""template"": [
      ""default"",
      ""modern""
    ],
    ""postProcessors"": [],
    ""keepFileLink"": false,
    ""disableGitFeatures"": false
  }
}";

        public const string FilterConfigDefaultContent = @"apiRules:
- include: # The namespaces to generate
    uidRegex: ^{{NAMESPACE_NAME_REGEX}}
    type: Namespace
- include:
    uidRegex: ^Global
    type: Namespace
- exclude:
    uidRegex: ^{{NAMESPACE_NAME_REGEX}}\.Editor
- exclude:
    uidRegex: ^{{NAMESPACE_NAME_REGEX}}\.Samples";

        public const string IndexDefaultContent = @"# {{PROJECT_NAME}}

{{PACKAGE_DESCRIPTION}}";

        public const string RootTocDefaultContent = @"- name: Manual
  href: manual/
- name: Scripting API
  href: api/
  homepage: api/index.md
";

        public const string ManualTocDefaultContent = @"- name: {{PROJECT_NAME}}
  href: ../index.md
";

        public const string PdfTocDefaultContent = @"order: 200
items:
- name: Manual
  href: ../manual/toc.yml
- name: Scripting API
  href: ../api/toc.yml";
    }
}
