﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Tests {
    [TestFixture]
    public class Testat02 : TestSuite {
        [Serializable]
        public class Move {
            public KeyControl[] keys;
            public Vector3 direction;

            public override string ToString() {
                return string.Join("+", keys.Select(key => key.name));
            }
        }

        const string SCENE_PATH = "./Assets/Scenes/InputTest.unity";
        const string SCENE_NAME = "InputTest";
        const string AVATAR_NAME = "Avatar";
        const string AVATAR_SPEED_FIELD = "movementSpeed";

        static readonly Regex EMAIL_PATTERN = new Regex(@"^[\w@.]+uni-bayreuth\.de$");
        static readonly string[] GIT_FILES = new[] { "./.gitignore", "./.gitattributes" };
        static readonly float[] AVATAR_SPEED_VALUES = new[] { 0, 5f };
        static readonly int[] AVATAR_SPEED_DURATIONS = new[] { 0, 2, 4 };
        static Move[] MOVEMENT_DIRECTIONS {
            get {
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

        [Test]
        public void T01_GitFiles([ValueSource(nameof(GIT_FILES))] string path) {
            var file = new FileInfo(path);
            Assert.IsTrue(file.Exists, $"File '{file.FullName}' not found.");
        }

        [Test]
        public void T02_EmailAddress() {
            Assert.IsTrue(EMAIL_PATTERN.IsMatch(Application.companyName), $"Company Name must be a valid e-mail address, but was '{Application.companyName}'");
        }

        [Test]
        public void T03a_SceneExists() {
            var file = new FileInfo(SCENE_PATH);
            Assert.IsTrue(file.Exists, $"File '{file.FullName}' not found.");
        }
        [UnityTest]
        public IEnumerator T03b_AvatarExists() {
            yield return LoadTestScene(SCENE_NAME);
            var avatars = FindAvatars();

            Assert.AreEqual(1, avatars.Count(), $"There must be exactly 1 GameObject with the name of '{AVATAR_NAME}' in scene '{SCENE_NAME}'!");
        }
        [UnityTest]
        public IEnumerator T03c_AvatarInput([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            yield return LoadTestScene(SCENE_NAME);
            var avatar = FindAvatars().First();

            var target = move.direction;
            avatar.transform.position = Vector3.zero;
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(Vector3.zero, avatar.transform.position, $"Must not move avatar when no input is happening.");

            Vector3 position;

            using (new InputPress(input, move.keys)) {
                yield return new WaitForFixedUpdate();
                position = avatar.transform.position;
                Assert.AreEqual(Math.Sign(target.x), Math.Sign(position.x), $"With input {move}, avatar's x should've moved towards {target.x}");
                Assert.AreEqual(Math.Sign(target.y), Math.Sign(position.y), $"With input {move}, avatar's y should've moved towards {target.y}");
                Assert.AreEqual(Math.Sign(target.z), Math.Sign(position.z), $"With input {move}, avatar's z should've moved towards {target.z}");

                yield return new WaitForFixedUpdate();

                Assert.AreNotEqual(position, avatar.transform.position, $"Avatar must keep moving in direction {target}");

                position = avatar.transform.position;
            }

            yield return new WaitForFixedUpdate();

            Assert.AreEqual(position, avatar.transform.position, $"Avatar should've stopped at position {position}");
        }
        IEnumerable<GameObject> FindAvatars() {
            return currentScene.GetObjectsByName(AVATAR_NAME);
        }
        IEnumerable<(Component, FieldInfo)> FindSpeedFields(GameObject obj) {
            foreach (var component in obj.GetComponents<Component>()) {
                var fields = component
                    .GetType()
                    .GetFields()
                    .Where(f => f.Name == AVATAR_SPEED_FIELD);
                foreach (var field in fields) {
                    yield return (component, field);
                }
            }
        }
        [UnityTest]
        public IEnumerator T04a_AvatarSpeedFieldExists() {
            yield return LoadTestScene(SCENE_NAME);
            var avatar = FindAvatars().First();
            var speedFields = FindSpeedFields(avatar);

            Assert.AreEqual(1, speedFields.Count(), $"There must be exactly 1 field with the name of '{AVATAR_SPEED_FIELD}' in GameObject '{AVATAR_NAME}'!");
        }
        [UnityTest]
        public IEnumerator T04b_AvatarSpeedFieldWorks(
            [ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move,
            [ValueSource(nameof(AVATAR_SPEED_VALUES))] float speed,
            [ValueSource(nameof(AVATAR_SPEED_DURATIONS))] int frames) {
            yield return LoadTestScene(SCENE_NAME);
            var avatar = FindAvatars().First();
            (var speedComponent, var speedField) = FindSpeedFields(avatar).First();

            speedField.SetValue(speedComponent, speed);

            float duration = frames * Time.fixedDeltaTime;

            string keyName = string.Join("+", move.keys.Select(key => key.name));
            var target = speed * move.direction * duration;
            avatar.transform.position = Vector3.zero;

            yield return new WaitForFixedUpdate();

            using (new InputPress(input, move.keys)) {

                for (int i = 0; i < frames; i++) {
                    yield return new WaitForFixedUpdate();
                }

                var actual = avatar.transform.position;

                Assert.IsTrue(TestUtils.Approximately(target, actual), $"With input {keyName}, speed {speed}m/s and waiting {frames} FixedUpdate frames, avatar should have arrived at position {target}, but was {actual}");
            }

            yield return new WaitForFixedUpdate();
        }
    }
}