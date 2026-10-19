using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Debug = UnityEngine.Debug;

namespace Doji.PackageAuthoring.Utilities {
    /// <summary>
    /// Runs the git commands used to initialize generated repositories.
    /// </summary>
    internal static class GitUtility {
        private const int DefaultCommandTimeoutMilliseconds = 120000;
        private const string InitialCommitMessage = "initial commit";
        private static readonly GitCommandRunner DefaultRunner = new("git", DefaultCommandTimeoutMilliseconds);

        /// <summary>
        /// Initializes a git repository and optionally assigns an <c>origin</c> remote before the first commit.
        /// </summary>
        /// <param name="workingDirectory">Root directory of the generated repository.</param>
        /// <param name="repositoryUrl">Remote URL assigned to <c>origin</c> when provided.</param>
        public static void InitializeRepository(string workingDirectory, string repositoryUrl = null) {
            InitializeRepository(workingDirectory, repositoryUrl, DefaultRunner);
        }

        internal static void InitializeRepository(
            string workingDirectory,
            string repositoryUrl,
            GitCommandRunner runner) {
            if (Directory.Exists(Path.Combine(workingDirectory, ".git"))) {
                Debug.Log(
                    $"git repository already exists in '{Path.GetFullPath(workingDirectory)}'. Skipping git initialization step. (This message is harmless)");
                return;
            }

            try {
                if (!runner.Run("init", workingDirectory).Succeeded) {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(repositoryUrl)) {
                    GitCommandResult remoteResult = runner.Run(
                        $"remote add origin {QuoteArgument(repositoryUrl)}",
                        workingDirectory,
                        displayArguments: "remote add origin <redacted-url>");
                    if (!remoteResult.Succeeded) {
                        return;
                    }
                }

                CommitInitialChanges(workingDirectory, runner);
            }
            catch (Exception exception) when (exception is TimeoutException or IOException or InvalidOperationException) {
                Debug.LogError(
                    $"Git initialization failed for '{Path.GetFullPath(workingDirectory)}'. " +
                    "Generated files remain on disk. " +
                    exception.Message);
            }
        }

        public static void CommitInitialChanges(string workingDirectory) {
            CommitInitialChanges(workingDirectory, DefaultRunner);
        }

        internal static void CommitInitialChanges(string workingDirectory, GitCommandRunner runner) {
            // Stage all files
            if (!runner.Run("add .", workingDirectory, true).Succeeded) {
                return;
            }

            // Commit with the message "initial commit"
            runner.Run(
                $"-c commit.gpgsign=false commit --no-verify -m {QuoteArgument(InitialCommitMessage)}",
                workingDirectory);
        }

        private static string QuoteArgument(string value) {
            if (string.IsNullOrEmpty(value)) {
                return "\"\"";
            }

            StringBuilder builder = new("\"");
            int pendingBackslashes = 0;
            foreach (char character in value) {
                if (character == '\\') {
                    pendingBackslashes++;
                    continue;
                }

                if (character == '"') {
                    builder.Append('\\', pendingBackslashes * 2 + 1);
                    builder.Append('"');
                    pendingBackslashes = 0;
                    continue;
                }

                builder.Append('\\', pendingBackslashes);
                pendingBackslashes = 0;
                builder.Append(character);
            }

            builder.Append('\\', pendingBackslashes * 2);
            builder.Append('"');
            return builder.ToString();
        }
    }

    /// <summary>
    /// Executes git as a bounded child process so editor tooling can fail fast instead of blocking Unity.
    /// </summary>
    internal sealed class GitCommandRunner {
        private const int KillWaitMilliseconds = 5000;
        private readonly string _executablePath;
        private readonly int _timeoutMilliseconds;

        public GitCommandRunner(string executablePath, int timeoutMilliseconds) {
            _executablePath = string.IsNullOrWhiteSpace(executablePath)
                ? "git"
                : executablePath;
            _timeoutMilliseconds = timeoutMilliseconds > 0
                ? timeoutMilliseconds
                : 120000;
        }

