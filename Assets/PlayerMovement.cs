using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour {

	private CharacterController controller;

	public static PlayerMovement Instance;

	public Vector3 velocity = Vector3.zero;
	public StateMachine SM;

	private Vector3 directionBeforeWallSticking = Vector3.zero;
	private Vector3 lastHitWallNormal = Vector3.zero;
	private Vector3 movementInput = Vector3.zero;
	private (float wallStick, float wallStickCooldown, float wallStrafeEnd) timers = (0f, 0f, 0f);

	// private (float ground, float air) friction = (10f, 30f);

	private const float maxSpeed = 30.0f;
	private const float walkingSpeed = 7.0f;
	private const float sprintingSpeed = 10.0f;
	private const float gravity = 18f;
	private const float jumpHeight = 5f;
	private const float wallJumpForce = 1.4f;
	private const float wallJumpHeight = 10f;
	private const float wallStickDuration = 0.35f;
	private const float wallStickCooldown = 1f;
	private const float wallStrafeEndDuration = 0.15f;
	private const float airStrafeSpeed = 5.0f;
	private const float wallStrafeSpeed = 7.0f;

	private void Awake() {
		if(Instance == null) {
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}

		controller = GetComponent<CharacterController>();
	}

	private void Start() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		SM = new StateMachine();

		// standing
		State standing = SM.AddState(new State("standing", () => {
			velocity = Vector3.zero;
		}));

		// walking
		State walking = SM.AddState(new State("walking", () => {
			velocity = movementInput * walkingSpeed;
		}));

		// jumping
		State jumping = SM.AddState(new State("jumping", () => {}, () => {
			velocity.y = jumpHeight;
		}));

		// falling
		State falling = SM.AddState(new State("falling"));

		// airStrafing
		State airStrafing = SM.AddState(new State("airStrafing", () => {
			Vector2 currDirection = new Vector2(velocity.x, velocity.z);
			Vector3 newVelocity = currDirection.magnitude > 0 ? movementInput * currDirection.magnitude : movementInput * walkingSpeed;

			velocity.x = newVelocity.x;
			velocity.z = newVelocity.z;
		}));

		// sprinting
		State sprinting = SM.AddState(new State("sprinting", () => {
			velocity = movementInput * sprintingSpeed;
		}));

		// wallSticking
		State wallSticking = SM.AddState(new State("wallSticking",
			() => {
				velocity = Vector3.zero;
				timers.wallStick += Time.fixedDeltaTime;
			}, 
			() => {
				directionBeforeWallSticking = new Vector3(velocity.x, 0, velocity.z);
			},
			() => {
				timers.wallStick = 0f;
				timers.wallStickCooldown = 0f;
			}
		));

		//wallJumping
		State wallJumping = SM.AddState(new State("wallJumping", () => {}, () => {
			Vector3 wallJumpDirection = Vector3.Reflect(directionBeforeWallSticking, lastHitWallNormal) * wallJumpForce;
						
			wallJumpDirection.y = wallJumpHeight;
			velocity = wallJumpDirection;
		},
		() => {
				directionBeforeWallSticking = Vector3.zero;
			}
		));

		// wallStrafing
		State wallStrafing = SM.AddState(new State("wallStrafing", 
			() => {
				velocity = movementInput * wallStrafeSpeed;
				velocity.y = 0;
			},
			null,
			() => {
				timers.wallStrafeEnd = 0f;
			}
		));

		SM.currState = SM.states["standing"];

		SM.conditions["moveInput"] = () => movementInput != Vector3.zero;
		SM.conditions["noMoveInput"] = () => movementInput == Vector3.zero;
		SM.conditions["jumpInput"] = () => Input.GetButtonDown("Jump");
		SM.conditions["startedFalling"] = () => velocity.y < 0;
		SM.conditions["touchedGround"] = () => controller.isGrounded;
		SM.conditions["sprintInput"] = () => Input.GetButton("Fire3");
		SM.conditions["wallStickTimerEnded"] = () => timers.wallStick >= wallStickDuration;
		SM.conditions["wallStickCooldownEnded"] = () => timers.wallStickCooldown >= wallStickCooldown;
		SM.conditions["collidedWithWall"] = () => controller.collisionFlags == CollisionFlags.Sides;
		SM.conditions["wallCollisionEnded"] = () => controller.collisionFlags != CollisionFlags.Sides;
		SM.conditions["wallStrafeEnded"] = () => timers.wallStrafeEnd >= wallStrafeEndDuration;

		standing.AddTransition(walking, SM.conditions["moveInput"]);
		standing.AddTransition(jumping, SM.conditions["jumpInput"]);

		walking.AddTransition(standing, SM.conditions["noMoveInput"]);
		walking.AddTransition(jumping, SM.conditions["jumpInput"]);
		walking.AddTransition(sprinting, SM.conditions["sprintInput"]);

		jumping.AddTransition(falling, SM.conditions["startedFalling"]);
		jumping.AddTransition(airStrafing, SM.conditions["moveInput"]);
		jumping.AddTransition(wallSticking, () => SM.conditions["collidedWithWall"]() && SM.conditions["wallStickCooldownEnded"]());
		
		falling.AddTransition(walking, SM.conditions["touchedGround"]);
		falling.AddTransition(airStrafing, SM.conditions["moveInput"]);
		falling.AddTransition(wallSticking, () => SM.conditions["collidedWithWall"]() && SM.conditions["wallStickCooldownEnded"]());

		airStrafing.AddTransition(standing, SM.conditions["touchedGround"]);
		airStrafing.AddTransition(falling, SM.conditions["noMoveInput"]);
		airStrafing.AddTransition(wallSticking, () => SM.conditions["collidedWithWall"]() && SM.conditions["wallStickCooldownEnded"]());

		sprinting.AddTransition(jumping, SM.conditions["jumpInput"]);
		sprinting.AddTransition(standing, SM.conditions["noMoveInput"]);

		wallSticking.AddTransition(wallJumping, SM.conditions["jumpInput"]);
		wallSticking.AddTransition(falling, SM.conditions["wallStickTimerEnded"]);
		wallSticking.AddTransition(wallStrafing, SM.conditions["moveInput"]);

		wallJumping.AddTransition(airStrafing, SM.conditions["moveInput"]);
		wallJumping.AddTransition(falling, SM.conditions["startedFalling"]);

		wallStrafing.AddTransition(wallJumping, SM.conditions["jumpInput"]);
		wallStrafing.AddTransition(falling, () => SM.conditions["wallCollisionEnded"]() && SM.conditions["wallStrafeEnded"]());
	}

	private void Update() {
		movementInput = transform.TransformDirection(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"))).normalized;

		SM.TryToChangeState();

		controller.Move(velocity * Time.deltaTime);
	}

	private void FixedUpdate() {
		SM.currState.Update();

		if(SM.currState == SM.states["wallStrafing"] && SM.conditions["wallCollisionEnded"]()) {
			timers.wallStrafeEnd += Time.fixedDeltaTime;
		}

		timers.wallStickCooldown += Time.fixedDeltaTime;
		velocity.y -= gravity * Time.fixedDeltaTime;

		velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
		velocity.z = Mathf.Clamp(velocity.z, -maxSpeed, maxSpeed);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit) {
		if(controller.collisionFlags == CollisionFlags.Sides) {
			lastHitWallNormal = hit.normal;
		}
	}
}

// wallstrafing timer
// remove SM code duplication
// sprinting timer?
// air friction?
// wallsticking only after grounded jump?
// store curr speed as magnitude?
// use curr speed for walljumps?