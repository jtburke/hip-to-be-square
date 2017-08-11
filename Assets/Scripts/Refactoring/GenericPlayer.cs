﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerStats
{
    public string name;
    public Vector3 position;
    public float focus;
    public Vector3 velocity;
    public int deaths;
}

public abstract class GenericPlayer : ResettableObject {
    
    private Collider2D _collider;
    private Rigidbody2D _rigidbody;

    public PlayerStats playerStats;
    [Header("Control Parameters")]
    protected float moveVelocity;                           //the amount of velocity on the x-axis of the player
    protected float jumpVelocity;                           //the amount of y-axis velocity when the player jumps
    protected float jumpDuration;
    private float jumpDurationCurrent;
    private bool isGrounded;
    private bool stoppedJumping;

    [Range(0.05f, 0.25f)] private float footSensorRadius;
    [Range(0.05f, 0.25f)] private float headSensorRadius;
    public LayerMask whatIsGround = LayerMask.GetMask("Ground");//specify the type of layer that we will use to detect if we are on the ground and able to perform a jump (used to determined the grounded boolean)
    [Header("Focus Settings")]
    public bool freeFocus;                                  //!! change to game manager eventually
    public float slowDownModifier;                          //decimal point value

    private Animator myAnimator;		//reference to animator so we can set animator parameters

    // Use this for initialization
    void Awake () {
        name = this.ToString();
	}

	// Update is called once per frame
	void Update () {
        /* The update method here will constantly update the x-axis velocity of the player. It will also need to
		 * handle the jumping mechanics, allowing the user to jump as desired. This includes not starting a jump
		 * in midair and ensuring you can only jump when your feet are on the ground (rather than face against a wall)*/
        _rigidbody.velocity = new Vector2(moveVelocity, _rigidbody.velocity.y);                                         //updates the x-axis velocity of the player based on his playerSpeed variable
        isGrounded = Physics2D.OverlapCircle(GetFeetPosition(), footSensorRadius, LayerMask.NameToLayer("Ground"));     //create a sphere collider at feet, detect if it comes into contact with the layer type associated with "ground".

        if (Physics2D.OverlapCircle(GetHeadPosition(), headSensorRadius, whatIsGround))
        {                                                                   //stop jumping when HEAD collides with underside of platform
            jumpDurationCurrent = 0;                                        //expend all of the jump time
            stoppedJumping = true;
        }
        
        if (Input.GetButtonDown("jump"))                                    //AS THE KEY IS PRESSED IN, CHECK IF WE ARE GROUND (GROUND LAYER TOUCHING FEET SENSOR), IF SO ADD JUMP FORCE, SET STOPPED JUMPING TO TRUE
        {                                                                   //check when they key is first pressed to begin the jump
            if (isGrounded)
            {                                                                                       //only allow the following if grounded is true, meaning player is on a layer defined as "Ground".
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpVelocity);             //add upwards velocity to the character
                stoppedJumping = false;                                                             //this indicates that we have started jumping and are able to hold the key to increase the size of the jump
            }
        }
        
        if (Input.GetButton("jump") && !stoppedJumping)                     //WHILE THE KEY IS HELD AND WE HAVEN'T STOPPED JUMPING, CHECK WE HAVE JUMP TIME LEFT, IF SO ADD JUMP FORCE AND DECREASE JUMP TIME
        {                                                                   //check if button is being held down, if so - continue with upwards velocity until jump time is 0
            if (jumpDurationCurrent > 0)
            {                                                               //while jump time still remains
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpVelocity);                 //continue applying the jump force on the y axis
                jumpDurationCurrent -= Time.deltaTime;                                                  //reduce the timer
            }
            else
            {
                stoppedJumping = true;  //MAYBE
            }
        }
        
        if (Input.GetButtonUp("jump"))                                      //WHEN THE KEY IS RELEASED, RESET JUMP COUNTER, STOPPED JUMPING = TRUE;
        {                                                                   //when the user comes off the key, stop them being able to expend anymore of the jumpTime (rocketpack)
            jumpDurationCurrent = 0;                                        //expend all of the jump time
            stoppedJumping = true;                                          //set variable that will stop player from entering if statement that allows continued jump velocity
        }

        if (Input.GetButton("focus"))
        {                                                                   //check if shift being pressed in order to use up slow down points and half the speed of the game
            if (freeFocus || playerStats.focus > 0f)
            {                                                               //ensure slow down points remain
                if (!freeFocus)
                {
                    playerStats.focus -= Time.unscaledDeltaTime;            //reduce the slow down points (we * by slowDownModifier in order to counter the effect of slowing time on a counter)
                }
                else
                {
                    //Debug.Log("FREE FOCUS ON: Slowdown modifier is: "+slowDownModifier);
                }

                Time.timeScale = slowDownModifier;                          //set the game speed based on the slow down modifier (example: 2 would mean speed is halved)
                myAnimator.SetBool("isFocusing", true);                     //boolean that trigger the animation to start                                                                                                   //myAnimator.SetBool("isMoving", false);
            }
        }

        if (Input.GetButtonUp("focus") || playerStats.focus <= 0)
        {                                                               //to track when player stops expending slow down speed points
            Time.timeScale = 1f;                                        //reset the speed to the original speed
            if (playerStats.focus < 0)
            {                                                           //to ensure that the lowest playerStats.focus possible is 0.00. 
                playerStats.focus = 0;
            }
            myAnimator.SetBool("isFocusing", false);                    //boolean that trigger the animation to stop                                                                                                           
        }
        if (isGrounded)                                                 //when the player lands, reset the timer so the player can again jump
        {                       
            jumpDurationCurrent = jumpDuration;
            myAnimator.SetBool("isJumping", false);
        }
        else
        {
            myAnimator.SetBool("isJumping", true);
        }
    }

    private void UpdatePlayerStats()
    {
        playerStats.position = transform.position;
        playerStats.velocity = _rigidbody.velocity;
    }


    private Vector3 GetFeetPosition()
    {
        Vector3 feetPosition = _collider.bounds.center;                 //get middle
        feetPosition.y -= _collider.bounds.extents.y;                   //subtract half of the y size
        feetPosition.y += _collider.offset.y;                           //take into account any offset provided
        feetPosition.x += _collider.offset.x;
        return feetPosition;
    }

    private Vector3 GetHeadPosition()
    {
        Vector3 headPosition = _collider.bounds.center;                 //get middle
        headPosition.y += _collider.bounds.extents.y;                   //add half of the y size
        headPosition.y += _collider.offset.y;                           //take into account any offset provided
        headPosition.x += _collider.offset.x;
        return headPosition;
    }

    public override string ToString() {
        return this.GetType().ToString();
    }

}