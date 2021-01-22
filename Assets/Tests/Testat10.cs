using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Tests {
    public class Testat10 : TestSuite {
        class RobotBridge : GameObjectBridge {
            public RobotBridge(GameObject gameObject, bool isInstance = false) : base(gameObject) {
                if (isInstance) {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
            public readonly Physics2DEvents physics;
            public Rigidbody2D rigidbody => FindComponent<Rigidbody2D>();
            public BoxCollider2D collider => FindComponent<BoxCollider2D>();
            public SpriteRenderer renderer => FindComponentInChildren<SpriteRenderer>();
            public Animator animator => FindComponentInChildren<Animator>();
            public AnimatorStateInfo animatorState => animator.GetCurrentAnimatorStateInfo(0);
            public float maximumSpeed {
                get => FindField<float>(nameof(maximumSpeed)).value;
                set => FindField<float>(nameof(maximumSpeed)).value = value;
            }
            public float jumpStartSpeed {
                get => FindField<float>(nameof(jumpStartSpeed)).value;
                set => FindField<float>(nameof(jumpStartSpeed)).value = value;
            }
            public float jumpStopSpeed {
                get => FindField<float>(nameof(jumpStopSpeed)).value;
                set => FindField<float>(nameof(jumpStopSpeed)).value = value;
            }
            public float accelerationDuration {
                get => FindField<float>(nameof(accelerationDuration)).value;
                set => FindField<float>(nameof(accelerationDuration)).value = value;
            }
            public int doubleJumpCount {
                get => FindField<int>(nameof(doubleJumpCount)).value;
                set => FindField<int>(nameof(doubleJumpCount)).value = value;
            }
            public bool isGrounded {
                get => FindField<bool>(nameof(isGrounded)).value;
                set => FindField<bool>(nameof(isGrounded)).value = value;
            }
            public bool isJumping {
                get => FindField<bool>(nameof(isJumping)).value;
                set => FindField<bool>(nameof(isJumping)).value = value;
            }
            public bool isCrouching {
                get => FindField<bool>(nameof(isCrouching)).value;
                set => FindField<bool>(nameof(isCrouching)).value = value;
            }
        }
        public class PlatformBridge : GameObjectBridge {
            public BoxCollider2D collider => FindComponent<BoxCollider2D>();
            public PlatformBridge(GameObject gameObject) : base(gameObject) {
            }
        }
        public class TeleporterBridge : GameObjectBridge {
            public TeleporterBridge(GameObject gameObject) : base(gameObject) {
            }
            public Transform target {
                get => FindField<Transform>(nameof(target)).value;
                set => FindField<Transform>(nameof(target)).value = value;
            }
            public Vector2 ejectVelocity {
                get => FindField<Vector2>(nameof(ejectVelocity)).value;
                set => FindField<Vector2>(nameof(ejectVelocity)).value = value;
            }
        }
        [Serializable]
        public class Move {
            public KeyControl[] keys;
            public int sign;

            public override string ToString() {
                return string.Join("+", keys.Select(key => key.name));
            }
        }

        const float SCENE_TIMEOUT = 1;
        static readonly Vector2[] V2_POSITIONS = new[] { new Vector2(2, 3), new Vector2(-4, 2) };
        static readonly int[] DOUBLE_JUMPS = new[] { 0, 1, 2 };

        static readonly string PREFAB_ROBOT = "Assets/Prefabs/Robot.prefab";
        static readonly string PREFAB_TELEPORTER = "Assets/Prefabs/Teleporter.prefab";
        static readonly string[] ANIMATOR_FILES = new[] {
            "Assets/Art/Animators/Robot.controller",
            "Assets/Art/Animations/Robot_Crouch.anim",
            "Assets/Art/Animations/Robot_CrouchingWalk.anim",
            "Assets/Art/Animations/Robot_Fall.anim",
            "Assets/Art/Animations/Robot_Idle.anim",
            "Assets/Art/Animations/Robot_Jump.anim",
            "Assets/Art/Animations/Robot_Run.anim",
            "Assets/Art/Animations/Robot_Walk.anim",
        };
        static Move[] MOVEMENT_DIRECTIONS {
            get {
                var keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
                return new[]
                {
                    new Move {
                        keys = Array.Empty<KeyControl>(),
                        sign = 0
                    },
                    new Move {
                        keys = new[] { keyboard.leftArrowKey },
                        sign = -1
                    },
                    new Move {
                        keys = new[] { keyboard.rightArrowKey },
                        sign = 1
                    },
                };
            }
        }

        [Test]
        public void T02a_VerifyRobotPrefab() {
            var robot = LoadRobotPrefab();
            Assert.GreaterOrEqual(robot.accelerationDuration, 0, $"{robot}'s '{nameof(robot.accelerationDuration)}' must be positive!");
            Assert.GreaterOrEqual(robot.doubleJumpCount, 0, $"{robot}'s '{nameof(robot.doubleJumpCount)}' must be positive!");

            Assert.Greater(robot.jumpStartSpeed, 0, $"{robot}'s '{nameof(robot.jumpStartSpeed)}' must be positive!");
            Assert.Greater(robot.jumpStopSpeed, 0, $"{robot}'s '{nameof(robot.jumpStopSpeed)}' must be positive!");
            Assert.Greater(robot.maximumSpeed, 0, $"{robot}'s '{nameof(robot.jumpStopSpeed)}' must be positive!");

            Assert.IsNotNull(robot.rigidbody, $"{robot} must have a {typeof(Rigidbody)} component!");
            Assert.IsNotNull(robot.collider, $"{robot} must have a {typeof(BoxCollider2D)} component!");
            Assert.IsNotNull(robot.renderer, $"{robot} must have a {typeof(SpriteRenderer)} component!");
            Assert.IsNotNull(robot.animator, $"{robot} must have a {typeof(Animator)} component!");
        }
        [Test]
        public void T02b_AnimatorFileExists([ValueSource(nameof(ANIMATOR_FILES))] string path) {
            var file = new FileInfo(path);
            FileAssert.Exists(file);
            string name = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            var robot = LoadRobotPrefab();
            if (file.Extension == ".controller") {
                Assert.AreEqual(name, robot.animator.runtimeAnimatorController.name);
            }
            if (file.Extension == ".anim") {
                CollectionAssert.Contains(
                   robot.animator.runtimeAnimatorController.animationClips.Select(anim => anim.name),
                   name,
                   $"Animation '{file}' should be used in {robot}'s animator!"
                );
            }
        }
        [UnityTest]
        public IEnumerator T02c_VerifyRobotMovement([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            yield return SpawnRobotOnPlatform();
            Assert.AreEqual(Vector2.zero, robot.rigidbody.velocity, $"After spawning, {robot}'s velocity should be zero!");
            yield return WaitForState("Idle", "After spawning, ");
            using (new InputPress(input, move.keys)) {
                float timeout = Time.time + robot.accelerationDuration;
                yield return WaitForState(move.sign == 0 ? "Idle" : "Walk", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
                yield return WaitForState(move.sign == 0 ? "Idle" : "Run", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
                if (move.sign != 0) {
                    float targetSpeed = robot.maximumSpeed * 0.9f;
                    yield return new WaitUntil(() => Time.time > timeout);
                    Assert.Less(robot.rigidbody.velocity.x * move.sign, targetSpeed, $"{robot} should not reach their maximum speed until after his '{nameof(robot.accelerationDuration)}' of {robot.accelerationDuration}s!");
                    Assert.AreEqual(move.sign, Math.Sign(robot.rigidbody.velocity.x), $"While pressing '{move}', {robot}'s direction should be '{move.sign}'!");
                    timeout = Time.time + SCENE_TIMEOUT;
                    yield return new WaitUntil(() => robot.rigidbody.velocity.x * move.sign > targetSpeed || Time.time > timeout);
                    Assert.GreaterOrEqual(robot.rigidbody.velocity.x * move.sign, targetSpeed, $"{robot} should reach their maximum speed of '{robot.maximumSpeed}', but was '{robot.rigidbody.velocity.x}'!");
                }
            }
            yield return WaitForState("Idle", $"After releasing '{move}' and waiting {SCENE_TIMEOUT}s, ", 20);
        }
        [UnityTest]
        public IEnumerator T02d_VerifyRobotCrouching([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            yield return SpawnRobotOnPlatform();
            yield return WaitForState("Idle", "After spawning, ");
            Assert.AreEqual(new Vector2(1, 2), robot.collider.size, $"When idle, {robot}'s collider should have a size of (1, 2)!");
            using (new InputPress(input, Keyboard.current.downArrowKey)) {
                yield return WaitForState("Crouch", $"While pressing '{Keyboard.current.downArrowKey}' and waiting {SCENE_TIMEOUT}s, ");
                Assert.AreEqual(new Vector2(1, 1), robot.collider.size, $"When crouching, {robot}'s collider should have a size of (1, 1)!");
                using (new InputPress(input, move.keys)) {
                    yield return WaitForState(move.sign == 0 ? "Crouch" : "CrouchingWalk", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
                }
                yield return WaitForState("Crouch", $"After releasing '{move}' and waiting {SCENE_TIMEOUT}s, ");
            }
            yield return WaitForState("Idle", $"After releasing '{Keyboard.current.downArrowKey}' and waiting {SCENE_TIMEOUT}s, ");
            Assert.AreEqual(new Vector2(1, 2), robot.collider.size, $"When idle, {robot}'s collider should have a size of (1, 2)!");
        }
        [UnityTest]
        public IEnumerator T02e_VerifyRobotJumpingWhenHoldingSpace() {
            yield return SpawnRobotOnPlatform();
            var key = Keyboard.current.spaceKey;
            Assert.AreEqual(0, FindObjectsInScene<ParticleSystem>().Length, $"Robot shouldn't have spawned ParticleSystems when landing!");
            using (new InputPress(input, key)) {
                yield return WaitForState("Jumping", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ");
                Assert.Greater(robot.rigidbody.velocity.y, robot.jumpStartSpeed * 0.75f, $"When jumping, {robot}'s vertical speed should become his '{nameof(robot.jumpStartSpeed)}'!");
                Assert.LessOrEqual(1, FindObjectsInScene<ParticleSystem>().Length, $"Robot should've spawned a ParticleSystem when jumping!");
                yield return WaitForState("Falling", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ");
                Assert.Less(robot.rigidbody.velocity.y, robot.jumpStopSpeed, $"When falling, {robot}'s vertical speed should become his '{nameof(robot.jumpStopSpeed)}'!");
                yield return WaitForState("Idle", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ", 20);
            }
            yield return WaitForState("Idle", $"After releasing '{key}'s, ", 20);
        }
        [UnityTest]
        public IEnumerator T02f_VerifyRobotJumpingWhenTappingSpace() {
            yield return SpawnRobotOnPlatform();
            var key = Keyboard.current.spaceKey;
            Assert.AreEqual(0, FindObjectsInScene<ParticleSystem>().Length, $"Robot shouldn't have spawned ParticleSystems when landing!");
            using (new InputPress(input, key)) {
                yield return WaitForState("Jumping", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ");
                Assert.Greater(robot.rigidbody.velocity.y, robot.jumpStartSpeed * 0.75f, $"When jumping, {robot}'s vertical speed should become his '{nameof(robot.jumpStartSpeed)}'!");
                Assert.AreEqual(1, FindObjectsInScene<ParticleSystem>().Length, $"Robot should've spawned a ParticleSystem when jumping!");
            }
            yield return WaitForState("Falling", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ");
            Assert.LessOrEqual(robot.rigidbody.velocity.y, robot.jumpStopSpeed, $"When falling, {robot}'s vertical speed should become his '{nameof(robot.jumpStopSpeed)}'!");
            yield return WaitForState("Idle", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ", 20);
        }
        [UnityTest]
        public IEnumerator T02g_VerifyRobotDoubleJump([ValueSource(nameof(DOUBLE_JUMPS))] int count) {
            yield return SpawnRobotOnPlatform();
            robot.doubleJumpCount = count;
            int frameCount = 5;
            var key = Keyboard.current.spaceKey;
            using (new InputPress(input, key)) {
                yield return WaitForState("Jumping", $"While pressing '{key}' and waiting {SCENE_TIMEOUT}s, ", frameCount);
                Assert.AreEqual(1, FindObjectsInScene<ParticleSystem>().Length, $"Robot should've spawned a ParticleSystem when jumping!");
            }
            yield return WaitForState("Falling", $"When releasing '{key}' and waiting {SCENE_TIMEOUT}s, ", frameCount);
            for (int i = 0; i < count; i++) {
                using (new InputPress(input, key)) {
                    yield return WaitForState("Jumping", $"While pressing '{key}' again and waiting {SCENE_TIMEOUT}s, ", frameCount);
                    Assert.AreEqual(i + 2, FindObjectsInScene<ParticleSystem>().Length, $"Robot should've spawned a ParticleSystem when jumping!");
                }
                yield return WaitForState("Falling", $"When releasing '{key}' and waiting {SCENE_TIMEOUT}s, ", frameCount);
            }
            using (new InputPress(input, key)) {
                for (int i = 0; i < frameCount; i++) {
                    Assert.IsTrue(robot.animatorState.IsName("Falling"), $"We used up all our double-jumps, so Robot should've kept falling!");
                    yield return new WaitForFixedUpdate();
                }
                yield return WaitForState("Idle", $"After expending all double-jumps waiting {SCENE_TIMEOUT}s, ", 20);
            }
            yield return WaitForState("Idle", $"After expending all double-jumps waiting {SCENE_TIMEOUT}s, ", 20);
        }
        [Test]
        public void T03a_VerifyTeleporterPrefab() {
            var teleporter = LoadTeleporterPrefab();
            Assert.IsInstanceOf<Transform>(teleporter.target, $"{teleporter} requires a field '{nameof(teleporter.target)}'!");
            Assert.IsNotNull(teleporter.target, $"{teleporter} requires is '{nameof(teleporter.target)}' to be assigned!");
            Assert.IsNotNull(teleporter.transform.GetComponent<ParticleSystem>(), $"{teleporter} requires a '{typeof(ParticleSystem)}' component!");
            Assert.IsNotNull(teleporter.target.GetComponent<ParticleSystem>(), $"{teleporter}'s target requires a '{typeof(ParticleSystem)}' component!");
        }
        [UnityTest]
        public IEnumerator T03b_VerifyTeleporterBehavior(
            [ValueSource(nameof(V2_POSITIONS))] Vector2 position,
            [ValueSource(nameof(V2_POSITIONS))] Vector2 velocity) {
            yield return SpawnRobotOnPlatform();
            robot.rigidbody.gravityScale = 0;

            InstantiateTeleporter(Vector3.up);
            teleporter.target.position = position;
            teleporter.ejectVelocity = velocity;
            yield return new WaitForFixedUpdate();

            var comparer = new Vector3EqualityComparer(0.1f);

            Assert.That(
                robot.rigidbody.position,
                Is.EqualTo(position).Using(comparer),
                $"Teleporter failed to teleport Robot to position {position}!"
            );
            Assert.That(
                robot.rigidbody.velocity,
                Is.EqualTo(velocity).Using(comparer),
                $"Teleporting should set Robot's velocity to its {nameof(teleporter.ejectVelocity)}!"
            );
            yield return WaitForState("Falling", $"After getting teleported, ", 30);
        }
        IEnumerator SpawnRobotOnPlatform() {
            InstantiateCamera(new Vector3(0, 0, -10));
            InstantiateRobot(new Vector3(0, 2, 0));
            robot.isCrouching = false;
            robot.isGrounded = false;
            robot.isJumping = false;
            robot.accelerationDuration = 0.1f;
            robot.maximumSpeed = 5;
            robot.jumpStartSpeed = 4;
            robot.jumpStopSpeed = 2;
            robot.doubleJumpCount = 1;

            InstantiatePlatform();
            platform.collider.size = new Vector2(100, 1);

            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => robot.isGrounded || Time.time > timeout);
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(robot.isGrounded, $"Robot should've become grounded, but didn't!");
        }
        IEnumerator WaitForState(string state, string message, int framesToStayInState = 0) {
            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => robot.animatorState.IsName(state) || Time.time > timeout);
            while (framesToStayInState >= 0) {
                Assert.IsTrue(robot.animatorState.IsName(state), $"{message}Robot should've arrived in state {state}, but didn't!");
                if (framesToStayInState == 0) {
                    break;
                }
                framesToStayInState--;
                yield return new WaitForFixedUpdate();
            }
        }
        RobotBridge LoadRobotPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_ROBOT);
            return new RobotBridge(prefab);
        }
        TeleporterBridge LoadTeleporterPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_TELEPORTER);
            return new TeleporterBridge(prefab);
        }

        RobotBridge robot;
        PlatformBridge platform;
        TeleporterBridge teleporter;

        void InstantiateRobot(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_ROBOT);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            robot = new RobotBridge(instance, true);
        }
        void InstantiatePlatform() {
            var instance = CreateGameObject("Platform");
            instance.AddComponent<BoxCollider2D>();
            platform = new PlatformBridge(instance);
        }
        void InstantiateTeleporter(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_TELEPORTER);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            teleporter = new TeleporterBridge(instance);
        }
        void InstantiateCamera(Vector3 position) {
            var instance = CreateGameObject("Camera");
            instance.AddComponent<Camera>();
            instance.transform.position = position;
        }
    }
}