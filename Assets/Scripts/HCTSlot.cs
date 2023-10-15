using UnityEngine;

public class HCTSlot : MonoBehaviour
{
    HCTManager _hCTManager;
    
    void Start()
    {
        _hCTManager = GameObject.Find("HCT Manager").GetComponent<HCTManager>();
    }

    public void Clicked()
    {
        _hCTManager.SelectedSlot = this;
        _hCTManager.SelectedImage.SetActive(true);
        _hCTManager.SelectedImage.transform.position = this.transform.position;
    }
}
