using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/*Manages and updates the HUD, which contains your health bar, coins, etc*/

public class HUD : MonoBehaviour
{
    [Header("Reference")]
    public Animator animator;
    [SerializeField] private GameObject ammoBar;
    public TextMeshProUGUI coinsMesh;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image inventoryItemGraphic;
    [SerializeField] private GameObject startUp;

    private float ammoBarWidth;
    private float ammoBarWidthEased; //Easing variables slowly ease towards a number
    [System.NonSerialized] public Sprite blankUI; //The sprite that is shown in the UI when you don't have any items
    private float coins;
    private float coinsEased;
    private float healthBarWidth;
    private float healthBarWidthEased;
    [System.NonSerialized] public string loadSceneName;
    [System.NonSerialized] public bool resetPlayer;

    void Start()
    {
        //Set all bar widths to 1, and also the smooth variables.
        healthBarWidth = 1;
        healthBarWidthEased = healthBarWidth;
        ammoBarWidth = 1;
        ammoBarWidthEased = ammoBarWidth;
        coins = (float)NewPlayer.Instance.coins;
        coinsEased = coins;
        blankUI = inventoryItemGraphic.GetComponent<Image>().sprite;
    }

    void Update()
    {
        //Update coins text mesh to reflect how many coins the player has! However, we want them to count up.
        coinsMesh.text = Mathf.Round(coinsEased).ToString();
        coinsEased += ((float)NewPlayer.Instance.coins - coinsEased) * Time.deltaTime * 5f;

        //if (coinsEased >= coins)
        //{
        //    //animator.SetTrigger("getGem");
        //    coins = coinsEased + 1;
        //}

        //Controls the width of the health bar based on the player's total health
        /// GEtting NaN, because you have to set Healthbarwidth to 1 during the start, the problem when life is close to death the xhealth/divide will be lessened. 
        /// Without the multiplier with healthbarwidth eased the width wont reach to the infinity e-17
        /// // find bette rlife bar system.Calculation may be heavy for computer.


        healthBarWidth = (float)NewPlayer.Instance.health / (float)NewPlayer.Instance.maxHealth;
           
            healthBarWidthEased += (healthBarWidth - healthBarWidthEased) * Time.deltaTime * healthBarWidthEased;
           

            healthBar.transform.localScale = new Vector2(healthBarWidthEased, 1);
       

        //if (healthBar.transform.localScale.x >= 0)
        //{
        //    healthBarWidth = (float)NewPlayer.Instance.health / (float)NewPlayer.Instance.maxHealth;
        //    Debug.Log(healthBarWidth + "Health bar width");
        //    healthBarWidthEased += (healthBarWidth - healthBarWidthEased) * Time.deltaTime* 100;
        //    Debug.Log(healthBarWidthEased + "ease out");

        //    healthBar.transform.localScale = new Vector2(healthBarWidthEased, 1);
        //}

        //Controls the width of the ammo bar based on the player's total ammo
        if (ammoBar)
        {
            ammoBarWidth = (float)NewPlayer.Instance.ammo / (float)NewPlayer.Instance.maxAmmo;
            ammoBarWidthEased += (ammoBarWidth - ammoBarWidthEased) * Time.deltaTime * ammoBarWidthEased;
            ammoBar.transform.localScale = new Vector2(ammoBarWidthEased, transform.localScale.y);
        }

    }

    public void HealthBarHurt()
    {
        //animator.SetTrigger("hurt");
    }

    public void SetInventoryImage(Sprite image)
    {
        inventoryItemGraphic.sprite = image;
    }

    //void ResetScene()
    //{
    //    if (GameManager.Instance.inventory.ContainsKey("reachedCheckpoint"))
    //    {
    //        //Send player back to the checkpoint if they reached one!
    //        NewPlayer.Instance.ResetLevel();
    //    }
    //    else
    //    {
    //        //Reload entire scene
    //        SceneManager.LoadScene(loadSceneName);
    //    }
    //}

}