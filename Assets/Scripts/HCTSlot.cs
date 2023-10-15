using UnityEngine;

public class HCTSlot : MonoBehaviour
{
    HCTManager _hCTManager;
    
    void Start() => _hCTManager = FindAnyObjectByType<HCTManager>();

    public void Clicked()
    {
        _hCTManager.SelectedSlot = this;
        _hCTManager.SelectedImage.SetActive(true);
        _hCTManager.SelectedImage.transform.position = this.transform.position;
    }
}
