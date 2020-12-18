using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat01 : TestSuite
    {
        private const string UNITY_VERSION = "2019.4.12f1";
        private const string GIT_PATH = "./.git";
        private const string SCENE_PATH = "./Assets/Scenes/HelloWorld.unity";
        private const string SCENE_NAME = "HelloWorld";
        private const string DEBUG_MESSAGE = "Hallo Welt!";
        [Test]
        public void T02_TestUnityVersion()
        {
            Assert.AreEqual(UNITY_VERSION, Application.unityVersion);
        }
        [Test]
        public void T01_GitInitialized()
        {
            DirectoryInfo directory = new DirectoryInfo(GIT_PATH);
            Assert.IsTrue(directory.Exists, $"Directory '{directory.FullName}' not found. Did you initialize git?");
        }

        [Test]
        public void T03a_SceneExists()
        {
            FileInfo file = new FileInfo(SCENE_PATH);
            Assert.IsTrue(file.Exists, $"File '{file.FullName}' not found.");
        }

        [UnityTest]
        public IEnumerator T03b_HelloWorldLog()
        {
            List<string> messages = new List<string>();
            void listener(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Log)
                {
                    messages.Add(condition);
                }
            }
            Application.logMessageReceived += listener;
            yield return LoadTestScene(SCENE_NAME);
            Application.logMessageReceived -= listener;

            CollectionAssert.AreEqual(new[] { DEBUG_MESSAGE }, messages);
        }
    }
}
