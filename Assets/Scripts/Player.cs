﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PhysicsObject
{

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
	private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxis("Horizontal");

		move.y = Input.GetAxis ("Vertical");
	
		if (move.y > 0 && grounded) {
			velocity.y = jumpTakeOffSpeed;
		} else if (move.y < 0) {
			velocity.y = -jumpTakeOffSpeed * 2f;
		}
        bool flipSprite = (spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f));
       if (flipSprite)
        {
         
			spriteRenderer.flipX = !spriteRenderer.flipX;
        }

       // animator.SetBool("grounded", grounded);
        //animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

        targetVelocity = move * maxSpeed;
    }
}