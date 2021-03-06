﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Tests {
    [TestFixture]
    public class Testat08 : TestSuite {
        class MarioBridge : GameObjectBridge {
            public bool isGrounded {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            readonly FieldBridge<bool> isGroundedBridge;
            public bool isJumping {
                get => isJumpingBridge.value;
                set => isJumpingBridge.value = value;
            }
            readonly FieldBridge<bool> isJumpingBridge;
            public float jumpSpeed {
                get => jumpSpeedBridge.value;
                set => jumpSpeedBridge.value = value;
            }
            readonly FieldBridge<float> jumpSpeedBridge;
            public float jumpForwardBoost {
                get => jumpForwardBoostBridge.value;
                set => jumpForwardBoostBridge.value = value;
            }
            readonly FieldBridge<float> jumpForwardBoostBridge;
            public float jumpStopSpeed {
                get => jumpStopSpeedBridge.value;
                set => jumpStopSpeedBridge.value = value;
            }
            readonly FieldBridge<float> jumpStopSpeedBridge;
            public float defaultAcceleration {
                get => defaultAccelerationBridge.value;
                set => defaultAccelerationBridge.value = value;
            }
            readonly FieldBridge<float> defaultAccelerationBridge;
            public float maximumSpeed {
                get => maximumSpeedBridge.value;
                set => maximumSpeedBridge.value = value;
            }
            readonly FieldBridge<float> maximumSpeedBridge;

            public Renderer renderer {
                get;
                private set;
            }
            public AnimatorStateInfo animatorState => animator.GetCurrentAnimatorStateInfo(0);
            public Animator animator {
                get;
                private set;
            }
            public Physics2DEvents physics {
                get;
                private set;
            }

            public MarioBridge(GameObject gameObject, bool isInstance = false) : base(gameObject) {
                isGroundedBridge = FindField<bool>(nameof(isGrounded));
                isJumpingBridge = FindField<bool>(nameof(isJumping));
                jumpSpeedBridge = FindField<float>(nameof(jumpSpeed));
                jumpForwardBoostBridge = FindField<float>(nameof(jumpForwardBoost));
                jumpStopSpeedBridge = FindField<float>(nameof(jumpStopSpeed));
                defaultAccelerationBridge = FindField<float>(nameof(defaultAcceleration));
                maximumSpeedBridge = FindField<float>(nameof(maximumSpeed));
                renderer = FindComponentInChildren<Renderer>();
                animator = FindComponentInChildren<Animator>();
                if (isInstance) {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
        }
        public class PlatformBridge : GameObjectBridge {
            public float allowedAcceleration {
                get => allowedAccelerationBridge.value;
                set => allowedAccelerationBridge.value = value;
            }
            readonly FieldBridge<float> allowedAccelerationBridge;
            public float jumpSpeedMultiplier {
                get => jumpSpeedMultiplierBridge.value;
                set => jumpSpeedMultiplierBridge.value = value;
            }
            readonly FieldBridge<float> jumpSpeedMultiplierBridge;

            public BoxCollider2D collider {
                get;
                private set;
            }
            public Color rendererColor => renderer.material.color;
            public Renderer renderer {
                get;
                private set;
            }

            public PlatformBridge(GameObject gameObject) : base(gameObject) {
                allowedAccelerationBridge = FindField<float>(nameof(allowedAcceleration));
                jumpSpeedMultiplierBridge = FindField<float>(nameof(jumpSpeedMultiplier));
                collider = FindComponent<BoxCollider2D>();
                renderer = FindComponentInChildren<Renderer>();
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

        static readonly string PREFAB_MARIO = "Assets/Prefabs/Mario.prefab";
        static readonly string PREFAB_PLATFORM_DIRT = "Assets/Prefabs/Platform_Dirt.prefab";

        static Move[] MOVEMENT_DIRECTIONS {
            get {
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
        public void T03_VerifyGitIgnore() {
            var file = new FileInfo("./.gitignore");
            FileAssert.Exists(file);
            StringAssert.Contains("InitTestScene", File.ReadAllText(file.FullName));
        }

        static readonly string[] ASSET_DIRECTORIES = new string[]
        {
            "Assets/Standard Assets/2D/Animations",
            "Assets/Standard Assets/2D/Animator",
            "Assets/Standard Assets/2D/Sprites"
        };

        [Test]
        public void T04_StandardAssetsImported([ValueSource(nameof(ASSET_DIRECTORIES))] string path) {
            var directory = new DirectoryInfo(path);
            DirectoryAssert.Exists(directory);
        }

        [Test]
        public void T05a_TestSpriteRenderer() {
            var mario = LoadMarioPrefab();
            Assert.IsNotNull(mario.renderer, $"'{PREFAB_MARIO}' needs a Renderer!");
            Assert.IsInstanceOf(typeof(SpriteRenderer), mario.renderer);
            if (mario.renderer is SpriteRenderer spriteRenderer) {
                Assert.IsNotNull(spriteRenderer.sprite, "Mario's SpriteRenderer needs a sprite assigned!");
                Assert.AreEqual("RobotBoyIdle00", spriteRenderer.sprite.name, "Mario's SpriteRenderer should use RobotBoyIdle00!");
                Assert.IsFalse(spriteRenderer.flipX, $"By default, Mario should face right!");
            }
        }

        [UnityTest]
        public IEnumerator T05b_TestSpriteRendererFlipping([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            yield return SpawnMarioOnPlatform();
            var spriteRenderer = mario.renderer as SpriteRenderer;
            Assert.IsNotNull(spriteRenderer);
            using (new InputPress(input, move.keys)) {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(spriteRenderer.flipX, move.sign == -1, $"While pressing '{move}', Mario's SpriteRenderer's flipX should've changed!");
            }
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(spriteRenderer.flipX, move.sign == -1, $"After releasing '{move}', Mario's SpriteRenderer's flipX shouldn't have changed!");
        }

        [Test]
        public void T06a_TestAnimator() {
            var mario = LoadMarioPrefab();
            Assert.IsNotNull(mario.animator, $"'{PREFAB_MARIO}' needs an Animator!");
            Assert.IsNotNull(mario.renderer, $"'{PREFAB_MARIO}' needs a Renderer!");
            Assert.IsNotNull(mario.animator.runtimeAnimatorController, $"Mario's Animator requires a controller!");
            Assert.AreEqual(mario.transform, mario.animator.transform.parent, $"Mario's Animator must be on a child GameObject!");
            Assert.AreEqual(mario.animator.transform, mario.renderer.transform, $"Mario's Animator and Mario's Renderer must be on the same GameObject!");
        }

        [UnityTest]
        public IEnumerator T06b_TestAnimatorStates([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            yield return SpawnMarioOnPlatform();
            yield return WaitForState("Idle", "After spawning, ");
            using (new InputPress(input, move.keys)) {
                yield return WaitForState(move.sign == 0 ? "Idle" : "Walk", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
                yield return WaitForState(move.sign == 0 ? "Idle" : "Run", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
            }
            yield return WaitForState("Idle", $"After releasing '{move}' and waiting {SCENE_TIMEOUT}s, ");
        }

        [UnityTest]
        public IEnumerator T07a_TestCrouchingGrounded([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            yield return SpawnMarioOnPlatform();
            yield return WaitForState("Idle", "After spawning, ");
            using (new InputPress(input, keyboard.downArrowKey)) {
                yield return WaitForState("Crouch", $"While pressing '{keyboard.downArrowKey}' and waiting {SCENE_TIMEOUT}s, ");
                using (new InputPress(input, move.keys)) {
                    yield return WaitForState(move.sign == 0 ? "Crouch" : "CrouchingWalk", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
                }
                yield return WaitForState("Crouch", $"After releasing '{move}' and waiting {SCENE_TIMEOUT}s, ");
            }
            yield return WaitForState("Idle", $"After releasing '{keyboard.downArrowKey}' and waiting {SCENE_TIMEOUT}s, ");
        }

        [UnityTest]
        public IEnumerator T07b_TestCrouchingAirborne([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move) {
            InstantiateMario(Vector3.zero);
            mario.isGrounded = false;
            yield return WaitForState("Jumping", "After spawning in free-fall, ");
            using (new InputPress(input, keyboard.downArrowKey)) {
                yield return WaitForState("Jumping", $"While pressing '{keyboard.downArrowKey}' and waiting {SCENE_TIMEOUT}s, ");
                using (new InputPress(input, move.keys)) {
                    yield return WaitForState("Jumping", $"While pressing '{move}' and waiting {SCENE_TIMEOUT}s, ");
                }
                yield return WaitForState("Jumping", $"After releasing '{move}' and waiting {SCENE_TIMEOUT}s, ");
            }
            yield return WaitForState("Jumping", $"After releasing '{keyboard.downArrowKey}' and waiting {SCENE_TIMEOUT}s, ");
        }

        MarioBridge LoadMarioPrefab() {
            var prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            return new MarioBridge(prefab);
        }

        MarioBridge mario;
        PlatformBridge platform;

        IEnumerator SpawnMarioOnPlatform() {
            InstantiateMario(Vector3.up);
            mario.isGrounded = false;
            mario.isJumping = false;
            mario.defaultAcceleration = 20;
            mario.maximumSpeed = 3;
            mario.jumpSpeed = 4;
            mario.jumpStopSpeed = 2;
            mario.jumpForwardBoost = 1;
            InstantiatePlatform(Vector3.down, PREFAB_PLATFORM_DIRT);
            platform.jumpSpeedMultiplier = 1;
            platform.allowedAcceleration = mario.defaultAcceleration;
            platform.collider.sharedMaterial = null;
            platform.collider.size = new Vector2(100, 1);
            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => mario.isGrounded || Time.time > timeout);
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(mario.isGrounded, $"Mario's should've become grounded, but didn't!");
        }
        IEnumerator WaitForState(string state, string message) {
            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => mario.animatorState.IsName(state) || Time.time > timeout);
            Assert.IsTrue(mario.animatorState.IsName(state), $"{message}Mario should've arrived in state {state}, but didn't!");
        }

        void InstantiateMario(Vector3 position) {
            var prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            mario = new MarioBridge(instance, true);
        }
        void InstantiatePlatform(Vector3 position, string prefabFile) {
            var prefab = TestUtils.LoadPrefab(prefabFile);
            var instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            platform = new PlatformBridge(instance);
        }
    }
}