using NUnit.Framework;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class TestUtils
    {
        private const float EPSILON = 0.000001f;
        public static bool Approximately(float a, float b, float threshold = EPSILON)
        {
            if (threshold > 0f)
            {
                return Mathf.Abs(a - b) <= threshold;
            }
            else
            {
                return Mathf.Approximately(a, b);
            }
        }
        public static bool Approximately(Vector2 one, Vector2 two, float threshold = EPSILON)
        {
            return Approximately(one.x, two.x, threshold)
                && Approximately(one.y, two.y, threshold);
        }
        public static bool Approximately(Vector3 one, Vector3 two, float threshold = EPSILON)
        {
            return Approximately(one.x, two.x, threshold)
                && Approximately(one.y, two.y, threshold)
                && Approximately(one.z, two.z, threshold);
        }
        public static bool Approximately(Bounds one, Bounds two, float threshold = EPSILON)
        {
            return Approximately(one.center, two.center, threshold)
                && Approximately(one.size, two.size, threshold);
        }
        public static string RunGitCommand(string projectPath, string gitCommand)
        {
            // Set up our processInfo to run the git command and log to output and errorOutput.
            var processInfo = new System.Diagnostics.ProcessStartInfo("git", @gitCommand)
            {
                WorkingDirectory = projectPath,
                CreateNoWindow = true,          // We want no visible pop-ups
                UseShellExecute = false,        // Allows us to redirect input, output and error streams
                RedirectStandardOutput = true,  // Allows us to read the output stream
                RedirectStandardError = true    // Allows us to read the error stream
            };

            // Set up the Process
            var process = new System.Diagnostics.Process
            {
                StartInfo = processInfo
            };

            try
            {
                process.Start();  // Try to start it, catching any exceptions if it fails
            }
            catch (Exception e)
            {
                // For now just assume its failed cause it can't find git.
                Debug.LogError("Git is not set-up correctly, required to be on PATH, and to be a git project.");
                throw e;
            }

            // Strings that will catch the output from our process.
            // Read the results back from the process so we can get the output and check for errors
            string output = process.StandardOutput.ReadToEnd();
            string errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();  // Make sure we wait till the process has fully finished.
            process.Close();        // Close the process ensuring it frees it resources.

            // Log any errors.
            if (errorOutput != "")
            {
                Debug.LogError("Git Error: " + errorOutput);
            }

            return output;  // Return the output from git.
        }

        public static GameObject LoadPrefab(string path)
        {

            FileInfo file = new FileInfo(path);
            FileAssert.Exists(file);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.NotNull(obj, $"Prefab '{path}' is empty?");
            return obj;
        }
    }
}