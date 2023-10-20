using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] bool _playerChoosingInitialCards;
    [SerializeField] string _heroName;
    [SerializeField] int _health = 100, _speed;
    [SerializeField] List<Card> _deck = new List<Card>();
    [SerializeField] List<Card> _hand = new List<Card>();
    [SerializeField] List<Card> _discardPile = new List<Card>();
    [SerializeField] Card _cardInPlay;
    UIManager _uIManager;

    public string HeroName { get => _heroName; set => _heroName = value; }
    public int Health { get => _health; set => _health = value; }
    public int Speed { get => _speed; set => _speed = value; }
    public Card CardInPlay { get => _cardInPlay; set => _cardInPlay = value; }
    public bool PlayerChoosingInitialCards { get => _playerChoosingInitialCards; set => _playerChoosingInitialCards = value; }
    public List<Card> Deck { get => _deck; set => _deck = value; }
    public List<Card> Hand { get => _hand; set => _hand = value; }
    public List<Card> DiscardPile { get => _discardPile; set => _discardPile = value; }

    void Awake() 
    {// just have all the cards as prefabs in hhe game and find them based on string
        _uIManager = FindAnyObjectByType<UIManager>();
        //PutAllCardsInDeck();
    }

    public void PutAllCardsInDeck() 
    {
        _deck.Clear();
        //print(deck.Count);
        _hand.Clear();
        _discardPile.Clear();
        _cardInPlay = null;
        for (int i = 0; i < this.gameObject.transform.GetChild(0).childCount; i++) 
        {
            Card card = this.gameObject.transform.GetChild(0).GetChild(i).GetComponent<Card>();
            //print(i + "  " + card);
            _deck.Add(card);
            card?.AssignChildren();
            card?.HideHand();
            card?.HideDiscardX();
            card?.HideTick();
            if (card != null) 
            {
                card.Locked = false;
                card.Selected = false;
                card.ToBeDiscarded = false;
                card.Discarded = false;
                card.InDeck = true;
                card.OtherCard1 = null;
                card.OtherCard2 = null;
            }
        }
        _playerChoosingInitialCards = true;
    }

    public void SetCardPositionsInDeck(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Card card = transform.GetChild(i).GetComponent<Card>();
            Transform deckPos;
            if (NetworkManager.Singleton.IsServer)
                deckPos = GameObject.Find("Blue Deck " + i).transform;
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
        if (_playerChoosingInitialCards) 
        {  
            _playerChoosingInitialCards = false;////
            //Debug.Log(this + " moving selected cards to hand");
            MoveSelectedCardsToHand();
        }
        else {
            int cardsRequired = 3 - _hand.Count;
            if (cardsRequired == 2 && _deck.Count == 1)
            {
                _hand.Add(_deck[0]);
                _deck.Remove(_deck[0]);
            }
            if (_deck.Count == 0) {
                // Put all discard pile in deck
                FilldeckFromDiscardPile();
            }
            MoveRandomCardsToHand(cardsRequired);
        }
    }

    public void FilldeckFromDiscardPile() 
    {
        // Show overlay
        _uIManager.SetText(_uIManager.MessageTextBlue, this.name + " shuffling discard pile\nto fill their hand"); ////
        _uIManager.CallShowMessageOverlay();
        List<Card> cardsToRemove = new List<Card>();
        foreach (Card card in _discardPile) {
            _deck.Add(card);
            card.Discarded = false;
            card.HideDiscardX();
            card.HideTick();
            card.HideHand();
            card.InDeck = true;
            cardsToRemove.Add(card);
        }
        foreach (Card cardToRemove in cardsToRemove) 
            _discardPile.Remove(cardToRemove);
    }

    void MoveSelectedCardsToHand()
    {
        List<Card> selectedCards = new List<Card>();
        foreach (Transform child in transform.GetChild(0)) 
        {
            //Debug.Log(child.name);
            Card card = child.GetComponent<Card>();
            if (card.Selected) {
                //Debug.Log("Selected card: " + card);
                selectedCards.Add(card);
                card.InDeck = false;
                card.Selected = false;
            }
        }
        //Debug.Log("Now adding selected cards to _hand");
        foreach (Card card in selectedCards) 
        {
            _hand.Add(card);
            _deck.Remove(card);
        }
    }

    private void MoveRandomCardsToHand(int cardsRequired) 
    {// def in here!
    //Debug.Log("Moving RANDOM cards to hand");
        Debug.Log(this + " adding " + cardsRequired + " RANDOM cards to hand");
        List<Card> randomCards = new List<Card>();

        while (randomCards.Count < cardsRequired && _deck.Count > 0) 
        {
            int randomIndex = Random.Range(0, _deck.Count);
            Card randomCard = _deck[randomIndex];
            randomCards.Add(randomCard);
            _deck.Remove(randomCard);
            randomCard.InDeck = false;
        }
        //Debug.Log(randomCards.Count);
        _hand.AddRange(randomCards);
    }

    public void DiscardCardFromHand(Card card) 
    { 
        //Debug.Log("Discarding " + card);
        card.Discarded = true; //?
        card.Locked = true; //?
        card.ShowDiscardX();
        _discardPile.Add(card);
        _hand.Remove(card);
    }

    public void DiscardRandomCardInHand() 
    {
        int randomNumber = Random.Range(0, 2);
        DiscardCardFromHand(_hand[randomNumber]);
    }

    public void HideDeck()
    {
        foreach (Card card in _deck) 
            card.gameObject.SetActive(false);

        foreach (Card card in _hand)
        {
            card.Locked = false;
            card.HideHand();
        }
        foreach (Card card in _discardPile) 
            card.gameObject.SetActive(false);
    }

    public void ShowDeck() 
    {
        foreach (Card card in _deck) 
            card.gameObject.SetActive(true);

        foreach (Card card in _hand) 
        {
            card.ShowHand();
            //Debug.Log(this.name + " showing card " + card);
            card.Locked = true;
        }
        foreach (Card card in _discardPile)
        {
            card.gameObject.SetActive(true);
            card.Selected = false;
        }
    }
}
