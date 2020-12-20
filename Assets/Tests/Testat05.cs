using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat05 : TestSuite
    {
        private class MarioBridge : GameObjectBridge
        {
            public bool isGrounded
            {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            private readonly FieldBridge<bool> isGroundedBridge;
            public float defaultAcceleration
            {
                get => defaultAccelerationBridge.value;
                set => defaultAccelerationBridge.value = value;
            }
            private readonly FieldBridge<float> defaultAccelerationBridge;
            public float maximumSpeed
            {
                get => maximumSpeedBridge.value;
                set => maximumSpeedBridge.value = value;
            }
            private readonly FieldBridge<float> maximumSpeedBridge;
            public float jumpSpeed
            {
                get => jumpSpeedBridge.value;
                set => jumpSpeedBridge.value = value;
            }
            private readonly FieldBridge<float> jumpSpeedBridge;
            public float GetCurrentAcceleration()
            {
                return getCurrentAccelerationBridge.Invoke();
            }

            private readonly MethodBridge<float> getCurrentAccelerationBridge;
            public float GetCurrentJumpSpeed()
            {
                return getCurrentJumpSpeedBridge.Invoke();
            }

            private readonly MethodBridge<float> getCurrentJumpSpeedBridge;

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
            public Color rendererColor => renderer.material.color;
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

            public MarioBridge(GameObject gameObject, bool isInstance = false) : base(gameObject)
            {
                isGroundedBridge = FindField<bool>(nameof(isGrounded));
                defaultAccelerationBridge = FindField<float>(nameof(defaultAcceleration));
                maximumSpeedBridge = FindField<float>(nameof(maximumSpeed));
                jumpSpeedBridge = FindField<float>(nameof(jumpSpeed));

                getCurrentAccelerationBridge = FindMethod<float>(nameof(GetCurrentAcceleration), 0, "float");
                getCurrentJumpSpeedBridge = FindMethod<float>(nameof(GetCurrentJumpSpeed), 0, "float");

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
            public float allowedAcceleration
            {
                get => allowedAccelerationBridge.value;
                set => allowedAccelerationBridge.value = value;
            }
            private readonly FieldBridge<float> allowedAccelerationBridge;
            public float jumpSpeedMultiplier
            {
                get => jumpSpeedMultiplierBridge.value;
                set => jumpSpeedMultiplierBridge.value = value;
            }
            private readonly FieldBridge<float> jumpSpeedMultiplierBridge;

            public BoxCollider2D collider
            {
                get;
                private set;
            }
            public Color rendererColor => renderer.material.color;
            public Renderer renderer
            {
                get;
                private set;
            }

            public PlatformBridge(GameObject gameObject) : base(gameObject)
            {
                allowedAccelerationBridge = FindField<float>(nameof(allowedAcceleration));
                jumpSpeedMultiplierBridge = FindField<float>(nameof(jumpSpeedMultiplier));
                collider = FindComponent<BoxCollider2D>();
                renderer = FindComponentInChildren<Renderer>();
            }
        }
        [System.Serializable]
        public class Move
        {
            public KeyControl key;
            public int sign;
            public float maximumSpeed;
            public float defaultAcceleration;

            public override string ToString()
            {
                return $"{key.name}, {defaultAcceleration}m/s², {maximumSpeed}m/s";
            }
        }
        private static Move[] MOVEMENT_DIRECTIONS
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    keyboard = InputSystem.AddDevice<Keyboard>();
                }
                return new[]
                {
                    new Move() {key = keyboard.rightArrowKey, sign = 1, maximumSpeed = 5, defaultAcceleration = 10 },
                    new Move() {key = keyboard.leftArrowKey, sign = -1, maximumSpeed = 10, defaultAcceleration = 15 },
                };
            }
        }

        private static string[] PREFAB_FILES => new[] { PREFAB_MARIO, PLATFORM_ICE_PREFAB, PLATFORM_METAL_PREFAB, PLATFORM_DIRT_PREFAB };
        private static readonly string PREFAB_MARIO = "Assets/Prefabs/Mario.prefab";
        private static readonly string PLATFORM_ICE_PREFAB = "Assets/Prefabs/Platform_Ice.prefab";
        private static readonly string PLATFORM_METAL_PREFAB = "Assets/Prefabs/Platform_Metal.prefab";
        private static readonly string PLATFORM_DIRT_PREFAB = "Assets/Prefabs/Platform_Dirt.prefab";
        private static string[] MATERIAL_FILES => new[] { MATERIAL_ICY, MATERIAL_DEFAULT, MATERIAL_STICKY };
        private static readonly string MATERIAL_ICY = "Assets/PhysicsMaterials/Icy.physicsMaterial2D";
        private static readonly string MATERIAL_DEFAULT = "Assets/PhysicsMaterials/Default.physicsMaterial2D";
        private static readonly string MATERIAL_STICKY = "Assets/PhysicsMaterials/Sticky.physicsMaterial2D";

        private static (string, string)[] PLATFORM_INFOS => new[]
        {
            (PLATFORM_ICE_PREFAB, MATERIAL_ICY),
            (PLATFORM_METAL_PREFAB, MATERIAL_DEFAULT),
            (PLATFORM_DIRT_PREFAB, MATERIAL_STICKY)
        };

        private const string SCENE_PATH = "./Assets/Scenes/MarioTest.unity";
        private const string SCENE_NAME = "MarioTest";
        private const int SCENE_AVATAR_COUNT = 1;
        private const int SCENE_PLATFORM_COUNT = 5;
        private const float SCENE_TIMEOUT = 5;

        [Test]
        public void T06_PrefabExists([ValueSource(nameof(PREFAB_FILES))] string path)
        {
            TestUtils.LoadPrefab(path);
        }
        [Test]
        public void T05a_PhysicsMaterialExists([ValueSource(nameof(MATERIAL_FILES))] string path)
        {
            LoadAsset<PhysicsMaterial2D>(path);
        }
        [Test]
        public void T05b_PhysicsMaterialsFriction()
        {
            PhysicsMaterial2D icy = LoadAsset<PhysicsMaterial2D>(MATERIAL_ICY);
            PhysicsMaterial2D dflt = LoadAsset<PhysicsMaterial2D>(MATERIAL_DEFAULT);
            PhysicsMaterial2D sticky = LoadAsset<PhysicsMaterial2D>(MATERIAL_STICKY);
            Assert.Less(icy.friction, dflt.friction, $"Friction of {icy} must be lower than friction of {dflt}!");
            Assert.Less(dflt.friction, sticky.friction, $"Friction of {dflt} must be lower than friction of {sticky}!");
        }
        [Test]
        public void T04_PlatformPrefab([ValueSource(nameof(PLATFORM_INFOS))] (string, string) info)
        {
            GameObject prefab = TestUtils.LoadPrefab(info.Item1);
            PhysicsMaterial2D mat = LoadAsset<PhysicsMaterial2D>(info.Item2);
            PlatformBridge platform = new PlatformBridge(prefab);
            Assert.AreEqual(mat, platform.collider.sharedMaterial, $"Platform {prefab} should use physics material {mat}!");
            Assert.Greater(platform.allowedAcceleration, 0, $"Platform {prefab} should have an {nameof(platform.allowedAcceleration)} > 0!");
            Assert.Greater(platform.jumpSpeedMultiplier, 0, $"Platform {prefab} should have an {nameof(platform.jumpSpeedMultiplier)} > 0!");
        }
        [Test]
        public void T02_MarioPrefab()
        {
            GameObject prefab = TestUtils.LoadPrefab(PREFAB_MARIO);

            MarioBridge mario = new MarioBridge(prefab);
            Vector2 targetOffset = new Vector2(0, 0);
            Vector2 targetSize = new Vector2(1, 2);
            Assert.AreNotEqual(mario.gameObject, mario.renderer.gameObject, $"Mario's Renderer should be on its own GameObject, as a child of Mario's prefab!");
            CustomAssert.AreEqual(Vector3.one, mario.transform.localScale, $"Mario's Transform must have an scale of {Vector3.one}!");
            CustomAssert.AreEqual(targetOffset, mario.collider.offset, $"Mario's Collider2D must have an offset of {targetOffset}!");
            CustomAssert.AreEqual(targetSize, mario.collider.size, $"Mario's Collider2D must have an size of {targetSize}!");
            CustomAssert.AreEqual(targetSize, (Vector2)mario.renderer.transform.localScale, $"Mario's Renderer's Transform must have an scale of {targetSize}!");
            Assert.AreEqual(RigidbodyType2D.Dynamic, mario.rigidbody.bodyType, $"Mario must have a Dynamic body type!");
            Assert.AreEqual(RigidbodyConstraints2D.FreezeRotation, mario.rigidbody.constraints, $"Mario should not be able to rotate!");
        }
        [UnityTest]
        public IEnumerator T03_MarioMovement([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move)
        {
            yield return new WaitForFixedUpdate();

            MarioBridge mario = InstantiateMario(Vector3.zero);
            mario.maximumSpeed = move.maximumSpeed;
            mario.defaultAcceleration = move.defaultAcceleration;
            mario.jumpSpeed = move.maximumSpeed;

            yield return new WaitForFixedUpdate();

            float timeout = Time.time + SCENE_TIMEOUT;
            float targetSpeed = move.sign * mario.maximumSpeed;
            float previousSpeed = mario.rigidbody.velocity.x;
            float currentSpeed = 0;

            Assert.AreEqual(0, previousSpeed, "Mario should start with horizontal speed of 0m/s!");
            Assert.AreEqual(move.maximumSpeed, mario.GetCurrentJumpSpeed(), $"In freefall, Mario's jump speed should be {move.maximumSpeed}m/s!");

            using (new InputPress(move.key))
            {
                do
                {
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    currentSpeed = mario.rigidbody.velocity.x;
                    Assert.IsFalse(mario.isGrounded, "Mario should not become grounded in empty scene!");
                    Assert.AreEqual(move.maximumSpeed, mario.maximumSpeed, $"Mario's {nameof(mario.maximumSpeed)} should not be changed by script!");
                    Assert.AreEqual(move.defaultAcceleration, mario.defaultAcceleration, $"Mario's {nameof(mario.defaultAcceleration)} should not be changed by script!");
                    Assert.AreEqual(move.defaultAcceleration, mario.GetCurrentAcceleration(), $"Mario's {nameof(mario.GetCurrentAcceleration)} should return {nameof(mario.defaultAcceleration)} in freefall!");
                    Assert.Greater(Mathf.Abs(currentSpeed), Mathf.Abs(previousSpeed), $"Mario's speed must increase each frame!");
                    if (Time.time > timeout)
                    {
                        Assert.Fail($"In freefall and with acceleration of {mario.defaultAcceleration}m/s², Mario should reach {targetSpeed}m/s in {SCENE_TIMEOUT}s, but was {mario.rigidbody.velocity.x}m/s!");
                    }
                    previousSpeed = currentSpeed;
                } while (Mathf.Abs(previousSpeed) < Mathf.Abs(targetSpeed));
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                currentSpeed = mario.rigidbody.velocity.x;
                Assert.AreEqual(targetSpeed, currentSpeed, $"In freefall and with acceleration of {mario.defaultAcceleration}m/s², Mario must never exceed his maximum speed of {targetSpeed}m/s!");
            }

            yield return new WaitForFixedUpdate();

            Object.Destroy(mario.gameObject);

            yield return new WaitForFixedUpdate();
        }
        [Test]
        public void T07a_SceneExists()
        {
            FileInfo file = new FileInfo(SCENE_PATH);
            FileAssert.Exists(file);
        }
        [UnityTest]
        public IEnumerator T07b_PrefabInstancesExistInScene()
        {
            void validatePlatforms(GameObject prefab, PlatformBridge[] instances)
            {
                Assert.GreaterOrEqual(
                    instances.Length,
                    1,
                    $"Scene {SCENE_NAME} must at least 1 instance of prefab {prefab}!"
                );
                Color color = instances[0].rendererColor;
                for (int i = 1; i < instances.Length; i++)
                {
                    CustomAssert.AreEqual(color, instances[i].rendererColor, $"All instancecs of {prefab} must have the same color!");
                }
            }
            yield return new WaitForFixedUpdate();

            GameObject avatarPrefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            GameObject icePrefab = TestUtils.LoadPrefab(PLATFORM_ICE_PREFAB);
            GameObject metalPrefab = TestUtils.LoadPrefab(PLATFORM_METAL_PREFAB);
            GameObject dirtPrefab = TestUtils.LoadPrefab(PLATFORM_DIRT_PREFAB);

            yield return LoadTestScene(SCENE_NAME);

            MarioBridge[] avatars = currentScene.GetPrefabInstances(avatarPrefab)
                .Select(obj => new MarioBridge(obj, true))
                .ToArray();

            PlatformBridge[] icePlatforms = currentScene.GetPrefabInstances(icePrefab)
                .Select(obj => new PlatformBridge(obj))
                .ToArray();
            PlatformBridge[] metalPlatforms = currentScene.GetPrefabInstances(metalPrefab)
                .Select(obj => new PlatformBridge(obj))
                .ToArray();
            PlatformBridge[] dirtPlatforms = currentScene.GetPrefabInstances(dirtPrefab)
                .Select(obj => new PlatformBridge(obj))
                .ToArray();

            PlatformBridge[] platforms = icePlatforms
                .Union(metalPlatforms)
                .Union(dirtPlatforms)
                .ToArray();

            Assert.AreEqual(
                SCENE_AVATAR_COUNT,
                avatars.Length,
                $"Scene {SCENE_NAME} must have exactly {SCENE_AVATAR_COUNT} instance(s) of prefab {avatarPrefab}!"
            );

            validatePlatforms(icePrefab, icePlatforms);
            validatePlatforms(metalPrefab, metalPlatforms);
            validatePlatforms(dirtPrefab, dirtPlatforms);

            CustomAssert.AreNotEqual(
                icePlatforms[0].rendererColor,
                metalPlatforms[0].rendererColor,
                $"Platform instancecs should not share a color!"
            );
            CustomAssert.AreNotEqual(
                icePlatforms[0].rendererColor,
                dirtPlatforms[0].rendererColor,
                $"Platform instancecs should not share a color!"
            );
            CustomAssert.AreNotEqual(
                metalPlatforms[0].rendererColor,
                dirtPlatforms[0].rendererColor,
                $"Platform instancecs should not share a color!"
            );

            Assert.GreaterOrEqual(
                platforms.Length,
                SCENE_PLATFORM_COUNT,
                $"Scene {SCENE_NAME} must have at least {SCENE_PLATFORM_COUNT} instances of platform prefabs!"
            );

            MarioBridge mario = avatars[0];
            PlatformBridge platform = default;
            float startingSpeed = mario.jumpSpeed;

            mario.physics.onCollisionEnter += collision =>
            {
                platform = new PlatformBridge(collision.gameObject);
            };

            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => mario.isGrounded || Time.time > timeout);
            Assert.IsTrue(mario.isGrounded, $"After waiting {SCENE_TIMEOUT}s, avatar should be grounded!");
            Assert.IsNotNull(platform, $"After being grounded, avatar should have collided with a platform!");
            Assert.AreEqual(platform.allowedAcceleration, mario.GetCurrentAcceleration(), $"Mario's {nameof(mario.GetCurrentAcceleration)} should have returned Platform's {nameof(platform.allowedAcceleration)}!");
            Assert.AreEqual(platform.jumpSpeedMultiplier * startingSpeed, mario.GetCurrentJumpSpeed(), $"Mario's {nameof(mario.GetCurrentJumpSpeed)} should have returned Mario's {nameof(mario.jumpSpeed)} multiplied by Platform's {nameof(platform.jumpSpeedMultiplier)}!");
        }

        private MarioBridge InstantiateMario(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            MarioBridge avatar = new MarioBridge(instance, true);
            avatar.rigidbody.mass = 1;
            avatar.rigidbody.drag = 0;
            return avatar;
        }
    }
}
