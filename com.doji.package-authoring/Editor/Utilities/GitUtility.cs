using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace Doji.PackageAuthoring.Utilities {
    /// <summary>
    /// Runs the git commands used to initialize generated repositories.
    /// </summary>
    internal static class GitUtility {
        /// <summary>
        /// Initializes a git repository and optionally assigns an <c>origin</c> remote before the first commit.
        /// </summary>
        /// <param name="workingDirectory">Root directory of the generated repository.</param>
        /// <param name="repositoryUrl">Remote URL assigned to <c>origin</c> when provided.</param>
        public static void InitializeRepository(string workingDirectory, string repositoryUrl = null) {
            if (Directory.Exists(Path.Combine(workingDirectory, ".git"))) {
                Debug.Log(
                    $"git repository already exists in '{Path.GetFullPath(workingDirectory)}'. Skipping git initialization step. (This message is harmless)");
                return;
            }

            RunGitCommand("init", workingDirectory);
            if (!string.IsNullOrWhiteSpace(repositoryUrl)) {
                RunGitCommand($"remote add origin \"{repositoryUrl}\"", workingDirectory);
            }

            CommitInitialChanges(workingDirectory);
        }

        private static void RunGitCommand(string arguments, string workingDirectory, bool suppressStdErr = false) {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };

            using (Process process = new Process()) {
                process.StartInfo = startInfo;
                process.Start();

                // Capture output and errors (if any)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(output)) {
                    Debug.Log(output);
                }

                if (!suppressStdErr && !string.IsNullOrEmpty(error)) {
                    Debug.LogError("Error: " + error);
                }
            }
        }

        public static void CommitInitialChanges(string workingDirectory) {
            // Stage all files
            RunGitCommand("add .", workingDirectory, suppressStdErr: true);

            // Commit with the message "initial commit"
            RunGitCommand("commit -m \"initial commit\"", workingDirectory);
        }
    }
}
