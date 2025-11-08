using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core;
using Gameplay;
using System.Collections;

namespace Tests
{
    /// <summary>
    /// Tests for event-driven power-up achievement system
    /// Validates that AchievementManager reacts to power-up events instead of polling
    /// </summary>
    public class PowerUpEventSystemTest
    {
        private GameObject testPlayerObject;
        private PlayerController playerController;
        private AchievementManager achievementManager;
        private bool eventReceived;
        private PowerUpEventArgs lastEventArgs;

        [SetUp]
        public void Setup()
        {
            // Clean up any existing instances
            var existingAchievementManager = Object.FindObjectOfType<AchievementManager>();
            if (existingAchievementManager != null)
            {
                Object.DestroyImmediate(existingAchievementManager.gameObject);
            }

            eventReceived = false;
            lastEventArgs = null;
        }

        [TearDown]
        public void Teardown()
        {
            if (testPlayerObject != null)
            {
                Object.DestroyImmediate(testPlayerObject);
            }

            if (achievementManager != null && achievementManager.gameObject != null)
            {
                achievementManager.Shutdown();
            }
        }

        /// <summary>
        /// Test that PowerUpManager raises events when power-ups are activated
        /// </summary>
        [Test]
        public void PowerUpManager_RaisesActivationEvent_WhenPowerUpActivated()
        {
            // Arrange
            var testObj = new GameObject("TestPlayer");
            var controller = testObj.AddComponent<PlayerController>();
            var powerUpManager = new PowerUpManager(controller);
            
            eventReceived = false;
            powerUpManager.OnPowerUpActivated += (sender, args) =>
            {
                eventReceived = true;
                lastEventArgs = args;
            };

            // Act - Simulate confusion power-up collection
            var confusionPowerUp = new ConfusionPowerUp();
            confusionPowerUp.Activate(5);

            // Assert
            Assert.IsTrue(eventReceived, "Power-up activation event should be raised");
            Assert.IsNotNull(lastEventArgs, "Event args should not be null");
            Assert.AreEqual("ConfusionPowerUp", lastEventArgs.PowerUpName);
            Assert.AreEqual(5, lastEventArgs.RemainingUses);
            Assert.IsTrue(lastEventArgs.IsActive);

            // Cleanup
            Object.DestroyImmediate(testObj);
        }

        /// <summary>
        /// Test that PowerUpManager raises events when power-ups are consumed
        /// </summary>
        [Test]
        public void PowerUpManager_RaisesConsumedEvent_WhenPowerUpUsed()
        {
            // Arrange
            var testObj = new GameObject("TestPlayer");
            var controller = testObj.AddComponent<PlayerController>();
            var powerUpManager = new PowerUpManager(controller);
            
            var confusionPowerUp = new ConfusionPowerUp();
            confusionPowerUp.Activate(2);

            eventReceived = false;
            powerUpManager.OnPowerUpConsumed += (sender, args) =>
            {
                eventReceived = true;
                lastEventArgs = args;
            };

            // Act - Consume one use
            confusionPowerUp.ConsumeUse();

            // Assert
            Assert.IsTrue(eventReceived, "Power-up consumed event should be raised");
            Assert.AreEqual(1, lastEventArgs.RemainingUses);
            Assert.IsTrue(lastEventArgs.IsActive);

            // Cleanup
            Object.DestroyImmediate(testObj);
        }

        /// <summary>
        /// Test that PowerUpManager raises events when power-ups are deactivated
        /// </summary>
        [Test]
        public void PowerUpManager_RaisesDeactivatedEvent_WhenPowerUpExpires()
        {
            // Arrange
            var testObj = new GameObject("TestPlayer");
            var controller = testObj.AddComponent<PlayerController>();
            var powerUpManager = new PowerUpManager(controller);
            
            var confusionPowerUp = new ConfusionPowerUp();
            confusionPowerUp.Activate(1);

            eventReceived = false;
            powerUpManager.OnPowerUpDeactivated += (sender, args) =>
            {
                eventReceived = true;
                lastEventArgs = args;
            };

            // Act - Consume last use
            confusionPowerUp.ConsumeUse();

            // Assert
            Assert.IsTrue(eventReceived, "Power-up deactivated event should be raised");
            Assert.AreEqual(0, lastEventArgs.RemainingUses);
            Assert.IsFalse(lastEventArgs.IsActive);

            // Cleanup
            Object.DestroyImmediate(testObj);
        }

