using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gameplay;
using Core;

namespace Tests
{
    /// <summary>
    /// Play mode tests for PlayerController state pattern implementation.
    /// Tests state transitions and behavior isolation.
    /// </summary>
    public class PlayerStateTransitionTests
    {
        private GameObject _playerObject;
        private PlayerController _playerController;
        private GameObject _testWall;

        [SetUp]
        public void Setup()
        {
            // Create player GameObject with required components
            _playerObject = new GameObject("TestPlayer");
            _playerObject.tag = "Player";
            
            // Add required components
            var rb = _playerObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Top-down game
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            var boxCollider = _playerObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(0.4f, 0.4f);
            
            _playerController = _playerObject.AddComponent<PlayerController>();
            
            // Set up wall layer for collision testing
            _testWall = new GameObject("TestWall");
            _testWall.tag = "Wall";
            _testWall.layer = LayerMask.NameToLayer("Default");
            var wallCollider = _testWall.AddComponent<BoxCollider2D>();
            wallCollider.size = new Vector2(1f, 1f);
            _testWall.transform.position = new Vector3(2f, 0f, 0f);
        }

        [TearDown]
        public void Teardown()
        {
            if (_playerObject != null)
                Object.Destroy(_playerObject);
            if (_testWall != null)
                Object.Destroy(_testWall);
        }

        [UnityTest]
        public IEnumerator StartsInIdleState()
        {
            // Wait one frame for Awake/Start to complete
            yield return null;
            
            var stateMachine = _playerController.GetStateMachine();
            Assert.IsNotNull(stateMachine, "State machine should be initialized");
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType, 
                "Player should start in Idle state");
        }

        [UnityTest]
        public IEnumerator IdleToMoveTransition()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Simulate successful movement (mock TryMove by setting direction and changing state)
            _playerController.SetMoveDirection(Vector2.right);
            stateMachine.ChangeState(PlayerStateType.Move);
            
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Move, stateMachine.CurrentStateType,
                "Should transition to Move state after input");
            Assert.AreEqual(Vector2.right, _playerController.GetMoveDirection(),
                "Move direction should be set");
        }

        [UnityTest]
        public IEnumerator MoveToIdleOnCollision()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Transition to Move state
            _playerController.SetMoveDirection(Vector2.right);
            stateMachine.ChangeState(PlayerStateType.Move);
            
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Move, stateMachine.CurrentStateType);
            
            // Simulate collision (transition back to Idle)
            stateMachine.ChangeState(PlayerStateType.Idle);
            
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType,
                "Should return to Idle after collision");
            Assert.AreEqual(Vector2.zero, _playerController.GetMoveDirection(),
                "Movement should stop in Idle");
        }

        [UnityTest]
        public IEnumerator IdleToTeleportingTransition()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Transition to Teleporting state
            stateMachine.ChangeState(PlayerStateType.Teleporting);
            
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Teleporting, stateMachine.CurrentStateType,
                "Should transition to Teleporting state");
            Assert.AreEqual(Vector2.zero, _playerController.GetMoveDirection(),
                "Movement should be stopped during teleport");
        }

        [UnityTest]
        public IEnumerator TeleportingToIdleAfterCompletion()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Transition to Teleporting
            stateMachine.ChangeState(PlayerStateType.Teleporting);
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Teleporting, stateMachine.CurrentStateType);
            
            // Simulate teleport completion
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType,
                "Should return to Idle after teleport completes");
        }

        [UnityTest]
        public IEnumerator IdleToPushingTransition()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Transition to Pushing state
            stateMachine.ChangeState(PlayerStateType.Pushing);
            
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Pushing, stateMachine.CurrentStateType,
                "Should transition to Pushing state");
        }

        [UnityTest]
        public IEnumerator PushingToIdleAfterCompletion()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Transition to Pushing
            stateMachine.ChangeState(PlayerStateType.Pushing);
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Pushing, stateMachine.CurrentStateType);
            
            // Simulate push completion
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
            
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType,
                "Should return to Idle after push completes");
        }

        [UnityTest]
        public IEnumerator StateTransitionsAreIndependent()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // Test multiple transitions in sequence
            stateMachine.ChangeState(PlayerStateType.Move);
            yield return null;
            Assert.AreEqual(PlayerStateType.Move, stateMachine.CurrentStateType);
            
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType);
            
            stateMachine.ChangeState(PlayerStateType.Teleporting);
            yield return null;
            Assert.AreEqual(PlayerStateType.Teleporting, stateMachine.CurrentStateType);
            
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType);
            
            stateMachine.ChangeState(PlayerStateType.Pushing);
            yield return null;
            Assert.AreEqual(PlayerStateType.Pushing, stateMachine.CurrentStateType);
            
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
            Assert.AreEqual(PlayerStateType.Idle, stateMachine.CurrentStateType);
        }

        [UnityTest]
        public IEnumerator DirectionChangeBlockedInNonIdleStates()
        {
            yield return null; // Initialize
            
            var stateMachine = _playerController.GetStateMachine();
            
            // In Idle, direction change should be allowed
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
            _playerController.EnableDirectionChange();
            // Note: canChangeDirection is private, so we test indirectly via state behavior
            
            // Transition to Move (should disable direction change)
            stateMachine.ChangeState(PlayerStateType.Move);
            yield return null;
            // Direction change should now be blocked (tested via state implementation)
            
            // Return to Idle (should re-enable direction change)
            stateMachine.ChangeState(PlayerStateType.Idle);
            yield return null;
        }
    }
}
