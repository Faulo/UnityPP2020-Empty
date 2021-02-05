using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Tests {
    [TestFixture]
    public class Testat12 : TestSuite {
        class RobotBridge : GameObjectBridge {
            public RobotBridge(GameObject gameObject, bool isInstance = false) : base(gameObject) {
                if (isInstance) {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
            public readonly Physics2DEvents physics;
            public Rigidbody2D rigidbody => FindComponent<Rigidbody2D>();
            public int currentCoinCount {
                get => FindField<int>(nameof(currentCoinCount)).value;
                set => FindField<int>(nameof(currentCoinCount)).value = value;
            }
            public int maximumCoinCount {
                get => FindField<int>(nameof(maximumCoinCount)).value;
                set => FindField<int>(nameof(maximumCoinCount)).value = value;
            }
            public int deathCount {
                get => FindField<int>(nameof(deathCount)).value;
                set => FindField<int>(nameof(deathCount)).value = value;
            }
        }
        public class CoinBridge : GameObjectBridge {
            public CoinBridge(GameObject gameObject) : base(gameObject) {
            }
            public CircleCollider2D collider => FindComponent<CircleCollider2D>();
            public MeshRenderer renderer => FindComponentInChildren<MeshRenderer>();
            public MeshFilter meshFilter => FindComponentInChildren<MeshFilter>();
        }
        public class LavaBridge : GameObjectBridge {
            public LavaBridge(GameObject gameObject) : base(gameObject) {
            }
            public BoxCollider2D collider => FindComponent<BoxCollider2D>();
            public MeshRenderer renderer => FindComponentInChildren<MeshRenderer>();
            public MeshFilter meshFilter => FindComponentInChildren<MeshFilter>();
            public float deathDuration​ {
                get => FindField<float>(nameof(deathDuration​)).value;
                set => FindField<float>(nameof(deathDuration​)).value = value;
            }
            public float respawnDuration {
                get => FindField<float>(nameof(respawnDuration)).value;
                set => FindField<float>(nameof(respawnDuration)).value = value;
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
            public float moveDuration {
                get => FindField<float>(nameof(moveDuration)).value;
                set => FindField<float>(nameof(moveDuration)).value = value;
            }
        }
        public class HUDBridge : GameObjectBridge {
            public HUDBridge(GameObject gameObject) : base(gameObject) {
            }
            public TextMeshProUGUI coinText => FindComponentsInChildren<TextMeshProUGUI>()[0];
            public TextMeshProUGUI deathText => FindComponentsInChildren<TextMeshProUGUI>()[1];
        }

        const float TIMEOUT = 2;

        const string TAG_PLAYER = "Player";
        const string TAG_COIN = "Coin";
        const string TAG_LAVA = "Lava";

        const string PREFAB_ROBOT = "Assets/Prefabs/Robot.prefab";
        const string PREFAB_COIN = "Assets/Prefabs/Coin.prefab";
        const string PREFAB_LAVA = "Assets/Prefabs/LavaPlatform.prefab";
        const string PREFAB_CAMERA = "Assets/Prefabs/MainCamera.prefab";
        const string PREFAB_HUD = "Assets/Prefabs/HUD.prefab";

        const string MATERIAL_GOLD = "Assets/Materials/Gold.mat";
        const string MATERIAL_LAVA = "Assets/Materials/Lava.mat";

        const string SCENE_PATH = "./Assets/Scenes/Level_1.unity";
        const string SCENE_NAME = "Level_1";

        static readonly Vector2[] V2_POSITIONS = new[] { new Vector2(2, 3), new Vector2(-4, 2) };
        static readonly float[] F_HEIGHTS = new[] { 2f, 4f };
        static readonly float[] F_DURATIONS = new[] { 0, 0.5f };
        static readonly int[] I_AMOUNTS = new[] { 0, 2, 5 };

        [Test]
        public void T02a_VerifyCoinPrefab() {
            var coin = LoadCoinPrefab();
            Assert.IsNotNull(coin.collider, $"{coin} must have a {typeof(CircleCollider2D)} component!");
            Assert.IsNotNull(coin.renderer, $"{coin} must have a {typeof(MeshRenderer)} component!");
            Assert.IsNotNull(coin.meshFilter, $"{coin} must have a {typeof(MeshFilter)} component!");
            Assert.AreEqual("Cylinder", coin.meshFilter.sharedMesh.name, $"{coin}'s {typeof(MeshFilter)} should use the cylinder mesh!");
            Assert.AreEqual(TAG_COIN, coin.tag, $"{coin} must have a tag!");
        }
        [Test]
        public void T02b_VerifyCoinMaterial() {
            var mat = LoadAsset<Material>(MATERIAL_GOLD);
            var coin = LoadCoinPrefab();
            Assert.AreEqual(mat, coin.renderer.sharedMaterial, $"{coin} should use the material '{MATERIAL_GOLD}'!");
        }
        [Test]
        public void T02c_VerifyRobotPrefab() {
            var robot = LoadRobotPrefab();
            Assert.IsNotNull(robot.rigidbody, $"{robot} must have a {typeof(Rigidbody2D)} component!");
        }
        [UnityTest]
        public IEnumerator T02d_VerifyCoinIsWin([ValueSource(nameof(V2_POSITIONS))] Vector2 position) {
            var particles = new HashSet<ParticleSystem>();
            InstantiateCamera();
            yield return new WaitForFixedUpdate();
            var coin = InstantiateCoin(position);
            Assert.AreEqual(0, FindNewObjectsInScene(particles).Length, $"Coin shouldn't have spawned ParticleSystems!");
            yield return new WaitForFixedUpdate();
            var robot = InstantiateRobot(position);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(coin.gameObject, $"When spawning coin and robot both at {position}, coin should've gotten destroyed!");
            Assert.AreEqual(1, FindNewObjectsInScene(particles).Length, $"Collecting the coin should spawn a ParticleSystem!");
            CustomAssert.AreEqual((Vector3)position, particles.First().transform.position, $"ParticleSystem should've been created at {position}!");
        }
        [UnityTest]
        public IEnumerator T03a_VerifyCoinCounting() {
            InstantiateCamera();
            yield return new WaitForFixedUpdate();
            var coins = new HashSet<CoinBridge>();
            foreach (var position in V2_POSITIONS) {
                coins.Add(InstantiateCoin(position));
                yield return new WaitForFixedUpdate();
            }
            var robot = InstantiateRobot(Vector3.zero);
            robot.rigidbody.gravityScale = 0;
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(0, robot.currentCoinCount, $"When spawning robot, '{nameof(robot.currentCoinCount)}' should be 0!");
            Assert.AreEqual(coins.Count, robot.maximumCoinCount, $"When spawning robot and {coins.Count} coins, '{nameof(robot.maximumCoinCount)}' should be {coins.Count}!");

            foreach (var position in V2_POSITIONS) {
                robot.rigidbody.position = position;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(coins.Count, robot.currentCoinCount, $"After teleporting to collect all coins, '{nameof(robot.currentCoinCount)}' should be {coins.Count}!");
        }
        [UnityTest]
        public IEnumerator T03b_VerifyFakeCoin() {
            InstantiateCamera();
            yield return new WaitForFixedUpdate();
            var obj = new GameObject {
                tag = TAG_COIN
            };
            yield return new WaitForFixedUpdate();
            var robot = InstantiateRobot(Vector3.zero);
            robot.rigidbody.gravityScale = 0;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(0, robot.currentCoinCount, $"When spawning robot, '{nameof(robot.currentCoinCount)}' should be 0!");
            Assert.AreEqual(1, robot.maximumCoinCount, $"When spawning robot and an empty GameObject with tag {TAG_COIN}, '{nameof(robot.maximumCoinCount)}' should be 1!");
        }
        [Test]
        public void T04a_VerifyLavaPrefab() {
            var lava = LoadLavaPrefab();
            Assert.IsNotNull(lava.collider, $"{lava} must have a {typeof(BoxCollider2D)} component!");
            Assert.IsNotNull(lava.renderer, $"{lava} must have a {typeof(MeshRenderer)} component!");
            Assert.IsNotNull(lava.meshFilter, $"{lava} must have a {typeof(MeshFilter)} component!");
            Assert.AreEqual("Cube", lava.meshFilter.sharedMesh.name, $"{lava}'s {typeof(MeshFilter)} should use the cube mesh!");
            Assert.GreaterOrEqual(lava.deathDuration, 0, $"{lava}'s '{nameof(lava.deathDuration)}' must be positiv!");
            Assert.GreaterOrEqual(lava.respawnDuration, 0, $"{lava}'s '{nameof(lava.respawnDuration)}' must be positiv!");
        }
        [Test]
        public void T04b_VerifyLavaMaterial() {
            var mat = LoadAsset<Material>(MATERIAL_LAVA);
            var lava = LoadLavaPrefab();
            Assert.AreEqual(mat, lava.renderer.sharedMaterial, $"{lava} should use the material '{MATERIAL_LAVA}'!");
        }
        [Test]
        public void T04c_VerifyCameraPrefab() {
            var camera = LoadCameraPrefab();
            Assert.GreaterOrEqual(camera.moveDuration, 0, $"{camera}'s '{nameof(camera.moveDuration)}' must be positiv!");
        }
        [UnityTest]
        public IEnumerator T04d_VerifyLavaIsDefeat(
            [ValueSource(nameof(V2_POSITIONS))] Vector2 position,
            [ValueSource(nameof(F_HEIGHTS))] float height,
            [ValueSource(nameof(F_DURATIONS))] float cameraDuration) {
            var startPosition = position + new Vector2(0, height);
            var contactPosition = Vector2.zero;
            var comparer = new Vector2EqualityComparer(0.1f);
            var particles = new HashSet<ParticleSystem>();
            var camera = InstantiateCamera();
            var robot = InstantiateRobot(startPosition);
            var lava = InstantiateLava(position);
            camera.target = robot.transform;
            camera.moveDuration = cameraDuration;
            robot.physics.onCollisionEnter += collision => {
                var contactPositionSum = Vector2.zero;
                int contactPositionCount = 0;
                for (int i = 0; i < collision.contactCount; i++) {
                    var contact = collision.GetContact(i);
                    contactPositionSum += contact.point;
                    contactPositionCount++;
                }
                if (contactPositionCount > 0) {
                    // calculate the average
                    contactPosition = contactPositionSum / contactPositionCount;
                }
            };
            lava.deathDuration = 0.5f;
            lava.respawnDuration = 0.1f;
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(0, FindNewObjectsInScene(particles).Length, $"Lava shouldn't have spawned ParticleSystems!");
            yield return WaitFor(
                "i: spawning of death particles",
                () => {
                    FindNewObjectsInScene(particles);
                    return particles.Count == 1;
                }
            );
            Assert.That(
                (Vector2)particles.First().transform.position,
                Is.EqualTo(contactPosition).Using(comparer),
                $"Death particles should've spawned at contact position!"
            );
            yield return WaitFor(
                "ii: deactivating of robot",
                () => !robot.gameObject.activeSelf
            );
            robot.rigidbody.gravityScale = 0;
            yield return new WaitForSeconds(lava.deathDuration);
            yield return WaitFor(
                $"iv: moving of robot back to spawn {startPosition}",
                () => comparer.Equals(startPosition, robot.rigidbody.position)
            );
            yield return WaitFor(
                $"v: re-activating of robot",
                () => robot.gameObject.activeSelf
            );
            yield return WaitFor(
                $"vi: setting of camera's '{nameof(camera.moveDuration)}' to lava's '{nameof(lava.respawnDuration)}'",
                () => Mathf.Approximately(camera.moveDuration, lava.respawnDuration)
            );
            yield return new WaitForSeconds(camera.moveDuration);
            yield return WaitFor(
                $"vi: restoring of camera's '{nameof(camera.moveDuration)}'",
                () => Mathf.Approximately(camera.moveDuration, cameraDuration)
            );
        }
        [UnityTest]
        public IEnumerator T04e_VerifyRobotDeathCount([ValueSource(nameof(V2_POSITIONS))] Vector2 position) {
            var startPosition = position + new Vector2(0, 2);
            var contactPosition = Vector2.zero;
            var comparer = new Vector2EqualityComparer(0.1f);
            var particles = new HashSet<ParticleSystem>();
            var camera = InstantiateCamera();
            var robot = InstantiateRobot(startPosition);
            var lava = InstantiateLava(position);
            camera.target = robot.transform;
            camera.moveDuration = 0.1f;
            lava.deathDuration = 0.1f;
            lava.respawnDuration = 0.1f;
            Assert.AreEqual(0, robot.deathCount, $"{robot}'s '{nameof(robot.deathCount)}' should start out null!");
            for (int i = 1; i <= 3; i++) {
                yield return WaitFor(
                    $"the robot's death",
                    () => !robot.gameObject.activeSelf
                );
                Assert.AreEqual(i, robot.deathCount, $"{robot}'s '{nameof(robot.deathCount)}' should've increased!");
                yield return WaitFor(
                    $"the robot's respawning",
                    () => robot.gameObject.activeSelf
                );
            }
        }
        [Test]
        public void T06a_VerifyHUDPrefab() {
            var hud = LoadHUDPrefab();
            Assert.IsNotNull(hud.coinText, $"{hud} must have a {typeof(TextMeshProUGUI)} component for coins!");
            Assert.IsNotNull(hud.deathText, $"{hud} must have a {typeof(TextMeshProUGUI)} component for deaths!");
        }
        [UnityTest]
        public IEnumerator T06b_VerifyHUDTexts(
            [ValueSource(nameof(I_AMOUNTS))] int currentCoinCount,
            [ValueSource(nameof(I_AMOUNTS))] int maximumCoinCount,
            [ValueSource(nameof(I_AMOUNTS))] int deathCount) {
            var camera = InstantiateCamera();
            var robot = InstantiateRobot(Vector3.zero);
            camera.target = robot.transform;
            robot.rigidbody.gravityScale = 0;
            string coinText() => $"Coins collected: {robot.currentCoinCount}/{robot.maximumCoinCount}";
            string deathText() => $"Deaths: {robot.deathCount}";
            var hud = InstantiateHUD();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(coinText(), hud.coinText.text, $"{hud}'s coin text did not meet expectations!");
            Assert.AreEqual(deathText(), hud.deathText.text, $"{hud}'s death text did not meet expectations!");
            robot.currentCoinCount = currentCoinCount;
            robot.maximumCoinCount = maximumCoinCount;
            robot.deathCount = deathCount;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(coinText(), hud.coinText.text, $"{hud}'s coin text did not update itself!");
            Assert.AreEqual(deathText(), hud.deathText.text, $"{hud}'s death text did not update itself!");
        }
        [Test]
        public void T07a_SceneExists() {
            FileAssert.Exists(SCENE_PATH);
        }
        [UnityTest]
        public IEnumerator T07b_SceneContainsStuff() {
            yield return LoadTestScene(SCENE_NAME);
            Assert.AreEqual(1, FindObjectsInScene<Rigidbody2D>().Length, $"{SCENE_NAME} should contain exactly 1 Rigibody2D (the robot)!");
            Assert.GreaterOrEqual(FindObjectsInScene<BoxCollider2D>().Length, 10, $"{SCENE_NAME} should contain at least 10 BoxCollider2D (the platforms and lava platforms)!");
            Assert.GreaterOrEqual(FindObjectsInScene<CircleCollider2D>().Length, 5, $"{SCENE_NAME} should contain at least 5 CircleCollider2D (the coins)!");
        }

        IEnumerator WaitFor(string condition, Func<bool> predicate) {
            float timeout = Time.time + TIMEOUT;
            yield return new WaitUntil(() => predicate() || Time.time > timeout);
            Assert.IsTrue(predicate(), $"We waited for {TIMEOUT} seconds, but '{condition}' didn't happen!");
        }

        RobotBridge LoadRobotPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_ROBOT);
            return new RobotBridge(prefab);
        }
        CoinBridge LoadCoinPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_COIN);
            return new CoinBridge(prefab);
        }
        LavaBridge LoadLavaPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_LAVA);
            return new LavaBridge(prefab);
        }
        CameraBridge LoadCameraPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_CAMERA);
            return new CameraBridge(prefab);
        }
        HUDBridge LoadHUDPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_HUD);
            return new HUDBridge(prefab);
        }

        RobotBridge InstantiateRobot(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_ROBOT);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new RobotBridge(instance, true);
        }
        CoinBridge InstantiateCoin(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_COIN);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new CoinBridge(instance);
        }
        LavaBridge InstantiateLava(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_LAVA);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new LavaBridge(instance);
        }
        CameraBridge InstantiateCamera(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_CAMERA);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            var camera = new CameraBridge(instance);
            camera.target = camera.transform;
            return camera;
        }
        CameraBridge InstantiateCamera() => InstantiateCamera(new Vector3(0, 5, -10));
        HUDBridge InstantiateHUD() {
            var prefab = TestUtils.LoadPrefab(PREFAB_HUD);
            var instance = InstantiateGameObject(prefab, Vector3.zero, Quaternion.identity);
            return new HUDBridge(instance);
        }
    }
}