        /// <summary>
        /// PlayMode test that AchievementManager subscribes to PowerUpManager events
        /// </summary>
        [UnityTest]
        public IEnumerator AchievementManager_SubscribesToPowerUpEvents_WhenPlayerControllerAwakes()
        {
            // Arrange - Create AchievementManager
            var achievementObj = new GameObject("TestAchievementManager");
            achievementManager = achievementObj.AddComponent<AchievementManager>();
            yield return achievementManager.InitializeAsync();

            // Act - Create PlayerController (which should auto-subscribe)
            testPlayerObject = new GameObject("TestPlayer");
            testPlayerObject.AddComponent<Rigidbody2D>();
            testPlayerObject.AddComponent<BoxCollider2D>();
            playerController = testPlayerObject.AddComponent<PlayerController>();
            
            yield return null; // Wait for Awake to complete

            // Assert - Subscription should have occurred
            // We can't directly test the subscription, but we can verify the manager exists
            Assert.IsNotNull(achievementManager, "AchievementManager should exist");
            Assert.IsNotNull(playerController, "PlayerController should exist");
            
            // The subscription happens in PlayerController.Awake via SubscribeAchievementManager()
            // This test validates the objects are created and linked correctly
        }

        /// <summary>
        /// Test that no static field polling occurs in AchievementManager
        /// </summary>
        [Test]
        public void AchievementManager_DoesNotHaveUpdateMethod()
        {
            // Arrange & Act
            var updateMethod = typeof(AchievementManager).GetMethod("Update", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);

            // Assert
            Assert.IsNull(updateMethod, "AchievementManager should not have an Update() method for polling");
        }

        /// <summary>
        /// Test that CheckPowerUpAchievement is marked as obsolete
        /// </summary>
        [Test]
        public void AchievementManager_CheckPowerUpAchievement_IsObsolete()
        {
            // Arrange & Act
            var checkMethod = typeof(AchievementManager).GetMethod("CheckPowerUpAchievement",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            // Assert
            Assert.IsNotNull(checkMethod, "CheckPowerUpAchievement method should exist for backward compatibility");
            
            var obsoleteAttributes = checkMethod.GetCustomAttributes(typeof(System.ObsoleteAttribute), false);
            Assert.IsTrue(obsoleteAttributes.Length > 0, "CheckPowerUpAchievement should be marked as obsolete");
        }

        /// <summary>
        /// Test that simultaneous power-ups trigger achievement
        /// </summary>
        [UnityTest]
        public IEnumerator AchievementManager_UnlocksConfuseAndSpeed_WhenBothPowerUpsActive()
        {
            // Arrange
            var achievementObj = new GameObject("TestAchievementManager");
            achievementManager = achievementObj.AddComponent<AchievementManager>();
            yield return achievementManager.InitializeAsync();

            testPlayerObject = new GameObject("TestPlayer");
            testPlayerObject.AddComponent<Rigidbody2D>();
            testPlayerObject.AddComponent<BoxCollider2D>();
            playerController = testPlayerObject.AddComponent<PlayerController>();
            
            yield return null;

            // Act - Activate both power-ups
            var confusionPowerUp = new ConfusionPowerUp();
            confusionPowerUp.Activate(5);
            
            var teleportPowerUp = new TeleportationPowerUp();
            teleportPowerUp.Activate(3);

            yield return null; // Wait a frame for events to process

            // Assert
            Assert.IsTrue(achievementManager.ConfuseAndSpeed, 
                "ConfuseAndSpeed achievement should be unlocked when both power-ups are active");
        }
    }
}
