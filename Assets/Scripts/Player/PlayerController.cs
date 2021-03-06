﻿/*	Author: Powered by Coffee
 * 	Description: Player physics and controls are here.
 * 
 * 
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour, PlayerInterface
{	
	private Health health; 						 // Player health
	public float maxSpeed = 10f; 				 // Player max speed
	public bool facingRight = true; 			 // Check which way player is facing
	private Rigidbody2D rb2d; 					 // Rigidbody 2D that is on this object
	private Animator anim; 						 // Animator that is on this object
	private bool grounded = false; 				 // Check if player is on the ground
	public Transform groundCheck; 				 // transform of where to check for ground
	private float groundRadius = 0.02f; 		 // Circle below player that checks of ground
	public LayerMask whatIsGround; 				 // Indicates which layers of game are ground
	public float jumpForce = 700; 				 // Force of player jump
	private Armor playerArmor;	 // Players Armor
	private Boot playerBoot; 		 // Players Boots
	private Weapon playerWeapon1; // Players Weapon 1
	private Weapon playerWeapon2; // Players Weapon 2
	private bool jumpPending; 					 // Player is jumping or not
	public Weapon equippedWeapon;
	private CapsuleCollider2D playerCollider;
	private bool canFallThrough = true;
	private bool knocked_back = false;
	private float knock_back_x = 0f;
	private int knock_back_counter = 0;
	public const int knock_back_frames = 5;

	//Set and Get functions
	public void SetArmor(Armor armor)
	{
		this.playerArmor = armor;
	}
	public void SetBoot(Boot boot)
	{
		this.playerBoot = boot;
	}
	public void SetWeapon1(Weapon weapon1)
	{
		this.playerWeapon1 = weapon1;
	}
	public void SetWeapon2(Weapon weapon2)
	{
		this.playerWeapon2 = weapon2;
	}
	public Armor GetArmor()
	{
		return playerArmor;
	}
	public Boot GetBoot()
	{
		return playerBoot;
	}
	public Weapon GetWeapon1()
	{
		return playerWeapon1;
	}
	public Weapon GetWeapon2()
	{
		return playerWeapon2;
	}
	public void jump()
	{
		jumpPending = true;
	}
	public void ChangeWeapon()
	{
		if (equippedWeapon == playerWeapon1) 
		{
			equippedWeapon = playerWeapon2;
		} else 
		{
			equippedWeapon = playerWeapon1;
		}
	}

	void Awake()
	{	
		this.SetWeapon1 (Inventory.playerWeapon1);
		this.SetWeapon2 (Inventory.playerWeapon2);
		this.SetArmor (Inventory.playerArmor);
		this.SetBoot (Inventory.playerBoot);
		if (playerWeapon1 != null)
			playerWeapon1.calculateTotalDamage ();
		
		if (playerWeapon2 != null)
			playerWeapon2.calculateTotalDamage ();
		
		if (playerArmor != null)
			playerArmor.calculateTotalDefense ();
		
		playerCollider = gameObject.GetComponent<CapsuleCollider2D> ();
		equippedWeapon = playerWeapon1;
		health = (Health) GetComponent<Health> ();
		health.SetHealth ((int)(health.maxHealth * (Inventory.playerArmor.getGem1().getGrade() + 1)));
	}

	//Initialization
	void Start()
	{
		rb2d = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}

	//Player physics
	void FixedUpdate()
	{
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, whatIsGround);
		anim.SetBool ("Ground", grounded);
		anim.SetFloat("Speed", rb2d.velocity.y);
		float move = UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetAxis ("HorizontalMove") * 0.5f;
		anim.SetFloat ("Speed", Mathf.Abs (move));

		rb2d.velocity = new Vector2 (Mathf.Clamp((move * maxSpeed) + knock_back_x, -10, 10), rb2d.velocity.y); 

		if (knocked_back && (knock_back_counter < knock_back_frames || !grounded)) {
			knock_back_counter++;
		} else {
			knocked_back = false;
			knock_back_x = 0f;
		}

		if (move > 0 && !facingRight) 
		{
			Flip ();		
		}
		else if(move < 0 && facingRight)
		{
			Flip();
		}
	}

	void Update()
	{
		UpdateHealth ();
		//Vertical Movement
		if ((jumpPending | Input.GetKeyDown(KeyCode.Space)) && grounded) 
		{
			rb2d.AddForce (new Vector2 (0, jumpForce));
			anim.SetTrigger ("Jump");
		} 

		jumpPending = false;
		if (IsDead ())
		{
			Dead();
		}
	}

	void ScriptThatTurnsPlatformBackOn()
	{
		playerCollider.enabled = true;
		Invoke ("canFallthrough", 0.5f);
	}

	void canFallthrough()
	{
		canFallThrough = true;
	}
	//Flip sprite when changing direction
	void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
	void OnCollisionStay2D (Collision2D col)
	{
		float direction = UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetAxis ("VerticalShoot");
		if (direction < -0.6 && col.gameObject.layer == 9 && canFallThrough) {
			playerCollider.enabled = false;
			canFallThrough = false;
			Invoke ("ScriptThatTurnsPlatformBackOn", 0.28f);
		}
	}

	//Damage functions
	void OnCollisionEnter2D (Collision2D col) 
	{

		if (col.gameObject.tag.Contains("Enemy")) 
		{
			//Grab the damage of the incoming bullet
			int damageTaken = col.gameObject.GetComponent<Enemy> ().bodyDamage;

			knocked_back = true;
			knock_back_counter = 0;
			// Destroy current vector to avoid memory leak

			if (col.gameObject.GetComponent<Rigidbody2D> ().position.x > rb2d.position.x) {
				rb2d.velocity = new Vector2 (0f, 3f);
				knock_back_x = -3f;
			} else {
				rb2d.velocity = new Vector2(0f, 3f);
				knock_back_x = 3f;
			}

			char element = '#';

			if (col.gameObject.GetComponent<FireType> () != null)
				element = 'F';
			else if (col.gameObject.GetComponent<WaterType> () != null)
				element = 'W';
			else if (col.gameObject.GetComponent<EarthType> () != null)
				element = 'E';
			else if (col.gameObject.GetComponent<AirType> () != null)
				element = 'A';
			//Hurt this object
			if (element != '#')
				health.Damage (damageTaken, element);
			else
				health.Damage (damageTaken);
			
		}
	}

	//Death handlers
	bool IsDead()
	{
		if (health.health == 0) 
		{
			return true;
		} else {
			return false;
		}
	}

	void Destroy() 
	{
		Destroy (this.gameObject);
	}

	void Dead()
	{
		Inventory.essence -= Inventory.tempEssence;
		Inventory.tempEssence = 0;
		Application.LoadLevel ("GameOver");
	}

	void UpdateHealth()
	{
		GameObject.FindGameObjectWithTag ("HPbar").GetComponent<UnityEngine.UI.Text> ().text = health.GetHealth ().ToString ();
	}
}
