using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Counter : NetworkBehaviour
{
    [SerializeField] Hero _hero, _otherHero;
    [SerializeField] Counter _otherCounter;
    [SerializeField] int _weakAttackDamage = 25, _strongAttackDamage = 50;
    [SerializeField] Transform _startPos, _currentPos, _targetPos;
    [SerializeField] string _gridPosString; // Make sure this is set before play
    float _moveSpeed = 1f;
    CameraShake _cameraShake;
    GameManager _gameManager;
    AudioManager _audioManager;
    Animator _anim;

    public string GridPosString { get => _gridPosString; set => _gridPosString = value; }

    void Awake()
    {
        _audioManager = FindAnyObjectByType<AudioManager>();
        _gameManager = FindAnyObjectByType<GameManager>();
        _anim = GetComponentInChildren<Animator>();
        _cameraShake = FindAnyObjectByType<CameraShake>();
    }

    public void PlaceToStart()
    {
        gameObject.transform.position = _startPos.position;
        _currentPos = _startPos;
        _anim.SetBool("isDead", false);
        UpdateCounterPosString();
        AssignOtherCounterAndHero();
    }

    void AssignOtherCounterAndHero()
    {
        if (this.name == "Blue Counter(Clone)")
        {
            _otherCounter = _gameManager.RedCounter;
            _otherHero = _gameManager.RedHero;
        }
        else
        {
            _otherCounter = _gameManager.BlueCounter;
            _otherHero = _gameManager.BlueHero;
        }
    }

    public void ExecuteMove(string[] _ghostRefs)
    {
        //foreach (string gRef in _ghostRefs)
            //Debug.Log(gRef);
            //Debug.Log(this.name + " moving to " + _ghostRefs[_ghostRefs.Length - 1]);
        StartCoroutine(MoveCounter(_ghostRefs));
    }

    public void ExecuteWeakAttack(string[] _ghostRefs)
    {
        /*foreach (string gRef in _ghostRefs)
        {
            Debug.Log(gRef);
        }*/
        Debug.Log(this.name + " weak attacking " + _ghostRefs[_ghostRefs.Length - 1]);
        StartCoroutine(WeakAttack(_ghostRefs));
    }

    public void ExecuteStrongAttack(string[] _ghostRefs)
    {
        Debug.Log(this.name + " strong attacking " + _ghostRefs[_ghostRefs.Length - 1]);
        Transform[] squares = new Transform[_ghostRefs.Length];
        for (int i = 0; i < _ghostRefs.Length; i++)
        {
            //Debug.Log(i + ", " + _ghostRefs[i]);
            int targetCoord = int.Parse(_ghostRefs[i]);
            squares[i] = GameObject.Find("Red Star " + _ghostRefs[i]).transform;
        }
        _targetPos = squares[squares.Length - 1];
        //InstantiateStrongSlashClientRpc(); // come back to this later
        if (_otherCounter._gridPosString == _targetPos.gameObject.name.Substring(_targetPos.gameObject.name.Length - 2))
            TakeDamageClientRpc(_strongAttackDamage);
        else
            _audioManager.PlaySound(_audioManager.StrongAttackMiss);

        _anim.SetBool("isAttacking", true);
        SetAnimBoolClientRpc("isAttacking", true);
        StartCoroutine(Wait());
    }

    [ClientRpc]
    void InstantiateStrongSlashClientRpc() // come back to this later
    {
        var slash = Resources.Load<GameObject>("Strong Slash");
        Instantiate(slash, _targetPos.position, Quaternion.Euler(90, 0, 180));
    }

    IEnumerator MoveCounter(string[] _ghostRefs)
    {
        _audioManager.PlaySound(_audioManager.SlideCounter);
        Transform[] squares = new Transform[_ghostRefs.Length];
        for (int i = 0; i < _ghostRefs.Length; i++)
        {
            //Debug.Log(i + ", " + _ghostRefs[i]);
            squares[i] = GameObject.Find("Green Counter " + _ghostRefs[i]).transform;
        }

        _targetPos = squares[squares.Length -1];
        //Debug.Log("targetPos: " + _targetPos);
        _anim.SetBool("isMoving", true);
        SetAnimBoolClientRpc("isMoving", true);
        float time = 0;
        while (time <= 0.9)
        {
            time += Time.deltaTime / _moveSpeed;
            transform.position = Vector3.Lerp(_currentPos.position, _targetPos.position, Mathf.SmoothStep(0.0f, 1f, time));
            yield return null;
        }
        _anim.SetBool("isMoving", false);
        SetAnimBoolClientRpc("isMoving", false);
        _currentPos = _targetPos;
        UpdateCounterPosString();

        if (_gameManager.IdolPosInt == 0) yield break;

        if (int.Parse(_gridPosString) == _gameManager.IdolPosInt) _gameManager.PickupIdol(this);
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

    [ClientRpc]
    void SetAnimBoolClientRpc(string parameter,bool trueOrFalse) => _anim.SetBool(parameter, trueOrFalse);

    IEnumerator Wait() 
    {  
        float time = 0;
        while (time <= 0.9) 
        {
            time += Time.deltaTime;
            yield return null;
        }
    }

    [ClientRpc]
    void TakeDamageClientRpc(int attackDamage)
    {
        _gameManager.SetHealth(_otherHero, _otherHero.Health - attackDamage);
        _otherCounter._anim.SetBool("isHit", true);print(_otherCounter._anim.GetBool("IsHit"));
        //_cameraShake.CallShake();
        _audioManager.PlaySound(_audioManager.StrongAttackHit);
        _anim.SetBool("isAttacking", false);
        _otherCounter._anim.SetBool("isHit", false);
    }

    IEnumerator WeakAttack(string[] _ghostRefs) 
    {
        yield return null;
    }

    void UpdateCounterPosString()
    { 
        //Debug.Log(currentPos.gameObject.name);
        _gridPosString = _currentPos.gameObject.name.Substring(_currentPos.gameObject.name.Length - 2);
        //Debug.Log(gridPosString);
        UpdateCounterPosStringClientRpc(_gridPosString);
    }

    [ClientRpc]
    void UpdateCounterPosStringClientRpc(string sentGridPosString)
    {
        //Debug.Log(currentPos.gameObject.name);
        _gridPosString = sentGridPosString; // this did not update on the client!!!!!!!!
        //Debug.Log(gridPosString);
    }
}
