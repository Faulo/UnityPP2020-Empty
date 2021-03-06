﻿using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
    [TestFixture]
    public class Testat04 : TestSuite {
        class AvatarBridge : GameObjectBridge {
            public bool isGrounded {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            readonly FieldBridge<bool> isGroundedBridge;
            public Color avatarColor {
                get => avatarColorBridge.value;
                set => avatarColorBridge.value = value;
            }
            readonly FieldBridge<Color> avatarColorBridge;

            public Rigidbody2D rigidbody {
                get;
                private set;
            }
            public BoxCollider2D collider {
                get;
                private set;
            }
            public Renderer renderer {
                get;
                private set;
            }
            public Physics2DEvents physics {
                get;
                private set;
            }

            public AvatarBridge(GameObject gameObject, bool isInstance = false) : base(gameObject) {
                isGroundedBridge = FindField<bool>(nameof(isGrounded));
                avatarColorBridge = FindField<Color>(nameof(avatarColor));
                rigidbody = FindComponent<Rigidbody2D>();
                collider = FindComponent<BoxCollider2D>();
                renderer = FindComponentInChildren<Renderer>();
                if (isInstance) {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
        }
        public class PlatformBridge : GameObjectBridge {
            public Color platformColor {
                get => platformColorBridge.value;
                set => platformColorBridge.value = value;
            }
            readonly FieldBridge<Color> platformColorBridge;

            public BoxCollider2D collider {
                get;
                private set;
            }
            public Renderer renderer {
                get;
                private set;
            }

            public PlatformBridge(GameObject gameObject) : base(gameObject) {
                platformColorBridge = FindField<Color>(nameof(platformColor));
                collider = FindComponent<BoxCollider2D>();
                renderer = FindComponentInChildren<Renderer>();
            }
        }
        static string[] PREFAB_FILES => new[] { AVATAR_PREFAB, PLATFORM_PREFAB };
        static readonly string AVATAR_PREFAB = "Assets/Prefabs/Avatar.prefab";
        static readonly string PLATFORM_PREFAB = "Assets/Prefabs/Platform.prefab";
        static string[] MATERIAL_FILES => new[] { AVATAR_MATERIAL, PLATFORM_MATERIAL };
        static readonly string AVATAR_MATERIAL = "Assets/Materials/Avatar.mat";
        static readonly string PLATFORM_MATERIAL = "Assets/Materials/Platform.mat";

        const string SCENE_PATH = "./Assets/Scenes/PlatformTest.unity";
        const string SCENE_NAME = "PlatformTest";
        const int SCENE_AVATAR_COUNT = 1;
        const int SCENE_PLATFORM_COUNT = 5;
        const float SCENE_TIMEOUT = 5;

        static Color[] COLOR_VALUES => new[] { Color.green, Color.magenta };
        const string COLOR_KEY = "_BaseColor";

        [Test]
        public void T01_PrefabExists([ValueSource(nameof(PREFAB_FILES))] string path) {
            var file = new FileInfo(path);
            FileAssert.Exists(file);
        }
        [Test]
        public void T02_MaterialsExists([ValueSource(nameof(MATERIAL_FILES))] string path) {
            var mat = LoadAsset<Material>(path);
            Assert.IsTrue(mat.HasProperty(COLOR_KEY), $"Material {mat} must have property {COLOR_KEY}!");
        }
        [Test]
        public void T03_PlatformPrefab() {
            var prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            var platform = new PlatformBridge(prefab);
            Assert.IsNotNull(platform.collider, $"'{PLATFORM_PREFAB}' needs a Collider2D!");
            Assert.IsNotNull(platform.renderer, $"'{PLATFORM_PREFAB}' needs a Renderer!");
            CustomAssert.AreEqual(Vector2.zero, platform.collider.offset, $"Platform's Collider2D must have an offset of {Vector2.zero}, but was {platform.collider.offset}!");
            CustomAssert.AreEqual(Vector2.one, platform.collider.size, $"Platform's Collider2D must have an offset of {Vector2.one}, but was {platform.collider.size}!");
            Assert.AreEqual(LoadAsset<Material>(PLATFORM_MATERIAL), platform.renderer.sharedMaterial, $"Platform's renderer must use Platform material!");
        }
        [UnityTest]
        public IEnumerator T05_PlatformColor([ValueSource(nameof(COLOR_VALUES))] Color color) {
            yield return new WaitForFixedUpdate();

            var platform = InstantiatePlatform(Vector3.zero);
            platform.platformColor = color;
            for (int i = 0; i < 2; i++) {
                yield return new WaitForFixedUpdate();
                CustomAssert.AreEqual(color, platform.platformColor, $"Platform color must not change from script-set value of {color}!");
                CustomAssert.AreEqual(color, platform.renderer.material.GetColor(COLOR_KEY), $"Platform renderer's color should have been set to {color}!");
            }
            Object.Destroy(platform.gameObject);
        }
        [Test]
        public void T04_AvatarPrefab() {
            var prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);

            var avatar = new AvatarBridge(prefab);
            Assert.IsNotNull(avatar.rigidbody, $"'{AVATAR_PREFAB}' needs a Rigidbody2D!");
            Assert.IsNotNull(avatar.collider, $"'{AVATAR_PREFAB}' needs a Collider2D!");
            CustomAssert.AreEqual(Vector2.zero, avatar.collider.offset, $"Avatar's Collider2D must have an offset of {Vector2.zero}, but was {avatar.collider.offset}!");
            CustomAssert.AreEqual(Vector2.one, avatar.collider.size, $"Avatar's Collider2D must have an offset of {Vector2.one}, but was {avatar.collider.size}!");
            Assert.AreEqual(RigidbodyType2D.Dynamic, avatar.rigidbody.bodyType, $"Avatar must have a Dynamic body type!");
            Assert.AreEqual(LoadAsset<Material>(AVATAR_MATERIAL), avatar.renderer.sharedMaterial, $"Avatar's renderer must use Avatar material!");
        }
        [UnityTest]
        public IEnumerator T06_AvatarColor([ValueSource(nameof(COLOR_VALUES))] Color color) {
            yield return new WaitForFixedUpdate();

            var avatar = InstantiateAvatar(Vector3.zero);
            avatar.avatarColor = color;
            for (int i = 0; i < 2; i++) {
                yield return new WaitForFixedUpdate();
                CustomAssert.AreEqual(color, avatar.avatarColor, $"Avatar color must not change from script-set value of {color}!");
                CustomAssert.AreEqual(color, avatar.renderer.material.GetColor(COLOR_KEY), $"Avatar renderer's color should have been set to {color}!");
            }
            Object.Destroy(avatar.gameObject);
        }
        [Test]
        public void T07a_SceneExists() {
            var file = new FileInfo(SCENE_PATH);
            FileAssert.Exists(file);
        }
        [UnityTest]
        public IEnumerator T07b_PrefabInstancesExistInScene() {
            yield return new WaitForFixedUpdate();

            var avatarPrefab = TestUtils.LoadPrefab(AVATAR_PREFAB);
            var platformPrefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);

            yield return LoadTestScene(SCENE_NAME);

            var avatars = currentScene.GetPrefabInstances(avatarPrefab)
                .Select(obj => new AvatarBridge(obj, true))
                .ToArray();

            var platforms = currentScene.GetPrefabInstances(platformPrefab)
                .Select(obj => new PlatformBridge(obj))
                .ToArray();

            var colors = avatars
                .Select(a => a.avatarColor)
                .Concat(platforms.Select(p => p.platformColor))
                .Distinct()
                .ToArray();

            Assert.AreEqual(
                SCENE_AVATAR_COUNT,
                avatars.Length,
                $"Scene {SCENE_NAME} must have exactly {SCENE_AVATAR_COUNT} instance(s) of prefab {avatarPrefab}!"
            );
            Assert.AreEqual(
                SCENE_PLATFORM_COUNT,
                platforms.Length,
                $"Scene {SCENE_NAME} must have exactly {SCENE_PLATFORM_COUNT} instance(s) of prefab {platformPrefab}!"
            );
            Assert.AreEqual(
                SCENE_AVATAR_COUNT + SCENE_PLATFORM_COUNT,
                colors.Length,
                $"Each avatar and platform in scene {SCENE_NAME} must have a unique color!"
            );

            var avatar = avatars[0];
            PlatformBridge platform = default;

            avatar.physics.onCollisionEnter += collision => {
                platform = new PlatformBridge(collision.gameObject);
            };

            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => avatar.isGrounded || Time.time > timeout);
            Assert.IsTrue(avatar.isGrounded, $"After waiting {SCENE_TIMEOUT}s, avatar should be grounded!");
            Assert.IsNotNull(platform, $"After being grounded, avatar should have collided with a platform!");
            CustomAssert.AreEqual(platform.platformColor, avatar.renderer.material.GetColor(COLOR_KEY), $"After being grounded, avatar's color should have changed to platform's color!");
        }

        AvatarBridge InstantiateAvatar(Vector3 position) {

            var prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            var avatar = new AvatarBridge(instance, true);
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
