using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class UIManager : NetworkBehaviour
{
    [Header("Objects")]
    [SerializeField] Hero _blueHero;
    [SerializeField] Hero _redHero;

    [Header("Blue UI")]
    [SerializeField] Slider _blueHealthSliderBlue;
    [SerializeField] Slider _redHealthSliderBlue;
    [SerializeField] Text _blueHealthTextBlue, _redHealthTextBlue, _roundTextBlue, _turnTextBlue, _roundsTextBlue, _roundTimeLeftTextBlue,
        _blueReadyTextBlue, _redReadyTextBlue, _discardButtonTextBlue, _messageTextBlue;

    [Header("Red UI")]
    [SerializeField] Slider _blueHealthSliderRed;
    [SerializeField] Slider _redHealthSliderRed;
    [SerializeField] Text _blueHealthTextRed, _redHealthTextRed, _roundTextRed, _turnTextRed, _roundsTextRed, _roundTimeLeftTextRed,
        _redReadyTextRed, _blueReadyTextRed, _discardButtonTextRed, _messageTextRed;

    [Header("Blue Objects")]
    [SerializeField] GameObject _mainCanvasBlue;
    [SerializeField] GameObject _myDeckScreenBlue;
    [SerializeField] GameObject _gameLogCanvasBlue;
    [SerializeField] GameObject _lowerUIBlue;
    [SerializeField] GameObject _messageOverlayBlue;
    [SerializeField] GameObject _myDeckTextBlue;
    [SerializeField] GameObject _myDeckButtonBlue;
    [SerializeField] GameObject _closeMyDeckButtonBlue;
    [SerializeField] GameObject _startTurnButtonBlue;
    [SerializeField] GameObject _discardButtonBlue;
    [SerializeField] GameObject _resetButtonBlue;
    [SerializeField] GameObject _logButtonBlue;

    [Header("Red Objects")]
    [SerializeField] GameObject _mainCanvasRed;
    [SerializeField] GameObject _myDeckScreenRed;
    [SerializeField] GameObject _gameLogCanvasRede;
    [SerializeField] GameObject _lowerUIRed;
    [SerializeField] GameObject _messageOverlayRed;
    [SerializeField] GameObject _myDeckTextRed;
    [SerializeField] GameObject _startTurnButtonRed;
    [SerializeField] GameObject _myDeckButtonRed;
    [SerializeField] GameObject _closeMyDeckButtonRed;
    [SerializeField] GameObject _discardButtonRed;
    [SerializeField] GameObject _resetButtonRed;
    [SerializeField] GameObject _logButtonRed;

    public GameObject LogButtonBlue { get { return _logButtonBlue; } }
    public GameObject MyDeckButtonBlue { get { return _logButtonBlue; } }
    public GameObject LogButtonRed { get { return _logButtonBlue; } }
    public GameObject MyDeckButtonRed { get { return _logButtonBlue; } }

    public GameObject StartTurnButtonBlue { get => _startTurnButtonBlue; set => _startTurnButtonBlue = value; }
    public GameObject ResetButtonBlue { get => _resetButtonBlue; set => _resetButtonBlue = value; }
    public GameObject StartTurnButtonRed { get => _startTurnButtonRed; set => _startTurnButtonRed = value; }
    public GameObject ResetButtonRed { get => _resetButtonRed; set => _resetButtonRed = value; }
    public Slider BlueHealthSliderBlue { get => _blueHealthSliderBlue; set => _blueHealthSliderBlue = value; }
    public Slider RedHealthSliderBlue { get => _redHealthSliderBlue; set => _redHealthSliderBlue = value; }
    public Text BlueHealthTextBlue { get => _blueHealthTextBlue; set => _blueHealthTextBlue = value; }
    public Text RedHealthTextBlue { get => _redHealthTextBlue; set => _redHealthTextBlue = value; }
    public Text BlueHealthTextRed { get => _blueHealthTextRed; set => _blueHealthTextRed = value; }
    public Text RedHealthTextRed { get => _redHealthTextRed; set => _redHealthTextRed = value; }
    public Text MessageTextBlue { get => _messageTextBlue; set => _messageTextBlue = value; }
    public Text RoundTextBlue { get => _roundTextBlue; set => _roundTextBlue = value; }
    public Text RoundTextRed { get => _roundTextRed; set => _roundTextRed = value; }
    public Text RoundsTextBlue { get => _roundsTextBlue; set => _roundsTextBlue = value; }
    public Text RoundsTextRed { get => _roundsTextRed; set => _roundsTextRed = value; }
    public Text TurnTextBlue { get => _turnTextBlue; set => _turnTextBlue = value; }
    public Text TurnTextRed { get => _turnTextRed; set => _turnTextRed = value; }
    public Text BlueReadyTextBlue { get => _blueReadyTextBlue; set => _blueReadyTextBlue = value; }
    public Text RedReadyTextBlue { get => _redReadyTextBlue; set => _redReadyTextBlue = value; }
    public Text DiscardButtonTextBlue { get => _discardButtonTextBlue; set => _discardButtonTextBlue = value; }
    public Text MessageTextBlue1 { get => _messageTextBlue; set => _messageTextBlue = value; }
    public Text RedReadyTextRed { get => _redReadyTextRed; set => _redReadyTextRed = value; }
    public Text BlueReadyTextRed { get => _blueReadyTextRed; set => _blueReadyTextRed = value; }
    public Text DiscardButtonTextRed { get => _discardButtonTextRed; set => _discardButtonTextRed = value; }
    public Text MessageTextRed { get => _messageTextRed; set => _messageTextRed = value; }
    public Slider BlueHealthSliderRed { get => _blueHealthSliderRed; set => _blueHealthSliderRed = value; }
    public Slider RedHealthSliderRed { get => _redHealthSliderRed; set => _redHealthSliderRed = value; }
    public GameObject CloseMyDeckButtonBlue { get => _closeMyDeckButtonBlue; set => _closeMyDeckButtonBlue = value; }
    public GameObject CloseMyDeckButtonRed { get => _closeMyDeckButtonRed; set => _closeMyDeckButtonRed = value; }
    public GameObject DiscardButtonBlue { get => _discardButtonBlue; set => _discardButtonBlue = value; }
    public GameObject DiscardButtonRed { get => _discardButtonRed; set => _discardButtonRed = value; }

    GameManager _gameManager;

    void Start()
    {
        _gameManager = FindAnyObjectByType<GameManager>();  
        _blueHero = GameObject.Find("Blue Hero").GetComponent<Hero>();
        _redHero = GameObject.Find("Red Hero").GetComponent<Hero>();
    }

    public void ReloadScene() 
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void SetHealth(Slider slider, int valueToSet, Text text) 
    {
        slider.value = valueToSet;
        SetText(text, valueToSet.ToString());
    }

    public void ShowButton(GameObject button) => button.SetActive(true);

    public void SetText(Text text, string value) => text.text = value;

    public void ShowMainCanvas() => _mainCanvasBlue.SetActive(true);

    public void HideMainCanvas() => _mainCanvasBlue.SetActive(false);

    public void ShowMyDeckScreen() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _myDeckScreenBlue.SetActive(true);
            if (!_gameManager.BluePickingHand)
                ShowCloseMyDeckButton(_closeMyDeckButtonBlue);
            _blueHero.ShowDeck();
            _myDeckTextBlue.SetActive(true);
            _blueHero.SetCardPositionsInDeck(_blueHero.gameObject.transform.GetChild(0));
            _redHero.SetCardPositionsDummy(_redHero.gameObject.transform.GetChild(1));
        }
        else
        {
            _myDeckScreenRed.SetActive(true);
            if (!_gameManager.RedPickingHand)
                ShowCloseMyDeckButton(_closeMyDeckButtonRed);
            _redHero.ShowDeck();
            _myDeckTextRed.SetActive(true);
            _redHero.SetCardPositionsInDeck(_redHero.gameObject.transform.GetChild(0));
            _blueHero.SetCardPositionsDummy(_blueHero.gameObject.transform.GetChild(1));
        }
    }

    public void HideMyDeckScreen() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _myDeckScreenBlue.SetActive(false);
            //lowerUIBlue.SetActive(true);
            _mainCanvasBlue.SetActive(true);
            _blueHero.HideDeck();
            _myDeckTextBlue.SetActive(false);
        }
        else
        {
            _myDeckScreenRed.SetActive(false);
            //lowerUIRed.SetActive(true);
            _mainCanvasRed.SetActive(true);
            _redHero.HideDeck();
            _myDeckTextRed.SetActive(false);
        }
    }

    public void ShowGameLogScreen() 
    {
        //gameLogCanvas.SetActive(true);
    }

    public void HideGameLogScreen() => _gameLogCanvasBlue.SetActive(false); ////

    public void ShowStartTurnButton(GameObject startTurnButton) => startTurnButton.SetActive(true);
 
    public void HideStartTurnButton(GameObject startTurnButton) => startTurnButton.SetActive(false);

    public void ShowCloseMyDeckButton(GameObject button) => button.SetActive(true);

    public void HideCloseMyDeckButton(GameObject button) => button.SetActive(false);

    public void ShowLogAndMyDeckButton(GameObject logButton, GameObject myDeckButton)
    {
        logButton.SetActive(true);
        myDeckButton.SetActive(true);
    }

    public void HideLogAndMyDeckButton(GameObject logButton, GameObject myDeckButton)
    {
        logButton.SetActive(false);
        myDeckButton.SetActive(false);
    }

    public void ShowDiscardButton()
    {
        _discardButtonBlue.SetActive(true);
        _discardButtonRed.SetActive(true);
    }
    
    public void HideDiscardButton()
    {
        _discardButtonBlue.SetActive(false);
        _discardButtonRed.SetActive(false);
    }

    public void HideDiscardButton(GameObject button) => button.SetActive(false);

    public void ShowReadyText(Text readyText)  => readyText.enabled = true;

    public void HideReadyText(Text readyText) => readyText.enabled = false;

    public void DiscardButtonPress() 
    {
        //Debug.Log("Discard button press, hand has " + blueHero.hand.Count);
        List<Card> cardsToDiscard = new List<Card>();

        Hero hero;

        if (NetworkManager.Singleton.IsServer)
        {
            hero = _blueHero;
            ShowStartTurnButton(_startTurnButtonBlue);
            HideCloseMyDeckButton(_closeMyDeckButtonBlue);
            HideDiscardButton(DiscardButtonBlue);
        }
        else
        {
            hero = _redHero;
            ShowStartTurnButton(_startTurnButtonRed);
            HideCloseMyDeckButton(_closeMyDeckButtonRed); 
            HideDiscardButton(DiscardButtonRed);
        }

        foreach (Card card in hero.Hand)
            if (card.ToBeDiscarded)
                cardsToDiscard.Add(card);

        foreach (Card card in cardsToDiscard)
        {
            hero.DiscardCardFromHand(card);
            card.ToBeDiscarded = false;
        }
        _gameManager.BlueDiscarding = false;
        _gameManager.RedDiscarding = false;
        ShowMyDeckScreen();
    }

    public void UpdateDiscardButtonText(Hero hero) 
    {
        int cardsToDiscard = 0;
        foreach (Card card in hero.Hand) 
            if (card.Selected) 
                cardsToDiscard++;
            
        if (hero == _blueHero)
            _discardButtonTextBlue.text = "Discard\n" + cardsToDiscard.ToString();
        else
            _discardButtonTextRed.text = "Discard\n" + cardsToDiscard.ToString();
    }

    public void StartTurnButtonPress() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            UpdateDiscardButtonText(_blueHero);
            HideStartTurnButton(_startTurnButtonBlue);
            ShowReadyText(_blueReadyTextBlue);
            ShowLogAndMyDeckButton(_logButtonBlue, _myDeckButtonBlue);
            ShowBlueReadyTextClientRpc();
            _gameManager.StartTurnButtonPressed(_blueHero);
        }
        else
        {
            UpdateDiscardButtonText(_redHero);
            HideStartTurnButton(_startTurnButtonRed);
            ShowReadyText(_redReadyTextRed);
            ShowLogAndMyDeckButton(_logButtonRed, _myDeckButtonRed);
            ShowRedReadyTextServerRpc();
            _gameManager.StartTurnButtonPressed(_redHero);
        }
    }

    [ClientRpc]
    void ShowBlueReadyTextClientRpc() => ShowReadyText(_blueReadyTextRed);

    [ServerRpc(RequireOwnership = false)]
    void ShowRedReadyTextServerRpc() => ShowReadyText(_redReadyTextBlue);

    public void GameStarting()
    {
        ShowLogAndMyDeckButton(LogButtonRed, MyDeckButtonRed);
        ShowLogAndMyDeckButton(LogButtonBlue, MyDeckButtonBlue);
        HideReadyText(_blueReadyTextBlue);
        HideReadyText(_redReadyTextBlue);
        HideReadyText(_blueReadyTextRed);
        HideReadyText(_redReadyTextRed);
    }

    public void CallShowMessageOverlay() => StartCoroutine(ShowMessageOverlay());

    public IEnumerator ShowMessageOverlay()
    {
        _messageOverlayBlue.SetActive(true);
        yield return new WaitForSeconds(3);
        HideMessageOverlay();
    }

    void HideMessageOverlay() => _messageOverlayBlue.SetActive(false);
}
