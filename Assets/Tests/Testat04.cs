using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat04 : TestSuite
    {
        private class AvatarBridge : GameObjectBridge
        {
            public bool isGrounded
            {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            private readonly FieldBridge<bool> isGroundedBridge;
            public Color avatarColor
            {
                get => avatarColorBridge.value;
                set => avatarColorBridge.value = value;
            }
            private readonly FieldBridge<Color> avatarColorBridge;

            public Rigidbody2D rigidbody
            {
                get;
                private set;
            }
            public BoxCollider2D collider
            {
                get;
                private set;
            }
            public Renderer renderer
            {
                get;
                private set;
            }
            public Physics2DEvents physics
            {
                get;
                private set;
            }

            public AvatarBridge(GameObject gameObject, bool isInstance = false) : base(gameObject)
            {
                isGroundedBridge = FindField<bool>(nameof(isGrounded));
                avatarColorBridge = FindField<Color>(nameof(avatarColor));
                rigidbody = FindComponent<Rigidbody2D>();
                collider = FindComponent<BoxCollider2D>();
                renderer = FindComponentInChildren<Renderer>();
                if (isInstance)
                {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
        }
        public class PlatformBridge : GameObjectBridge
        {
            public Color platformColor
            {
                get => platformColorBridge.value;
                set => platformColorBridge.value = value;
            }
            private readonly FieldBridge<Color> platformColorBridge;

            public BoxCollider2D collider
            {
                get;
                private set;
            }
            public Renderer renderer
            {
                get;
                private set;
            }

            public PlatformBridge(GameObject gameObject) : base(gameObject)
            {
                platformColorBridge = FindField<Color>(nameof(platformColor));
                collider = FindComponent<BoxCollider2D>();
                renderer = FindComponentInChildren<Renderer>();
            }
        }
        private static string[] PREFAB_FILES => new[] { AVATAR_PREFAB, PLATFORM_PREFAB };
        private static readonly string AVATAR_PREFAB = "Assets/Prefabs/Avatar.prefab";
        private static readonly string PLATFORM_PREFAB = "Assets/Prefabs/Platform.prefab";
        private static string[] MATERIAL_FILES => new[] { AVATAR_MATERIAL, PLATFORM_MATERIAL };
        private static readonly string AVATAR_MATERIAL = "Assets/Materials/Avatar.mat";
        private static readonly string PLATFORM_MATERIAL = "Assets/Materials/Platform.mat";

        private const string SCENE_PATH = "./Assets/Scenes/PlatformTest.unity";
        private const string SCENE_NAME = "PlatformTest";
        private const int SCENE_AVATAR_COUNT = 1;
        private const int SCENE_PLATFORM_COUNT = 5;
        private const float SCENE_TIMEOUT = 5;

        private static Color[] COLOR_VALUES => new[] { Color.green, Color.magenta };
        private const string COLOR_KEY = "_BaseColor";

        [Test]
        public void TestPrefabExists([ValueSource(nameof(PREFAB_FILES))] string path)
        {
            FileInfo file = new FileInfo(path);
            FileAssert.Exists(file);
        }
        [Test]
        public void TestMaterialsExists([ValueSource(nameof(MATERIAL_FILES))] string path)
        {
            Material mat = LoadAsset<Material>(path);
            Assert.IsTrue(mat.HasProperty(COLOR_KEY), $"Material {mat} must have property {COLOR_KEY}!");
        }
        [Test]
        public void TestPlatformPrefab()
        {
            GameObject prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            PlatformBridge platform = new PlatformBridge(prefab);
            CustomAssert.AreEqual(Vector2.zero, platform.collider.offset, $"Platform's Collider2D must have an offset of {Vector2.zero}, but was {platform.collider.offset}!");
            CustomAssert.AreEqual(Vector2.one, platform.collider.size, $"Platform's Collider2D must have an offset of {Vector2.one}, but was {platform.collider.size}!");
            Assert.AreEqual(LoadAsset<Material>(PLATFORM_MATERIAL), platform.renderer.sharedMaterial, $"Platform's renderer must use Platform material!");
        }
        [UnityTest]
        public IEnumerator TestPlatformColor([ValueSource(nameof(COLOR_VALUES))] Color color)
        {
            yield return new WaitForFixedUpdate();

            PlatformBridge platform = InstantiatePlatform(Vector3.zero);
            platform.platformColor = color;
            for (int i = 0; i < 2; i++)
            {
                yield return new WaitForFixedUpdate();
                CustomAssert.AreEqual(color, platform.platformColor, $"Platform color must not change from script-set value of {color}!");
                CustomAssert.AreEqual(color, platform.renderer.material.GetColor(COLOR_KEY), $"Platform renderer's color should have been set to {color}!");
            }
            Object.Destroy(platform.gameObject);
        }
        [Test]
        public void TestAvatarPrefab()
        {
            GameObject prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);

            AvatarBridge avatar = new AvatarBridge(prefab);
            CustomAssert.AreEqual(Vector2.zero, avatar.collider.offset, $"Avatar's Collider2D must have an offset of {Vector2.zero}, but was {avatar.collider.offset}!");
            CustomAssert.AreEqual(Vector2.one, avatar.collider.size, $"Avatar's Collider2D must have an offset of {Vector2.one}, but was {avatar.collider.size}!");
            Assert.AreEqual(RigidbodyType2D.Dynamic, avatar.rigidbody.bodyType, $"Avatar must have a Dynamic body type!");
            Assert.AreEqual(LoadAsset<Material>(AVATAR_MATERIAL), avatar.renderer.sharedMaterial, $"Avatar's renderer must use Avatar material!");
        }
        [UnityTest]
        public IEnumerator TestAvatarColor([ValueSource(nameof(COLOR_VALUES))] Color color)
        {
            yield return new WaitForFixedUpdate();

            AvatarBridge avatar = InstantiateAvatar(Vector3.zero);
            avatar.avatarColor = color;
            for (int i = 0; i < 2; i++)
            {
                yield return new WaitForFixedUpdate();
                CustomAssert.AreEqual(color, avatar.avatarColor, $"Avatar color must not change from script-set value of {color}!");
                CustomAssert.AreEqual(color, avatar.renderer.material.GetColor(COLOR_KEY), $"Avatar renderer's color should have been set to {color}!");
            }
            Object.Destroy(avatar.gameObject);
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

            GameObject avatarPrefab = TestUtils.LoadPrefab(AVATAR_PREFAB);
            GameObject platformPrefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);

            LoadTestScene(SCENE_NAME);
            yield return new WaitForFixedUpdate();

            AvatarBridge[] avatars = FindPrefabInstances(avatarPrefab)
                .Select(obj => new AvatarBridge(obj, true))
                .ToArray();

            PlatformBridge[] platforms = FindPrefabInstances(platformPrefab)
                .Select(obj => new PlatformBridge(obj))
                .ToArray();

            Color[] colors = avatars
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

            AvatarBridge avatar = avatars[0];
            PlatformBridge platform = default;

            avatar.physics.onCollisionEnter += collision =>
            {
                platform = new PlatformBridge(collision.gameObject);
            };

            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => avatar.isGrounded || Time.time > timeout);
            Assert.IsTrue(avatar.isGrounded, $"After waiting {SCENE_TIMEOUT}s, avatar should be grounded!");
            Assert.IsNotNull(platform, $"After being grounded, avatar should have collided with a platform!");
            CustomAssert.AreEqual(platform.platformColor, avatar.renderer.material.GetColor(COLOR_KEY), $"After being grounded, avatar's color should have changed to platform's color!");
        }

        private IEnumerable<GameObject> FindPrefabInstances(GameObject prefab)
        {
            return Object.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.StartsWith(prefab.name));
        }

        private AvatarBridge InstantiateAvatar(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(AVATAR_PREFAB);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            AvatarBridge avatar = new AvatarBridge(instance, true);
            avatar.rigidbody.mass = 1;
            avatar.rigidbody.drag = 0;
            return avatar;
        }

        private PlatformBridge InstantiatePlatform(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(PLATFORM_PREFAB);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new PlatformBridge(instance);
        }
    }
}
