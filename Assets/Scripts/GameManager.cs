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
    [SerializeField] GameObject _blueCounterObj, _redCounterObj;
    [SerializeField] GameObject _hero1Obj, _hero2Obj;
    [SerializeField] Counter _blueCounter, _redCounter, _firstToGo, _secondToGo;
    [SerializeField] GameObject _blueCamera, _redCamera, _blueCup, _redCup;
    [SerializeField] int _idolPosInt;
    [SerializeField] Hero _blueHero, _redHero;
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
    public bool TurnRunning { get => _turnRunning; set => _turnRunning = value; }

    void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else
            Destroy(gameObject);

        AssignCamera();
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
        _uIManager.HideStartTurnButton(_uIManager.StartTurnButtonBlue);
        _uIManager.HideStartTurnButton(_uIManager.StartTurnButtonRed);

        if (NetworkManager.Singleton.IsServer)
            _redUIObjects.SetActive(false);
        else
            _blueUIObjects.SetActive(false);

        //AssignHCTValues();
        _blueHero.PutAllCardsInDeck();
        _redHero.PutAllCardsInDeck();
        AssignActionIdsAndCardIds();
        StartRound();
    }

    void AssignHCTValues()
    {
        _blueHero.Speed = _hCTManager.BlueSpeed;
        _redHero.Speed = _hCTManager.RedSpeed;

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
        AssignHCTValuesClientRpc(_blueHero.Speed, _redHero.Speed);
    }

    [ClientRpc]
    void AssignHCTValuesClientRpc(int blueSpeed, int redSpeed)
    {
        //print(blueHero);
        _blueHero.Speed = blueSpeed;
        _redHero.Speed = redSpeed;
        print("d");
    }

    void FlipArrowsForRed()
    {
        Dictionary<string, string> arrowMappings = new Dictionary<string, string>
        {
            { "N", "S" },
            { "E", "W" },
            { "S", "N" },
            { "W", "E" },
            { "NE", "SW" },
            { "SE", "NW" },
            { "SW", "NE" },
            { "NW", "SE" }
        };

        foreach (Transform arrow in transform)
        {
            string arrowName = arrow.gameObject.name;

            if (arrowMappings.ContainsKey(arrowName))
            {
                string flippedName = arrowMappings[arrowName];
                arrow.gameObject.name = arrowName.Replace(arrowName, flippedName);
            }
        }
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
            card.CardId = uniqueCardId; // Assign unique card ID
            foreach (Action action in card.BaseActions) // Iterate over the baseActions list
            {
                action.ActionId = uniqueActionId; // Assign unique action ID
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
        GameObject bc = Instantiate(_blueCounterObj, Vector3.zero, Quaternion.identity);
        GameObject rc = Instantiate(_redCounterObj, Vector3.zero, Quaternion.identity);
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
        _blueHero.Health = 100;
        _redHero.Health = 100;
        _hasIdol = null;
        _bluePickingHand = true;
        _redPickingHand = true; //// send these
        _blueCounter.PlaceToStart();
        _redCounter.PlaceToStart();

        DetermineFirstToGo(_blueHero, _redHero);

        if (NetworkManager.Singleton.IsServer)
        {
            PlaceIdol();
            _uIManager.HideReadyText(_uIManager.BlueReadyTextBlue); 
            _uIManager.HideReadyText(_uIManager.RedReadyTextBlue); 
            _uIManager.SetHealth(_uIManager.BlueHealthSliderBlue, _blueHero.Health, _uIManager.BlueHealthTextBlue); 
            _uIManager.SetHealth(_uIManager.RedHealthSliderBlue, _redHero.Health, _uIManager.RedHealthTextBlue); 
            _uIManager.SetText(_uIManager.RoundTextBlue, "Round " + _currentRound); 
        }
        else 
        {
            _uIManager.HideReadyText(_uIManager.BlueReadyTextRed); 
            _uIManager.HideReadyText(_uIManager.RedReadyTextRed); 
            _uIManager.SetHealth(_uIManager.BlueHealthSliderRed, _blueHero.Health, _uIManager.BlueHealthTextRed); 
            _uIManager.SetHealth(_uIManager.RedHealthSliderRed, _redHero.Health, _uIManager.RedHealthTextRed); 
            _uIManager.SetText(_uIManager.RoundTextRed, "Round " + _currentRound); 
        }

        _uIManager.ShowMyDeckScreen();
    }

    public void EndRound() 
    {
        Debug.Log("End of round " + _currentRound);
        _uIManager.HideCloseMyDeckButton(_uIManager.CloseMyDeckButtonBlue);
        _uIManager.HideCloseMyDeckButton(_uIManager.CloseMyDeckButtonRed);
        _uIManager.HideStartTurnButton(_uIManager.StartTurnButtonBlue);
        _uIManager.HideStartTurnButton(_uIManager.StartTurnButtonRed);
        //_audioManager.PlaySound(_audioManager.RoundWin);
        ResetAllCards();
        _currentRound++;
        _currentTurn = 1;
        _uIManager.SetText(_uIManager.TurnTextBlue, "Turn " + _currentTurn); ////
        _blueHero.PlayerChoosingInitialCards = true;
        StartRound();
    }

    public void StartTurn() // only call this when both players have picked 3 cards
    {
        _turnRunning = true;
        _blueTurnDone = _redTurnDone = false;
        _bluePickingHand = _redPickingHand = false;
        _uIManager.SetText(_uIManager.TurnTextBlue, "Turn " + _currentTurn); ////
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
        }
        else
        {
            _redReadyToStart = true;
            UpdateReadyToStartBoolServerRpc(true);
            _redDeckCardsSelected = 0;
        }
    }

    [ClientRpc]
    void UpdateReadyToStartBoolClientRpc(bool trueOrFalse) => _blueReadyToStart = trueOrFalse;

    [ServerRpc(RequireOwnership = false)]
    void UpdateReadyToStartBoolServerRpc(bool trueOrFalse) => _redReadyToStart = trueOrFalse;

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
            if (card._availableActions.Count > 0) 
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
        //Debug.Log(card._availableActions.Count);
        int actionIndex = UnityEngine.Random.Range (0, card._availableActions.Count);
        Action action = card._availableActions[actionIndex];
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
                _uIManager.ShowStartTurnButton(_uIManager.StartTurnButtonBlue);
            else
                _uIManager.HideStartTurnButton(_uIManager.StartTurnButtonBlue);
        }
        else
        {
            _redDeckCardsSelected = valueToSet;
            if (_redDeckCardsSelected == 3)
                _uIManager.ShowStartTurnButton(_uIManager.StartTurnButtonRed);
            else
                _uIManager.HideStartTurnButton(_uIManager.StartTurnButtonRed);
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
            ProcessCardInHand(hero.Hand[0], _blueCardInHandPos1, hero.Hand[1], hero.Hand[2]);
            ProcessCardInHand(hero.Hand[1], _blueCardInHandPos2, hero.Hand[0], hero.Hand[2]);
            ProcessCardInHand(hero.Hand[2], _blueCardInHandPos3, hero.Hand[0], hero.Hand[1]);
        }
        else
        {
            //Debug.Log(hero.hand.Count);
            ProcessCardInHand(hero.Hand[0], _redCardInHandPos1, hero.Hand[1], hero.Hand[2]);
            ProcessCardInHand(hero.Hand[1], _redCardInHandPos2, hero.Hand[0], hero.Hand[2]);
            ProcessCardInHand(hero.Hand[2], _redCardInHandPos3, hero.Hand[0], hero.Hand[1]);
        }
        
        CalcGhostPositions(hero, counter);
        LockCards(hero.Deck);
        _uIManager.HideMyDeckScreen();
    }

    void ProcessCardInHand(Card card, Transform cardPos, Card otherCard1, Card otherCard2) 
    {
         card.transform.position = cardPos.position;
         card.OtherCard1 = otherCard1;
         card.OtherCard2 = otherCard2;
    }

    void LockCards(List<Card> cards) 
    {
        foreach (Card card in cards) 
            card.Locked = true;
    }

    public void UnLockCards(List<Card> cards) 
    {
        foreach (Card card in cards) 
            card.Locked = false;
    }
        
    public void RegisterAction(Counter counter, Action.ActionType actionType, string[] _ghostRefs) 
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

        _actionsToExecute[flowPos] = _ghostRefs;

        if (counter == _blueCounter) 
        {
            _blueSelectedCard?.HideDestinations(_blueSelectedCard.AvailableActions);
            RegisterActionClientRpc(flowPos, ConvertToIntArray(_ghostRefs));
            //_uIManager.ShowReadyText(_uIManager.blueReadyTextBlue); ////
        }
        if (counter == _redCounter) 
        {
            redSelectedCard?.HideDestinations(redSelectedCard.AvailableActions);
            RegisterActionServerRpc(flowPos, ConvertToIntArray(_ghostRefs));
            //_uIManager.ShowReadyText(_uIManager.redReadyTextBlue); ////
        }
    }

    int[] ConvertToIntArray(string[] _ghostRefs) 
    {
        //Debug.Log(_ghostRefs.Length);
        int[] lastTwoDigitsArray = new int[_ghostRefs.Length];

        for (int i = 0; i < _ghostRefs.Length; i++)
        {
            string ghostRef = _ghostRefs[i];

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
    public void RegisterActionClientRpc(int flowPos, int[] int_ghostRefs)//convert back to string array
    {
        string[] _ghostRefsConverted = new string[int_ghostRefs.Length];
        for (int i = 0; i < int_ghostRefs.Length; i++)
            _ghostRefsConverted[i] = int_ghostRefs[i].ToString();
        _actionsToExecute[flowPos] = _ghostRefsConverted;
        //Debug.Log(_ghostRefsConverted[0]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterActionServerRpc(int flowPos, int[] int_ghostRefs)
    {
        string[] _ghostRefsConverted = new string[int_ghostRefs.Length];
        for (int i = 0; i < int_ghostRefs.Length; i++)
            _ghostRefsConverted[i] = int_ghostRefs[i].ToString();
        _actionsToExecute[flowPos] = _ghostRefsConverted;
        //Debug.Log(_ghostRefsConverted[0]);
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
                RoundWon();
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
                RoundWon();
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
                RoundWon();
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
                RoundWon();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break; 
            }
            if (_blueSelectedCard != null) 
                _blueSelectedCard.Selected = false;

            if (CheckForIdolWinner())
            {
                RoundWon();
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
                _blueHero.DiscardCardFromHand(_blueHero.CardInPlay);////
                _blueHero.DiscardRandomCardInHand();////
                UnLockCards(_blueHero.Deck);////
                _currentRound++;
                StartRound();
            }
            else
            {
                ShowPlayedCardTicksClientRpc();
                //_blueSelectedCard.PutDownCard(_blueSelectedCard.transform);// same for red
                UnLockCards(_blueHero.Hand);
                _blueSelectedCard.Locked = true;
                _blueDiscarding = true;
                CleanUpTurnStuffClientRpc();
            }
        }
    }

    private void RoundWon() // Maybe add the type of win?
    {
        _uIManager.SetText(_uIManager.RoundsTextBlue, "B : R " + _blueRoundsWon + " : " + _redRoundsWon);
        _uIManager.SetText(_uIManager.MessageTextBlue, _roundWinner + " wins round " + _currentRound);
        _uIManager.CallShowMessageOverlay();
    }

    [ClientRpc]
    void ShowPlayedCardTicksClientRpc()
    {
        foreach (Card card in _allCards)
        {
            //Debug.Log(card.cardId);
            if (card != null && card.CardId == _bluePlayedCardId || card.CardId == _redPlayedCardId)//////need to get dUMMY cards
            {
                Debug.Log(card.CardId + " should tick");
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
                redSelectedCard.Selected = false;

            UnLockCards(_redHero.Hand);
            //redSelectedCard.PutDownCard(redSelectedCard.transform);
            redSelectedCard.Locked = true;
            RedDiscarding = true;
        }
        _uIManager.ShowDiscardButton();
    }

    public void ActionSelected(Hero hero, Action.ActionType actionType, string[] _ghostRefs)
    {
        Debug.Log(hero + " selected " + _ghostRefs[0]);
        _audioManager.PlaySound(_audioManager.SelectSquare);
        _aIPlaying = false;
        LockCards(hero.Hand);
        if (hero == _blueHero)
        {
            _blueHero.CardInPlay = _blueSelectedCard;
            RegisterAction(_blueCounter, actionType, _ghostRefs);
            _bluePlayedCardId = _blueSelectedCard.CardId;
            SetBluePlayedCardClientRpc(_bluePlayedCardId);
            _blueTurnDone = true;
            UpdateTurnDoneBoolClientRpc(true);
        }
        else if (hero == _redHero)
        {
            _redHero.CardInPlay = redSelectedCard;
            RegisterAction(_redCounter, actionType, _ghostRefs);
            _redPlayedCardId = redSelectedCard.CardId;
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
        _uIManager.SetText(_uIManager.MessageTextBlue, _roundWinner + " player wins Match! ");/////////
        _uIManager.CallShowMessageOverlay();
        _uIManager.ShowButton(_uIManager.ResetButtonBlue);///////
        Debug.Log("Match Over!");
    }

    bool CheckForIdolWinner()
    {
        if (_currentTurn == 8)
        {
            string winner = GetIdolWinner();
            if (!string.IsNullOrEmpty(winner))
            {
                HandleIdolWin(winner);
                return true;
            }
        }
        return false;
    }

    string GetIdolWinner()
    {
        if (_hasIdol == _redHero)
            return "Red";
        if (_hasIdol == _blueHero)
            return "Blue";
        return null;
    }

    void HandleIdolWin(string winner)
    {
        _audioManager.PlaySound(_audioManager.IdolWin);

        if (winner == "Red")
            _redRoundsWon++;
        else if (winner == "Blue")
            _blueRoundsWon++;

        _roundWinner = winner;
    }

    bool CheckForDefeatWinner()
    {
        if (_blueHero.Health == 0)
        {
            HandleDefeat("Red");
            return true;
        }
        if (_redHero.Health == 0)
        {
            HandleDefeat("Blue");
            return true;
        }
        return false;
    }

    void HandleDefeat(string winner)
    {
        _audioManager.PlaySound(_audioManager.PlayerDead);

        if (winner == "Red")
            _redRoundsWon++;
        else if (winner == "Blue")
            _blueRoundsWon++;

        _roundWinner = winner;
    }

    bool CheckForMatchWinner() 
    {
        if (_blueRoundsWon == 2 || _redRoundsWon == 2) 
            return true;
        return false;
    }

    public void SetHealth(Hero hero, int health) 
    {
        hero.Health = health;
        if (hero == _blueHero) 
        {
            _uIManager.SetHealth(_uIManager.BlueHealthSliderBlue, health, _uIManager.BlueHealthTextBlue);
            _uIManager.SetHealth(_uIManager.BlueHealthSliderRed, health, _uIManager.BlueHealthTextRed);
        }
        else 
        {
            _uIManager.SetHealth(_uIManager.RedHealthSliderRed, health, _uIManager.RedHealthTextRed);
            _uIManager.SetHealth(_uIManager.RedHealthSliderBlue, health, _uIManager.RedHealthTextBlue);
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
        }print(_idolPosInt);
        PlaceIdolClientRpc(_idolPosInt);
    }

    [ClientRpc]
    void PlaceIdolClientRpc(int idolPosInt)
    {
        //print(_idolPosInt);
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
        if (blueHero.Speed > redHero.Speed) 
            SetPlayerOrder(_blueCounter, _redCounter, true, false);
        else if (blueHero.Speed < redHero.Speed)
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
        foreach (Card cardInHand in hero.Hand) 
            cardInHand.CalcOffsetForActions(counter, cardInHand);
    }

    void AddToLog(string textToAdd) 
    {
    //    Debug.Log(textToAdd);
    }
}
