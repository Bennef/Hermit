using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [Header("Game Stats")]
    [SerializeField] float turnTime = 60f;
    [SerializeField] float countDownTimer;
    [SerializeField] int currentTurn;
    [SerializeField] int currentRound = 1;
    [SerializeField] int blueRoundsWon;
    [SerializeField] int redRoundsWon;
    [SerializeField] Hero hasIdol;
    [SerializeField] string roundWinner = "";
    [SerializeField] public int blueDeckCardsSelected = 0;
    [SerializeField] public int redDeckCardsSelected = 0;

    [Header("Game Flow")]
    [SerializeField] public bool aIPlaying = true;
    [SerializeField] public bool blueDiscarding;
    [SerializeField] public bool redDiscarding;
    [SerializeField] public bool bluePickingHand = true;
    [SerializeField] public bool redPickingHand = true;
    [SerializeField] public bool blueReadyToStart = true;
    [SerializeField] public bool redReadyToStart = true;
    [SerializeField] public bool turnRunning = true;
    public Card blueSelectedCard, redSelectedCard;
    public int bluePlayedCardId, redPlayedCardId;
    [SerializeField] public bool blueTurnDone = true;
    [SerializeField] public bool redTurnDone = true;
    [SerializeField] string[][] actionsToExecute;

    [Header("Objects")]
    [SerializeField] GameObject blueUIObjects;
    [SerializeField] GameObject redUIObjects;
    [SerializeField] GameObject idol;
    [SerializeField] GameObject blueCounterObj;
    [SerializeField] GameObject redCounterObj;
    [SerializeField] GameObject hero1Obj;
    [SerializeField] GameObject hero2Obj;
    [SerializeField] Counter blueCounter, redCounter, firstToGo, secondToGo;
    [SerializeField] int _idolPosInt;
    public Hero blueHero, redHero;
    public GameObject blueCamera, redCamera, blueCup, redCup;
    public Transform blueHasIdolPos, redHasIdolPos, 
        blueCardInHandPos1, blueCardInHandPos2, blueCardInHandPos3, 
        redCardInHandPos1, redCardInHandPos2, redCardInHandPos3;

    HCTManager _hCTManager;
    UIManager uIManager;
    AudioManager audioManager;
    Coroutine turnTimerCoroutine;
    List<Card> allCards = new List<Card>();

    public static GameManager Singleton { get; private set; }

    public Counter BlueCounter { get { return blueCounter; }}
    public Counter RedCounter { get { return redCounter; }}
    public int IdolPosInt { get { return _idolPosInt; }}

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
        uIManager = FindObjectOfType<UIManager>();
        audioManager = FindObjectOfType<AudioManager>();
        blueHero = GameObject.Find("Blue Hero").GetComponent<Hero>();
        redHero = GameObject.Find("Red Hero").GetComponent<Hero>();
        blueCounter = GameObject.Find("Blue Counter(Clone)").GetComponent<Counter>();
        redCounter = GameObject.Find("Red Counter(Clone)").GetComponent<Counter>();
        uIManager.HideStartTurnButton(uIManager._startTurnButtonBlue);
        uIManager.HideStartTurnButton(uIManager._startTurnButtonRed);

        if (NetworkManager.Singleton.IsServer)
            redUIObjects.SetActive(false);
        else
            blueUIObjects.SetActive(false);

        AssignHCTValues();
        blueHero.PutAllCardsInDeck();
        AssignActionIdsAndCardIds();
        StartRound();
    }

    void AssignHCTValues()
    {
        // Speed
        blueHero.speed = _hCTManager.BlueSpeed;
        redHero.speed = _hCTManager.RedSpeed;

        Transform blueCards = blueHero.transform.Find("Cards");
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
        Card[] blueCards = blueHero.GetComponentsInChildren<Card>();
        Card[] redCards = redHero.GetComponentsInChildren<Card>();
        allCards.AddRange(blueCards);
        allCards.AddRange(redCards);

        int uniqueCardId = 1;
        int uniqueActionId = 1;

        foreach (Card card in allCards)
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
            blueCamera.SetActive(true);
            redCamera.SetActive(false);
        }
        else
        {
            redCamera.SetActive(true);
            blueCamera.SetActive(false);
        }
    }

    void SpawnObjects()
    {
        // Heroes
        GameObject blueHero = Instantiate(hero1Obj, Vector3.zero, Quaternion.identity);
        blueHero.name = "Blue Hero";
        GameObject redHero = Instantiate(hero2Obj, Vector3.zero, Quaternion.identity);
        redHero.name = "Red Hero";
        // Counters
        GameObject bc = Instantiate(blueCounterObj, Vector3.zero, Quaternion.identity);
        GameObject rc = Instantiate(redCounterObj, Vector3.zero, Quaternion.identity);
        // Camera layers
        int blueHeroLayer = LayerMask.NameToLayer("Blue Player");
        int redHeroLayer = LayerMask.NameToLayer("Red Player");
        SetLayer(blueHero.transform.GetChild(0), blueHeroLayer);
        SetLayer(blueHero.transform.GetChild(1), redHeroLayer);
        SetLayer(redHero.transform.GetChild(0), redHeroLayer);
        SetLayer(redHero.transform.GetChild(1), blueHeroLayer);
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
        if (!turnRunning && blueReadyToStart && redReadyToStart)
        {
            Debug.Log("Both ready to start turn");
            blueReadyToStart = false;
            redReadyToStart = false;
            OrganiseCardsForTurn();
            uIManager.GameStarting();
            currentTurn++;
            StartTurn();
        }

        if (!turnRunning) return;
        
        if (blueTurnDone && redTurnDone) 
        {
            turnRunning = blueTurnDone = redTurnDone = false;//////
            StartCoroutine(ExecuteFlowOfEvents());
        }
        /*
        countDownTimer -= Time.deltaTime;
        if (countDownTimer > 0) 
        {
            uIManager.SetText(uIManager.roundTimeLeftTextBlue, "Time\n" + countDownTimer.ToString().Substring(0, 3));////
        }
        else 
        {
            uIManager.SetText(uIManager.roundTimeLeftTextBlue, "Time\n" + "0"); ////
        }*/
    }

    public void StartRound() 
    {
        Debug.Log("Start of round " + currentRound);
        blueHero.health = 100;
        redHero.health = 100;
        hasIdol = null;
        bluePickingHand = true;
        redPickingHand = true; //// send these
        blueCounter.PlaceToStart();
        redCounter.PlaceToStart();

        DetermineFirstToGo(blueHero, redHero);

        if (NetworkManager.Singleton.IsServer)
        {
            PlaceIdol();
            uIManager.HideReadyText(uIManager._blueReadyTextBlue); 
            uIManager.HideReadyText(uIManager._redReadyTextBlue); 
            uIManager.SetHealth(uIManager._blueHealthSliderBlue, blueHero.health, uIManager._blueHealthTextBlue); 
            uIManager.SetHealth(uIManager._redHealthSliderBlue, redHero.health, uIManager._redHealthTextBlue); 
            uIManager.SetText(uIManager._roundTextBlue, "Round " + currentRound); 
        }
        else 
        {
            uIManager.HideReadyText(uIManager._blueReadyTextRed); 
            uIManager.HideReadyText(uIManager._redReadyTextRed); 
            uIManager.SetHealth(uIManager._blueHealthSliderRed, blueHero.health, uIManager._blueHealthTextRed); 
            uIManager.SetHealth(uIManager._redHealthSliderRed, redHero.health, uIManager._redHealthTextRed); 
            uIManager.SetText(uIManager._roundTextRed, "Round " + currentRound); 
        }

        uIManager.ShowMyDeckScreen();
    }

    public void EndRound() 
    {
        Debug.Log("End of round " + currentRound);
        uIManager.HideCloseMyDeckButton();
        uIManager.HideStartTurnButton(uIManager._startTurnButtonBlue);
        //audioManager.PlaySound(audioManager.RoundWin);
        ResetAllCards();
        currentRound++;
        currentTurn = 1;
        uIManager.SetText(uIManager._turnTextBlue, "Turn " + currentTurn); ////
        blueHero.playerChoosingInitialCards = true;
        StartRound();
    }

    public void StartTurn() // only call this when both players have picked 3 cards
    {
        turnRunning = true;
        blueTurnDone = redTurnDone = false;
        bluePickingHand = redPickingHand = false;
        uIManager.SetText(uIManager._turnTextBlue, "Turn " + currentTurn); ////
        Debug.Log("Turn " + currentTurn + " started");
        //PickCRandomCardsForHand(redHero);
        actionsToExecute = new string[6][]; // Clear the array
        //countDownTimer = turnTime;
        //turnTimerCoroutine = StartCoroutine(TurnTimer());
    }

    IEnumerator TurnTimer() 
    {
        yield return new WaitForSeconds(turnTime);
        if (turnRunning) EndTurn();
    }

    public void EndTurn() 
    {
        turnRunning = false;
        /*if (blueHero.hand.Count == 0) 
        {
            PickRandomCardsForHand(blueHero);
        }
        SelectRandomCardToPlay(redHero);*/
        Debug.Log("Turn over");
        if (turnTimerCoroutine != null) 
        {
            StopCoroutine(turnTimerCoroutine);
        }
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(ExecuteFlowOfEvents());//here to test for now
        }
    }

    public void StartTurnButtonPressed(Hero hero) /// Show ready text for player!!!
    {
        if (hero == blueHero)
        {
            blueReadyToStart = true;
            UpdateReadyToStartBoolClientRpc(true);
            blueDeckCardsSelected = 0;
            uIManager.HideStartTurnButton(uIManager._startTurnButtonBlue);
        }
        else
        {
            redReadyToStart = true;
            UpdateReadyToStartBoolServerRpc(true);
            redDeckCardsSelected = 0;
            uIManager.HideStartTurnButton(uIManager._startTurnButtonRed);
        }
    }

    [ClientRpc]
    void UpdateReadyToStartBoolClientRpc(bool trueOrFalse)
    {
        //Debug.Log("Setting blueIsReady to " + trueOrFalse + " on client");
        blueReadyToStart = trueOrFalse;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateReadyToStartBoolServerRpc(bool trueOrFalse)
    {
        //Debug.Log("Setting redIsReady to " + trueOrFalse + " on server");
        redReadyToStart = trueOrFalse;
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
            blueSelectedCard = hero.hand[index];
            hero.cardInPlay = blueSelectedCard;
            SelectRandomActionToPlay(blueSelectedCard, blueCounter, redCounter);
        }
        if (hero == redHero) 
        {
            Debug.Log(index);
            redSelectedCard = hero.hand[index];
            hero.cardInPlay = redSelectedCard;
            SelectRandomActionToPlay(redSelectedCard, redCounter, blueCounter);
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
        if (hero == blueHero)
        {
            blueDeckCardsSelected = valueToSet;
            if (blueDeckCardsSelected == 3)
            {
                uIManager.ShowStartTurnButton(uIManager._startTurnButtonBlue);
            }
            else
            {
                uIManager.HideStartTurnButton(uIManager._startTurnButtonBlue);
            }
        }
        else
        {
            redDeckCardsSelected = valueToSet;
            if (redDeckCardsSelected == 3)
            {
                uIManager.ShowStartTurnButton(uIManager._startTurnButtonRed);
            }
            else
            {
                uIManager.HideStartTurnButton(uIManager._startTurnButtonRed);
            }
        }
    }

    public void OrganiseCardsForTurn() 
    {
        Debug.Log("Organising cards");
        if (NetworkManager.Singleton.IsServer) 
        {
            blueHero.MoveCardsToHand();
            PositionHandCards(blueHero, blueCounter);
        }
        else 
        {
            redHero.MoveCardsToHand();
            PositionHandCards(redHero, redCounter);
        }
    }

    public void PositionHandCards(Hero hero, Counter counter) 
    {
        //Debug.Log(hero.hand.Count + " in hand");
        if (hero == blueHero)
        {
            ProcessCardInHand(hero.hand[0], blueCardInHandPos1, hero.hand[1], hero.hand[2]);
            ProcessCardInHand(hero.hand[1], blueCardInHandPos2, hero.hand[0], hero.hand[2]);
            ProcessCardInHand(hero.hand[2], blueCardInHandPos3, hero.hand[0], hero.hand[1]);
        }
        else
        {
            //Debug.Log(hero.hand.Count);
            ProcessCardInHand(hero.hand[0], redCardInHandPos1, hero.hand[1], hero.hand[2]);
            ProcessCardInHand(hero.hand[1], redCardInHandPos2, hero.hand[0], hero.hand[2]);
            ProcessCardInHand(hero.hand[2], redCardInHandPos3, hero.hand[0], hero.hand[1]);
        }
        
        CalcGhostPositions(hero, counter);
        LockCards(hero.deck);
        uIManager.HideMyDeckScreen();
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
        {
            card.locked = true;
        }
    }

    public void UnLockCards(List<Card> cards) 
    {
        foreach (Card card in cards) 
        {
            card.locked = false;
        }
    }
        
    public void RegisterAction(Counter counter, Action.ActionType actionType, string[] ghostRefs) 
    {
        int flowPos = 0;
        if (actionType == Action.ActionType.Move) 
        {
            if (counter == firstToGo) 
            {
                flowPos = 0;
            }
            else 
            {
                flowPos = 1;
            }
        }
        else if (actionType == Action.ActionType.WeakAttack)
        {
            if (counter == firstToGo) 
            {
                flowPos = 2;
            }
            else 
            {
                flowPos = 4;
            }
        }
        else if (actionType == Action.ActionType.StrongAttack)
        {
            if (counter == firstToGo)
            {
                flowPos = 3;
            }
            else
            {
                flowPos = 5;
            }
        }

        actionsToExecute[flowPos] = ghostRefs;

        if (counter == blueCounter) 
        {
            blueSelectedCard?.HideDestinations(blueSelectedCard.availableActions);
            RegisterActionClientRpc(flowPos, ConvertToIntArray(ghostRefs));
            //uIManager.ShowReadyText(uIManager.blueReadyTextBlue); ////
        }
        if (counter == redCounter) 
        {
            redSelectedCard?.HideDestinations(redSelectedCard.availableActions);
            RegisterActionServerRpc(flowPos, ConvertToIntArray(ghostRefs));
            //uIManager.ShowReadyText(uIManager.redReadyTextBlue); ////
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
                {
                    lastTwoDigitsArray[i] = lastTwoDigitsInt;
                }
                else
                {
                    // Handle parsing failure
                    lastTwoDigitsArray[i] = 0; // Or another appropriate default value
                }
            }
            else
            {
                // Handle strings with fewer than 2 characters
                lastTwoDigitsArray[i] = 0; // Or another appropriate default value
            }
        }
        return lastTwoDigitsArray;
    }
    
    [ClientRpc]
    public void RegisterActionClientRpc(int flowPos, int[] intGhostRefs)//convert back to string array
    {
        string[] ghostRefsConverted = new string[intGhostRefs.Length];
        for (int i = 0; i < intGhostRefs.Length; i++)
        {
            ghostRefsConverted[i] = intGhostRefs[i].ToString();
        }
        actionsToExecute[flowPos] = ghostRefsConverted;
        //Debug.Log(ghostRefsConverted[0]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterActionServerRpc(int flowPos, int[] intGhostRefs)
    {
        string[] ghostRefsConverted = new string[intGhostRefs.Length];
        for (int i = 0; i < intGhostRefs.Length; i++)
        {
            ghostRefsConverted[i] = intGhostRefs[i].ToString();
        }
        actionsToExecute[flowPos] = ghostRefsConverted;
        //Debug.Log(ghostRefsConverted[0]);
    }

    IEnumerator ExecuteFlowOfEvents() 
    {
        if (NetworkManager.Singleton.IsServer) // put outside of function
        {
            Debug.Log("Flow");
            // First to go Move
            if (actionsToExecute[0] != null)
            {
                Debug.Log("Flow 1: " + actionsToExecute[0][0]);
                firstToGo.ExecuteMove(actionsToExecute[0]);
                //AddToLog(firstToGo + " : " + actionsToExecute2[0].actionType + " to " + actionsToExecute[0].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }

            // Second to go Move
            if (actionsToExecute[1] != null)
            {
                Debug.Log("Flow 2: " + actionsToExecute[1][0]);
                secondToGo.ExecuteMove(actionsToExecute[1]);
                //AddToLog(secondToGo + " : " + actionsToExecute[1].actionType + " to " + actionsToExecute[1].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }

            // First to go Weak Attack
            if (actionsToExecute[2] != null)
            {
                firstToGo.ExecuteWeakAttack(actionsToExecute[2]);
                //AddToLog(firstToGo + " : " + actionsToExecute[2].actionType + " on " + actionsToExecute[2].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                uIManager.SetText(uIManager._roundsTextBlue, "B : R " + blueRoundsWon + " : " + redRoundsWon);////
                uIManager.SetText(uIManager._messageTextBlue, roundWinner + " player wins round " + currentRound);////
                uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break;
            }

            // First to go Strong Attack
            if (actionsToExecute[3] != null)
            {
                firstToGo.ExecuteStrongAttack(actionsToExecute[3]);
                //AddToLog(secondToGo + " : " + actionsToExecute[3].actionType + " on " + actionsToExecute[3].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                uIManager.SetText(uIManager._roundsTextBlue, "B : R " + blueRoundsWon + " : " + redRoundsWon);////
                uIManager.SetText(uIManager._messageTextBlue, roundWinner + " player wins round " + currentRound);////
                uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break; 
            }

            // Second to go Weak Attack
            if (actionsToExecute[4] != null)
            {
                secondToGo.ExecuteWeakAttack(actionsToExecute[4]);
                //AddToLog(firstToGo + " : " + actionsToExecute[4].actionType + " on " + actionsToExecute[4].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                uIManager.SetText(uIManager._roundsTextBlue, "B : R " + blueRoundsWon + " : " + redRoundsWon);////
                uIManager.SetText(uIManager._messageTextBlue, roundWinner + " player wins round " + currentRound);////
                uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break;
            }
            // Second to go Strong Attack
            if (actionsToExecute[5] != null)
            {
                secondToGo.ExecuteStrongAttack(actionsToExecute[5]);
                //AddToLog(secondToGo + " : " + actionsToExecute[5].actionType + " on " + actionsToExecute[5].selectedGridSquare);
                yield return new WaitForSeconds(1.5f);
            }
            if (CheckForDefeatWinner())
            {
                uIManager.SetText(uIManager._roundsTextBlue, "B : R " + blueRoundsWon + " : " + redRoundsWon);////
                uIManager.SetText(uIManager._messageTextBlue, roundWinner + " player wins round " + currentRound);////
                uIManager.CallShowMessageOverlay();
                if (CheckForMatchWinner())
                {
                    MatchOver();
                    yield break;
                }
                EndRound();
                yield break; 
            }
            if (blueSelectedCard != null)
            {
                blueSelectedCard.selected = false;
            }

            if (CheckForIdolWinner())
            {
                uIManager.SetText(uIManager._roundsTextBlue, "B : R " + blueRoundsWon + " : " + redRoundsWon);///
                uIManager.SetText(uIManager._messageTextBlue, roundWinner + " player wins round " + currentRound);///
                uIManager.CallShowMessageOverlay();
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
            if (aIPlaying)
            {
                blueHero.DiscardCardFromHand(blueHero.cardInPlay);////
                blueHero.DiscardRandomCardInHand();////
                UnLockCards(blueHero.deck);////
                currentRound++;
                StartRound();
            }
            else
            {
                ShowPlayedCardTicksClientRpc();
                //blueSelectedCard.PutDownCard(blueSelectedCard.transform);// same for red
                UnLockCards(blueHero.hand);
                blueSelectedCard.locked = true;
                blueDiscarding = true;
                CleanUpTurnStuffClientRpc();
            }
        }
    }

    [ClientRpc]
    void ShowPlayedCardTicksClientRpc()
    {
        foreach (Card card in allCards)
        {
            //Debug.Log(card.cardId);
            if (card != null && card.cardId == bluePlayedCardId || card.cardId == redPlayedCardId)//////need to get dUMMY cards
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
            UnLockCards(redHero.hand);
            //redSelectedCard.PutDownCard(redSelectedCard.transform);
            redSelectedCard.locked = true;
            redDiscarding = true;
        }
        uIManager.ShowDiscardButton();
    }

    public void ActionSelected(Hero hero, Action.ActionType actionType, string[] ghostRefs)
    {
        Debug.Log(ghostRefs[0]);
        audioManager.PlaySound(audioManager.SelectSquare);
        aIPlaying = false;
        LockCards(hero.hand);
        if (hero == blueHero)
        {
            blueHero.cardInPlay = blueSelectedCard;
            RegisterAction(blueCounter, actionType, ghostRefs);
            bluePlayedCardId = blueSelectedCard.cardId;
            SetBluePlayedCardClientRpc(bluePlayedCardId);
            blueTurnDone = true;
            UpdateTurnDoneBoolClientRpc(true);
        }
        else if (hero == redHero)
        {
            redHero.cardInPlay = redSelectedCard;
            RegisterAction(redCounter, actionType, ghostRefs);
            redPlayedCardId = redSelectedCard.cardId;
            SetRedPlayedCardServerRpc(redPlayedCardId);
            redTurnDone = true;
            UpdateTurnDoneBoolServerRpc(true);
        }
    }

    [ClientRpc]
    void SetBluePlayedCardClientRpc(int bluePlayedCardServer)
    {
        Debug.Log("SetBluePlayedCardClientRpc");
        bluePlayedCardId = bluePlayedCardServer;
    }

    [ServerRpc(RequireOwnership = false)]
    void SetRedPlayedCardServerRpc(int redPlayedCardClient)
    {
        Debug.Log("SetRedPlayedCardServerRpc");
        redPlayedCardId = redPlayedCardClient;
    }

    [ClientRpc]
    void UpdateTurnDoneBoolClientRpc(bool trueOrFalse)
    {
        blueTurnDone = trueOrFalse;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateTurnDoneBoolServerRpc(bool trueOrFalse)
    {
        redTurnDone = trueOrFalse;
    }

    public void ResetAllCards() 
    {
        blueHero.PutAllCardsInDeck();
        redHero.PutAllCardsInDeck();
        blueSelectedCard = redSelectedCard = null;
    }

    void MatchOver() 
    {
        uIManager.SetText(uIManager._messageTextBlue, roundWinner + " player wins Match! ");/////////
        uIManager.CallShowMessageOverlay();
        uIManager.ShowButton(uIManager._resetButtonBlue);///////
        Debug.Log("Match Over!");
    }

    bool CheckForIdolWinner() 
    {
        if (hasIdol == redHero && currentTurn == 8) 
        { 
            audioManager.PlaySound(audioManager.IdolWin);
            redRoundsWon++;
            roundWinner = "Red";
            return true;
        }
        if (hasIdol == blueHero && currentTurn == 8) 
        { 
            audioManager.PlaySound(audioManager.IdolWin);
            blueRoundsWon++;
            roundWinner = "Blue";
            return true;
        }
        return false;
    }

    bool CheckForDefeatWinner() 
    {
        if (blueHero.health == 0 ) 
        {
            audioManager.PlaySound(audioManager.PlayerDead);
            redRoundsWon++;
            roundWinner = "Red";
            return true;
        }
        if (redHero.health == 0 ) 
        {
            audioManager.PlaySound(audioManager.PlayerDead);
            blueRoundsWon++;
            roundWinner = "Blue";
            return true;
        }
        return false;
    }

    bool CheckForMatchWinner() 
    {
        if (blueRoundsWon == 2 || redRoundsWon == 2) 
            return true;
        return false;
    }

    public void SetHealth(Hero hero, int health) 
    {
        hero.health = health;
        if (hero == redHero) 
        {
            uIManager.SetHealth(uIManager._redHealthSliderBlue, health, uIManager._redHealthTextBlue);
            uIManager.SetHealth(uIManager._redHealthSliderBlue, health, uIManager._redHealthTextRed);
        }
        else 
        {
            uIManager.SetHealth(uIManager._blueHealthSliderBlue, health, uIManager._blueHealthTextBlue);
            uIManager.SetHealth(uIManager._redHealthSliderBlue, health, uIManager._redHealthTextRed);
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
                    idol.transform.position = GameObject.Find("Green Counter 13").transform.position;
                    _idolPosInt = 13;
                    break;
                case 2:
                    idol.transform.position = GameObject.Find("Green Counter 23").transform.position;
                    _idolPosInt = 23;
                    break;
                case 3:
                    idol.transform.position = GameObject.Find("Green Counter 33").transform.position;
                    _idolPosInt = 33;
                    break;
                case 4:
                    idol.transform.position = GameObject.Find("Green Counter 43").transform.position;
                    _idolPosInt = 43;
                    break;
                default:
                    idol.transform.position = GameObject.Find("Green Counter 53").transform.position;
                    _idolPosInt = 53;
                    break;
            }
        }
        PlaceIdolClientRpc(_idolPosInt);
    }

    [ClientRpc]
    void PlaceIdolClientRpc(int idolPosInt)
    {
        idol.transform.position = GameObject.Find("Green Counter " + idolPosInt.ToString()).transform.position;
    }

    public void PickupIdol(Counter counter) 
    {
        _idolPosInt = 0;
        audioManager.PlaySound(audioManager.CollectIdol);
        if (counter == blueCounter) 
        {
            idol.transform.position = blueHasIdolPos.position;
            hasIdol = blueHero;
        }
        else 
        {
            idol.transform.position = redHasIdolPos.position;
            hasIdol = redHero;
        }
    }

    public void DetermineFirstToGo(Hero blueHero, Hero redHero) 
    {
        if (blueHero.speed > redHero.speed) 
        {
            SetPlayerOrder(blueCounter, redCounter, true, false);
        } 
        else if (blueHero.speed < redHero.speed)
        {
            SetPlayerOrder(redCounter, blueCounter, false, true);
        } 
        else 
        {
            bool isBlueFirst = UnityEngine.Random.value < 0.5f;
            if (isBlueFirst) 
            {
                SetPlayerOrder(blueCounter, redCounter, true, false);
            } 
            else 
            {
                SetPlayerOrder(redCounter, blueCounter, true, false);
            }
        }

        if (currentRound == 2 && firstToGo == blueCounter) 
        {
            Debug.Log("Swapping first to go");
            SetPlayerOrder(redCounter, blueCounter, false, true);
        }
    }

    public void SetPlayerOrder(Counter firstPlayer, Counter secondPlayer, bool isFirstCupActive, bool isSecondCupActive) 
    {
        firstToGo = firstPlayer;
        secondToGo = secondPlayer;
        blueCup.SetActive(isFirstCupActive);
        redCup.SetActive(isSecondCupActive);
    }

    public void CalcGhostPositions(Hero hero, Counter counter) 
    {
        foreach (Card cardInHand in hero.hand) 
        {
            cardInHand.CalcOffsetForActions(counter, cardInHand);
        }
    }

    void AddToLog(string textToAdd) 
    {
    //    Debug.Log(textToAdd);
    }
}
