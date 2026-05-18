using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Doji.PackageAuthoring.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Doji.PackageAuthoring.Tests {
    /// <summary>
    /// Verifies git process execution fails fast instead of blocking the Unity editor.
    /// </summary>
    internal sealed class GitUtilityTests : PackageAuthoringApiTestBase {
        [Test]
        public void GitCommandRunner_DrainsStdErrWithoutBlocking() {
            CommandScript script = CreateCommandScript(
#if UNITY_EDITOR_WIN
                "@echo off\r\n" +
                "setlocal EnableDelayedExpansion\r\n" +
                $"set \"chunk={new string('x', 1024)}\"\r\n" +
                "for /L %%i in (1,1,512) do echo !chunk! 1>&2\r\n" +
                "exit /b 0\r\n"
#else
                "#!/bin/sh\n" +
                $"chunk='{new string('x', 1024)}'\n" +
                "i=0\n" +
                "while [ \"$i\" -lt 512 ]; do printf '%s\\n' \"$chunk\" >&2; i=$((i + 1)); done\n" +
                "exit 0\n"
#endif
            );

            GitCommandRunner runner = new(script.ExecutablePath, 5000);

            Assert.DoesNotThrow(() => runner.Run(script.Arguments, TempRoot, true));
        }

        [Test]
        public void GitCommandRunner_ReturnsExitCodeAndOutput() {
            CommandScript script = CreateCommandScript(
#if UNITY_EDITOR_WIN
                "@echo off\r\n" +
                "echo command output\r\n" +
                "exit /b 7\r\n"
#else
                "#!/bin/sh\n" +
                "echo command output\n" +
                "exit 7\n"
#endif
            );

            GitCommandRunner runner = new(script.ExecutablePath, 120000);

            GitCommandResult result = runner.Run(
                script.Arguments,
                TempRoot,
                true);

            Assert.That(result.ExitCode, Is.EqualTo(7));
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Output, Does.Contain("command output"));
        }

        [Test]
        public void GitCommandRunner_ThrowsWhenProcessExceedsTimeout() {
            CommandScript script = CreateCommandScript(
#if UNITY_EDITOR_WIN
                "@echo off\r\n" +
                ":loop\r\n" +
                "goto loop\r\n"
#else
                "#!/bin/sh\n" +
                "while :; do :; done\n"
#endif
            );

            GitCommandRunner runner = new(script.ExecutablePath, 250);
            LogAssert.Expect(LogType.Error, new Regex("git command timed out after 250 ms"));

            Stopwatch stopwatch = Stopwatch.StartNew();
            Assert.Throws<TimeoutException>(() => runner.Run(script.Arguments, TempRoot));

            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(4000));
        }

        private static string GetScriptExtension() {
#if UNITY_EDITOR_WIN
            return ".cmd";
#else
            return ".sh";
#endif
        }

        private static string GetScriptExecutablePath() {
#if UNITY_EDITOR_WIN
            return "cmd.exe";
#else
            return "/bin/sh";
#endif
        }

        private static string GetScriptArguments(string scriptPath) {
#if UNITY_EDITOR_WIN
            return $"/c \"{scriptPath}\"";
#else
            return $"\"{scriptPath}\"";
#endif
        }

        private CommandScript CreateCommandScript(string content) {
            Directory.CreateDirectory(TempRoot);
            string scriptPath = Path.Combine(TempRoot, $"{Guid.NewGuid():N}{GetScriptExtension()}");
            File.WriteAllText(scriptPath, content);
            return new CommandScript(
                GetScriptExecutablePath(),
                GetScriptArguments(scriptPath));
        }

        private readonly struct CommandScript {
            public CommandScript(string executablePath, string arguments) {
                ExecutablePath = executablePath;
                Arguments = arguments;
            }

            public string ExecutablePath { get; }

            public string Arguments { get; }
        }
    }
}
