using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace Tests
{
    public class Testat06 : TestSuite
    {
        private class MarioBridge : GameObjectBridge
        {
            public bool isGrounded
            {
                get => isGroundedBridge.value;
                set => isGroundedBridge.value = value;
            }
            private readonly FieldBridge<bool> isGroundedBridge;
            public GameObject contactParticlesPrefab
            {
                get => contactParticlesPrefabBridge.value;
                set => contactParticlesPrefabBridge.value = value;
            }
            private readonly FieldBridge<GameObject> contactParticlesPrefabBridge;

            public Physics2DEvents physics
            {
                get;
                private set;
            }

            public MarioBridge(GameObject gameObject, bool isInstance = false) : base(gameObject)
            {
                isGroundedBridge = FindField<bool>(nameof(isGrounded));
                contactParticlesPrefabBridge = FindField<GameObject>(nameof(contactParticlesPrefab));
                if (isInstance)
                {
                    physics = gameObject.AddComponent<Physics2DEvents>();
                }
            }
        }
        public class PlatformBridge : GameObjectBridge
        {
            public PlatformBridge(GameObject gameObject) : base(gameObject)
            {
            }
        }
        public class ParticleBridge : GameObjectBridge
        {
            public ParticleSystem particleSystem
            {
                get;
                private set;
            }
            public ParticleSystem.MainModule particleSystemMain
            {
                get;
                private set;
            }
            public ParticleSystemRenderer particleSystemRenderer
            {
                get;
                private set;
            }

            public ParticleBridge(GameObject gameObject) : base(gameObject)
            {
                particleSystem = FindComponent<ParticleSystem>();
                particleSystemMain = particleSystem.main;
                particleSystemRenderer = FindComponent<ParticleSystemRenderer>();
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

        private static string[] PREFAB_ALL => new[] { PREFAB_MARIO, PREFAB_PLATFORM_ICE, PREFAB_PLATFORM_METAL, PREFAB_PLATFORM_DIRT, PREFAB_PARTICLE };
        private static string[] PREFAB_PLATFORMS => new[] { PREFAB_PLATFORM_ICE, PREFAB_PLATFORM_METAL, PREFAB_PLATFORM_DIRT };
        private static readonly string PREFAB_MARIO = "Assets/Prefabs/Mario.prefab";
        private static readonly string PREFAB_PLATFORM_ICE = "Assets/Prefabs/Platform_Ice.prefab";
        private static readonly string PREFAB_PLATFORM_METAL = "Assets/Prefabs/Platform_Metal.prefab";
        private static readonly string PREFAB_PLATFORM_DIRT = "Assets/Prefabs/Platform_Dirt.prefab";
        private static readonly string PREFAB_PARTICLE = "Assets/Prefabs/ContactParticles.prefab";

        private const float SCENE_TIMEOUT = 5;
        private static (Vector3, bool)[] INTERACTION_INFOS => new[]
        {
            (Vector3.up, false),
            (Vector3.down, true),
        };
        private static (Vector3, Vector3)[] SPAWN_POSITIONS => new[]
        {
            (new Vector3(0, 0, 0), new Vector3(0, 2, 0)),
            (new Vector3(1.2f, -1, 0), new Vector3(1.4f, 1, 0)),
        };

        [Test]
        public void T01_PrefabExists([ValueSource(nameof(PREFAB_ALL))] string path)
        {
            TestUtils.LoadPrefab(path);
        }
        [Test]
        public void T02_ContactParticlesPrefab()
        {
            string path = PREFAB_PARTICLE;
            GameObject prefab = TestUtils.LoadPrefab(path);

            ParticleBridge particle = new ParticleBridge(prefab);
            Assert.IsFalse(particle.particleSystemMain.loop, $"Particle prefab '{path}' must not loop!");
            Assert.IsNotNull(particle.particleSystemRenderer.sharedMaterial, $"Particle prefab '{path}' requires a material!");
            Assert.IsNotNull(particle.particleSystemRenderer.sharedMaterial.shader, $"Particle prefab '{path}' material's requires a shader!");
            Assert.IsTrue(particle.particleSystemRenderer.sharedMaterial.shader.name.Contains("Particles/"), $"Particle prefab '{path}' material's requires a 'Particles' shader!");
        }
        [Test]
        public void T03a_MarioPrefab()
        {
            GameObject marioPrefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            GameObject particlePrefab = TestUtils.LoadPrefab(PREFAB_PARTICLE);

            MarioBridge mario = new MarioBridge(marioPrefab);
            Assert.AreEqual(particlePrefab, mario.contactParticlesPrefab, $"Mario '{PREFAB_MARIO}' must use particle prefab '{PREFAB_PARTICLE}' for {nameof(mario.contactParticlesPrefab)}!");
        }
        [UnityTest]
        public IEnumerator T03b_ContactParticleSpawn(
            [ValueSource(nameof(PREFAB_PLATFORMS))] string platformFile,
            [ValueSource(nameof(SPAWN_POSITIONS))] (Vector3, Vector3) spawn)
        {
            yield return CreatePlatformTest(platformFile, spawn.Item1, spawn.Item2, true);
        }
        [UnityTest]
        public IEnumerator T04_PlatformBug(
            [ValueSource(nameof(PREFAB_PLATFORMS))] string platformFile,
            [ValueSource(nameof(INTERACTION_INFOS))] (Vector3, bool) info)
        {
            yield return CreatePlatformTest(platformFile, info.Item1, Vector3.zero, info.Item2);
        }
        private IEnumerator CreatePlatformTest(string platformFile, Vector3 platformPos, Vector3 marioPos, bool shouldBeGrounded)
        {
            yield return new WaitForFixedUpdate();

            GameObject particlePrefab = TestUtils.LoadPrefab(PREFAB_PARTICLE);
            int particleCount = currentScene.GetPrefabInstances(particlePrefab).Count();

            Collision2D collision = null;
            Vector2 collisionPosition = default;
            string setupText = $"When spawning Mario at {marioPos} and Platform '{platformFile}' at {platformPos} in empty scene,";

            PlatformBridge platform = InstantiatePlatform(platformPos, platformFile);
            MarioBridge mario = InstantiateMario(marioPos);
            mario.physics.onCollisionEnter += newCollision =>
            {
                Assert.IsNull(collision, $"{setupText} there must be only 1 collision!");
                collision = newCollision;
                collisionPosition = collision
                    .contacts
                    .Select(contact => contact.point)
                    .Aggregate((sum, add) => sum + add) / collision.contactCount;
            };

            float timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => collision != null || Time.time > timeout);

            Assert.IsNotNull(collision, $"{setupText} they should have collided, but didn't!");

            yield return new WaitForFixedUpdate();

            if (shouldBeGrounded)
            {
                Assert.IsTrue(mario.isGrounded, $"{setupText} Mario should have become grounded, but didn't!");
            }
            else
            {
                Assert.IsFalse(mario.isGrounded, $"{setupText} Mario should not have become grounded, but did!");
            }

            IEnumerable<GameObject> particleInstances = currentScene.GetPrefabInstances(particlePrefab);
            Assert.AreEqual(particleCount + 1, particleInstances.Count(), $"{setupText} particles should've spawned, but didn't!");
            ParticleBridge particle = new ParticleBridge(particleInstances.Last());
            loadedObjects.Enqueue(particle.gameObject);
            CustomAssert.AreEqual(Quaternion.identity, particle.transform.rotation, $"{setupText} particles should've spawned with identity rotation!");
            CustomAssert.AreEqual(collisionPosition, (Vector2)particle.transform.position, $"{setupText} particles should've spawned at collision point average!");

            timeout = Time.time + SCENE_TIMEOUT;
            yield return new WaitUntil(() => !particle.gameObject || Time.time > timeout);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(!particle.gameObject, $"{setupText} spawning particles and waiting {SCENE_TIMEOUT}s, particles should've self-destructed!");

            yield return new WaitForFixedUpdate();
        }

        private MarioBridge InstantiateMario(Vector3 position)
        {

            GameObject prefab = TestUtils.LoadPrefab(PREFAB_MARIO);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            MarioBridge avatar = new MarioBridge(instance, true);
            return avatar;
        }

        private PlatformBridge InstantiatePlatform(Vector3 position, string prefabFile)
        {

            GameObject prefab = TestUtils.LoadPrefab(prefabFile);
            GameObject instance = InstantiateGameObject(prefab, position, Quaternion.identity);
            return new PlatformBridge(instance);
        }
    }
}
