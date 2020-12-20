using NUnit.Framework;
using System.Collections;
using UnityEngine;
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

            public IColorable iColorable
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
                iColorable = FindInterface<IColorable>();
                if (isInstance)
                {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
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

        [Test]
        public void T04a_MarioImplementsIColorable()
        {
            GameObject marioPrefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            MarioBridge mario = new MarioBridge(marioPrefab);
            Assert.IsNotNull(mario.iColorable, $"Mario '{PREFAB_MARIO}' must contain a script that implements {nameof(IColorable)}!");
        }
        [UnityTest]
        public IEnumerator T04b_IColorarableWorks([ValueSource(nameof(MARIO_COLORS))] (Color, Color, Color) colors)
        {
            var mario = InstantiateMario(Vector3.zero);
            yield return new WaitForFixedUpdate();

            mario.iColorable.SetColors(colors.Item1, colors.Item2, colors.Item3);
            yield return new WaitForFixedUpdate();

            mario.isGrounded = true;
            mario.isJumping = true;
            CustomAssert.AreEqual(
                colors.Item1, mario.iColorable.GetCurrentColor(),
                $"When grounded, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'groundedColor'!"
            );

            mario.isGrounded = true;
            mario.isJumping = false;
            CustomAssert.AreEqual(
                colors.Item1, mario.iColorable.GetCurrentColor(),
                $"When grounded, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'groundedColor'!"
            );

            mario.isGrounded = false;
            mario.isJumping = true;
            CustomAssert.AreEqual(
                colors.Item2, mario.iColorable.GetCurrentColor(),
                $"When airborne and jumping, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'jumpingColor'!"
            );

            mario.isGrounded = false;
            mario.isJumping = false;
            CustomAssert.AreEqual(
                colors.Item3, mario.iColorable.GetCurrentColor(),
                $"When airborne and falling, {nameof(IColorable.GetCurrentColor)} should return {nameof(IColorable.SetColors)}'s 'fallingColor'!"
            );
        }
        private MarioBridge InstantiateMario(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new MarioBridge(instance, true);
        }
    }
}
