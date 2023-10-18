using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [Header("Game Stats")]
    [SerializeField] float _turnTime = 60f;
    [SerializeField] float _countDownTimer;
    [SerializeField] int _currentTurn;
    [SerializeField] int _currentRound = 1;
    [SerializeField] int _blueRoundsWon, _redRoundsWon;
    [SerializeField] Hero _hasIdol;
    [SerializeField] string _roundWinner = "";
    [SerializeField] int _blueDeckCardsSelected, _redDeckCardsSelected;

    [Header("Game Flow")]
    [SerializeField] bool _aIPlaying = true;
    [SerializeField] bool _blueDiscarding, _redDiscarding;
    [SerializeField] bool _bluePickingHand = true, _redPickingHand = true;
    [SerializeField] bool _blueReadyToStart, _redReadyToStart;
    [SerializeField] bool _turnRunning;
    [SerializeField] Card _blueSelectedCard, redSelectedCard;
    [SerializeField] int _bluePlayedCardId, _redPlayedCardId;
    [SerializeField] bool _blueTurnDone = true, _redTurnDone = true;
    [SerializeField] string[][] _actionsToExecute;

    [Header("Objects")]
    [SerializeField] GameObject _idol;
    [SerializeField] GameObject _blueUIObjects, _redUIObjects;
    [SerializeField] GameObject __blueCounterObj, __redCounterObj;
    [SerializeField] GameObject _hero1Obj, _hero2Obj;
    [SerializeField] Counter _blueCounter, _redCounter, _firstToGo, _secondToGo;
    [SerializeField] int _idolPosInt;
    [SerializeField] Hero _blueHero, _redHero;
    [SerializeField] GameObject _blueCamera, _redCamera, _blueCup, _redCup;
    [SerializeField] Transform _blueHasIdolPos, _redHasIdolPos, 
        _blueCardInHandPos1, _blueCardInHandPos2, _blueCardInHandPos3, 
        _redCardInHandPos1, _redCardInHandPos2, _redCardInHandPos3;

    HCTManager _hCTManager;
    UIManager _uIManager;
    AudioManager _audioManager;
    Coroutine _turnTimerCoroutine;
    List<Card> _allCards = new List<Card>();

    public static GameManager Singleton { get; private set; }
    public Counter BlueCounter { get { return _blueCounter; }}
    public Counter RedCounter { get { return _redCounter; }}
    public int IdolPosInt { get { return _idolPosInt; }}
    public bool BlueDiscarding { get => _blueDiscarding; set => _blueDiscarding = value; }
    public bool RedDiscarding { get => _redDiscarding; set => _redDiscarding = value; }
    public bool BluePickingHand { get => _bluePickingHand; set => _bluePickingHand = value; }
    public bool RedPickingHand { get => _redPickingHand; set => _redPickingHand = value; }
    public int BlueDeckCardsSelected { get => _blueDeckCardsSelected; }
    public int RedDeckCardsSelected { get => _redDeckCardsSelected; }
    public Card BlueSelectedCard { get => _blueSelectedCard; set => _blueSelectedCard = value; }
    public Card RedSelectedCard { get => redSelectedCard; set => redSelectedCard = value; }
    public bool BlueTurnDone { get => _blueTurnDone; set => _blueTurnDone = value; }
    public bool RedTurnDone { get => _redTurnDone; set => _redTurnDone = value; }
    public Hero BlueHero { get => _blueHero; set => _blueHero = value; }
    public Hero RedHero { get => _redHero; set => _redHero = value; }

    void Awake()
    {
        AssignCamera();

        if (Singleton == null)
            Singleton = this;
        else
            Destroy(gameObject);

        SpawnObjects();

        if (!NetworkManager.Singleton.IsServer)
            FlipArrowsForRed();
    }

    void Start()
    {
        _hCTManager = FindObjectOfType<HCTManager>();
        _uIManager = FindObjectOfType<UIManager>();
        _audioManager = FindObjectOfType<AudioManager>();
        _blueHero = GameObject.Find("Blue Hero").GetComponent<Hero>(); 
        _redHero = GameObject.Find("Red Hero").GetComponent<Hero>();
        _blueCounter = GameObject.Find("Blue Counter(Clone)").GetComponent<Counter>();
        _redCounter = GameObject.Find("Red Counter(Clone)").GetComponent<Counter>();
        _uIManager.HideStartTurnButton(_uIManager._startTurnButtonBlue);
        _uIManager.HideStartTurnButton(_uIManager._startTurnButtonRed);

        if (NetworkManager.Singleton.IsServer)
            _redUIObjects.SetActive(false);
        else
            _blueUIObjects.SetActive(false);

        AssignHCTValues();
        _blueHero.PutAllCardsInDeck();
        _redHero.PutAllCardsInDeck();
        AssignActionIdsAndCardIds();
        StartRound();
    }

    void AssignHCTValues()
    {
        _blueHero.speed = _hCTManager.BlueSpeed;
        _redHero.speed = _hCTManager.RedSpeed;

        Transform blueCards = _blueHero.transform.Find("Cards");
        // should be 8 - check the hct count
        for (int i = 0; i < _hCTManager.BlueCards.Length; i++)
        {
            if (_hCTManager.BlueCards[i] != null)
            {
                GameObject newCard = Instantiate(_hCTManager.BlueCards[i]);

                newCard.transform.SetPositionAndRotation(blueCards.GetChild(i).position, blueCards.GetChild(i).rotation);
                newCard.transform.localScale = blueCards.GetChild(i).localScale;
                newCard.transform.SetParent(blueCards);
                newCard.name = newCard.name.Replace("(Clone)", "").Trim();
                Destroy(blueCards.GetChild(i).gameObject);
            }
        }print(blueCards.childCount);
        // Set dummy cards as dummy
        AssignHCTValuesClientRpc(_blueHero.speed, _redHero.speed);
    }

    [ClientRpc]
    void AssignHCTValuesClientRpc(int blueSpeed, int redSpeed)
    {
        //print(blueHero);
        _blueHero.speed = blueSpeed;
        _redHero.speed = redSpeed;
        print("d");
    }

    void FlipArrowsForRed()
    {
        GameObject N = GameObject.Find("N");
        GameObject E = GameObject.Find("E");
        GameObject S = GameObject.Find("S");
        GameObject W = GameObject.Find("W");
        GameObject NE = GameObject.Find("NE");
        GameObject SE = GameObject.Find("SE");
        GameObject SW = GameObject.Find("SW");
        GameObject NW = GameObject.Find("NW");
        foreach (Transform arrow in N.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("N", "S");

        foreach (Transform arrow in E.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("E", "W");

        foreach (Transform arrow in S.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("S", "N");

        foreach (Transform arrow in W.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("W", "E");

        foreach (Transform arrow in NE.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("NE", "SW");

        foreach (Transform arrow in SE.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("SE", "NW");

        foreach (Transform arrow in SW.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("SW", "NE");

        foreach (Transform arrow in NW.transform)
            arrow.gameObject.name = arrow.gameObject.name.Replace("NW", "SE");
    }

    void AssignActionIdsAndCardIds()
    {
        Card[] blueCards = _blueHero.GetComponentsInChildren<Card>();
        Card[] redCards = _redHero.GetComponentsInChildren<Card>();
        _allCards.AddRange(blueCards);
        _allCards.AddRange(redCards);

        int uniqueCardId = 1;
        int uniqueActionId = 1;

        foreach (Card card in _allCards)
        {
            card.cardId = uniqueCardId; // Assign unique card ID
            foreach (Action action in card.baseActions) // Iterate over the baseActions list
            {
                action.actionId = uniqueActionId; // Assign unique action ID
                uniqueActionId++;
            }
            uniqueCardId++;
        }
    }

    void AssignCamera()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _blueCamera.SetActive(true);
            _redCamera.SetActive(false);
        }
        else
        {
            _redCamera.SetActive(true);
            _blueCamera.SetActive(false);
        }
    }

    void SpawnObjects()
    {
        // Heroes
        GameObject blueHeroObj = Instantiate(_hero1Obj, Vector3.zero, Quaternion.identity);
        blueHeroObj.name = "Blue Hero";
        GameObject redHeroObj = Instantiate(_hero2Obj, Vector3.zero, Quaternion.identity);
        redHeroObj.name = "Red Hero";
        // Counters
        GameObject bc = Instantiate(__blueCounterObj, Vector3.zero, Quaternion.identity);
        GameObject rc = Instantiate(__redCounterObj, Vector3.zero, Quaternion.identity);
        // Camera layers
        int blueHeroLayer = LayerMask.NameToLayer("Blue Player");
        int redHeroLayer = LayerMask.NameToLayer("Red Player");
        SetLayer(blueHeroObj.transform.GetChild(0), blueHeroLayer);
        SetLayer(blueHeroObj.transform.GetChild(1), redHeroLayer);
        SetLayer(redHeroObj.transform.GetChild(0), redHeroLayer);
        SetLayer(redHeroObj.transform.GetChild(1), blueHeroLayer);
    }

    void SetLayer(Transform transform, int newLayer)
    {
        transform.gameObject.layer = newLayer;
        
        foreach (Transform child in transform)
        {
            child.transform.gameObject.layer = newLayer;
            foreach (Transform child1 in child.transform)
                child1.transform.gameObject.layer = newLayer;
        }
    }

    void Update() 
    {
        if (!_turnRunning && _blueReadyToStart && _redReadyToStart)
        {
            Debug.Log("Both ready to start turn");
            _blueReadyToStart = false;
            _redReadyToStart = false;
            OrganiseCardsForTurn();
            _uIManager.GameStarting();
            _currentTurn++;
            StartTurn();
        }

        if (!_turnRunning) return;
        
        if (_blueTurnDone && _redTurnDone) 
        {
            _turnRunning = _blueTurnDone = _redTurnDone = false;//////
            StartCoroutine(ExecuteFlowOfEvents());
        }
        /*
        _countDownTimer -= Time.deltaTime;
        if (_countDownTimer > 0) 
        {
            _uIManager.SetText(_uIManager.roundTimeLeftTextBlue, "Time\n" + _countDownTimer.ToString().Substring(0, 3));////
        }
        else 
        {
            _uIManager.SetText(_uIManager.roundTimeLeftTextBlue, "Time\n" + "0"); ////
        }*/
    }

    public void StartRound() 
    {
        Debug.Log("Start of round " + _currentRound);
        _blueHero.health = 100;
        _redHero.health = 100;
        _hasIdol = null;
        _bluePickingHand = true;
        _redPickingHand = true; //// send these
        _blueCounter.PlaceToStart();
        _redCounter.PlaceToStart();

        DetermineFirstToGo(_blueHero, _redHero);

        if (NetworkManager.Singleton.IsServer)
        {
            PlaceIdol();
            _uIManager.HideReadyText(_uIManager._blueReadyTextBlue); 
            _uIManager.HideReadyText(_uIManager._redReadyTextBlue); 
            _uIManager.SetHealth(_uIManager._blueHealthSliderBlue, _blueHero.health, _uIManager._blueHealthTextBlue); 
            _uIManager.SetHealth(_uIManager._redHealthSliderBlue, _redHero.health, _uIManager._redHealthTextBlue); 
            _uIManager.SetText(_uIManager._roundTextBlue, "Round " + _currentRound); 
        }
        else 
        {
            _uIManager.HideReadyText(_uIManager._blueReadyTextRed); 
            _uIManager.HideReadyText(_uIManager._redReadyTextRed); 
            _uIManager.SetHealth(_uIManager._blueHealthSliderRed, _blueHero.health, _uIManager._blueHealthTextRed); 
            _uIManager.SetHealth(_uIManager._redHealthSliderRed, _redHero.health, _uIManager._redHealthTextRed); 
            _uIManager.SetText(_uIManager._roundTextRed, "Round " + _currentRound); 
        }

        _uIManager.ShowMyDeckScreen();
    }

    public void EndRound() 
    {
        Debug.Log("End of round " + _currentRound);
        _uIManager.HideCloseMyDeckButton();
        _uIManager.HideStartTurnButton(_uIManager._startTurnButtonBlue);
        //_audioManager.PlaySound(_audioManager.RoundWin);
        ResetAllCards();
        _currentRound++;
        _currentTurn = 1;
        _uIManager.SetText(_uIManager._turnTextBlue, "Turn " + _currentTurn); ////
        _blueHero.playerChoosingInitialCards = true;
        StartRound();
    }

    public void StartTurn() // only call this when both players have picked 3 cards
    {
        _turnRunning = true;
        _blueTurnDone = _redTurnDone = false;
        _bluePickingHand = _redPickingHand = false;
        _uIManager.SetText(_uIManager._turnTextBlue, "Turn " + _currentTurn); ////
        Debug.Log("Turn " + _currentTurn + " started");
        //PickCRandomCardsForHand(redHero);
        _actionsToExecute = new string[6][]; // Clear the array
        //_countDownTimer = _turnTime;
        //_turnTimerCoroutine = StartCoroutine(TurnTimer());
    }

    IEnumerator TurnTimer() 
    {
        yield return new WaitForSeconds(_turnTime);
        if (_turnRunning) 
            EndTurn();
    }

    public void EndTurn() 
    {
        _turnRunning = false;
        /*if (blueHero.hand.Count == 0) 
        {
            PickRandomCardsForHand(blueHero);
        }
        SelectRandomCardToPlay(redHero);*/
        Debug.Log("Turn over");
        if (_turnTimerCoroutine != null) 
            StopCoroutine(_turnTimerCoroutine);

        if (NetworkManager.Singleton.IsServer)
            StartCoroutine(ExecuteFlowOfEvents());
    }

    public void StartTurnButtonPressed(Hero hero) /// Show ready text for player!!!
    {
        if (hero == _blueHero)
        {
            _blueReadyToStart = true;
            UpdateReadyToStartBoolClientRpc(true);
            _blueDeckCardsSelected = 0;
            _uIManager.HideStartTurnButton(_uIManager._startTurnButtonBlue);
        }
        else
        {
            _redReadyToStart = true;
            UpdateReadyToStartBoolServerRpc(true);
            _redDeckCardsSelected = 0;
            _uIManager.HideStartTurnButton(_uIManager._startTurnButtonRed);
        }
    }

    [ClientRpc]
    void UpdateReadyToStartBoolClientRpc(bool trueOrFalse)
    {
        //Debug.Log("Setting blueIsReady to " + trueOrFalse + " on client");
        _blueReadyToStart = trueOrFalse;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateReadyToStartBoolServerRpc(bool trueOrFalse)
    {
        //Debug.Log("Setting redIsReady to " + trueOrFalse + " on server");
        _redReadyToStart = trueOrFalse;
    }  
    /*
    void PickRandomCardsForHand(Hero hero) 
    {
        Debug.Log(hero + "picking random cards! Hand = " + hero.hand.Count + ", Deck = " + hero.deck.Count);
        int numOfCardsToPick = 3 - hero.hand.Count;
        if (hero.hand.Count == 1 && hero.deck.Count == 1) 
        {
            /// we nedd to add the last card and then shuffle discard pile here
            numOfCardsToPick = 1;
            hero.deck[0].selected = true; // Select our last deck card for hand
            hero.FilldeckFromDiscardPile();
        }

        int[] randomIndexArray = GetUniqueRandomArray(0, hero.deck.Count, numOfCardsToPick);
        hero.deck[randomIndexArray[0]].selected = true;
        if (numOfCardsToPick >= 2) 
        {
            hero.deck[randomIndexArray[1]].selected = true;
        }
        if (numOfCardsToPick >= 3) 
        {
            hero.deck[randomIndexArray[2]].selected = true;
        }
        Debug.Log(hero + " randomly choosing card");
        OrganiseCardsForTurn(hero);
    }

    void SelectRandomCardToPlay(Hero hero) 
    {
        Debug.Log(hero + " selecting random card to play");
        List<Card> cardsWithValidMoves = new List<Card>();
        foreach (Card card in hero.hand) {
            if (card.availableActions.Count > 0) 
            {
                cardsWithValidMoves.Add(card);
            }
        }
        Debug.Log("cardsWithValidMoves: " + cardsWithValidMoves.Count);
        int index = UnityEngine.Random.Range (0, cardsWithValidMoves.Count);        
        if (hero == blueHero) {
            _blueSelectedCard = hero.hand[index];
            hero.cardInPlay = _blueSelectedCard;
            SelectRandomActionToPlay(_blueSelectedCard, _blueCounter, _redCounter);
        }
        if (hero == redHero) 
        {
            Debug.Log(index);
            redSelectedCard = hero.hand[index];
            hero.cardInPlay = redSelectedCard;
            SelectRandomActionToPlay(redSelectedCard, _redCounter, _blueCounter);
        }
    }

    void SelectRandomActionToPlay(Card card, Counter counter, Counter otherCounter) 
    {
        //Debug.Log(card.availableActions.Count);
        int actionIndex = UnityEngine.Random.Range (0, card.availableActions.Count);
        Action action = card.availableActions[actionIndex];
        // Select random square to play
        int ghostCounterIndex = UnityEngine.Random.Range (0, action.ghostCounters.Length);
        // Select random gridsquare to target
        action.selectedGridSquare = action.ghostCounters[ghostCounterIndex].GetComponent<GhostCounter>().gridSquare;
        if (action.ghostCounters[ghostCounterIndex].gridPosString != otherCounter.gridPosString) {
            RegisterAction(counter, action);
        } 
        else 
        {
            SelectRandomActionToPlay(card, counter, otherCounter);
        }
    }

    public static int[] GetUniqueRandomArray(int min, int max, int count) 
    {
        int[] result = new int[count];
        List<int> numbersInOrder = new List<int>();
        for (var x = min; x < max; x++) 
        {
            numbersInOrder.Add(x);
        }
        for (var x = 0; x < count; x++) 
        {
            if (numbersInOrder.Count == 0) 
            {
                // Handle the case when there are fewer elements available than the requested count.
                break;
            }
            var randomIndex = UnityEngine.Random.Range(0, numbersInOrder.Count);
            result[x] = numbersInOrder[randomIndex];
            numbersInOrder.RemoveAt(randomIndex);
        }
        return result;
    }
    */
    public void SetDeckCardsSelected(Hero hero, int valueToSet) 
    {
        if (hero == _blueHero)
        {
            _blueDeckCardsSelected = valueToSet;
            if (_blueDeckCardsSelected == 3)
                _uIManager.ShowStartTurnButton(_uIManager._startTurnButtonBlue);
            else
                _uIManager.HideStartTurnButton(_uIManager._startTurnButtonBlue);
        }
        else
        {
            _redDeckCardsSelected = valueToSet;
            if (_redDeckCardsSelected == 3)
                _uIManager.ShowStartTurnButton(_uIManager._startTurnButtonRed);
            else
                _uIManager.HideStartTurnButton(_uIManager._startTurnButtonRed);
        }
    }

    public void OrganiseCardsForTurn() 
    {
        Debug.Log("Organising cards");
        if (NetworkManager.Singleton.IsServer) 
        {
            _blueHero.MoveCardsToHand();
            PositionHandCards(_blueHero, _blueCounter);
        }
        else 
        {
            _redHero.MoveCardsToHand();
            PositionHandCards(_redHero, _redCounter);
        }
    }

    public void PositionHandCards(Hero hero, Counter counter) 
    {
        //Debug.Log(hero.hand.Count + " in hand");
        if (hero == _blueHero)
        {
            ProcessCardInHand(hero.hand[0], _blueCardInHandPos1, hero.hand[1], hero.hand[2]);
            ProcessCardInHand(hero.hand[1], _blueCardInHandPos2, hero.hand[0], hero.hand[2]);
            ProcessCardInHand(hero.hand[2], _blueCardInHandPos3, hero.hand[0], hero.hand[1]);
        }
        else
        {
            //Debug.Log(hero.hand.Count);
            ProcessCardInHand(hero.hand[0], _redCardInHandPos1, hero.hand[1], hero.hand[2]);
            ProcessCardInHand(hero.hand[1], _redCardInHandPos2, hero.hand[0], hero.hand[2]);
            ProcessCardInHand(hero.hand[2], _redCardInHandPos3, hero.hand[0], hero.hand[1]);
        }
        
        CalcGhostPositions(hero, counter);
        LockCards(hero.deck);
        _uIManager.HideMyDeckScreen();
    }

    void ProcessCardInHand(Card card, Transform cardPos, Card otherCard1, Card otherCard2) 
    {
         card.transform.position = cardPos.position;
         card.otherCard1 = otherCard1;
         card.otherCard2 = otherCard2;
    }

    void LockCards(List<Card> cards) 
    {
        foreach (Card card in cards) 
            card.locked = true;
    }

    public void UnLockCards(List<Card> cards) 
    {
        foreach (Card card in cards) 
            card.locked = false;
    }
        
    public void RegisterAction(Counter counter, Action.ActionType actionType, string[] ghostRefs) 
    {
        int flowPos = 0;
        if (actionType == Action.ActionType.Move) 
        {
            if (counter == _firstToGo) 
                flowPos = 0;
            else 
                flowPos = 1;
        }
        else if (actionType == Action.ActionType.WeakAttack)
        {
            if (counter == _firstToGo) 
                flowPos = 2;
            else 
                flowPos = 4;
        }
        else if (actionType == Action.ActionType.StrongAttack)
        {
            if (counter == _firstToGo)
                flowPos = 3;
            else
                flowPos = 5;
        }

        _actionsToExecute[flowPos] = ghostRefs;

        if (counter == _blueCounter) 
        {
            _blueSelectedCard?.HideDestinations(_blueSelectedCard.availableActions);
            RegisterActionClientRpc(flowPos, ConvertToIntArray(ghostRefs));
            //_uIManager.ShowReadyText(_uIManager.blueReadyTextBlue); ////
        }
        if (counter == _redCounter) 
        {
            redSelectedCard?.HideDestinations(redSelectedCard.availableActions);
            RegisterActionServerRpc(flowPos, ConvertToIntArray(ghostRefs));
            //_uIManager.ShowReadyText(_uIManager.redReadyTextBlue); ////
        }
    }

    int[] ConvertToIntArray(string[] ghostRefs) 
    {
        //Debug.Log(ghostRefs.Length);
        int[] lastTwoDigitsArray = new int[ghostRefs.Length];

        for (int i = 0; i < ghostRefs.Length; i++)
        {
            string ghostRef = ghostRefs[i];

            if (ghostRef.Length >= 2)
            {
                string lastTwoDigits = ghostRef.Substring(ghostRef.Length - 2);
                if (int.TryParse(lastTwoDigits, out int lastTwoDigitsInt))
                    lastTwoDigitsArray[i] = lastTwoDigitsInt;
                else
                    lastTwoDigitsArray[i] = 0; // Or another appropriate default value
            }
            else
                lastTwoDigitsArray[i] = 0; // Or another appropriate default value
        }
        return lastTwoDigitsArray;
    }
    
    [ClientRpc]
    public void RegisterActionClientRpc(int flowPos, int[] intGhostRefs)//convert back to string array
    {
        string[] ghostRefsConverted = new string[intGhostRefs.Length];
        for (int i = 0; i < intGhostRefs.Length; i++)
            ghostRefsConverted[i] = intGhostRefs[i].ToString();
        _actionsToExecute[flowPos] = ghostRefsConverted;
        //Debug.Log(ghostRefsConverted[0]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterActionServerRpc(int flowPos, int[] intGhostRefs)
    {
        string[] ghostRefsConverted = new string[intGhostRefs.Length];
        for (int i = 0; i < intGhostRefs.Length; i++)
            ghostRefsConverted[i] = intGhostRefs[i].ToString();
        _actionsToExecute[flowPos] = ghostRefsConverted;
        //Debug.Log(ghostRefsConverted[0]);
    }

    IEnumerator ExecuteFlowOfEvents() 
    {
        if (NetworkManager.Singleton.IsServer) // put outside of function
        {
            Debug.Log("Flow");
            // First to go Move
            if (_actionsToExecute[0] != null)
            {
                Debug.Log("Flow 1: " + _actionsToExecute[0][0]);
                _firstToGo.ExecuteMove(_actionsToExecute[0]);
                //AddToLog(_firstToGo + " : " + _actionsToExecute2[0].actionType + " to " + _actionsToExecute[0].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }

            // Second to go Move
            if (_actionsToExecute[1] != null)
            {
                Debug.Log("Flow 2: " + _actionsToExecute[1][0]);
                _secondToGo.ExecuteMove(_actionsToExecute[1]);
                //AddToLog(_secondToGo + " : " + _actionsToExecute[1].actionType + " to " + _actionsToExecute[1].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }

            // First to go Weak Attack
            if (_actionsToExecute[2] != null)
            {
                _firstToGo.ExecuteWeakAttack(_actionsToExecute[2]);
                //AddToLog(_firstToGo + " : " + _actionsToExecute[2].actionType + " on " + _actionsToExecute[2].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                _uIManager.SetText(_uIManager._roundsTextBlue, "B : R " + _blueRoundsWon + " : " + _redRoundsWon);////
                _uIManager.SetText(_uIManager._messageTextBlue, _roundWinner + " player wins round " + _currentRound);////
                _uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break;
            }

            // First to go Strong Attack
            if (_actionsToExecute[3] != null)
            {
                _firstToGo.ExecuteStrongAttack(_actionsToExecute[3]);
                //AddToLog(_secondToGo + " : " + _actionsToExecute[3].actionType + " on " + _actionsToExecute[3].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                _uIManager.SetText(_uIManager._roundsTextBlue, "B : R " + _blueRoundsWon + " : " + _redRoundsWon);////
                _uIManager.SetText(_uIManager._messageTextBlue, _roundWinner + " player wins round " + _currentRound);////
                _uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break; 
            }

            // Second to go Weak Attack
            if (_actionsToExecute[4] != null)
            {
                _secondToGo.ExecuteWeakAttack(_actionsToExecute[4]);
                //AddToLog(_firstToGo + " : " + _actionsToExecute[4].actionType + " on " + _actionsToExecute[4].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                _uIManager.SetText(_uIManager._roundsTextBlue, "B : R " + _blueRoundsWon + " : " + _redRoundsWon);////
                _uIManager.SetText(_uIManager._messageTextBlue, _roundWinner + " player wins round " + _currentRound);////
                _uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break;
            }
            // Second to go Strong Attack
            if (_actionsToExecute[5] != null)
            {
                _secondToGo.ExecuteStrongAttack(_actionsToExecute[5]);
                //AddToLog(_secondToGo + " : " + _actionsToExecute[5].actionType + " on " + _actionsToExecute[5].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                _uIManager.SetText(_uIManager._roundsTextBlue, "B : R " + _blueRoundsWon + " : " + _redRoundsWon);////
                _uIManager.SetText(_uIManager._messageTextBlue, _roundWinner + " player wins round " + _currentRound);////
                _uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break; 
            }
            if (_blueSelectedCard != null) 
                _blueSelectedCard.selected = false;

            if (CheckForIdolWinner())
            {
                _uIManager.SetText(_uIManager._roundsTextBlue, "B : R " + _blueRoundsWon + " : " + _redRoundsWon);///
                _uIManager.SetText(_uIManager._messageTextBlue, _roundWinner + " player wins round " + _currentRound);///
                _uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break; 
            }
            //redHero.DiscardCardFromHand(redHero.cardInPlay); // Th card just played
            //redHero.DiscardRandomCardInHand(); // BUG - Red always discards 1 card here
            if (_aIPlaying)
            {
                _blueHero.DiscardCardFromHand(_blueHero.cardInPlay);////
                _blueHero.DiscardRandomCardInHand();////
                UnLockCards(_blueHero.deck);////
                _currentRound++;
                StartRound();
            }
            else
            {
                ShowPlayedCardTicksClientRpc();
                //_blueSelectedCard.PutDownCard(_blueSelectedCard.transform);// same for red
                UnLockCards(_blueHero.hand);
                _blueSelectedCard.locked = true;
                _blueDiscarding = true;
                CleanUpTurnStuffClientRpc();
            }
        }
    }

    [ClientRpc]
    void ShowPlayedCardTicksClientRpc()
    {
        foreach (Card card in _allCards)
        {
            //Debug.Log(card.cardId);
            if (card != null && card.cardId == _bluePlayedCardId || card.cardId == _redPlayedCardId)//////need to get dUMMY cards
            {
                Debug.Log(card.cardId + " should tick");
                card.ShowTick();
            }
        }
    }

    [ClientRpc]
    void CleanUpTurnStuffClientRpc()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            if (redSelectedCard != null)
            {
                redSelectedCard.selected = false;
            }
            UnLockCards(_redHero.hand);
            //redSelectedCard.PutDownCard(redSelectedCard.transform);
            redSelectedCard.locked = true;
            RedDiscarding = true;
        }
        _uIManager.ShowDiscardButton();
    }

    public void ActionSelected(Hero hero, Action.ActionType actionType, string[] ghostRefs)
    {
        Debug.Log(ghostRefs[0]);
        _audioManager.PlaySound(_audioManager.SelectSquare);
        _aIPlaying = false;
        LockCards(hero.hand);
        if (hero == _blueHero)
        {
            _blueHero.cardInPlay = _blueSelectedCard;
            RegisterAction(_blueCounter, actionType, ghostRefs);
            _bluePlayedCardId = _blueSelectedCard.cardId;
            SetBluePlayedCardClientRpc(_bluePlayedCardId);
            _blueTurnDone = true;
            UpdateTurnDoneBoolClientRpc(true);
        }
        else if (hero == _redHero)
        {
            _redHero.cardInPlay = redSelectedCard;
            RegisterAction(_redCounter, actionType, ghostRefs);
            _redPlayedCardId = redSelectedCard.cardId;
            SetRedPlayedCardServerRpc(_redPlayedCardId);
            _redTurnDone = true;
            UpdateTurnDoneBoolServerRpc(true);
        }
    }

    [ClientRpc]
    void SetBluePlayedCardClientRpc(int bluePlayedCardServer)
    {
        Debug.Log("SetBluePlayedCardClientRpc");
        _bluePlayedCardId = bluePlayedCardServer;
    }

    [ServerRpc(RequireOwnership = false)]
    void SetRedPlayedCardServerRpc(int redPlayedCardClient)
    {
        Debug.Log("SetRedPlayedCardServerRpc");
        _redPlayedCardId = redPlayedCardClient;
    }

    [ClientRpc]
    void UpdateTurnDoneBoolClientRpc(bool trueOrFalse) => _blueTurnDone = trueOrFalse;

    [ServerRpc(RequireOwnership = false)]
    void UpdateTurnDoneBoolServerRpc(bool trueOrFalse) => _redTurnDone = trueOrFalse;

    public void ResetAllCards() 
    {
        _blueHero.PutAllCardsInDeck();
        _redHero.PutAllCardsInDeck();
        _blueSelectedCard = redSelectedCard = null;
    }

    void MatchOver() 
    {
        _uIManager.SetText(_uIManager._messageTextBlue, _roundWinner + " player wins Match! ");/////////
        _uIManager.CallShowMessageOverlay();
        _uIManager.ShowButton(_uIManager._resetButtonBlue);///////
        Debug.Log("Match Over!");
    }

    bool CheckForIdolWinner() 
    {
        if (_hasIdol == _redHero && _currentTurn == 8) 
        { 
            _audioManager.PlaySound(_audioManager.IdolWin);
            _redRoundsWon++;
            _roundWinner = "Red";
            return true;
        }
        if (_hasIdol == _blueHero && _currentTurn == 8) 
        { 
            _audioManager.PlaySound(_audioManager.IdolWin);
            _blueRoundsWon++;
            _roundWinner = "Blue";
            return true;
        }
        return false;
    }

    bool CheckForDefeatWinner() 
    {
        if (_blueHero.health == 0 ) 
        {
            _audioManager.PlaySound(_audioManager.PlayerDead);
            _redRoundsWon++;
            _roundWinner = "Red";
            return true;
        }
        if (_redHero.health == 0 ) 
        {
            _audioManager.PlaySound(_audioManager.PlayerDead);
            _blueRoundsWon++;
            _roundWinner = "Blue";
            return true;
        }
        return false;
    }

    bool CheckForMatchWinner() 
    {
        if (_blueRoundsWon == 2 || _redRoundsWon == 2) 
            return true;
        return false;
    }

    public void SetHealth(Hero hero, int health) 
    {
        hero.health = health;
        if (hero == _redHero) 
        {
            _uIManager.SetHealth(_uIManager._redHealthSliderBlue, health, _uIManager._redHealthTextBlue);
            _uIManager.SetHealth(_uIManager._redHealthSliderBlue, health, _uIManager._redHealthTextRed);
        }
        else 
        {
            _uIManager.SetHealth(_uIManager._blueHealthSliderBlue, health, _uIManager._blueHealthTextBlue);
            _uIManager.SetHealth(_uIManager._redHealthSliderBlue, health, _uIManager._redHealthTextRed);
        }
    }

    void PlaceIdol() 
    {
        _idolPosInt = _hCTManager.IdolPosInt;
        if (_idolPosInt == 0)
        {
            float randomNum = UnityEngine.Random.Range(1, 6);
            switch (randomNum)
            {
                case 1:
                    _idol.transform.position = GameObject.Find("Green Counter 13").transform.position;
                    _idolPosInt = 13;
                    break;
                case 2:
                    _idol.transform.position = GameObject.Find("Green Counter 23").transform.position;
                    _idolPosInt = 23;
                    break;
                case 3:
                    _idol.transform.position = GameObject.Find("Green Counter 33").transform.position;
                    _idolPosInt = 33;
                    break;
                case 4:
                    _idol.transform.position = GameObject.Find("Green Counter 43").transform.position;
                    _idolPosInt = 43;
                    break;
                default:
                    _idol.transform.position = GameObject.Find("Green Counter 53").transform.position;
                    _idolPosInt = 53;
                    break;
            }
        }
        PlaceIdolClientRpc(_idolPosInt);
    }

    [ClientRpc]
    void PlaceIdolClientRpc(int idolPosInt)
    {
        _idol.transform.position = GameObject.Find("Green Counter " + idolPosInt.ToString()).transform.position;
    }

    public void PickupIdol(Counter counter) 
    {
        _idolPosInt = 0;
        _audioManager.PlaySound(_audioManager.CollectIdol);
        if (counter == _blueCounter) 
        {
            _idol.transform.position = _blueHasIdolPos.position;
            _hasIdol = _blueHero;
        }
        else 
        {
            _idol.transform.position = _redHasIdolPos.position;
            _hasIdol = _redHero;
        }
    }

    public void DetermineFirstToGo(Hero blueHero, Hero redHero) 
    {
        if (blueHero.speed > redHero.speed) 
            SetPlayerOrder(_blueCounter, _redCounter, true, false);
        else if (blueHero.speed < redHero.speed)
            SetPlayerOrder(_redCounter, _blueCounter, false, true);
        else 
        {
            bool isBlueFirst = UnityEngine.Random.value < 0.5f;
            if (isBlueFirst) 
                SetPlayerOrder(_blueCounter, _redCounter, true, false);
            else 
                SetPlayerOrder(_redCounter, _blueCounter, true, false);
        }

        if (_currentRound == 2 && _firstToGo == _blueCounter) 
        {
            Debug.Log("Swapping first to go");
            SetPlayerOrder(_redCounter, _blueCounter, false, true);
        }
    }

    public void SetPlayerOrder(Counter firstPlayer, Counter secondPlayer, bool isFirstCupActive, bool isSecondCupActive) 
    {
        _firstToGo = firstPlayer;
        _secondToGo = secondPlayer;
        _blueCup.SetActive(isFirstCupActive);
        _redCup.SetActive(isSecondCupActive);
    }

    public void CalcGhostPositions(Hero hero, Counter counter) 
    {
        foreach (Card cardInHand in hero.hand) 
            cardInHand.CalcOffsetForActions(counter, cardInHand);
    }

    void AddToLog(string textToAdd) 
    {
    //    Debug.Log(textToAdd);
    }
}
