using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Used for coins, health, inventory items, and even ammo if you want to create a gun shooting mechanic!*/

public class Collectable : MonoBehaviour
{

    enum ItemType { InventoryItem, Coin, Health, Ammo,Weapon }; //Creates an ItemType category
    [SerializeField] ItemType itemType; //Allows us to select what type of item the gameObject is in the inspector
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioClip[] collectSounds;
    [SerializeField] private int itemAmount;
    [SerializeField] private string itemName; //If an inventory item, what is its name?
    [SerializeField] private Sprite UIImage; //What image will be displayed if we collect an inventory item?
    public Rigidbody2D myRigid;
    public BoxCollider2D parentCollider;

    Walker enemyWalker;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        myRigid = transform.parent.GetComponent<Rigidbody2D>();
        parentCollider = transform.parent.GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == NewPlayer.Instance.gameObject)
        {
            Collect();
            
        }
        if (col.gameObject.name.Contains("EnemyWalker"))
        {
            enemyWalker = col.GetComponent<Walker>();
            if (enemyWalker.UsingWeapon == false)   // i believe to change into using weapon instead of using gun, after that identify the string and set the using gun to true.
            {
                EnemyCollect();
            }
            else
            {
                Physics2D.IgnoreCollision(parentCollider, col);
            }
        }
        if (col.gameObject.name.Contains("Thief"))
        {
            enemyWalker = col.GetComponent<Walker>();
            if (enemyWalker.UsingWeapon == false)   // i believe to change into using weapon instead of using gun, after that identify the string and set the using gun to true.
            {
                EnemyCollect();
            }
            else
            {
                Physics2D.IgnoreCollision(parentCollider, col);
            }
        }
        //Collect me if I trigger with an object tagged "Death Zone", aka an area the player can fall to certain death
        if (col.gameObject.layer == 14)
        {
            Collect();
        }
    }

    public void EnemyCollect()
    {
        if (itemType == ItemType.Weapon)
        {
            enemyWalker.StorageitemName = this.itemName;
            enemyWalker.inventory.Add(itemName, itemAmount);
            enemyWalker.UsingWeapon = true;
            enemyWalker.ammo += itemAmount;
        }
        if (itemType == ItemType.InventoryItem)
        {
            if (itemName != "")
            {
                enemyWalker.inventory.Add(itemName, itemAmount);
            }
        }

        // GameManager.Instance.audioSource.PlayOneShot(collectSounds[Random.Range(0, collectSounds.Length)], Random.Range(.6f, 1f));

        //   NewPlayer.Instance.FlashEffect();


        //If my parent has an Ejector script, it means that my parent is actually what needs to be destroyed, along with me, once collected
        if (transform.parent.GetComponent<Ejector>() != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Collect()
    {
        if (itemType == ItemType.InventoryItem)
        {
            if (itemName != "")
            {
                GameManager.Instance.GetInventoryItem(itemName, UIImage);
            }
        }
        else if (itemType == ItemType.Coin)
        {
            NewPlayer.Instance.coins += itemAmount;
        }
        else if (itemType == ItemType.Health)
        {
            if (NewPlayer.Instance.health < NewPlayer.Instance.maxHealth)
            {
                GameManager.Instance.hud.HealthBarHurt();
                NewPlayer.Instance.health += itemAmount;
            }
        }
        else if (itemType == ItemType.Ammo)
        {
            if (NewPlayer.Instance.ammo < NewPlayer.Instance.maxAmmo)
            {
                GameManager.Instance.hud.HealthBarHurt();
                NewPlayer.Instance.ammo += itemAmount;
            }
        }

       // GameManager.Instance.audioSource.PlayOneShot(collectSounds[Random.Range(0, collectSounds.Length)], Random.Range(.6f, 1f));

     //   NewPlayer.Instance.FlashEffect();


        //If my parent has an Ejector script, it means that my parent is actually what needs to be destroyed, along with me, once collected
        if (transform.parent.GetComponent<Ejector>() != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}