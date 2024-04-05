using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/*Adds player functionality to a physics object*/

[RequireComponent(typeof(RecoveryCounter))]

public class NewPlayer : PhysicsObject
{
    [Header("Reference")]
    public AudioSource audioSource;
    [SerializeField] private Animator animator;
   // private AnimatorFunctions animatorFunctions;
    public GameObject attackHit;
    private CapsuleCollider2D capsuleCollider;
    public CameraEffects cameraEffects;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private AudioSource flameParticlesAudioSource;
    [SerializeField] private GameObject graphic;
    [SerializeField] private Component[] graphicSprites;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private GameObject pauseMenu;
    public RecoveryCounter recoveryCounter;
    
    // Singleton instantiation
    private static NewPlayer instance;
    public static NewPlayer Instance
    {
        get
        {
            if (instance == null) instance = GameObject.FindObjectOfType<NewPlayer>();
            return instance;
        }
    }
    Rigidbody2D myRigid;
    private float StartingGravity;
    [Header("Properties")]
    [SerializeField] private string[] cheatItems;
    public bool dead = false;
    public bool frozen = false;
    private float fallForgivenessCounter; //Counts how long the player has fallen off a ledge
    [SerializeField] private float fallForgiveness = 0.2f; //How long the player can fall from a ledge and still jump
    [System.NonSerialized] public string groundType = "grass";
    [System.NonSerialized] public RaycastHit2D ground;
    [SerializeField] private LayerMask WhatisGround;
    [SerializeField] Vector2 hurtLaunchPower; //How much force should be applied to the player when getting hurt?
    private float launch; //The float added to x and y moveSpeed. This is set with hurtLaunchPower, and is always brought back to zero
    [SerializeField] private float launchRecovery; //How slow should recovering from the launch be? (Higher the number, the longer the launch will last)
    public float maxSpeed = 7; //Max move speed
    public float jumpPower = 17;
    public bool jumping;
    public bool grabbing;
    public bool InterractionWithObject;
    bool Facingright =true;
    Vector2 offesetPointForJump = new Vector2(0, -1f);
    Vector2 rayCastsize = new Vector2(0, -2);
    private Vector3 origLocalScale;
    [System.NonSerialized] public bool pounded;
    [System.NonSerialized] public bool pounding;
    [System.NonSerialized] public bool shooting = false;

    [Header("Inventory")]
    public float ammo;
    public int coins;
    public int health;
    public int maxHealth;
    public int maxAmmo;

    [Header("Sounds")]
    public AudioClip deathSound;
    public AudioClip equipSound;
    public AudioClip grassSound;
    public AudioClip hurtSound;
    public AudioClip[] hurtSounds;
    public AudioClip holsterSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip poundSound;
    public AudioClip punchSound;
    public AudioClip[] poundActivationSounds;
    public AudioClip outOfAmmoSound;
    public AudioClip stepSound;
    [System.NonSerialized] public int whichHurtSound;

    float test;
    bool groundcheck;
    void Start()
    {
        Cursor.visible = false;
        SetUpCheatItems();
        health = maxHealth;
      //  animatorFunctions = GetComponent<AnimatorFunctions>();
        origLocalScale = transform.localScale;
        recoveryCounter = GetComponent<RecoveryCounter>();
        graphicSprites = GetComponentsInChildren<SpriteRenderer>();
        SetGroundType();
        myRigid = GetComponent<Rigidbody2D>();
        StartingGravity = myRigid.gravityScale;
        

    }

     void Update()
    {
        
        ComputeVelocity();
    }
 

