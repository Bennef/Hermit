using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public bool playerChoosingInitialCards;
    public string heroName;
    public int health = 100, speed;
    public List<Card> deck = new List<Card>();
    public List<Card> hand = new List<Card>();
    public List<Card> discardPile = new List<Card>();
    public Card cardInPlay;
    UIManager uIManager;

    void Awake() 
    {// just have all the cards as prefabs in hhe game and find them based on string
        uIManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
        PutAllCardsInDeck();
    }

    public void PutAllCardsInDeck() 
    {
        deck.Clear();
        hand.Clear();
        discardPile.Clear();
        cardInPlay = null;
        for (int i = 0; i < this.gameObject.transform.GetChild(0).childCount; i++) 
        {
            Card card = this.gameObject.transform.GetChild(0).GetChild(i).GetComponent<Card>();
            deck.Add(card);
            card?.AssignChildren();
            card?.HideHand();
            card?.HideDiscardX();
            card?.HideTick();
            if (card != null) 
            {
                card.locked = false;
                card.selected = false;
                card.toBeDiscarded = false;
                card.discarded = false;
                card.inDeck = true;
                card.otherCard1 = null;
                card.otherCard2 = null;
            }
        }
        playerChoosingInitialCards = true;
    }

    public void SetCardPositionsInDeck(Transform transform) 
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Card card = transform.GetChild(i).GetComponent<Card>();
            Transform deckPos;
            if (NetworkManager.Singleton.IsServer)
            {
                deckPos = GameObject.Find("Blue Deck " + i).transform;
            }
            else
            {
                deckPos = GameObject.Find("Red Deck " + i).transform;
                card.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            card.transform.position = deckPos.position;
        }
    }

    public void SetCardPositionsDummy(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Card card = transform.GetChild(i).GetComponent<Card>();
            Transform dummyPos;
            if (NetworkManager.Singleton.IsServer)
            {
                dummyPos = GameObject.Find("Red Dummy " + i).transform;
                card.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            else
            {
                dummyPos = GameObject.Find("Blue Dummy " + i).transform;
            }
            card.transform.position = dummyPos.position;
        }
    }

    public void MoveCardsToHand() {
        if (playerChoosingInitialCards) 
        {  
            playerChoosingInitialCards = false;////
            //Debug.Log(this + " moving selected cards to hand");
            MoveSelectedCardsToHand();
        }
        else {
            int cardsRequired = 3 - hand.Count;
            if (cardsRequired == 2 && deck.Count == 1)
            {
                hand.Add(deck[0]);
                deck.Remove(deck[0]);
            }
            if (deck.Count == 0) {
                // Put all discard pile in deck
                FilldeckFromDiscardPile();
            }
            MoveRandomCardsToHand(cardsRequired);
        }
    }

    public void FilldeckFromDiscardPile() 
    {
        // Show overlay
        uIManager.SetText(uIManager.messageTextBlue, this.name + " shuffling discard pile\nto fill their hand"); ////
        uIManager.CallShowMessageOverlay();
        List<Card> cardsToRemove = new List<Card>();
        foreach (Card card in discardPile) {
            deck.Add(card);
            card.discarded = false;
            card.HideDiscardX();
            card.HideTick();
            card.HideHand();
            card.inDeck = true;
            cardsToRemove.Add(card);
        }
        foreach (Card cardToRemove in cardsToRemove) 
        {
            discardPile.Remove(cardToRemove);
        }
    }

    void MoveSelectedCardsToHand()
    {
        List<Card> selectedCards = new List<Card>();
        foreach (Transform child in transform.GetChild(0)) 
        {
            //Debug.Log(child.name);
            Card card = child.GetComponent<Card>();
            if (card.selected) {
                //Debug.Log("Selected card: " + card);
                selectedCards.Add(card);
                card.inDeck = false;
                card.selected = false;
            }
        }
        //Debug.Log("Now adding selected cards to hand");
        foreach (Card card in selectedCards) 
        {
            hand.Add(card);
            deck.Remove(card);
        }
    }

    private void MoveRandomCardsToHand(int cardsRequired) 
    {// def in here!
    //Debug.Log("Moving RANDOM cards to hand");
        Debug.Log(this + " adding " + cardsRequired + " RANDOM cards to hand");
        List<Card> randomCards = new List<Card>();

        while (randomCards.Count < cardsRequired && deck.Count > 0) 
        {
            int randomIndex = Random.Range(0, deck.Count);
            Card randomCard = deck[randomIndex];
            randomCards.Add(randomCard);
            deck.Remove(randomCard);
            randomCard.inDeck = false;
        }
        //Debug.Log(randomCards.Count);
        hand.AddRange(randomCards);
    }

    public void DiscardCardFromHand(Card card) 
    { 
        //Debug.Log("Discarding " + card);
        card.discarded = true; //?
        card.locked = true; //?
        card.ShowDiscardX();
        discardPile.Add(card);
        hand.Remove(card);
    }

    public void DiscardRandomCardInHand() 
    {
        int randomNumber = Random.Range(0, 2);
        DiscardCardFromHand(hand[randomNumber]);
    }

    public void HideDeck()
    {
        foreach (Card card in deck) 
        {
            card.gameObject.SetActive(false);
        }
        foreach (Card card in hand)
        {
            card.locked = false;
            card.HideHand();
        }
        foreach (Card card in discardPile) 
        {
            card.gameObject.SetActive(false);
        }
    }

    public void ShowDeck() 
    {
        foreach (Card card in deck) 
        {
            card.gameObject.SetActive(true);
        }
        foreach (Card card in hand) 
        {
            card.ShowHand();
            //Debug.Log(this.name + " showing card " + card);
            card.locked = true;
        }
        foreach (Card card in discardPile)
        {
            card.gameObject.SetActive(true);
            card.selected = false;
        }
    }
}
