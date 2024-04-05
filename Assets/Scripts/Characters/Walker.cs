using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : PhysicsObject
{
    [Header("Reference")]
    public EnemyBase enemyBase;
    [SerializeField] private GameObject graphic;

    [Header("Properties")]
    [SerializeField] private LayerMask layerMask; //What can the Walker actually touch?
    [SerializeField] enum EnemyType { Bug, Zombie, Man , Thief}; //Bugs will simply patrol. Zombie's will immediately start chasing you forever until you defeat them.
    [SerializeField] EnemyType enemyType;
    [SerializeField] GameObject TargetMission;
    private Vector2 distanceFromObjective;
    [SerializeField] private bool followObjective;
    public float attentionRange;
    public float shootingRange;
    public float meleeRange;
    public bool UsingWeapon;
    public bool UsingGun;
    public bool UsingCloseRangeWeapon;
    enum WeaponType {melee, gun, machinegun, bazooka,blade };
    [SerializeField] WeaponType StartingWeapon;
    public bool UsingMelee;
    public int ammo;
    public int ammoConsumption;
    public GameObject bullet;
    public GameObject bulletParent;
    public float FireRate;
    public float nextFireTime;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
   [SerializeField] public string StorageitemName;

    public float changeDirectionEase = 1; //How slowly should we change directions? A higher number is slower!
    [System.NonSerialized] public float direction = 1;
    private Vector2 distanceFromPlayer; //How far is this enemy from the player?
    [System.NonSerialized] public float directionSmooth = 1; //The float value that lerps to the direction integer.
    [SerializeField] private bool followPlayer;
    [SerializeField] private bool flipWhenTurning = false; //Should the graphic flip along localScale.x?
    private RaycastHit2D ground;
    public float hurtLaunchPower = 10; //How much force should be applied to the player when getting hurt?
    [SerializeField] private bool jumping;
    public float jumpPower = 7;
    [System.NonSerialized] public bool jump = false;
    [System.NonSerialized] public float launch = 1; //The float added to x and y moveSpeed. This is set with hurtLaunchPower, and is always brought back to zero
    public float maxSpeed = 7;
    [SerializeField] private float maxSpeedDeviation; //How much should we randomly deviate from maxSpeed? Ensures enemies don't move at exact same speed, thus syncing up.
    [SerializeField] private bool neverStopFollowing = false; //Once the player is seen by an enemy, it will forever follow the player.
    private Vector3 origScale;
    [SerializeField] private Vector2 rayCastSize = new Vector2(1.5f, 1); //The raycast size: (Width, height)
    private Vector2 rayCastSizeOrig;
    [SerializeField] private Vector2 rayCastOffset;
    private RaycastHit2D rightWall;
    private RaycastHit2D leftWall;
    private RaycastHit2D rightLedge;
    private RaycastHit2D leftLedge;

    private float sitStillMultiplier = 1; //If 1, the enemy will move normally. But, if it is set to 0, the enemy will stop completely. 
    [SerializeField] private bool sitStillWhenNotFollowing = false; //Controls the sitStillMultiplier

    [Header("Sounds")]
    public AudioClip jumpSound;
    public AudioClip stepSound;


    [Header("Animator")]

    public Animator animator;
    // optional setting


    [SerializeField] private bool DisableIniator;  // The purpose is to create a thief Mob who can steal key and drop Key
    
    Instantiator instantiator;

    void Start()
    {
        animator = GetComponent<Animator>();
        instantiator = GetComponent<Instantiator>();
        if(DisableIniator)
        {
            instantiator.enabled = false;
        }
        if(TargetMission != null)
        {
            TargetMission = GameObject.Find("Key");
        }
        enemyBase = GetComponent<EnemyBase>();
        origScale = transform.localScale;
        rayCastSizeOrig = rayCastSize;
        maxSpeed -= Random.Range(0, maxSpeedDeviation);
        launch = 0;

       
        //if (enemyType == EnemyType.Zombie)
        //{
        //    direction = 0;
        //    directionSmooth = 0;
        //}
    }

    //private static Walker instance;
    //public static Walker Instance
    //{
    //    get
    //    {
    //        if (instance == null) instance = GameObject.FindObjectOfType<Walker>();
    //        return instance;
    //    }
    //}

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attentionRange);

        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }

    private void Update()
    {
        ComputeVelocity();
       
    }

    protected void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;

        if (!NewPlayer.Instance.frozen)
        {
            distanceFromPlayer = new Vector2(NewPlayer.Instance.gameObject.transform.position.x - transform.position.x, NewPlayer.Instance.gameObject.transform.position.y - transform.position.y);
            if(TargetMission != null)
            {
                distanceFromObjective = new Vector2(TargetMission.transform.position.x - transform.position.x, TargetMission.transform.position.y - transform.position.y);
            }
            directionSmooth += ((direction * sitStillMultiplier) - directionSmooth) * Time.deltaTime * changeDirectionEase;
            move.x = (1 * directionSmooth) + launch;
            launch += (0 - launch) * Time.deltaTime;

            if (move.x < 0)
            {
                transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
            }
            else
            {
                transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);
            }

            if (!enemyBase.recoveryCounter.recovering)
            {
                //Flip the graphic depending on the speed
                if (move.x > 0.01f)
                {
                    if (graphic.transform.localScale.x == -1)
                    {
                        graphic.transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                    }
                }
                else if (move.x < -0.01f)
                {
                    if (graphic.transform.localScale.x == 1)
                    {
                        graphic.transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                    }
                }
                //Check floor type
                ground = Physics2D.Raycast(transform.position, -Vector2.up);
                Debug.DrawRay(transform.position, -Vector2.up, Color.green);

              if(enemyType == EnemyType.Thief)
                {
                  
                    if ((Mathf.Abs(distanceFromObjective.x) < attentionRange) && (Mathf.Abs(distanceFromObjective.y) < attentionRange))
                    {
                        followObjective = true;
                        if (inventory.ContainsKey("Key"))
                        {
                            instantiator.enabled = true;
                            followObjective = false;

                            if (distanceFromPlayer.x < 0)
                            {
                                direction = 1;
                            }
                            else
                            {
                                direction = -1;
                            }
                        }
                    }
                }
                if (enemyType == EnemyType.Man)
                {

                    if (StartingWeapon == WeaponType.melee)
                    {
                        UsingMelee = true;
                        meleeRange = 3;
                    }
                    if (inventory.ContainsKey("gun")|| StartingWeapon == WeaponType.gun)
                    {
                        UsingWeapon = true;
                        StorageitemName = "gun";
                        
                        UsingGun = true;
                        FireRate = 1;
                        ammoConsumption = 1;
                    }
                    if (inventory.ContainsKey("Key"))
                    {
                        instantiator.enabled = true;
                    }

                    if (inventory.ContainsKey("blade") || StartingWeapon == WeaponType.blade)
                    {
                        UsingWeapon = true;
                        StorageitemName = "blade";
                        UsingMelee = true;
                        meleeRange = 6;
                        ammoConsumption = 1;
                    }
                    if (inventory.ContainsKey("machinegun") || StartingWeapon == WeaponType.machinegun)
                    {

                        UsingWeapon = true;
                        StorageitemName = "machinegun";
                        UsingGun = true;
                        FireRate = 0.3f;
                        ammoConsumption = 1;
                    }
                    if (inventory.ContainsKey("bazooka"))
                    {
                        StorageitemName = "bazooka";
                        UsingGun = true;
                        FireRate = 1.5f;
                        ammoConsumption = 1;
                    }
                    if ((Mathf.Abs(distanceFromPlayer.x) < attentionRange) && (Mathf.Abs(distanceFromPlayer.y) < attentionRange))
                    {
                        followPlayer = true;
                        sitStillMultiplier = 1;

                        // weapon for ranged
                        if (UsingGun)
                        {
                            if (ammo >= 0)
                            {
                                if ((Mathf.Abs(distanceFromPlayer.x) < shootingRange))
                                {
                                    sitStillMultiplier = 0;
                                    Shoot(true, ammoConsumption);
                                }
                            }
                            else
                            {
                             
                                inventory.Remove(StorageitemName);
                                UsingWeapon = false;
                                UsingGun = false;
                                UsingMelee = true;
                            }
                        }
                        if (UsingMelee)
                        {
                            if ((Mathf.Abs(distanceFromPlayer.x) < meleeRange))
                            {
                                sitStillMultiplier = 0;
                                Swipe();

                            }
                        }
                    }
                    else
                    {
                        followPlayer = false;
                    }
                }
                if (followPlayer)
                {
                    rayCastSize.y = 200;
                    if (distanceFromPlayer.x < 0)
                    {
                        direction = -1;
                    }
                    else
                    {
                        direction = 1;
                    }
                }
                else
                {
                    rayCastSize.y = rayCastSizeOrig.y;
                }
                if (followObjective)
                {
                    rayCastSize.y = 200;
                    if (distanceFromObjective.x < 0)
                    {
                        direction = -1;
                    }
                    else
                    {
                        direction = 1;
                    }
                }
                else
                {
                    rayCastSize.y = rayCastSizeOrig.y;
                }

                //Check for walls
                rightWall = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + rayCastOffset.y), Vector2.right, rayCastSize.x, layerMask);
                Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + rayCastOffset.y), Vector2.right * rayCastSize.x, Color.yellow);

                if (rightWall.collider != null)
                {
                    if (!followPlayer)
                    {
                        direction = -1;
                    }
                    else if (direction == 1)
                    {
                        Jump();
                    }

                }

                leftWall = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + rayCastOffset.y), Vector2.left, rayCastSize.x, layerMask);
                Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + rayCastOffset.y), Vector2.left * rayCastSize.x, Color.blue);

                if (leftWall.collider != null)
                {
                    if (!followPlayer)
                    {
                        direction = 1;
                    }
                    else if (direction == -1)
                    {
                        Jump();
                    }
                }

                //Check for ledges. Walker's height check is much higher! They will fall pretty far, but will not fall to death. 
                rightLedge = Physics2D.Raycast(new Vector2(transform.position.x + rayCastOffset.x, transform.position.y), Vector2.down, rayCastSize.y, layerMask);
                Debug.DrawRay(new Vector2(transform.position.x + rayCastOffset.x, transform.position.y), Vector2.down * rayCastSize.y, Color.blue);
                if ((rightLedge.collider == null || rightLedge.collider.gameObject.layer == 14) && direction == 1)
                {
                    direction = -1;
                }

                leftLedge = Physics2D.Raycast(new Vector2(transform.position.x - rayCastOffset.x, transform.position.y), Vector2.down, rayCastSize.y, layerMask);
                Debug.DrawRay(new Vector2(transform.position.x - rayCastOffset.x, transform.position.y), Vector2.down * rayCastSize.y, Color.blue);
                if ((leftLedge.collider == null || leftLedge.collider.gameObject.layer == 14) && direction == -1)
                {
                    direction = 1;
                }
            }
        }

        //enemyBase.animator.SetBool("grounded", grounded);
        //enemyBase.animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
        targetVelocity = move * maxSpeed;
    }

    public void Shoot(bool equip, int ammoConsumption)
    {
        if (equip)
        {
            if (nextFireTime < Time.time)
            {
                Instantiate(bullet, bulletParent.transform.position, Quaternion.identity);
                ammo -= ammoConsumption;
                nextFireTime = Time.time + FireRate;
            }
        }
    }

    public void Swipe()
    {
        animator.SetBool("attack", false);

        if (nextFireTime < Time.time)
            {
            animator.SetBool("attack", true);

            nextFireTime = Time.time + FireRate;
            }
       
    }

    public void Jump()
    {
        if (grounded)
        {
            velocity.y = jumpPower;
            PlayJumpSound();
            PlayStepSound();
        }
    }
    public void PlayStepSound()
    {
        enemyBase.audioSource.pitch = (Random.Range(0.6f, 1f));
        enemyBase.audioSource.PlayOneShot(stepSound);
    }

    public void PlayJumpSound()
    {
        enemyBase.audioSource.pitch = (Random.Range(0.6f, 1f));
        enemyBase.audioSource.PlayOneShot(jumpSound);
    }

}