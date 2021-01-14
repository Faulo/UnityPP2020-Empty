using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat07 : TestSuite
    {
        private class MarioBridge : GameObjectBridge
        {
            public bool isGrounded
            {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            private readonly FieldBridge<bool> isGroundedBridge;
            public bool isJumping
            {
                get => isJumpingBridge.value;
                set => isJumpingBridge.value = value;
            }
            private readonly FieldBridge<bool> isJumpingBridge;
            public float jumpSpeed
            {
                get => jumpSpeedBridge.value;
                set => jumpSpeedBridge.value = value;
            }
            private readonly FieldBridge<float> jumpSpeedBridge;
            public float jumpForwardBoost
            {
                get => jumpForwardBoostBridge.value;
                set => jumpForwardBoostBridge.value = value;
            }
            private readonly FieldBridge<float> jumpForwardBoostBridge;
            public float jumpStopSpeed
            {
                get => jumpStopSpeedBridge.value;
                set => jumpStopSpeedBridge.value = value;
            }
            private readonly FieldBridge<float> jumpStopSpeedBridge;
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

            public IColorable iColorable
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
            public Rigidbody2D rigidbody
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
                isJumpingBridge = FindField<bool>(nameof(isJumping));
                jumpSpeedBridge = FindField<float>(nameof(jumpSpeed));
                jumpForwardBoostBridge = FindField<float>(nameof(jumpForwardBoost));
                jumpStopSpeedBridge = FindField<float>(nameof(jumpStopSpeed));
                defaultAccelerationBridge = FindField<float>(nameof(defaultAcceleration));
                maximumSpeedBridge = FindField<float>(nameof(maximumSpeed));
                iColorable = FindInterface<IColorable>();
                renderer = FindComponentInChildren<Renderer>();
                rigidbody = FindComponentInChildren<Rigidbody2D>();
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
        [Serializable]
        public class Move
        {
            public KeyControl[] keys;
            public int sign;

            public override string ToString()
            {
                return string.Join("+", keys.Select(key => key.name));
            }
        }

        private static string[] PREFAB_ALL => new[] { PREFAB_MARIO, PREFAB_PLATFORM_ICE, PREFAB_PLATFORM_METAL, PREFAB_PLATFORM_DIRT, PREFAB_PARTICLE };
        private static string[] PREFAB_AVATARS => new[] { PREFAB_MARIO };
        private static string[] PREFAB_PLATFORMS => new[] { PREFAB_PLATFORM_ICE, PREFAB_PLATFORM_METAL, PREFAB_PLATFORM_DIRT };
        private static readonly string PREFAB_MARIO = "Assets/Prefabs/Mario.prefab";
        private static readonly string PREFAB_PLATFORM_ICE = "Assets/Prefabs/Platform_Ice.prefab";
        private static readonly string PREFAB_PLATFORM_METAL = "Assets/Prefabs/Platform_Metal.prefab";
        private static readonly string PREFAB_PLATFORM_DIRT = "Assets/Prefabs/Platform_Dirt.prefab";
        private static readonly string PREFAB_PARTICLE = "Assets/Prefabs/ContactParticles.prefab";

        private static readonly (Color, Color, Color)[] MARIO_COLORS = new[] {
            (Color.red, Color.blue, Color.yellow),
            (Color.white, Color.black, Color.gray),
        };

        private const float SCENE_TIMEOUT = 5;

        private static readonly float[] BOOST_SPEEDS = new float[] {
            4,
            8,
            0
        };
        private static readonly float[] JUMP_SPEEDS = new float[] {
            4,
            8
        };
        private static readonly float[] JUMP_STOP_SPEEDS = new float[] {
            2,
            0
        };

        private static Move[] MOVEMENT_DIRECTIONS
        {
            get
            {
                Keyboard keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
                return new[]
                {
                    new Move {
                        keys = new[] { keyboard.spaceKey },
                        sign = 0
                    },
                    new Move {
                        keys = new[] { keyboard.spaceKey, keyboard.leftArrowKey },
                        sign = -1
                    },
                    new Move {
                        keys = new[] { keyboard.spaceKey, keyboard.rightArrowKey },
                        sign = 1
                    },
                };
            }
        }

        [Test]
        public void T01_VerifyMarioPrefab()
        {
            MarioBridge mario = LoadMarioPrefab();
            Assert.IsNotNull(mario.rigidbody, $"'{PREFAB_MARIO}' needs a Rigidbody2D!");
            Assert.AreEqual(1, mario.rigidbody.gravityScale, "Mario's Rigidbody's gravity scale should be 1!");
            Assert.IsFalse(mario.rigidbody.isKinematic, "Mario's Rigidbody's must not be kinematic!");
            Assert.Greater(mario.jumpForwardBoost, 0, $"Mario's {nameof(mario.jumpForwardBoost)} should be greater than zero!");
            Assert.GreaterOrEqual(mario.jumpStopSpeed, 0, $"Mario's {nameof(mario.jumpStopSpeed)} should be greater than zero!");
            Assert.Greater(mario.jumpSpeed, 0, $"Mario's {nameof(mario.jumpSpeed)} should be greater than zero!");
        }

        [UnityTest]
        public IEnumerator T02a_MarioJumpSpeed([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move, [ValueSource(nameof(BOOST_SPEEDS))] float jumpSpeed)
        {
            yield return SpawnMarioOnPlatform();

            mario.jumpSpeed = jumpSpeed;
            Vector2 velocity = mario.rigidbody.velocity;

            using (new InputPress(input, move.keys))
            {
                yield return new WaitForFixedUpdate();
                float deltaY = mario.rigidbody.velocity.y - velocity.y;
                Assert.LessOrEqual(deltaY, jumpSpeed, $"With input {move}, Mario's Rigidbody2D's vertical velocity should become about equal to his '{nameof(mario.jumpSpeed)}'!");
            }
        }

        [UnityTest]
        public IEnumerator T02b_MarioForwardBoost([ValueSource(nameof(MOVEMENT_DIRECTIONS))] Move move, [ValueSource(nameof(BOOST_SPEEDS))] float jumpForwardBoost)
        {
            yield return SpawnMarioOnPlatform();

            mario.jumpForwardBoost = jumpForwardBoost;
            Vector2 velocity = mario.rigidbody.velocity;

            using (new InputPress(input, move.keys))
            {
                yield return new WaitForFixedUpdate();
                float deltaX = mario.rigidbody.velocity.x - velocity.x;
                switch (move.sign)
                {
                    case 1:
                    case -1:
                        Assert.Greater(move.sign * deltaX, mario.jumpForwardBoost, $"With input {move}, Mario's Rigidbody2D's horizontal velocity should be greater than his '{nameof(mario.jumpForwardBoost)}'!");
                        break;
                    case 0:
                        Assert.AreEqual(deltaX, 0, $"Without input, Mario's Rigidbody2D's horizontal velocity should stay zero!");
                        break;
                }
            }
        }

        [UnityTest]
        public IEnumerator T03a_HoldSpaceForever([ValueSource(nameof(JUMP_SPEEDS))] float jumpSpeed, [ValueSource(nameof(JUMP_STOP_SPEEDS))] float jumpStopSpeed)
        {
            Keyboard keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();

            yield return SpawnMarioOnPlatform();

            mario.jumpSpeed = jumpSpeed;
            mario.jumpStopSpeed = jumpStopSpeed;

            using (new InputPress(input, new[] { keyboard.spaceKey }))
            {
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(mario.isJumping, $"After pressing space, Mario's {nameof(mario.isJumping)}' should have become true!");
                var timeout = Time.time + SCENE_TIMEOUT;
                yield return new WaitUntil(() => !mario.isJumping || Time.time > timeout);
                Assert.IsFalse(mario.isJumping, $"After waiting {SCENE_TIMEOUT}s, Mario should have stopped jumping!");
                Assert.LessOrEqual(mario.rigidbody.velocity.y, jumpStopSpeed, $"When stopping to jump, Mario's Rigidbody's vertical velocity should be less than his '{nameof(mario.jumpStopSpeed)}'!");
            }
        }

        [UnityTest]
        public IEnumerator T03b_TapSpaceBriefly([ValueSource(nameof(JUMP_SPEEDS))] float jumpSpeed, [ValueSource(nameof(JUMP_STOP_SPEEDS))] float jumpStopSpeed)
        {
            Keyboard keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();

            yield return SpawnMarioOnPlatform();

            mario.jumpSpeed = jumpSpeed;
            mario.jumpStopSpeed = jumpStopSpeed;

            using (new InputPress(input, new[] { keyboard.spaceKey }))
            {
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(mario.isJumping, $"After pressing space, Mario's {nameof(mario.isJumping)}' should have become true!");
            }
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(mario.isJumping, $"After releasing space, Mario should have stopped jumping!");
            Assert.LessOrEqual(mario.rigidbody.velocity.y, jumpStopSpeed, $"When stopping to jump, Mario's Rigidbody's vertical velocity should be less than his '{nameof(mario.jumpStopSpeed)}'!");
        }

        [UnityTest]
        public IEnumerator T03c_PressSpaceWhileAirborne([ValueSource(nameof(JUMP_SPEEDS))] float jumpSpeed, [ValueSource(nameof(JUMP_STOP_SPEEDS))] float jumpStopSpeed)
        {
            Keyboard keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();

            InstantiateMario(Vector3.up);
            mario.isGrounded = false;
            mario.isJumping = false;
            yield return new WaitForFixedUpdate();

            mario.jumpSpeed = jumpSpeed;
            mario.jumpStopSpeed = jumpStopSpeed;

            using (new InputPress(input, new[] { keyboard.spaceKey }))
            {
                yield return new WaitForFixedUpdate();
                Assert.IsFalse(mario.isJumping, $"After pressing space while airborne, Mario's {nameof(mario.isJumping)}' should have stayed false!");
            }
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(mario.isJumping, $"After pressing space while airborne, Mario's {nameof(mario.isJumping)}' should have stayed false!");
        }

        [Test]
        public void T04a_MarioImplementsIColorable()
        {
            GameObject marioPrefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            MarioBridge mario = new MarioBridge(marioPrefab);
            Assert.IsNotNull(mario.renderer, $"'{PREFAB_MARIO}' needs a Renderer!");
            Assert.IsNotNull(mario.iColorable, $"'{PREFAB_MARIO}' must contain a script that implements {nameof(IColorable)}!");
        }
        [UnityTest]
        public IEnumerator T04b_IColorarableWorks([ValueSource(nameof(MARIO_COLORS))] (Color, Color, Color) colors)
        {
            yield return SpawnMarioOnPlatform();

            mario.iColorable.SetColors(colors.Item1, colors.Item2, colors.Item3);
            yield return new WaitForFixedUpdate();

            mario.isGrounded = true;
            mario.isJumping = true;
            CustomAssert.AreEqual(
                colors.Item1, mario.iColorable.GetCurrentColor(),
                $"When grounded, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'groundedColor'!"
            );
            yield return null;

            CustomAssert.AreEqual(
                mario.iColorable.GetCurrentColor(), mario.rendererColor,
                $"When grounded, Mario's MeshRenderer's Material's color should become {nameof(IColorable.SetColors)}'s 'groundedColor'!"
            );
            yield return new WaitForFixedUpdate();

            mario.isGrounded = true;
            mario.isJumping = false;
            CustomAssert.AreEqual(
                colors.Item1, mario.iColorable.GetCurrentColor(),
                $"When grounded, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'groundedColor'!"
            );

            yield return null;
            CustomAssert.AreEqual(
                mario.iColorable.GetCurrentColor(), mario.rendererColor,
                $"When grounded, Mario's MeshRenderer's Material's color should become {nameof(IColorable.SetColors)}'s 'groundedColor'!"
            );

            mario.isGrounded = false;
            mario.isJumping = true;
            CustomAssert.AreEqual(
                colors.Item2, mario.iColorable.GetCurrentColor(),
                $"When airborne and jumping, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'jumpingColor'!"
            );
            yield return null;
            CustomAssert.AreEqual(
                mario.iColorable.GetCurrentColor(), mario.rendererColor,
                $"When airborne and jumping, Mario's MeshRenderer's Material's color should become {nameof(IColorable.SetColors)}'s 'jumpingColor'!"
            );
            yield return new WaitForFixedUpdate();

            mario.isGrounded = false;
            mario.isJumping = false;
            CustomAssert.AreEqual(
                colors.Item3, mario.iColorable.GetCurrentColor(),
                $"When airborne and falling, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'fallingColor'!"
            );
            yield return null;
            CustomAssert.AreEqual(
                mario.iColorable.GetCurrentColor(), mario.rendererColor,
                $"When airborne and falling, Mario's MeshRenderer's Material's color should become {nameof(IColorable.SetColors)}'s 'fallingColor'!"
            );
            yield return new WaitForFixedUpdate();
        }
        private MarioBridge LoadMarioPrefab()
        {

            GameObject prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            return new MarioBridge(prefab);
        }
        private MarioBridge mario;
        private PlatformBridge platform;

        private IEnumerator SpawnMarioOnPlatform()
        {
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

        private void InstantiateMario(Vector3 position)
        {
            GameObject prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            mario = new MarioBridge(instance, true);
        }
        private void InstantiatePlatform(Vector3 position, string prefabFile)
        {
            GameObject prefab = TestUtils.LoadPrefab(prefabFile);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            platform = new PlatformBridge(instance);
        }
    }
}
