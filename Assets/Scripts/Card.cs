using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] int _cardId;
    [SerializeField] bool _isDummy;
    [SerializeField] enum ActionType { Move, WeakAttack, StrongAttack};
    [SerializeField] ActionType _actionType;
    [SerializeField] string _cardString; // from NFT metadata
    [SerializeField] Card _otherCard1, _otherCard2;
    [SerializeField] bool _locked, _selected, _toBeDiscarded, _discarded, _inDeck = true; // discarded or played cards should be locked
    [SerializeField] Action[] _baseActions;
    [SerializeField] List<Action> _availableActions = new List<Action>();
    [SerializeField] GameObject _availableActionsObj; // We are going to add action components to this obj
    Vector3 _cardOffset;
    Transform _x, _hand, _tick;
    Counter _blueCounter, _redCounter;
    GameManager _gameManager;
    UIManager _uIManager;
    AudioManager _audioManager;
    Hero _hero;

    public int CardId { get => _cardId; set => _cardId = value; }
    public Card OtherCard1 { get => _otherCard1; set => _otherCard1 = value; }
    public Card OtherCard2 { get => _otherCard2; set => _otherCard2 = value; }
    public bool Locked { get => _locked; set => _locked = value; }
    public bool Selected { get => _selected; set => _selected = value; }
    public bool ToBeDiscarded { get => _toBeDiscarded; set => _toBeDiscarded = value; }
    public List<Action> AvailableActions { get => _availableActions; set => _availableActions = value; }
    public bool Discarded { get => _discarded; set => _discarded = value; }
    public bool InDeck { get => _inDeck; set => _inDeck = value; }
    public Action[] BaseActions { get => _baseActions; set => _baseActions = value; }
    public GameObject AvailableActionsObj { get => _availableActionsObj; set => _availableActionsObj = value; }

    void Awake() 
    {
        _gameManager = FindAnyObjectByType<GameManager>();
        _uIManager = FindAnyObjectByType<UIManager>();
        _audioManager = FindAnyObjectByType<AudioManager>();
        _baseActions = GetComponents<Action>();
        AssignChildren();
    }

    void Start()
    {
        _blueCounter = GameObject.Find("Blue Counter(Clone)").GetComponent<Counter>();
        _redCounter = GameObject.Find("Red Counter(Clone)").GetComponent<Counter>();
        foreach (Action action in _baseActions)
            action.AssignGhostCounters();
    }

    public void AssignChildren() 
    {
        _x = transform.GetChild(0);
        _hand = transform.GetChild(1);
        _tick = transform.GetChild(2);
        _availableActionsObj = transform.GetChild(3).gameObject;
    }
    
    void OnMouseDown() 
    {
        if (_locked || _isDummy)
            return;

        if (NetworkManager.Singleton.IsServer)
            _hero = _gameManager.BlueHero;
        else
            _hero = _gameManager.RedHero;

        if (_inDeck) 
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (!_selected)
                {
                    PickUpCard(gameObject.transform);
                    ShowHand();
                    _gameManager.SetDeckCardsSelected(_hero, _gameManager.BlueDeckCardsSelected + 1);
                }
                else
                {
                    PutDownCard(gameObject.transform);
                    HideHand();
                    _gameManager.SetDeckCardsSelected(_hero, _gameManager.BlueDeckCardsSelected - 1);
                }
            }
            else
            {
                if (!_selected)
                {
                    PickUpCard(gameObject.transform);
                    ShowHand();
                    _gameManager.SetDeckCardsSelected(_hero, _gameManager.RedDeckCardsSelected + 1);
                }
                else
                {
                    PutDownCard(gameObject.transform);
                    HideHand();
                    _gameManager.SetDeckCardsSelected(_hero, _gameManager.RedDeckCardsSelected - 1);
                }
            }
        }
        else if (!_selected) 
        {
            PickUpCard(gameObject.transform);
            if (NetworkManager.Singleton.IsServer)
                _gameManager.BlueSelectedCard = this;
            else
                _gameManager.RedSelectedCard = this;

            HandleOtherCardSelection(_otherCard1);
            HandleOtherCardSelection(_otherCard2);
            HandlePlayerDiscarding();////
            _toBeDiscarded = true;
        }
        else {
            PutDownCard(gameObject.transform);
            HideDestinations(_availableActions);
            HideDiscardX();
            _toBeDiscarded = false;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            if (_gameManager.BlueDiscarding)
                _uIManager.UpdateDiscardButtonText(_hero);
        }
        else
        {
            if (_gameManager.RedDiscarding)
                _uIManager.UpdateDiscardButtonText(_hero);
        }
    }

    void HandleOtherCardSelection(Card otherCard) 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (otherCard._selected && !_gameManager.BlueDiscarding)
            {
                PutDownCard(otherCard.GetComponent<Transform>());
                HideDestinations(otherCard._availableActions);
            }
        }
        else
        {
            if (otherCard._selected && !_gameManager.RedDiscarding)
            {
                PutDownCard(otherCard.GetComponent<Transform>());
                HideDestinations(otherCard._availableActions);
            }
        }
    }

    void HandlePlayerDiscarding() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (_gameManager.BlueDiscarding)
                ShowDiscardX();
            else
                ShowDestinations(_availableActions);
        }
        else
        {
            if (_gameManager.RedDiscarding)
                ShowDiscardX();
            else
                ShowDestinations(_availableActions);
        }
    }

    void PickUpCard(Transform card)
    {
        _cardOffset = new Vector3(card.position.x, card.position.y, card.position.z - 0.2f);
        card.position = _cardOffset;
        card.GetComponent<Card>()._selected = true;
        _audioManager.PlaySound(_audioManager.CardUp);
    }

    public void PutDownCard(Transform card)
    {
        _cardOffset = new Vector3(card.position.x, card.position.y, card.position.z + 0.2f);
        card.position = _cardOffset;
        card.GetComponent<Card>()._selected = false;
        card.GetComponent<Card>()._toBeDiscarded = false;
        _audioManager.PlaySound(_audioManager.CardDown);
    }

    void ShowDestinations(List<Action> _availableActions) 
    {
        foreach (Action action in _availableActions)
        {
            foreach (GhostCounter ghostCounter in action.GhostCounters) 
            {
                if (ghostCounter.GridPosString != _redCounter.GridPosString && 
                    ghostCounter.GridPosString != _blueCounter.GridPosString ||
                    ghostCounter.GCActionType != GhostCounter.ActionType.Move) 
                {
                    Vector3 newScale = new Vector3(1.3f, 1.3f, 0.1f);
                    if (ghostCounter.GCActionType == GhostCounter.ActionType.Move) 
                        newScale = new Vector3(1.3f, 1.3f, 0.1f);
                    else if (ghostCounter.GCActionType == GhostCounter.ActionType.StrongAttack) 
                        newScale = new Vector3(0.1f, 0.1f, 0.1f);
                    else if (ghostCounter.GCActionType == GhostCounter.ActionType.WeakAttack) 
                        newScale = new Vector3(0.7f, 0.7f, 0.1f);
                    else
                        newScale = new Vector3(100f, 30f, 50f);
                    ghostCounter.transform.localScale = newScale;
                }
            }
        }
    }

    public void HideDestinations(List<Action> _availableActions) 
    {
        foreach (Action action in _availableActions) 
            foreach (GhostCounter counter in action.GhostCounters)
            {
                counter.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                GhostCounter ghostCounter = counter.GetComponent<GhostCounter>();
            }
    }

    public void CalcOffsetForActions(Counter counter, Card card) 
    {
        RemoveChildActions();
        _availableActions.Clear();
        //Debug.Log("Calculating offsets for " + counter.name + ", GridPosString: " + counter.gridPosString);
        //Debug.Log("Card: " + card.name);
        string counterString = "";
        
        //Debug.Log(counter.name + "GridPosString: " + counter.gridPosString);
        //Debug.Log("Base actions: " + card.baseActions.Length);
        foreach (Action action in card._baseActions)
        {
            GhostCounter[] newGcs = new GhostCounter[action.GhostCounters.Length];
            bool copyAction = true;
            for (int i = action.GhostCounters.Length - 1; i >= 0; i--)
            {
                if (!copyAction) break;
                if (action.GhostCounters[i] == null) break;
                GhostCounter gc = action.GhostCounters[i];
                switch (gc.GCActionType)
                {
                    case GhostCounter.ActionType.Move:
                        counterString = "Green Counter ";
                        break;
                    case GhostCounter.ActionType.StrongAttack:
                        counterString = "Red Star ";
                        break;
                    case GhostCounter.ActionType.WeakAttack:
                        counterString = "Orange Star ";
                        break;
                    case GhostCounter.ActionType.N:
                        counterString = "N ";
                        break;
                    case GhostCounter.ActionType.NE:
                        counterString = "NE ";
                        break;
                    case GhostCounter.ActionType.E:
                        counterString = "E ";
                        break;
                    case GhostCounter.ActionType.SE:
                        counterString = "SE ";
                        break;
                    case GhostCounter.ActionType.S:
                        counterString = "S ";
                        break;
                    case GhostCounter.ActionType.SW:
                        counterString = "SW ";
                        break;
                    case GhostCounter.ActionType.W:
                        counterString = "W ";
                        break;
                    case GhostCounter.ActionType.NW:
                        counterString = "NW ";
                        break;
                }
                //if (card.name == "4") Debug.Log(action.ghostCounters[i]);
                GhostCounter ghostCounter = action.GhostCounters[i].GetComponent<GhostCounter>();
                //Debug.Log(counter.gridPosString);
                int ghostCounterCoordX = int.Parse(ghostCounter.GridPosString.Substring(0, 1));
                int ghostCounterCoordY = int.Parse(ghostCounter.GridPosString.Substring(1, 1));
                int playerCounterCoordX = int.Parse(counter.GridPosString.Substring(0, 1));
                int playerCounterCoordY = int.Parse(counter.GridPosString.Substring(1, 1));
                //if (card.name == "4") Debug.Log("ghostCounterCoordX: " + ghostCounterCoordX + ", ghostCounterCoordY: " + ghostCounterCoordY);

                int gcX = ghostCounterCoordX - 3;
                int gcY = ghostCounterCoordY - 3;

                if (counter == _redCounter)
                {
                    gcX *= -1;
                    gcY *= -1;
                }

                int newPosX = playerCounterCoordX + gcX;
                int newPosY = playerCounterCoordY + gcY;

                string updatedCounterPosString = newPosX.ToString() + newPosY.ToString();
                //Debug.Log("updatedCounterPosString: " + updatedCounterPosString);
                // Check if out of bounds or if a counter is in the way, if yes then DON'T add action
                if ((newPosX > 5 || newPosY > 5 || newPosX < 1 || newPosY < 1) ||
                   (card._actionType == Card.ActionType.Move &&
                   (updatedCounterPosString == _redCounter.GridPosString || updatedCounterPosString == _blueCounter.GridPosString)))
                {
                    copyAction = false;
                }
                else
                {
                    string newPosString = counterString + updatedCounterPosString; print(newPosString);
                    newGcs[i] = GameObject.Find(newPosString).GetComponent<GhostCounter>();
                }
            }
            if (copyAction)
                CopyAction(action, newGcs);
        }
    }

    void CopyAction(Action action, GhostCounter[] newGcArray)
    {
        //if (card.name == "Card 4") Debug.Log("Copying Action, action: " + action.actionId.ToString());
        if (_availableActionsObj != null)
        {
            Action newAction = _availableActionsObj.AddComponent<Action>();

            newAction.ActionId = action.ActionId;

            // Copy the original ghostCounters array to the new Action component
            //newAction.ghostCounters = (GhostCounter[])action.ghostCounters.Clone();
            newAction.GhostCounters = newGcArray;
            //Debug.Log(counterString + updatedCounterPosString);
            for (int i = 0; i < action.GhostCounters.Length; i++)
            {
                // Update the specific GhostCounter reference in the new Action component
                newAction.GhostRefs = new string[newAction.GhostCounters.Length];

                for (int j = 0; j < action.GhostRefs.Length; j++)
                {
                    //Debug.Log(action._ghostRefs[j]);
                    if (action.GhostRefs[j] == "00") break;
                    //Debug.Log(action._ghostRefs[j]);
                    newAction.GhostRefs[j] = newAction.GhostCounters[j].name;
                }
            }
            // Copy the ActionType from the original action to the new action
            newAction.actionType = action.actionType;

            //Debug.Log("Adding action to list: " + newAction);
            _availableActions.Add(newAction);
        }
        else
        {
            Debug.LogError("__availableActionsObj is null. Make sure it's properly initialized.");
        }
    }

    void RemoveChildActions() 
    {
        Action[] actions = _availableActionsObj.GetComponents<Action>();
        foreach (Action action in actions) 
            Destroy(action);
    }

    public void ShowDiscardX() => _x.gameObject.SetActive(true);

    public void HideDiscardX() => _x.gameObject.SetActive(false);

    public void ShowHand() => _hand.gameObject.SetActive(true);

    public void HideHand() => _hand.gameObject.SetActive(false);

    public void ShowTick() => _tick.gameObject.SetActive(true);

    public void HideTick() => _tick.gameObject.SetActive(false);
}
