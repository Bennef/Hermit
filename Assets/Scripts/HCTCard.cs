using UnityEngine;
using UnityEngine.UI;

public class HCTCard : MonoBehaviour
{
    HCTManager _hCTManager;

    void Start() => _hCTManager = FindAnyObjectByType<HCTManager>();

    public void Clicked()
    {
        if (_hCTManager.SelectedSlot != null)
        {
            SwapImage();
            AddCardToArray();
        }
    }

    void SwapImage()
    {
        Image image = GetComponent<Image>();
        _hCTManager.SelectedSlot.GetComponent<Image>().sprite = image.sprite;
    }

    void AddCardToArray()
    {
        GameObject card = Resources.Load("Prefabs/Cards/Moves/" + this.name) as GameObject;
        //print(card);
        GameObject[] cardArray;
        if (_hCTManager.SelectedSlot.transform.parent.name.Contains("Blue"))
            cardArray = _hCTManager.BlueCards;
        else
            cardArray = _hCTManager.RedCards;
        int index = int.Parse(_hCTManager.SelectedSlot.name) - 1;
        _hCTManager.InsertCardToList(index, card, cardArray);
    }
}
