using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat03
    {
        [System.Serializable]
        public class Move
        {
            public KeyControl key;
            public int sign;
            public override string ToString()
            {
                return key.name;
            }
        }
        private static readonly InputTestFixture input = new InputTestFixture();

        private const string PROJECT_PATH = ".";
        private static readonly Regex SPLIT_PATTERN = new Regex(@"\s+");
        private static readonly string[] GIT_TAGS = new[] { "Tx01", "Tx02" };

        private static string[] PREFAB_FILES => new[] { AVATAR_PREFAB, PLATFORM_PREFAB };
        private static readonly string AVATAR_PREFAB = "Assets/Prefabs/Avatar.prefab";
        private static readonly string PLATFORM_PREFAB = "Assets/Prefabs/Platform.prefab";

        private static readonly float[] AVATAR_SPEED_VALUES = new[] { 0, 5f };
        private static readonly int[] AVATAR_SPEED_DURATIONS = new[] { 2, 8 };
        private static KeyControl AVATAR_JUMPKEY => Keyboard.current.spaceKey;

        private const string SCENE_PATH = "./Assets/Scenes/PlatformTest.unity";
        private const string SCENE_NAME = "PlatformTest";
        private const int SCENE_AVATAR_COUNT = 1;
        private const int SCENE_PLATFORM_COUNT = 5;
        private const float SCENE_TIMEOUT = 5;
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
                    new Move() {key = keyboard.rightArrowKey, sign = 1 },
                    new Move() {key = keyboard.leftArrowKey, sign = -1 },
                };
            }
        }
        [Test]
        public void TestGitTagExists([ValueSource(nameof(GIT_TAGS))] string tag)
        {
            var directory = new DirectoryInfo(PROJECT_PATH);
            var tags = TestUtils.RunGitCommand(directory.FullName, "tag");
            Assert.AreNotEqual("", tags, "No git tags found!");
            CollectionAssert.Contains(SPLIT_PATTERN.Split(tags), tag);
        }
        [Test]
        public void TestPrefabExists([ValueSource(nameof(PREFAB_FILES))] string path)
        {
            FileInfo file = new FileInfo(path);
            FileAssert.Exists(file);
        }
        [Test]
        public void TestPlatformPrefab()
        {
            GameObject prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            var platform = new PlatformBridge(prefab);
            Assert.IsTrue(TestUtils.Approximately(Vector2.zero, platform.collider.offset), $"Platform's Collider2D must have an offset of {Vector2.zero}, but was {platform.collider.offset}!");
            Assert.IsTrue(TestUtils.Approximately(Vector2.one, platform.collider.size), $"Platform's Collider2D must have an offset of {Vector2.one}, but was {platform.collider.size}!");
        }
        [UnityTest]
        public IEnumerator TestPlatformGravity()
        {
            yield return new WaitForFixedUpdate();

            var platform = InstantiatePlatform(Vector3.zero);
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(TestUtils.Approximately(Vector3.zero, platform.position), "Platform should not move, ever!");
            }
            Object.Destroy(platform.gameObject);
        }
        [Test]
        public void TestAvatarPrefab()
        {
            GameObject prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);

            var avatar = new AvatarBridge(prefab);
            Assert.IsTrue(TestUtils.Approximately(Vector2.zero, avatar.collider.offset), $"Avatar's Collider2D must have an offset of {Vector2.zero}, but was {avatar.collider.offset}!");
            Assert.IsTrue(TestUtils.Approximately(Vector2.one, avatar.collider.size), $"Avatar's Collider2D must have an offset of {Vector2.one}, but was {avatar.collider.size}!");
            Assert.AreEqual(RigidbodyType2D.Dynamic, avatar.rigidbody.bodyType, $"Avatar must have a Dynamic body type!");
        }
        [UnityTest]
        public IEnumerator TestAvatarGravityWhenFalling()
        {
            yield return new WaitForFixedUpdate();

            var avatar = InstantiateAvatar(Vector3.zero);
            avatar.isGrounded = false;
            float oldY = avatar.position.y;
            float oldDistance = 0;
            for (int i = 0; i < 10; i++)
            {
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
        public IEnumerator TestAvatarMovement(
            [ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move,
            [ValueSource(nameof(AVATAR_SPEED_VALUES))] float speed,
            [ValueSource(nameof(AVATAR_SPEED_DURATIONS))] int frames)
        {
            yield return new WaitForFixedUpdate();

            var avatar = InstantiateAvatar(Vector3.zero);
            avatar.isGrounded = false;
            avatar.movementSpeed = speed;

            yield return new WaitForFixedUpdate();

            float target = avatar.position.x;
            target += move.sign * speed * frames * Time.fixedDeltaTime;

            input.Press(move.key);
            InputSystem.Update();

            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float actual = avatar.position.x;
            Assert.IsTrue(TestUtils.Approximately(target, actual), $"With input {move}, speed {speed}m/s and waiting {frames} FixedUpdate frames, avatar should have arrived at X={target}, but was at X={actual}!");

            input.Release(move.key);
            InputSystem.Update();

            Object.Destroy(avatar.gameObject);
        }
        [UnityTest]
        public IEnumerator TestAvatarGravityWhenGrounded()
        {
            yield return new WaitForFixedUpdate();

            var platform = InstantiatePlatform(Vector3.zero);
            platform.scale = Vector3.one;

            var avatar = InstantiateAvatar(Vector3.up);
            avatar.scale = Vector3.one;
            avatar.isGrounded = false;

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(avatar.isGrounded, $"Avatar should not set isGrounded to false when standing on a platform.");
            Assert.IsTrue(TestUtils.Approximately(Vector3.up, avatar.position, 0.1f), $"Avatar should not move when grounded, but was {avatar.position.y}.");

            Object.Destroy(avatar.gameObject);
            Object.Destroy(platform.gameObject);
        }


        [Test]
        public void TestSceneExists()
        {
            FileInfo file = new FileInfo(SCENE_PATH);
            FileAssert.Exists(file);
        }
        [UnityTest]
        public IEnumerator TestPrefabInstancesExistInScene()
        {
            yield return new WaitForFixedUpdate();

            GameObject platformPrefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            GameObject avatarPrefab = TestUtils.LoadPrefab(AVATAR_PREFAB);

            AsyncOperation operation = SceneManager.LoadSceneAsync(SCENE_NAME);
            yield return new WaitUntil(() => operation.isDone);
            var pairs = FindPrefabInstances().ToArray();

            var avatars = pairs
                .Where(pair => pair.Item1.StartsWith(avatarPrefab.name))
                .Select(pair => pair.Item2)
                .ToArray();
            var platforms = pairs
                .Where(pair => pair.Item1.StartsWith(platformPrefab.name))
                .Select(pair => pair.Item2)
                .ToArray();

            Assert.AreEqual(SCENE_AVATAR_COUNT, avatars.Length, $"Scene {SCENE_NAME} must have exactly {SCENE_AVATAR_COUNT} instance(s) of prefab {avatarPrefab}!"); ; ;
            Assert.AreEqual(
                SCENE_PLATFORM_COUNT, platforms.Length, $"Scene {SCENE_NAME} must have exactly {SCENE_PLATFORM_COUNT} instance(s) of prefab {platformPrefab}!");

            var avatar = new AvatarBridge(avatars[0]);
            Assert.IsFalse(avatar.isGrounded, $"At scene start, avatar should be airborne!");
            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => avatar.isGrounded || Time.time > timeout);
            Assert.IsTrue(avatar.isGrounded, $"After waiting {SCENE_TIMEOUT}s, avatar should be grounded!");

            input.Press(AVATAR_JUMPKEY);
            InputSystem.Update();

            timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => !avatar.isGrounded || Time.time > timeout);
            Assert.IsFalse(avatar.isGrounded, $"After pressing {AVATAR_JUMPKEY} and waiting {SCENE_TIMEOUT}s, avatar should have been airborne!");

            input.Release(AVATAR_JUMPKEY);
            InputSystem.Update();

            timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => avatar.isGrounded || Time.time > timeout);
            Assert.IsTrue(avatar.isGrounded, $"After jumping and waiting {SCENE_TIMEOUT}s, avatar should have landed!");
        }

        private IEnumerable<(string, GameObject)> FindPrefabInstances()
        {
            var instances = SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(obj => obj.GetComponentsInChildren<Transform>())
                .Select(t => t.gameObject);
            foreach (var instance in instances)
            {
                yield return (instance.name, instance);
            }

        }

        private AvatarBridge InstantiateAvatar(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);
            GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity);
            var avatar = new AvatarBridge(instance);
            avatar.rigidbody.mass = 1;
            avatar.rigidbody.drag = 0;
            return avatar;
        }

        private PlatformBridge InstantiatePlatform(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity);
            return new PlatformBridge(instance);
        }
    }
}
