﻿using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Tests {
    [TestFixture]
    public class Testat03 : TestSuite {
        class AvatarBridge : GameObjectBridge {
            public bool isGrounded {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            readonly FieldBridge<bool> isGroundedBridge;

            public float movementSpeed {
                get => movementSpeedBridge.value;
                set => movementSpeedBridge.value = value;
            }
            readonly FieldBridge<float> movementSpeedBridge;

            public float jumpSpeed {
                get => jumpSpeedBridge.value;
                set => jumpSpeedBridge.value = value;
            }
            readonly FieldBridge<float> jumpSpeedBridge;

            public InputAction movementAction => movementActionBridge.value;
            readonly FieldBridge<InputAction> movementActionBridge;
            public InputAction jumpAction => jumpActionBridge.value;
            readonly FieldBridge<InputAction> jumpActionBridge;

            public Rigidbody2D rigidbody {
                get;
                private set;
            }
            public BoxCollider2D collider {
                get;
                private set;
            }

            public AvatarBridge(GameObject gameObject) : base(gameObject) {
                isGroundedBridge = FindField<bool>(nameof(isGrounded));
                movementSpeedBridge = FindField<float>(nameof(movementSpeed));
                jumpSpeedBridge = FindField<float>(nameof(jumpSpeed));
                movementActionBridge = FindField<InputAction>(nameof(movementAction));
                jumpActionBridge = FindField<InputAction>(nameof(jumpAction));
                rigidbody = FindComponent<Rigidbody2D>();
                collider = FindComponent<BoxCollider2D>();
            }
        }
        public class PlatformBridge : GameObjectBridge {
            public BoxCollider2D collider {
                get;
                private set;
            }

            public PlatformBridge(GameObject gameObject) : base(gameObject) {
                collider = FindComponent<BoxCollider2D>();
            }
        }
        [System.Serializable]
        public class Move {
            public KeyControl key;
            public int sign;
            public override string ToString() {
                return key.name;
            }
        }

        const string PROJECT_PATH = ".";
        static readonly Regex SPLIT_PATTERN = new Regex(@"\s+");
        static readonly string[] GIT_TAGS = new[] { "Tx01", "Tx02" };

        static string[] PREFAB_FILES => new[] { AVATAR_PREFAB, PLATFORM_PREFAB };
        static readonly string AVATAR_PREFAB = "Assets/Prefabs/Avatar.prefab";
        static readonly string PLATFORM_PREFAB = "Assets/Prefabs/Platform.prefab";

        static readonly float[] AVATAR_SPEED_VALUES = new[] { 0, 5f };
        static readonly int[] AVATAR_SPEED_DURATIONS = new[] { 2, 8 };
        static KeyControl AVATAR_JUMPKEY => keyboard.spaceKey;

        const string SCENE_PATH = "./Assets/Scenes/PlatformTest.unity";
        const string SCENE_NAME = "PlatformTest";
        const int SCENE_AVATAR_COUNT = 1;
        const int SCENE_PLATFORM_COUNT = 5;
        const float SCENE_TIMEOUT = 5;
        static Move[] MOVEMENT_DIRECTIONS {
            get {
                return new[]
                {
                    new Move() {key = keyboard.rightArrowKey, sign = 1 },
                    new Move() {key = keyboard.leftArrowKey, sign = -1 },
                };
            }
        }
        [Test]
        public void T02_GitTagExists([ValueSource(nameof(GIT_TAGS))] string tag) {
            var directory = new DirectoryInfo(PROJECT_PATH);
            string tags = TestUtils.RunGitCommand(directory.FullName, "tag");
            Assert.AreNotEqual("", tags, "No git tags found!");
            CollectionAssert.Contains(SPLIT_PATTERN.Split(tags), tag);
        }
        [Test]
        public void T03_PrefabExists([ValueSource(nameof(PREFAB_FILES))] string path) {
            var file = new FileInfo(path);
            FileAssert.Exists(file);
        }
        [Test]
        public void T04a_PlatformPrefab() {
            var prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            var platform = new PlatformBridge(prefab);
            Assert.IsNotNull(platform.collider, $"'{PLATFORM_PREFAB}' needs a Collider2D!");
            Assert.IsTrue(TestUtils.Approximately(Vector2.zero, platform.collider.offset), $"Platform's Collider2D must have an offset of {Vector2.zero}, but was {platform.collider.offset}!");
            Assert.IsTrue(TestUtils.Approximately(Vector2.one, platform.collider.size), $"Platform's Collider2D must have an offset of {Vector2.one}, but was {platform.collider.size}!");
        }
        [UnityTest]
        public IEnumerator T04b_PlatformGravity() {
            yield return new WaitForFixedUpdate();

            var platform = InstantiatePlatform(Vector3.zero);
            for (int i = 0; i < 10; i++) {
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(TestUtils.Approximately(Vector3.zero, platform.position), "Platform should not move, ever!");
            }
            Object.Destroy(platform.gameObject);
        }
        [Test]
        public void T05a_AvatarPrefab() {
            var prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);

            var avatar = new AvatarBridge(prefab);
            Assert.IsNotNull(avatar.rigidbody, $"'{AVATAR_PREFAB}' needs a Rigidbody2D!");
            Assert.IsNotNull(avatar.collider, $"'{AVATAR_PREFAB}' needs a Collider2D!");
            Assert.IsTrue(TestUtils.Approximately(Vector2.zero, avatar.collider.offset), $"Avatar's Collider2D must have an offset of {Vector2.zero}, but was {avatar.collider.offset}!");
            Assert.IsTrue(TestUtils.Approximately(Vector2.one, avatar.collider.size), $"Avatar's Collider2D must have an offset of {Vector2.one}, but was {avatar.collider.size}!");
            Assert.AreEqual(RigidbodyType2D.Dynamic, avatar.rigidbody.bodyType, $"Avatar must have a Dynamic body type!");
        }
        [UnityTest]
        public IEnumerator T05b_AvatarGravityWhenFalling() {
            yield return new WaitForFixedUpdate();

            var avatar = InstantiateAvatar(Vector3.zero);
            avatar.isGrounded = false;
            float oldY = avatar.position.y;
            float oldDistance = 0;
            for (int i = 0; i < 10; i++) {
                yield return new WaitForFixedUpdate();
                float newY = avatar.position.y;
                float newDistance = Mathf.Abs(newY - oldY);
                Assert.IsFalse(avatar.isGrounded, $"Avatar should not set isGrounded to true in empty scene.");
                Assert.Less(newY, oldY, $"Avatar's Y should have been less than {oldY}, but was {newY}");
                Assert.Greater(newDistance, oldDistance, $"Avatar should've travelled further than {oldDistance}, but but only travelled {newDistance} units!");
                oldY = newY;
                oldDistance = newDistance;
            }
            Object.Destroy(avatar.gameObject);
        }
        [UnityTest]
        public IEnumerator T05c_AvatarMovement(
            [ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move,
            [ValueSource(nameof(AVATAR_SPEED_VALUES))] float speed,
            [ValueSource(nameof(AVATAR_SPEED_DURATIONS))] int frames) {
            yield return new WaitForFixedUpdate();

            var avatar = InstantiateAvatar(Vector3.zero);
            avatar.isGrounded = false;
            avatar.movementSpeed = speed;

            yield return new WaitForFixedUpdate();

            float target = avatar.position.x;
            target += move.sign * speed * frames * Time.fixedDeltaTime;

            using (new InputPress(input, move.key)) {
                for (int i = 0; i < frames; i++) {
                    yield return new WaitForFixedUpdate();
                }

                float actual = avatar.position.x;
                Assert.IsTrue(TestUtils.Approximately(target, actual), $"With input {move}, speed {speed}m/s and waiting {frames} FixedUpdate frames, avatar should have arrived at X={target}, but was at X={actual}!");
            }

            Object.Destroy(avatar.gameObject);

            yield return new WaitForFixedUpdate();
        }
        [UnityTest]
        public IEnumerator T05d_AvatarGravityWhenGrounded() {
            yield return new WaitForFixedUpdate();

            var platform = InstantiatePlatform(Vector3.zero);
            platform.scale = Vector3.one;

            var avatar = InstantiateAvatar(Vector3.up);
            avatar.scale = Vector3.one;
            avatar.isGrounded = false;

            for (int i = 0; i < 10; i++) {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(avatar.isGrounded, $"Avatar should not set isGrounded to false when standing on a platform.");
            Assert.IsTrue(TestUtils.Approximately(Vector3.up, avatar.position, 0.1f), $"Avatar should not move when grounded, but was {avatar.position.y}.");

            Object.Destroy(avatar.gameObject);
            Object.Destroy(platform.gameObject);
        }


        [Test]
        public void T06a_SceneExists() {
            var file = new FileInfo(SCENE_PATH);
            FileAssert.Exists(file);
        }
        [UnityTest]
        public IEnumerator T06b_PrefabInstancesExistInScene() {
            yield return new WaitForFixedUpdate();

            var platformPrefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            var avatarPrefab = TestUtils.LoadPrefab(AVATAR_PREFAB);

            yield return LoadTestScene(SCENE_NAME);

            var avatars = currentScene
                .GetPrefabInstances(avatarPrefab)
                .ToArray();
            var platforms = currentScene
                .GetPrefabInstances(platformPrefab)
                .ToArray();

            Assert.AreEqual(SCENE_AVATAR_COUNT, avatars.Length, $"Scene {SCENE_NAME} must have exactly {SCENE_AVATAR_COUNT} instance(s) of prefab {avatarPrefab}!");
            ;
            ;
            Assert.AreEqual(
                SCENE_PLATFORM_COUNT, platforms.Length, $"Scene {SCENE_NAME} must have exactly {SCENE_PLATFORM_COUNT} instance(s) of prefab {platformPrefab}!");

            var avatar = new AvatarBridge(avatars[0]);

            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => avatar.isGrounded || Time.time > timeout);
            Assert.IsTrue(avatar.isGrounded, $"After waiting {SCENE_TIMEOUT}s, avatar should be grounded!");

            using (new InputPress(input, AVATAR_JUMPKEY)) {

                timeout = Time.time + SCENE_TIMEOUT;
                yield return new WaitUntil(() => !avatar.isGrounded || Time.time > timeout);
                Assert.IsFalse(avatar.isGrounded, $"After pressing {AVATAR_JUMPKEY} and waiting {SCENE_TIMEOUT}s, avatar should have been airborne!");
            }

            timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => avatar.isGrounded || Time.time > timeout);
            Assert.IsTrue(avatar.isGrounded, $"After jumping and waiting {SCENE_TIMEOUT}s, avatar should have landed!");

            yield return new WaitForFixedUpdate();
        }

        AvatarBridge InstantiateAvatar(Vector3 position) {

            var prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            var avatar = new AvatarBridge(instance);
            avatar.rigidbody.mass = 1;
            avatar.rigidbody.drag = 0;
            return avatar;
        }

        PlatformBridge InstantiatePlatform(Vector3 position) {

            var prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new PlatformBridge(instance);
        }
    }
}
