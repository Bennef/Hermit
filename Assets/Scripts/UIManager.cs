using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class UIManager : NetworkBehaviour
{
    [Header("Objects")]
    [SerializeField] Hero blueHero;
    [SerializeField] Hero redHero;
    [SerializeField] GameManager gameManager;

    [Header("Blue UI")]
    [SerializeField] public Slider blueHealthSliderBlue;
    [SerializeField] public Slider redHealthSliderBlue;
    public Text blueHealthTextBlue, redHealthTextBlue, roundTextBlue, turnTextBlue, roundsTextBlue, roundTimeLeftTextBlue,
                redReadyTextBlue, blueReadyTextBlue, discardButtonTextBlue, messageTextBlue;

    [Header("Red UI")]
    [SerializeField] public Slider blueHealthSliderRed;
    [SerializeField] public Slider redHealthSliderRed;
    public Text blueHealthTextRed, redHealthTextRed, roundTextRed, turnTextRed, roundsTextRed, roundTimeLeftTextRed,
                redReadyTextRed, blueReadyTextRed, discardButtonTextRed, messageTextRed;

    [Header("Blue Objects")]
    [SerializeField] GameObject mainCanvasBlue;
    [SerializeField] GameObject myDeckScreenBlue;
    [SerializeField] GameObject logButtonBlue;
    [SerializeField] GameObject myDeckButtonBlue;
    [SerializeField] GameObject gameLogCanvasBlue;
    [SerializeField] GameObject lowerUIBlue;
    [SerializeField] GameObject closeMyDeckButtonBlue;
    [SerializeField] GameObject discardButtonBlue;
    [SerializeField] GameObject messageOverlayBlue;
    [SerializeField] GameObject myDeckTextBlue;
    public GameObject startTurnButtonBlue;
    public GameObject resetButtonBlue;

    [Header("Red Objects")]
    [SerializeField] GameObject mainCanvasRed;
    [SerializeField] GameObject myDeckScreenRed;
    [SerializeField] GameObject logButtonRed;
    [SerializeField] GameObject myDeckButtonRed;
    [SerializeField] GameObject gameLogCanvasRede;
    [SerializeField] GameObject lowerUIRed;
    [SerializeField] GameObject closeMyDeckButtonRed;
    [SerializeField] GameObject discardButtonRed;
    [SerializeField] GameObject messageOverlayRed;
    [SerializeField] GameObject myDeckTextRed;
    public GameObject startTurnButtonRed;
    public GameObject resetButtonRed;

    public GameObject LogButtonBlue { get { return logButtonBlue; } }
    public GameObject MyDeckButtonBlue { get { return logButtonBlue; } }
    public GameObject LogButtonRed { get { return logButtonBlue; } }
    public GameObject MyDeckButtonRed { get { return logButtonBlue; } }

    void Start()
    {
        blueHero = GameObject.Find("Blue Hero").GetComponent<Hero>();
        redHero = GameObject.Find("Red Hero").GetComponent<Hero>();
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

    public void ShowMainCanvas() => mainCanvasBlue.SetActive(true);

    public void HideMainCanvas() => mainCanvasBlue.SetActive(false);

    public void ShowMyDeckScreen() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            myDeckScreenBlue.SetActive(true);
            if (!gameManager.bluePickingHand)
                ShowCloseMyDeckButton();
            blueHero.ShowDeck();
            myDeckTextBlue.SetActive(true);
            blueHero.SetCardPositionsInDeck(blueHero.gameObject.transform.GetChild(0));
            redHero.SetCardPositionsDummy(redHero.gameObject.transform.GetChild(1));
        }
        else
        {
            myDeckScreenRed.SetActive(true);
            if (!gameManager.redPickingHand)
                ShowCloseMyDeckButton();
            redHero.ShowDeck();
            myDeckTextRed.SetActive(true);
            redHero.SetCardPositionsInDeck(redHero.gameObject.transform.GetChild(0));
            blueHero.SetCardPositionsDummy(blueHero.gameObject.transform.GetChild(1));
        }
    }

    public void HideMyDeckScreen() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            myDeckScreenBlue.SetActive(false);
            //lowerUIBlue.SetActive(true);
            mainCanvasBlue.SetActive(true);
            blueHero.HideDeck();
            myDeckTextBlue.SetActive(false);
        }
        else
        {
            myDeckScreenRed.SetActive(false);
            //lowerUIRed.SetActive(true);
            mainCanvasRed.SetActive(true);
            redHero.HideDeck();
            myDeckTextRed.SetActive(false);
        }
    }

    public void ShowGameLogScreen() 
    {
        //gameLogCanvas.SetActive(true);
    }

    public void HideGameLogScreen() => gameLogCanvasBlue.SetActive(false); ////

    public void ShowStartTurnButton(GameObject startTurnButton) => startTurnButton.SetActive(true);
 
    public void HideStartTurnButton(GameObject startTurnButton) => startTurnButton.SetActive(false);

    public void ShowCloseMyDeckButton() => closeMyDeckButtonBlue.SetActive(true);

    public void HideCloseMyDeckButton() => closeMyDeckButtonBlue.SetActive(false);

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
        discardButtonBlue.SetActive(true);
        discardButtonRed.SetActive(true);
    }

    public void HideDiscardButton() 
    {
        discardButtonBlue.SetActive(false);
        discardButtonRed.SetActive(false);
    }

    public void ShowReadyText(Text readyText)  => readyText.enabled = true;

    public void HideReadyText(Text readyText) => readyText.enabled = false;

    public void DiscardButtonPress() 
    {
        //Debug.Log("Discard button press, hand has " + blueHero.hand.Count);
        List<Card> cardsToDiscard = new List<Card>();

        Hero hero;

        if (NetworkManager.Singleton.IsServer)
        {
            hero = blueHero;
            ShowStartTurnButton(startTurnButtonBlue);
        }
        else
        {
            hero = redHero;
            ShowStartTurnButton(startTurnButtonRed);
        }

        foreach (Card card in hero.hand)
        {
            if (card.toBeDiscarded)
            {
                cardsToDiscard.Add(card);
            }
        }

        foreach (Card card in cardsToDiscard)
        {
            hero.DiscardCardFromHand(card);
            card.toBeDiscarded = false;
        }
        gameManager.blueDiscarding = false;
        gameManager.redDiscarding = false;
        HideDiscardButton();
        ShowMyDeckScreen();
        HideCloseMyDeckButton();
    }

    public void UpdateDiscardButtonText(Hero hero) 
    {
        int cardsToDiscard = 0;
        foreach (Card card in hero.hand) 
        {
            if (card.selected) {
                cardsToDiscard++;
            }
        }
        if (hero == blueHero)
        {
            discardButtonTextBlue.text = "Discard\n" + cardsToDiscard.ToString();
        }
        else
        {
            discardButtonTextRed.text = "Discard\n" + cardsToDiscard.ToString();
        }
    }

    public void StartTurnButtonPress() 
    {
        if (NetworkManager.Singleton.IsServer)
        {
            UpdateDiscardButtonText(blueHero);
            ShowReadyText(blueReadyTextBlue);
            ShowLogAndMyDeckButton(logButtonBlue, myDeckButtonBlue);
            ShowBlueReadyTextClientRpc();
            gameManager.StartTurnButtonPressed(blueHero);
        }
        else
        {
            UpdateDiscardButtonText(redHero);
            ShowReadyText(redReadyTextRed);
            ShowLogAndMyDeckButton(logButtonRed, myDeckButtonRed);
            ShowRedReadyTextServerRpc();
            gameManager.StartTurnButtonPressed(redHero);
        }
    }

    [ClientRpc]
    void ShowBlueReadyTextClientRpc() => ShowReadyText(blueReadyTextRed);

    [ServerRpc(RequireOwnership = false)]
    void ShowRedReadyTextServerRpc() => ShowReadyText(redReadyTextBlue);

    public void GameStarting()
    {
        ShowLogAndMyDeckButton(LogButtonRed, MyDeckButtonRed);
        ShowLogAndMyDeckButton(LogButtonBlue, MyDeckButtonBlue);
        HideReadyText(blueReadyTextBlue);
        HideReadyText(redReadyTextBlue);
        HideReadyText(blueReadyTextRed);
        HideReadyText(redReadyTextRed);
    }

    public void CallShowMessageOverlay() => StartCoroutine(ShowMessageOverlay());

    public IEnumerator ShowMessageOverlay()
    {
        messageOverlayBlue.SetActive(true);
        yield return new WaitForSeconds(3);
        HideMessageOverlay();
    }

    public void HideMessageOverlay() => messageOverlayBlue.SetActive(false);
}
