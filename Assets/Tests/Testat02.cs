using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat02
    {
        [Serializable]
        public class Move
        {
            public KeyControl[] keys;
            public Vector3 direction;

            public override string ToString()
            {
                return string.Join("+", keys.Select(key => key.name));
            }
        }

        private const string SCENE_PATH = "./Assets/Scenes/InputTest.unity";
        private const string SCENE_NAME = "InputTest";
        private const string AVATAR_NAME = "Avatar";
        private const string AVATAR_SPEED_FIELD = "movementSpeed";

        private static readonly Regex EMAIL_PATTERN = new Regex(@"^[\w@.]+uni-bayreuth\.de$");
        private static readonly string[] GIT_FILES = new[] { "./.gitignore", "./.gitattributes" };
        private static readonly float[] AVATAR_SPEED_VALUES = new[] { 0, 5f };
        private static readonly int[] AVATAR_SPEED_DURATIONS = new[] { 0, 2, 4 };
        private static Move[] MOVEMENT_DIRECTIONS
        {
            get
            {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    keyboard = InputSystem.AddDevice<Keyboard>();
                }
                return new[]
                {
                    new Move {
                        keys = new[] { keyboard.upArrowKey },
                        direction = Vector3.up
                    },
                    new Move {
                        keys = new[] { keyboard.downArrowKey },
                        direction = Vector3.down
                    },
                    new Move {
                        keys = new[] { keyboard.leftArrowKey },
                        direction = Vector3.left
                    },
                    new Move {
                        keys = new[] { keyboard.rightArrowKey },
                        direction = Vector3.right
                    },
                    new Move {
                        keys = new[] { keyboard.upArrowKey, keyboard.rightArrowKey },
                        direction = Vector3.Normalize(Vector3.up + Vector3.right)
                    },
                    new Move {
                        keys = new[] { keyboard.downArrowKey, keyboard.rightArrowKey },
                        direction = Vector3.Normalize(Vector3.down + Vector3.right)
                    },
                    new Move {
                        keys = new[] { keyboard.upArrowKey, keyboard.leftArrowKey },
                        direction = Vector3.Normalize(Vector3.up + Vector3.left)
                    },
                    new Move {
                        keys = new[] { keyboard.downArrowKey, keyboard.leftArrowKey },
                        direction = Vector3.Normalize(Vector3.down + Vector3.left)
                    },
                };
            }
        }


        private static readonly InputTestFixture input = new InputTestFixture();

        [Test]
        public void TestGitFiles([ValueSource(nameof(GIT_FILES))] string path)
        {
            FileInfo file = new FileInfo(path);
            Assert.IsTrue(file.Exists, $"File '{file.FullName}' not found.");
        }

        [Test]
        public void TestEmailAddress()
        {
            Assert.IsTrue(EMAIL_PATTERN.IsMatch(Application.companyName), $"Company Name must be a valid e-mail address, but was '{Application.companyName}'");
        }

        [Test]
        public void TestSceneExists()
        {
            FileInfo file = new FileInfo(SCENE_PATH);
            Assert.IsTrue(file.Exists, $"File '{file.FullName}' not found.");
        }

        private IEnumerable<Transform> FindAvatars()
        {
            return SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(obj => obj.GetComponentsInChildren<Transform>())
                .Where(transform => transform.gameObject.name == AVATAR_NAME);
        }
        [UnityTest]
        public IEnumerator TestAvatarExists()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(SCENE_NAME);
            yield return new WaitUntil(() => operation.isDone);
            var avatars = FindAvatars();

            Assert.AreEqual(1, avatars.Count(), $"There must be exactly 1 GameObject with the name of '{AVATAR_NAME}' in scene '{SCENE_NAME}'!");
        }
        [UnityTest]
        public IEnumerator TestAvatarInput([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(SCENE_NAME);
            yield return new WaitUntil(() => operation.isDone);
            var avatar = FindAvatars().First();

            var target = move.direction;
            avatar.transform.position = Vector3.zero;
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(Vector3.zero, avatar.transform.position, $"Must not move avatar when no input is happening.");

            Array.ForEach(move.keys, key => input.Press(key));
            InputSystem.Update();
            yield return new WaitForFixedUpdate();
            var actual = avatar.transform.position;
            Assert.AreEqual(Math.Sign(target.x), Math.Sign(actual.x), $"With input {move}, avatar's x should've moved towards {target.x}");
            Assert.AreEqual(Math.Sign(target.y), Math.Sign(actual.y), $"With input {move}, avatar's y should've moved towards {target.y}");
            Assert.AreEqual(Math.Sign(target.z), Math.Sign(actual.z), $"With input {move}, avatar's z should've moved towards {target.z}");

            yield return new WaitForFixedUpdate();

            Assert.AreNotEqual(actual, avatar.transform.position, $"Avatar must keep moving in direction {target}");

            actual = avatar.transform.position;

            Array.ForEach(move.keys, key => input.Release(key));
            InputSystem.Update();

            yield return new WaitForFixedUpdate();

            Assert.AreEqual(actual, avatar.transform.position, $"Avatar should've stopped at position {actual}");
        }
        private IEnumerable<(Component, FieldInfo)> FindSpeedFields(Component obj)
        {
            foreach (var component in obj.GetComponents<Component>())
            {
                var fields = component
                    .GetType()
                    .GetFields()
                    .Where(f => f.Name == AVATAR_SPEED_FIELD);
                foreach (var field in fields)
                {
                    yield return (component, field);
                }
            }
        }
        [UnityTest]
        public IEnumerator TestAvatarSpeedFieldExists()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(SCENE_NAME);
            yield return new WaitUntil(() => operation.isDone);
            var avatar = FindAvatars().First();
            var speedFields = FindSpeedFields(avatar);

            Assert.AreEqual(1, speedFields.Count(), $"There must be exactly 1 field with the name of '{AVATAR_SPEED_FIELD}' in GameObject '{AVATAR_NAME}'!");
        }
        [UnityTest]
        public IEnumerator TestAvatarSpeedFieldWorks(
            [ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move,
            [ValueSource(nameof(AVATAR_SPEED_VALUES))] float speed,
            [ValueSource(nameof(AVATAR_SPEED_DURATIONS))] int frames)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(SCENE_NAME);
            yield return new WaitUntil(() => operation.isDone);
            var avatar = FindAvatars().First();
            var (speedComponent, speedField) = FindSpeedFields(avatar).First();

            speedField.SetValue(speedComponent, speed);

            float duration = frames * Time.fixedDeltaTime;

            var keyName = string.Join("+", move.keys.Select(key => key.name));
            var target = speed * move.direction * duration;
            avatar.transform.position = Vector3.zero;

            yield return new WaitForFixedUpdate();

            Array.ForEach(move.keys, key => input.Press(key));
            InputSystem.Update();

            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            var actual = avatar.transform.position;

            Assert.IsTrue(TestUtils.Approximately(target, actual), $"With input {keyName}, speed {speed}m/s and waiting {frames} FixedUpdate frames, avatar should have arrived at position {target}, but was {actual}");

            Array.ForEach(move.keys, key => input.Release(key));
            InputSystem.Update();
        }
    }
}