using Guille_dlCH.TFG.Input;
using UnityEngine;
using KinematicCharacterController;

namespace Guille_dlCH.TFG.Player {
	public class PlayerCharacter : MonoBehaviour, ICharacterController {
		
		#region Serialized Attributes

		[SerializeField] private KinematicCharacterMotor motor;
		[SerializeField] private Transform root;
		[Space]
		[SerializeField] private float maxStamina = 3f;
		[SerializeField] private float minStamina = 0.1f;
		[SerializeField] private float staminaCooldown = 0.5f;
		[Tooltip("How much faster the stamina bar fills up")]
		[SerializeField] private float staminaRecMult = 2f;
		[Space]
		[SerializeField] private float walkSpeed = 20f;
		[SerializeField] private float crouchSpeed = 7f;
		[SerializeField] private float sprintSpeed = 30f;
		[Space]
		[SerializeField] private float walkResponse = 25f;
		[SerializeField] private float crouchResponse = 20f;
		[SerializeField] private float sprintResponse = 30f;
		[Space]
		[SerializeField] private float airSpeed = 15f;
		[SerializeField] private float airAcceleration = 180;
		[Space]
		[SerializeField] private float jumpSpeed = 20f;
		[SerializeField] private float coyoteTime = 0.2f;
		[SerializeField] private float jumpStaminaCost = 0.3f;
		[SerializeField] private float jumpSustainGravityMultiplier = 0.55f;
		[SerializeField] private float gravity = -90f;
		[SerializeField] private float terminalFallSpeed = -50f;
		[Space]
		[SerializeField] private float standHeight = 2f;
		[SerializeField] private float crouchHeight = 1f;
		[SerializeField] private float crouchHeightResponse = 15f;
		[Range(0f, 1f)]
		[SerializeField] private float standCameraHeight = 0.9f;
		[Range(0f, 1f)]
		[SerializeField] private float crouchCameraHeight = 0.7f;

		#endregion
		
		#region Private Attributes
		
		private float _currentStamina;

		private Quaternion _requestedRotation;
		private Vector3 _requestedMovement;
		private bool _requestedJump;
		private bool _requestedSustainedJump;
		private bool _requestedCrouch;
		private bool _requestedSprint;
		
		private float _timeSinceUngrounded;
		private float _timeSinceJumpRequest;
		private bool _ungroundedDueToJump;
		
		private float _timeSinceNoStamina;
		
		private Collider[] _uncrouchOverlapResults;

		private Vector3 velocity;
		
		#endregion

		public void Initialize() {
			_uncrouchOverlapResults = new Collider[5];

			motor.CharacterController = this;
		}
		
		public void UpdateInput(CharacterMovementInput movementInput) {
			//Rotation input
			_requestedRotation = movementInput.Rotation;
			//Takes the 2D input vector and transforms it into a 3D vector onto the XZ plane
			_requestedMovement = new Vector3(movementInput.Move.x, 0, movementInput.Move.y);
			//Normalizes it so diagonal movement is the same as any movement
			_requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1);
			//Orient it so it's relative to the direction the player is facing
			_requestedMovement = movementInput.Rotation * _requestedMovement;

			//Jump input
			var wasRequestingJump = _requestedJump;				//Previous jump request
			_requestedJump = _requestedJump || movementInput.Jump;		//Current jump request
			if (_requestedJump && !wasRequestingJump)
				_timeSinceJumpRequest = 0f;						//Reset the jump timer
		
			_requestedSustainedJump = movementInput.JumpSustain;
		
			//Crouch input
			_requestedCrouch = movementInput.Crouch;
		
			//Sprint input
			_requestedSprint = movementInput.Sprint;
		}

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
			//We need to update the character's rotation to face
			//in the same direction as the requested rotation
		
			//We don't want the character to pitch up and down,
			//so we need to "flatten" the direction the character looks
		
			//We do this by projecting the direction onto a flat ground plane

			var forward = Vector3.ProjectOnPlane(
				_requestedRotation * Vector3.forward,
				motor.CharacterUp);
		
