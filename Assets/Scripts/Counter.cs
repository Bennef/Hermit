using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Counter : NetworkBehaviour
{
    public Hero hero, otherHero;
    public Counter otherCounter;
    public int WeakAttackDamage = 25, StrongAttackDamage = 50;
    public Transform startPos, currentPos, targetPos;
    public string gridPosString; // Make sure this is set before play
    float moveSpeed = 1f;
    CameraShake cameraShake;
    GameManager gameManager;
    AudioManager audioManager;
    Animator anim;

    void Awake()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        anim = GetComponentInChildren<Animator>();
        //Debug.Log(anim);
    }

    public void PlaceToStart()
    {
        gameObject.transform.position = startPos.position;
        currentPos = startPos;
        anim.SetBool("isDead", false);
        UpdateCounterPosString();
        AssignOtherCounterAndHero();
    }

    void AssignOtherCounterAndHero()
    {
        if (this.name == "Blue Counter(Clone)")
        {
            otherCounter = gameManager.RedCounter;
            otherHero = gameManager.redHero;
        }
        else
        {
            otherCounter = gameManager.BlueCounter;
            otherHero = gameManager.blueHero;
        }
    }
    public void ExecuteMove(string[] ghostRefs)
    {
        foreach (string gRef in ghostRefs)
        {
            Debug.Log(gRef);
        }
        Debug.Log(this.name + " moving to " + ghostRefs[ghostRefs.Length - 1]);
        StartCoroutine(MoveCounter(ghostRefs));
    }

    public void ExecuteWeakAttack(string[] ghostRefs)
    {
        foreach (string gRef in ghostRefs)
        {
            //Debug.Log(gRef);
        }
        Debug.Log(this.name + " weak attacking " + ghostRefs[ghostRefs.Length - 1]);
        StartCoroutine(WeakAttack(ghostRefs));
    }

    public void ExecuteStrongAttack(string[] ghostRefs)
    {
        Debug.Log(this.name + " strong attacking " + ghostRefs[ghostRefs.Length - 1]);

        var slash = Resources.Load<GameObject>("Strong Slash");
        Transform[] squares = new Transform[ghostRefs.Length];
        for (int i = 0; i < ghostRefs.Length; i++)
        {
            Debug.Log(i + ", " + ghostRefs[i]);
            int targetCoord = int.Parse(ghostRefs[i]);
            //squares[i] = GameObject.Find("Green Counter " + ghostRefs[i]).transform;
        }
        targetPos = squares[squares.Length - 1];

        Instantiate(slash, targetPos.position, Quaternion.Euler(90, 0, 180));
        if (otherCounter.gridPosString == targetPos.gameObject.name.Substring(targetPos.gameObject.name.Length - 2))
        {
            TakeDamageClientRpc(StrongAttackDamage);
        }
        else
        {
            audioManager.PlaySound(audioManager.StrongAttackMiss);
        }
        anim.SetBool("isAttacking", true);
        StartCoroutine(Wait());
    }

    IEnumerator MoveCounter(string[] ghostRefs)
    {
        audioManager.PlaySound(audioManager.SlideCounter);
        Transform[] squares = new Transform[ghostRefs.Length];
        for (int i = 0; i < ghostRefs.Length; i++)
        {
            Debug.Log(i + ", " + ghostRefs[i]);
            squares[i] = GameObject.Find("Green Counter " + ghostRefs[i]).transform;
        }

        targetPos = squares[squares.Length -1];
        Debug.Log("targetPos: " + targetPos);
        anim.SetBool("isMoving", true);
        float time = 0;
        while (time <= 0.9)
        {
            time += Time.deltaTime / moveSpeed;
            transform.position = Vector3.Lerp(currentPos.position, targetPos.position, Mathf.SmoothStep(0.0f, 1f, time));
            yield return null;
        }
        anim.SetBool("isMoving", false);
        //string oldPos = gridPosString;
        currentPos = targetPos;
        UpdateCounterPosString();

        if (gameManager.idolPosInt == 0) yield break;
        if (int.Parse(gridPosString) == gameManager.idolPosInt)
        {
            gameManager.PickupIdol(this);
        }
        // check if pickup idol - we should also check along route for other player
        /*foreach (Transform ghostCounter in squares)//////////////////////////////////////////////
        {
            Debug.Log(int.Parse(ghostCounter.GetComponent<GhostCounter>().gridPosString));
            Debug.Log(gameManager.idolPosInt);
            if (gameManager.idolPosInt == int.Parse(ghostCounter.GetComponent<GhostCounter>().gridPosString))
            {
                gameManager.PickupIdol(this);
            }
        }*/
    }   
    
    IEnumerator Wait() 
    {  
        /*var slash = Resources.Load<GameObject>("Strong Slash");
        Transform[] squares = new Transform[ghostRefs.Length];
        for (int i = 0; i < ghostRefs.Length; i++)
        {
            Debug.Log(i + ", " + ghostRefs[i]);
            squares[i] = GameObject.Find("Green Counter " + ghostRefs[i]).transform;
        }
        targetPos = squares[squares.Length - 1];
        Instantiate(slash, targetPos.position, Quaternion.Euler(90, 0, 180));
        if (otherCounter.gridPosString == targetPos.gameObject.name.Substring(targetPos.gameObject.name.Length - 2)) 
        {
            TakeDamageClientRpc(StrongAttackDamage); //otherCounter.TakeDamage(StrongAttackDamage);
        } 
        else 
        {
            audioManager.PlaySound(audioManager.StrongAttackMiss);
        }
        anim.SetBool("isAttacking", true);*/
        float time = 0;
        while (time <= 0.9) 
        {
            time += Time.deltaTime;
            yield return null;
        }
        //anim.SetBool("isAttacking", false);
        //otherCounter.anim.SetBool("isHit", false);
    }

    private void TakeDamage(int attackDamage)
    {
        gameManager.SetHealth(otherHero, otherHero.health - attackDamage);
        otherCounter.anim.SetBool("isHit", true);
        //cameraShake.CallShake();
        audioManager.PlaySound(audioManager.StrongAttackHit);
        TakeDamageClientRpc(attackDamage);
    }

    [ClientRpc]
    void TakeDamageClientRpc(int attackDamage)
    {
        Debug.Log("SA sent");
        gameManager.SetHealth(otherHero, otherHero.health - attackDamage);
        otherCounter.anim.SetBool("isHit", true);
        //cameraShake.CallShake();
        audioManager.PlaySound(audioManager.StrongAttackHit);
        anim.SetBool("isAttacking", false);
        otherCounter.anim.SetBool("isHit", false);
    }

    IEnumerator WeakAttack(string[] ghostRefs) 
    {
        yield return null;
    }

    void UpdateCounterPosString()
    { 
        //Debug.Log(currentPos.gameObject.name);
        gridPosString = currentPos.gameObject.name.Substring(currentPos.gameObject.name.Length - 2);
        //Debug.Log(gridPosString);
        UpdateCounterPosStringClientRpc(gridPosString);
    }

    [ClientRpc]
    void UpdateCounterPosStringClientRpc(string sentGridPosString)
    {
        //Debug.Log(currentPos.gameObject.name);
        gridPosString = sentGridPosString; // this did not update on the client!!!!!!!!
        //Debug.Log(gridPosString);
    }
}
