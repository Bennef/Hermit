using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int cardId;
    public bool isDummy;
    public enum ActionType { Move, WeakAttack, StrongAttack};
    public ActionType actionType;
    public string cardString; // from NFT metadata
    public Card otherCard1, otherCard2;
    public bool locked = false, selected = false, toBeDiscarded = false, 
                discarded = false, inDeck = true; // discarded or played cards should be locked
    public Action[] baseActions;
    public List<Action> availableActions = new List<Action>();
    public GameObject availableActionsObj; // We are going to add action components to this obj
    Vector3 cardOffset;
    Transform x, hand, tick;
    Counter blueCounter, redCounter;
    GameManager gameManager;
    UIManager uIManager;
    AudioManager audioManager;
    Hero hero;

    void Awake() 
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        uIManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        baseActions = GetComponents<Action>();
        AssignChildren();
    }

    void Start()
    {
        blueCounter = GameObject.Find("Blue Counter(Clone)").GetComponent<Counter>();
        redCounter = GameObject.Find("Red Counter(Clone)").GetComponent<Counter>();
        //Debug.Log(this);
        foreach (Action action in baseActions)
        {
            action.AssignGhostCounters();
        }
    }

    public void AssignChildren() 
    {
        x = transform.GetChild(0);
        hand = transform.GetChild(1);
        tick = transform.GetChild(2);
        availableActionsObj = transform.GetChild(3).gameObject;
    }
    
    void OnMouseDown() 
    {
        if (locked || isDummy)
            return;

        if (NetworkManager.Singleton.IsServer)
        {
            hero = gameManager.blueHero;
        }
        else
        {
            hero = gameManager.redHero;
        }

        if (inDeck) 
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (!selected)
                {
                    PickUpCard(gameObject.transform);
                    ShowHand();
                    gameManager.SetDeckCardsSelected(hero, gameManager.blueDeckCardsSelected + 1);
                }
                else
                {
                    PutDownCard(gameObject.transform);
                    HideHand();
                    gameManager.SetDeckCardsSelected(hero, gameManager.blueDeckCardsSelected - 1);
                }
            }
            else
            {
                if (!selected)
                {
                    PickUpCard(gameObject.transform);
                    ShowHand();
                    gameManager.SetDeckCardsSelected(hero, gameManager.redDeckCardsSelected + 1);
                }
                else
                {
                    PutDownCard(gameObject.transform);
                    HideHand();
                    gameManager.SetDeckCardsSelected(hero, gameManager.redDeckCardsSelected - 1);
                }
            }
        }
        else if (!selected) 
        {
            PickUpCard(gameObject.transform);
            if (NetworkManager.Singleton.IsServer)
            {
                gameManager.blueSelectedCard = this;
            }
            else
            {
                gameManager.redSelectedCard = this;
            }
            
            HandleOtherCardSelection(otherCard1);
            HandleOtherCardSelection(otherCard2);
            HandlePlayerDiscarding();////
            toBeDiscarded = true;
        }
        else {
            PutDownCard(gameObject.transform);
            HideDestinations(availableActions);
            HideDiscardX();
            toBeDiscarded = false;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            if (gameManager.blueDiscarding)
                uIManager.UpdateDiscardButtonText(hero);
        }
        else
        {
            if (gameManager.redDiscarding)
                uIManager.UpdateDiscardButtonText(hero);
        }
    }

    void HandleOtherCardSelection(Card otherCard) 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (otherCard.selected && !gameManager.blueDiscarding)
            {
                PutDownCard(otherCard.GetComponent<Transform>());
                HideDestinations(otherCard.availableActions);
            }
        }
        else
        {
            if (otherCard.selected && !gameManager.redDiscarding)
            {
                PutDownCard(otherCard.GetComponent<Transform>());
                HideDestinations(otherCard.availableActions);
            }
        }
    }

    void HandlePlayerDiscarding() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (gameManager.blueDiscarding)
                ShowDiscardX();
            else
                ShowDestinations(availableActions);
        }
        else
        {
            if (gameManager.redDiscarding)
                ShowDiscardX();
            else
                ShowDestinations(availableActions);
        }
    }

    void PickUpCard(Transform card)
    {
        cardOffset = new Vector3(card.position.x, card.position.y, card.position.z - 0.2f);
        card.position = cardOffset;
        card.GetComponent<Card>().selected = true;
        audioManager.PlaySound(audioManager.CardUp);
    }

    public void PutDownCard(Transform card)
    {
        cardOffset = new Vector3(card.position.x, card.position.y, card.position.z + 0.2f);
        card.position = cardOffset;
        card.GetComponent<Card>().selected = false;
        card.GetComponent<Card>().toBeDiscarded = false;
        audioManager.PlaySound(audioManager.CardDown);
    }

    void ShowDestinations(List<Action> availableActions) 
    {
        foreach (Action action in availableActions)
        {
            foreach (GhostCounter ghostCounter in action.ghostCounters) 
            {
                if (ghostCounter.gridPosString != redCounter.gridPosString && 
                    ghostCounter.gridPosString != blueCounter.gridPosString ||
                    ghostCounter.actionType != GhostCounter.ActionType.Move) 
                {
                    Vector3 newScale = new Vector3(1.3f, 1.3f, 0.1f);
                    if (ghostCounter.actionType == GhostCounter.ActionType.Move) 
                    {
                        newScale = new Vector3(1.3f, 1.3f, 0.1f);
                    } 
                    else if (ghostCounter.actionType == GhostCounter.ActionType.StrongAttack) 
                    {
                        newScale = new Vector3(0.1f, 0.1f, 0.1f);
                    } 
                    else if (ghostCounter.actionType == GhostCounter.ActionType.WeakAttack) 
                    {
                        newScale = new Vector3(0.7f, 0.7f, 0.1f);
                    }
                    else
                    {
                        newScale = new Vector3(100f, 30f, 50f);
                    }
                    ghostCounter.transform.localScale = newScale;
                }
            }
        }
    }

    public void HideDestinations(List<Action> availableActions) 
    {
        foreach (Action action in availableActions) 
        {
            foreach (GhostCounter counter in action.ghostCounters)
            {
                counter.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                GhostCounter ghostCounter = counter.GetComponent<GhostCounter>();
            }
        }
    }

    public void CalcOffsetForActions(Counter counter, Card card) 
    {
        RemoveChildActions();
        availableActions.Clear();
        //Debug.Log("Calculating offsets for " + counter.name + ", GridPosString: " + counter.gridPosString);
        //Debug.Log("Card: " + card.name);
        string counterString = "";
        
        //Debug.Log(counter.name + "GridPosString: " + counter.gridPosString);
        //Debug.Log("Base actions: " + card.baseActions.Length);
        foreach (Action action in card.baseActions)
        {
            GhostCounter[] newGcs = new GhostCounter[action.ghostCounters.Length];
            bool copyAction = true;
            for (int i = action.ghostCounters.Length - 1; i >= 0; i--)
            {
                if (!copyAction) break;
                if (action.ghostCounters[i] == null) break;
                GhostCounter gc = action.ghostCounters[i];
                switch (gc.actionType)
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
                GhostCounter ghostCounter = action.ghostCounters[i].GetComponent<GhostCounter>();
                //Debug.Log(counter.gridPosString);
                int ghostCounterCoordX = int.Parse(ghostCounter.gridPosString.Substring(0, 1));
                int ghostCounterCoordY = int.Parse(ghostCounter.gridPosString.Substring(1, 1));
                int playerCounterCoordX = int.Parse(counter.gridPosString.Substring(0, 1));
                int playerCounterCoordY = int.Parse(counter.gridPosString.Substring(1, 1));
                //if (card.name == "4") Debug.Log("ghostCounterCoordX: " + ghostCounterCoordX + ", ghostCounterCoordY: " + ghostCounterCoordY);

                int gcX = ghostCounterCoordX - 3;
                int gcY = ghostCounterCoordY - 3;

                if (counter == redCounter)
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
                   (card.actionType == Card.ActionType.Move &&
                   (updatedCounterPosString == redCounter.gridPosString || updatedCounterPosString == blueCounter.gridPosString)))
                {
                    copyAction = false;
                }
                else
                {
                    string newPosString = counterString + updatedCounterPosString;
                    newGcs[i] = GameObject.Find(newPosString).GetComponent<GhostCounter>();
                }
            }
            if (copyAction)
            {
                CopyAction(action, newGcs);
            }
        }
    }

    void CopyAction(Action action, GhostCounter[] newGcArray)
    {
        //if (card.name == "Card 4") Debug.Log("Copying Action, action: " + action.actionId.ToString());
        if (availableActionsObj != null)
        {
            Action newAction = availableActionsObj.AddComponent<Action>();

            newAction.actionId = action.actionId;

            // Copy the original ghostCounters array to the new Action component
            //newAction.ghostCounters = (GhostCounter[])action.ghostCounters.Clone();
            newAction.ghostCounters = newGcArray;
            //Debug.Log(counterString + updatedCounterPosString);
            for (int i = 0; i < action.ghostCounters.Length; i++)
            {
                // Update the specific GhostCounter reference in the new Action component
                newAction.ghostRefs = new string[newAction.ghostCounters.Length];

                for (int j = 0; j < action.ghostRefs.Length; j++)
                {
                    Debug.Log(action.ghostRefs[j]);
                    if (action.ghostRefs[j] == "00") break;
                    Debug.Log(action.ghostRefs[j]);
                    newAction.ghostRefs[j] = newAction.ghostCounters[j].name;
                }
            }
            // Copy the ActionType from the original action to the new action
            newAction.actionType = action.actionType;

            //Debug.Log("Adding action to list: " + newAction);
            availableActions.Add(newAction);
        }
        else
        {
            Debug.LogError("availableActionsObj is null. Make sure it's properly initialized.");
        }
    }

    void RemoveChildActions() 
    {
        Action[] actions = availableActionsObj.GetComponents<Action>();
        foreach (Action action in actions) 
        {
            Destroy(action);
        }
    }

    public void ShowDiscardX() 
    {
        x.gameObject.SetActive(true);
    }

    public void HideDiscardX() 
    {
        x.gameObject.SetActive(false);
    }

    public void ShowHand() 
    {
        hand.gameObject.SetActive(true);
    }

    public void HideHand() 
    {
        hand.gameObject.SetActive(false);
    }

    public void ShowTick()
    {
        tick.gameObject.SetActive(true);
    }

    public void HideTick() 
    {
        tick.gameObject.SetActive(false);
    }
}