			if(forward != Vector3.zero)
				currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
		}

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
			var deltaStamina = deltaTime * staminaRecMult;
		
			//If the player is grounded
			if (motor.GroundingStatus.IsStableOnGround) {
				
				_timeSinceUngrounded = 0f;					//Reset the ungrounded timer
				_ungroundedDueToJump = false;				//Uncheck the ungrounded due to jump flag
				
				//Snap the movement to the angle of the surface the character is walking on
				var groundedMovement = motor.GetDirectionTangentToSurface(
					direction: _requestedMovement,
					surfaceNormal: motor.GroundingStatus.GroundNormal) * _requestedMovement.magnitude;

				//Calculate the speed and character's responsiveness based on the stance
				float speed = walkSpeed;
				float response = walkResponse;
				
				//Set target velocity
				var targetVelocity = groundedMovement * speed;
				//And smoothly move it along the ground in that direction
				currentVelocity = Vector3.Lerp(
					currentVelocity,
					targetVelocity,
					1 - Mathf.Exp(-response * deltaTime));
			}
			//If the player is in the air
			else {

				deltaStamina = 0;
				_timeSinceUngrounded += deltaTime;			//Tick the ungrounded timer
				
				//Move in the air
				if (_requestedMovement.sqrMagnitude > 0f) {
					//Requested movement projected onto movement plane with preserved magnitude
					var planarMovement = Vector3.ProjectOnPlane(
						_requestedMovement,
						motor.CharacterUp
						) * _requestedMovement.magnitude;
					
					//Current planar velocity on movement plane
					var currentPlanarVelocity = Vector3.ProjectOnPlane(
						currentVelocity,
						motor.CharacterUp);
					
					//Calculate the movement force
					//Will be changed depending on current velocity
					var movementForce = planarMovement * (airAcceleration * deltaTime);
					
					//If moving slower than the max air speed, treat movementForce as a simple steering force
					if (currentPlanarVelocity.magnitude < airSpeed) {
						//Add it to the current planar velocity to get the target velocity
						var targetPlanarVelocity = currentPlanarVelocity + movementForce;
					
						//Limit target velocity to air speed
						targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
					
						//Steer towards current velocity
						movementForce += targetPlanarVelocity - currentPlanarVelocity;
					}
					//Otherwise, nerf the movement force when it is in the direction of the current planar velocity
					//to prevent accelerating further beyond the max air speed
					else if(Vector3.Dot(currentPlanarVelocity, movementForce) > 0f) {
						//Project movement force onto the plane whose normal is the current planar velocity
						var constrainedMovementForce = Vector3.ProjectOnPlane(
							movementForce,
							currentPlanarVelocity.normalized);
						movementForce = constrainedMovementForce;
					}
					
					currentVelocity += movementForce;
				}
				
				//Apply gravity
				var effectiveGravity = gravity;
				var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
				
				if (_requestedSustainedJump && verticalSpeed > 0)
					effectiveGravity *= jumpSustainGravityMultiplier;
				
				currentVelocity += motor.CharacterUp * (effectiveGravity * deltaTime);
				
				//Clamp to terminal velocity
				verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
				if (verticalSpeed < terminalFallSpeed) {
					currentVelocity -= motor.CharacterUp * verticalSpeed;
					currentVelocity += motor.CharacterUp * terminalFallSpeed;
				}
			}
			
			//Jumping
			if (_requestedJump) {

				var grounded = motor.GroundingStatus.IsStableOnGround;
				var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;
				if (grounded || canCoyoteJump) {
					_requestedJump = false;
					_currentStamina -= jumpStaminaCost;
				
					//Unstick the player from the ground
					motor.ForceUnground(time: 0.1f);
					_ungroundedDueToJump = true;			//Check the ungrounded due to jump flag
				
					//Set minimum vertical speed to the jump speed
					var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
					var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
					//Add the difference
					currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
				}
				else {
					_timeSinceJumpRequest += deltaTime;		//Tick the jump timer

					//Defer the jump request until coyote time has passed
					var canJumpLater = _timeSinceJumpRequest < coyoteTime;
					_requestedJump = canJumpLater;
				}
			}
			
			_timeSinceNoStamina += deltaTime;				//Tick stamina timer
			_currentStamina += deltaStamina;				//Tick stamina
			if (_currentStamina <= 0)
				_timeSinceNoStamina = 0f;					//Reset stamina timer
			//Clamp stamina
			_currentStamina = Mathf.Clamp(_currentStamina, 0f, maxStamina);

			velocity = currentVelocity;
		}
		public void BeforeCharacterUpdate(float deltaTime) { }
		public void PostGroundingUpdate(float deltaTime) { }
		public void AfterCharacterUpdate(float deltaTime) { }
		public bool IsColliderValidForCollisions(Collider coll) {
			return true;
		}
		public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

		public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport) { }

		public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
			Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

		public void OnDiscreteCollisionDetected(Collider hitCollider) { }

		public float GetSpeed() {
			return velocity.magnitude;
		}
	}
}