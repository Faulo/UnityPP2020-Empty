using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests {
    [TestFixture]
    public class Testat13 : TestSuite {
        class RobotBridge : GameObjectBridge {
            public RobotBridge(GameObject gameObject, bool isInstance = false) : base(gameObject) {
                if (isInstance) {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
            public readonly Physics2DEvents physics;
            public UnityEvent onLevelComplete {
                get => FindField<UnityEvent>(nameof(onLevelComplete)).value;
                set => FindField<UnityEvent>(nameof(onLevelComplete)).value = value;
            }
        }
        class LevelManagerBridge : ScriptableObjectBridge {
            public LevelManagerBridge(ScriptableObject asset) : base(asset) {
                FindMethod("LoadMainMenu", 0);
                FindMethod("LoadNextLevel", 0);
                FindMethod("LoadCredits", 0);
                FindMethod("QuitGame", 0);
            }
            public string mainMenuScene {
                get => FindField<string>(nameof(mainMenuScene)).value;
                set => FindField<string>(nameof(mainMenuScene)).value = value;
            }
            public string creditsScene {
                get => FindField<string>(nameof(creditsScene)).value;
                set => FindField<string>(nameof(creditsScene)).value = value;
            }
            public IEnumerable<string> levelScenes {
                get => FindField<IEnumerable<string>>(nameof(levelScenes)).value;
                set => FindField<IEnumerable<string>>(nameof(levelScenes)).value = value;
            }
        }

        const string ASSET_LEVEL_MANAGER = "LevelManager";
        const string PREFAB_ROBOT = "Assets/Prefabs/Robot.prefab";

        const string SCENE_MAIN_MENU = "MainMenu";
        const string SCENE_CREDITS = "Credits";
        const string SCENE_LEVEL_2 = "Level_2";

        [Test]
        public void T02a_VerifyLevelManagerExists() {
            FileAssert.Exists($"./Assets/Resources/{ASSET_LEVEL_MANAGER}.asset");
            var manager = Resources.Load<ScriptableObject>(ASSET_LEVEL_MANAGER);
            Assert.IsNotNull(manager, $"The asset '{ASSET_LEVEL_MANAGER}' must be a {typeof(ScriptableObject)}!");
        }
        [Test]
        public void T02a_VerifyLevelManagerFieldsAndMethods() {
            var manager = LoadLevelManager();
            Assert.IsNotNull(manager.mainMenuScene, $"'mainMenuScene' must not be null!");
            Assert.IsNotNull(manager.creditsScene, $"'creditsScene' must not be null!");
            Assert.IsNotNull(manager.levelScenes, $"'levelScenes' must not be null!");
            Assert.AreEqual(2, manager.levelScenes.Count(), $"'levelScenes' must contain exactly 2 scenes!");
        }
        [Test]
        public void T03a_VerifyMainMenuSceneExists() {
            FileAssert.Exists($"./Assets/Scenes/{SCENE_MAIN_MENU}.unity");
            Assert.AreEqual(SCENE_MAIN_MENU, LoadLevelManager().mainMenuScene);
        }
        [Test]
        public void T04a_VerifyCreditsSceneExists() {
            FileAssert.Exists($"./Assets/Scenes/{SCENE_CREDITS}.unity");
            Assert.AreEqual(SCENE_CREDITS, LoadLevelManager().creditsScene);
        }
        [Test]
        public void T06a_VerifyLevel2SceneExists() {
            FileAssert.Exists($"./Assets/Scenes/{SCENE_LEVEL_2}.unity");
            Assert.AreEqual(SCENE_LEVEL_2, LoadLevelManager().levelScenes.ElementAt(1));
        }
        [UnityTest]
        public IEnumerator T03b_VerifyMainMenuSceneContainsStuff() {
            yield return LoadTestScene(SCENE_MAIN_MENU);
            Assert.GreaterOrEqual(1, FindObjectsInScene<Canvas>().Length, $"{SCENE_MAIN_MENU} should contain at least 1 Canvas!");
            FindButtonInScene("Start");
            FindButtonInScene("Quit");
        }
        [UnityTest]
        public IEnumerator T04b_VerifyCreditsSceneContainsStuff() {
            yield return LoadTestScene(SCENE_CREDITS);
            Assert.GreaterOrEqual(1, FindObjectsInScene<Canvas>().Length, $"{SCENE_CREDITS} should contain at least 1 Canvas!");
            FindButtonInScene("Back");
        }
        [UnityTest]
        public IEnumerator T06b_VerifyLevel2SceneContainsStuff() {
            yield return LoadTestScene(SCENE_LEVEL_2);
            Assert.AreEqual(1, FindObjectsInScene<Rigidbody2D>().Length, $"{SCENE_LEVEL_2} should contain exactly 1 Rigibody2D (the robot)!");
            Assert.GreaterOrEqual(FindObjectsInScene<BoxCollider2D>().Length, 1, $"{SCENE_LEVEL_2} should contain at least 1 BoxCollider2D (the platforms and lava platforms)!");
            Assert.GreaterOrEqual(FindObjectsInScene<CircleCollider2D>().Length, 1, $"{SCENE_LEVEL_2} should contain at least 1 CircleCollider2D (the coins)!");
        }
        [Test]
        public void T05_VerifyRobotPrefab() {
            var robot = LoadRobotPrefab();
            Assert.IsNotNull(robot.onLevelComplete, $"{robot} must have a {typeof(UnityEvent)} onLevelComplete!");
            Assert.GreaterOrEqual(1, robot.onLevelComplete.GetPersistentEventCount(), $"Robot's onLevelComplete should have a callback registered!");
        }
        [Test]
        public void T07a_VerifyBuildDirectoryExists() {
            DirectoryAssert.Exists("Build");
        }
        [Test]
        public void T07b_VerifyBuildExecutableExists() {
            var directory = new DirectoryInfo("Build");
            var files = directory
                .GetFiles()
                .Where(file => file.Extension == ".exe");
            CollectionAssert.IsNotEmpty(files, $"The directory '{directory}' should contain a Windows executable!");
        }
        Button FindButtonInScene(string label) {
            var buttons = FindObjectsInScene<Button>()
                .Where(button => button.GetComponentsInChildren<TextMeshProUGUI>().Any(text => text.text == label))
                .ToArray();
            Assert.AreEqual(1, buttons.Length, $"There should be 1 {typeof(Button)} with a {typeof(TextMeshProUGUI)} child that has the text '{label}'!");
            Assert.GreaterOrEqual(1, buttons[0].onClick.GetPersistentEventCount(), $"The Button {buttons[0]} should have an 'OnClick' callback registered!");
            return buttons[0];
        }
        RobotBridge LoadRobotPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_ROBOT);
            return new RobotBridge(prefab);
        }
        LevelManagerBridge LoadLevelManager() {
            return new LevelManagerBridge(Resources.Load<ScriptableObject>(ASSET_LEVEL_MANAGER));
        }
    }
}