    protected void ComputeVelocity()
    {
        //Player movement & attack
        Vector2 move = Vector2.zero;
        //ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), -Vector2.up,WhatisGround);
        groundcheck = IsGrounded();
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), Vector2.down, Color.red);
        launch += (0 - launch) * Time.deltaTime * launchRecovery;
        test += (0 - test) * Time.deltaTime* launchRecovery;

        var InputHorizontal = Input.GetAxisRaw("Horizontal");
    
        if (!frozen)
        {
            move.x = Input.GetAxis("Horizontal") +launch +test;
            if (Input.GetButtonDown("Jump") && groundcheck)   ///&& animator.GetBool("grounded") == true
            {

                //animator.SetBool("pounded", false);
                animator.SetBool("OnJump", true);
                jumping = true;
                velocity.y = jumpPower;
                groundcheck = false;
            }
            PlayerFlip(move);
            //Punch
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("attack");
                Shoot(false);
                if (InterractionWithObject)
                {
                    Interraction();
                }
                else
                {
                    animator.SetBool("parkour", false);
                }
            }
           
            if (Input.GetMouseButtonDown(1))
            {
                // ledge grab and snap above the ground
                transform.position = new Vector3(transform.position.x +(0.5f * transform.localScale.x), transform.position.y * 0.5f,transform.position.z);
               

            }
            else if (Input.GetMouseButtonUp(0))
            {
                Shoot(false);
            }
            if (shooting)
            {
                SubtractAmmo();
            }

            if(grabbing)
            {
                transform.position = new Vector3(transform.position.x + (0.8f * transform.localScale.x), transform.position.y * 0.5f, transform.position.z);

                Debug.Log(0.8f * transform.localScale.x);
                grabbing = false;
                myRigid.gravityScale = StartingGravity;

            }
            // Allow the player to jump even if they have just fallen off an edge("fall forgiveness")
            if (!grounded)
            {
                Debug.Log("FAlling");
                if (fallForgivenessCounter < fallForgiveness && !jumping)
                {
                    fallForgivenessCounter += Time.deltaTime;
                }
                else
                {
                    //animator.SetBool("grounded", false);
                }
            }
            else
            {
                fallForgivenessCounter = 0;
                //animator.SetBool("grounded", true);
            }



            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
            animator.SetFloat("velocityY", velocity.y);
            //animator.SetInteger("attackDirectionY", (int)Input.GetAxis("VerticalDirection"));
            animator.SetInteger("moveDirection", (int)Input.GetAxis("Horizontal"));  // HorizontalDirection
            //animator.SetBool("hasChair", GameManager.Instance.inventory.ContainsKey("chair"));
            targetVelocity = move * maxSpeed;

        }
        else
        {
            //If the player is set to frozen, his launch should be zeroed out!
            launch = 0;        }
    }
    public void PlayerFlip(Vector2 move)
    {
        //Flip the graphic's localScale
        if (move.x > 0.01f && !Facingright)
        {
            Flip();
            // graphic.transform.localScale = new Vector3(origLocalScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (move.x < -0.01f && Facingright)
        {
            Flip();
            //graphic.transform.localScale = new Vector3(-origLocalScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    public void Flip()
    {
        Vector3 currentscale = graphic.transform.localScale;
        currentscale.x *= -1;
        graphic.transform.localScale = currentscale;
        Facingright = !Facingright;
    }
    public void Interraction()
    {
        animator.SetBool("parkour",true);
        if (Facingright)
        {
            test = 1 * (hurtLaunchPower.x);
        }
        if (!Facingright)
        {
            test = -1 * (hurtLaunchPower.x);
        }
    }
    public bool IsGrounded()
    {      
        return  ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + offesetPointForJump.y), Vector2.down, 2f,WhatisGround);
         
    }

    public void SetGroundType()
    {
        //If we want to add variable ground types with different sounds, it can be done here
        switch (groundType)
        {
            case "Ground":
                stepSound = grassSound;
               
                break;
        }
    }
    public void Freeze(bool freeze)
    {
        //Set all animator params to ensure the player stops running, jumping, etc and simply stands
        if (freeze)
        {
            animator.SetInteger("moveDirection", 0);
            animator.SetBool("grounded", true);
            animator.SetFloat("velocityX", 0f);
            animator.SetFloat("velocityY", 0f);
            GetComponent<PhysicsObject>().targetVelocity = Vector2.zero;
        }

        frozen = freeze;
        shooting = false;
        launch = 0;
      
    }


    public void GetHurt(int hurtDirection, int hitPower)
    {
        //If the player is not frozen (ie talking, spawning, etc), recovering, and pounding, get hurt!
        if (!frozen && !recoveryCounter.recovering && !pounding)
        {
            //HurtEffect();
            cameraEffects.Shake(100, 1);
            animator.SetTrigger("hurt");
            velocity.y = hurtLaunchPower.y;
            launch = hurtDirection * (hurtLaunchPower.x);
            recoveryCounter.counter = 0;

            if (health <= 0)
            {
                StartCoroutine(Die());
            }
            else
            {
                health -= hitPower;
            }

            GameManager.Instance.hud.HealthBarHurt();
        }
    }

    private void HurtEffect()
    {
        GameManager.Instance.audioSource.PlayOneShot(hurtSound);
        StartCoroutine(FreezeFrameEffect());
        GameManager.Instance.audioSource.PlayOneShot(hurtSounds[whichHurtSound]);

        if (whichHurtSound >= hurtSounds.Length - 1)
        {
            whichHurtSound = 0;
        }
        else
        {
            whichHurtSound++;
        }
        cameraEffects.Shake(100, 1f);
    }

    public IEnumerator FreezeFrameEffect(float length = .007f)
    {
        Time.timeScale = .1f;
        yield return new WaitForSeconds(length);
        Time.timeScale = 1f;
    }


    public IEnumerator Die()
    {
        if (!frozen)
        {
            dead = true;
           
            Hide(true);
            Time.timeScale = .6f;
            yield return new WaitForSeconds(5f);
            //deathParticles.Emit(10);
            //GameManager.Instance.audioSource.PlayOneShot(deathSound);
            // GameManager.Instance.hud.animator.SetTrigger("coverScreen");
            //GameManager.Instance.hud.loadSceneName = SceneManager.GetActiveScene().name;
            Time.timeScale = 1f;
        }
    }

    public void ResetLevel()
    {
        Freeze(true);
        dead = false;
        health = maxHealth;
    }

    public void SubtractAmmo()
    {
        if (ammo > 0)
        {
            ammo -= 20 * Time.deltaTime;
        }
    }

    public void Jump(float jumpMultiplier)
    {
        if (velocity.y != jumpPower)  // maybe the limiter i dont know
        {
            velocity.y = jumpPower * jumpMultiplier; //The jumpMultiplier allows us to use the Jump function to also launch the player from bounce platforms
            PlayJumpSound();
            PlayStepSound();
            JumpEffect();
            jumping = true;

        }
    }

    public void PlayStepSound()
    {
        if (audioSource != null)
        {
            //Play a step sound at a random pitch between two floats, while also increasing the volume based on the Horizontal axis
            audioSource.pitch = (Random.Range(0.9f, 1.1f));
            audioSource.PlayOneShot(stepSound, Mathf.Abs(Input.GetAxis("Horizontal") / 10));
        }
    }

    public void PlayJumpSound()
    {
        if (audioSource != null)
        {
            //audioSource.pitch = (Random.Range(1f, 1f));
            GameManager.Instance.audioSource.PlayOneShot(jumpSound, .1f);
        }
    }


    public void JumpEffect()
    {
        if (jumpParticles != null)
        {
            jumpParticles.Emit(1);
        }
        if (audioSource != null)
        {
            audioSource.pitch = (Random.Range(0.6f, 1f));
            audioSource.PlayOneShot(landSound);
        }
    }

    public void LandEffect()    // problem here is that the reference is from an animation editor Unitor is called.
    {
       
           // jumpParticles.Emit(1);
          //  audioSource.pitch = (Random.Range(0.6f, 1f));
           // audioSource.PlayOneShot(landSound);
            jumping = false;
        
    }

    public void PunchEffect()
    {
        GameManager.Instance.audioSource.PlayOneShot(punchSound);
        cameraEffects.Shake(100, 1f);
    }

    public void ActivatePound()
    {
        //A series of events needs to occur when the player activates the pound ability
        if (!pounding)
        {
            animator.SetBool("pounded", false);

            if (velocity.y <= 0)
            {
                velocity = new Vector3(velocity.x, hurtLaunchPower.y / 2, 0.0f);
            }

            GameManager.Instance.audioSource.PlayOneShot(poundActivationSounds[Random.Range(0, poundActivationSounds.Length)]);
            pounding = true;
            FreezeFrameEffect(.3f);
        }
    }
    //public void PoundEffect()
    //{
    //    //As long as the player as activated the pound in ActivatePound, the following will occur when hitting the ground.
    //    if (pounding)
    //    {
    //        animator.ResetTrigger("attack");
    //        velocity.y = jumpPower / 1.4f;
    //        animator.SetBool("pounded", true);
    //        GameManager.Instance.audioSource.PlayOneShot(poundSound);
    //        cameraEffects.Shake(200, 1f);
    //        pounding = false;
    //        recoveryCounter.counter = 0;
    //        animator.SetBool("pounded", true);
    //    }
    //}

    public void FlashEffect()
    {
        //Flash the player quickly
        animator.SetTrigger("flash");
    }

    public void Hide(bool hide)
    {
        Freeze(hide);
        foreach (SpriteRenderer sprite in graphicSprites)
            sprite.gameObject.SetActive(!hide);
    }

    public void Shoot(bool equip)
    {
        //Flamethrower ability
        if (GameManager.Instance.inventory.ContainsKey("flamethrower"))
        {
            if (equip)
            {
                if (!shooting)
                {
                    animator.SetBool("shooting", true);
                    GameManager.Instance.audioSource.PlayOneShot(equipSound);
                    flameParticlesAudioSource.Play();
                    shooting = true;
                }
            }
            else
            {
                if (shooting)
                {
                    animator.SetBool("shooting", false);
                    flameParticlesAudioSource.Stop();
                    GameManager.Instance.audioSource.PlayOneShot(holsterSound);
                    shooting = false;
                }
            }
        }
    }

    public void SetUpCheatItems()
    {
        //Allows us to get various items immediately after hitting play, allowing for testing. 
        for (int i = 0; i < cheatItems.Length; i++)
        {
            GameManager.Instance.GetInventoryItem(cheatItems[i], null);
        }
    }
}