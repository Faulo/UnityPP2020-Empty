using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Tests {
    public class Testat09 : TestSuite {
        class MarioBridge : GameObjectBridge {
            public MarioBridge(GameObject gameObject, bool isInstance = false) : base(gameObject) {
                if (isInstance) {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
            public readonly Physics2DEvents physics;
            public Rigidbody2D rigidbody => FindComponent<Rigidbody2D>();
            public bool isGrounded {
                get => FindField<bool>(nameof(isGrounded)).value;
                set => FindField<bool>(nameof(isGrounded)).value = value;
            }
        }
        public class PlatformBridge : GameObjectBridge
        {
            public BoxCollider2D collider => FindComponent<BoxCollider2D>();
            public PlatformBridge(GameObject gameObject) : base(gameObject) {
            }
        }
        public class CameraBridge : GameObjectBridge {
            public CameraBridge(GameObject gameObject) : base(gameObject) {
            }
            public Camera camera => FindComponent<Camera>();
            public Transform target {
                get => FindField<Transform>(nameof(target)).value;
                set => FindField<Transform>(nameof(target)).value = value;
            }
            public float distance {
                get => FindField<float>(nameof(distance)).value;
                set => FindField<float>(nameof(distance)).value = value;
            }
            public float moveDuration {
                get => FindField<float>(nameof(moveDuration)).value;
                set => FindField<float>(nameof(moveDuration)).value = value;
            }
        }
        public class TeleporterBridge : GameObjectBridge {
            public TeleporterBridge(GameObject gameObject) : base(gameObject) {
            }
            public Transform target {
                get => FindField<Transform>(nameof(target)).value;
                set => FindField<Transform>(nameof(target)).value = value;
            }
        }


        const float SCENE_TIMEOUT = 1;
        static readonly Vector3[] CAMERA_POSITIONS = new[] { new Vector3(0, 10, 0), new Vector3(5, 5, 0) };
        static readonly float[] CAMERA_DISTANCES = new[] { 4f, 8f };
        static readonly float[] CAMERA_DURATONS = new[] { 0.1f, 0.3f };

        static readonly string PREFAB_MARIO = "Assets/Prefabs/Mario.prefab";
        static readonly string PREFAB_PLATFORM_DIRT = "Assets/Prefabs/Platform_Dirt.prefab";
        static readonly string PREFAB_CAMERA = "Assets/Prefabs/MainCamera.prefab";
        static readonly string PREFAB_TELEPORTER = "Assets/Prefabs/Teleporter.prefab";

        [Test]
        public void T02a_VerifyCameraPrefab() {
            var camera = LoadCameraPrefab();
            Assert.Greater(camera.distance, 0, $"{camera}'s '{nameof(camera.distance)}' must be positive!");
            Assert.Greater(camera.moveDuration, 0, $"{camera}'s '{nameof(camera.moveDuration)}' must be positive!");
            Assert.IsInstanceOf<Camera>(camera.camera, $"{camera} requires a '{typeof(Camera)}' component!");
            Assert.IsInstanceOf<Transform>(camera.target, $"{camera} requires a field '{nameof(camera.target)}'!");
        }
        [UnityTest]
        public IEnumerator T02b_VerifyCameraBehavior(
                [ValueSource(nameof(CAMERA_POSITIONS))] Vector3 cameraPosition,
                [ValueSource(nameof(CAMERA_DISTANCES))] float cameraDistance,
                [ValueSource(nameof(CAMERA_DURATONS))] float cameraDuration) {
            InstantiateCamera(cameraPosition);
            var target = CreateGameObject("CameraTarget").transform;
            camera.target = target;
            camera.distance = cameraDistance;
            camera.moveDuration = cameraDuration;

            var comparer = new Vector3EqualityComparer(0.1f);

            var position = target.position - new Vector3(0, 0, camera.distance);
            float timeout = Time.time + SCENE_TIMEOUT;
            float start = Time.time;
            yield return new WaitUntil(() => comparer.Equals(camera.position, position) || Time.time > timeout);
            float duration = Time.time - start;
            Assert.That(
                camera.position, Is.EqualTo(position).Using(comparer),
                $"Starting at {cameraPosition} and with {nameof(camera.target)} at {target.position} and {nameof(camera.distance)} {camera.distance}, Camera should've moved to position {position}!"
            );
            Assert.GreaterOrEqual(
                duration, camera.moveDuration,
                $"Camera movement should've taken at least {camera.moveDuration} seconds according to its {nameof(camera.moveDuration)}!"
            );
        }
        [Test]
        public void T03a_VerifyTeleporterPrefab() {
            var teleporter = LoadTeleporterPrefab();
            Assert.IsInstanceOf<Transform>(teleporter.target, $"{teleporter} requires a field '{nameof(teleporter.target)}'!");
            Assert.IsNotNull(teleporter.target, $"{teleporter} requires is '{nameof(teleporter.target)}' to be assigned!");
            Assert.IsInstanceOf<ParticleSystem>(teleporter.transform.GetComponent<ParticleSystem>(), $"{teleporter} requires a '{typeof(ParticleSystem)}' component!");
            Assert.IsInstanceOf<ParticleSystem>(teleporter.target.GetComponent<ParticleSystem>(), $"{teleporter}'s target requires a '{typeof(ParticleSystem)}' component!");
        }
        [UnityTest]
        public IEnumerator T03b_VerifyTeleporterBehavior([ValueSource(nameof(CAMERA_POSITIONS))] Vector3 position) {
            InstantiateTeleporter(Vector3.zero);
            teleporter.target.position = position;
            InstantiateMario(Vector3.zero);
            mario.rigidbody.gravityScale = 0;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var comparer = new Vector3EqualityComparer(0.1f);

            Assert.That(
                mario.rigidbody.velocity,
                Is.EqualTo(Vector2.zero).Using(comparer),
                $"Teleporting should reset Mario's velocity!"
            );
            Assert.That(
                mario.position,
                Is.EqualTo(position).Using(comparer),
                $"Teleporter failed to teleport Mario to position {position}!"
            );
        }
        [UnityTest]
        public IEnumerator T04_VerifyPlatformBug() {
            InstantiatePlatform(new Vector3(0, -1, 0), PREFAB_PLATFORM_DIRT);
            var backup = platform;
            InstantiatePlatform(new Vector3(-1, 0, 0), PREFAB_PLATFORM_DIRT);

            var platforms = new List<GameObject>();
            InstantiateMario(Vector3.zero);
            mario.physics.onCollisionEnter += collision => {
                platforms.Add(collision.gameObject);
            };

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.AreEqual(2, platforms.Count, $"Mario should've hit platform {backup} and platform {platform}!");
            Assert.IsTrue(mario.isGrounded, $"After colliding with 2 platforms, Mario should be grounded!");
            Assert.AreEqual(2, FindObjectsInScene<ParticleSystem>().Length, $"After colliding with 2 platforms, exactly 2 ParticleSystems should've been spawned!");

            DestroyGameObject(platform.gameObject);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(mario.isGrounded, $"After leaving one of the 2 platforms, Mario should've stayed grounded! (This is the bug)");
        }
        MarioBridge LoadMarioPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            return new MarioBridge(prefab);
        }
        CameraBridge LoadCameraPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_CAMERA);
            return new CameraBridge(prefab);
        }
        TeleporterBridge LoadTeleporterPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_TELEPORTER);
            return new TeleporterBridge(prefab);
        }

        MarioBridge mario;
        PlatformBridge platform;
        CameraBridge camera;
        TeleporterBridge teleporter;

        void InstantiateMario(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            mario = new MarioBridge(instance, true);
        }
        void InstantiatePlatform(Vector3 position, string prefabFile) {
            var prefab = TestUtils.LoadPrefab(prefabFile);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            platform = new PlatformBridge(instance);
            platform.transform.localScale = Vector3.one;
            platform.collider.size = Vector2.one;
        }
        void InstantiateCamera(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_CAMERA);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            camera = new CameraBridge(instance);
        }
        void InstantiateTeleporter(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_TELEPORTER);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            teleporter = new TeleporterBridge(instance);
        }

    }
}