        public GitCommandResult Run(
            string arguments,
            string workingDirectory,
            bool suppressStdErr = false,
            string displayArguments = null) {
            string displayedArguments = displayArguments ?? arguments;
            ProcessStartInfo startInfo = new() {
                FileName = _executablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };

            ConfigureNonInteractiveGit(startInfo);
            StringBuilder outputBuilder = new();
            StringBuilder errorBuilder = new();
            Stopwatch stopwatch = Stopwatch.StartNew();

            Debug.Log($"Running git command: git {displayedArguments}");

            using (Process process = new()) {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (_, eventArgs) => AppendLine(outputBuilder, eventArgs.Data);
                process.ErrorDataReceived += (_, eventArgs) => AppendLine(errorBuilder, eventArgs.Data);
                process.Start();
                process.StandardInput.Close();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(_timeoutMilliseconds)) {
                    string message =
                        $"git command timed out after {_timeoutMilliseconds} ms: git {displayedArguments}";
                    KillProcess(process);
                    Debug.LogError(message);
                    throw new TimeoutException(message);
                }

                process.WaitForExit();
                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();
                Debug.Log(
                    $"git command completed in {stopwatch.ElapsedMilliseconds} ms " +
                    $"with exit code {process.ExitCode}: git {displayedArguments}");

                if (!string.IsNullOrEmpty(output)) {
                    Debug.Log(output);
                }

                if (!string.IsNullOrEmpty(error) && (!suppressStdErr || process.ExitCode != 0)) {
                    Debug.LogError("Error: " + error);
                }

                if (!suppressStdErr && process.ExitCode != 0 && string.IsNullOrEmpty(error)) {
                    Debug.LogError($"git exited with code {process.ExitCode}: git {displayedArguments}");
                }

                return new GitCommandResult(process.ExitCode, output, error);
            }
        }

        private static void ConfigureNonInteractiveGit(ProcessStartInfo startInfo) {
            startInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            startInfo.EnvironmentVariables["GCM_INTERACTIVE"] = "Never";
            startInfo.EnvironmentVariables["GIT_ASKPASS"] = GetAskPassExecutable();
            startInfo.EnvironmentVariables["SSH_ASKPASS"] = GetAskPassExecutable();
            startInfo.EnvironmentVariables["GIT_SSH_COMMAND"] = "ssh -o BatchMode=yes";
            startInfo.EnvironmentVariables["GIT_EDITOR"] = "true";
            startInfo.EnvironmentVariables["GIT_SEQUENCE_EDITOR"] = "true";
        }

        private static string GetAskPassExecutable() {
#if UNITY_EDITOR_WIN
            return "echo";
#else
            return "/bin/echo";
#endif
        }

        private static void AppendLine(StringBuilder builder, string line) {
            if (line == null) {
                return;
            }

            builder.AppendLine(line);
        }

        private static void KillProcess(Process process) {
            try {
                if (!process.HasExited) {
                    if (!TryKillProcessTree(process)) {
                        process.Kill();
                    }
                }

                process.WaitForExit(KillWaitMilliseconds);
            }
            catch (InvalidOperationException) {
                // Process already exited between timeout detection and kill.
            }
        }

        private static bool TryKillProcessTree(Process process) {
            System.Reflection.MethodInfo killMethod = typeof(Process).GetMethod(
                nameof(Process.Kill),
                new[] { typeof(bool) });

            if (killMethod == null) {
                return false;
            }

            try {
                killMethod.Invoke(process, new object[] { true });
                return true;
            }
            catch {
                return false;
            }
        }
    }

    /// <summary>
    /// Captures the result of a bounded git command without forcing callers to parse Unity logs.
    /// </summary>
    internal readonly struct GitCommandResult {
        public GitCommandResult(int exitCode, string output, string error) {
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }

        public int ExitCode { get; }

        public string Output { get; }

        public string Error { get; }

        public bool Succeeded => ExitCode == 0;
    }
}
