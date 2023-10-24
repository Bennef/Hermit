using Unity.Netcode;
using UnityEngine;

public class GhostCounter : MonoBehaviour
{
    public enum ActionType { Move, WeakAttack, StrongAttack, N, NE, E, SE, S, SW, W, NW};
    [SerializeField] ActionType _actionType;
    [SerializeField] string _gridPosString;
    Renderer _counterRenderer;
    GameManager _gameManager;

    public string GridPosString { get => _gridPosString; set => _gridPosString = value; }
    public ActionType GCActionType { get => _actionType; set => _actionType = value; }

    void Awake()
    {
        _gameManager = FindAnyObjectByType<GameManager>();
        _counterRenderer = gameObject.GetComponent<Renderer>();
        SetGridPosString();
    }

    private void SetGridPosString()
    {
        _gridPosString = name.Substring(name.Length - 2);
    }

    void Update() 
    {
        if (_actionType == ActionType.StrongAttack) 
            _counterRenderer.material.color = new Color (1f, 0f, 0f, Mathf.PingPong(Time.time, 0.5f));
        else if (_actionType == ActionType.WeakAttack) 
            _counterRenderer.material.color = new Color (1f, 0.5f, 0f, Mathf.PingPong(Time.time, 0.5f));
        else
            _counterRenderer.material.color = new Color(0f, 0.5f, 0f, Mathf.PingPong(Time.time, 0.5f));
    }

    void OnMouseDown()
    {
        Hero selectedHero = (NetworkManager.Singleton.IsServer) ? _gameManager.BlueHero : _gameManager.RedHero;
        Card selectedCard = (NetworkManager.Singleton.IsServer) ? _gameManager.BlueSelectedCard : _gameManager.RedSelectedCard;

        foreach (Action availableAction in selectedCard.AvailableActionsObj.GetComponents<Action>())
            foreach (GhostCounter gc in availableAction.GhostCounters)
                if (gc == this)
                    _gameManager.ActionSelected(selectedHero, availableAction.actionType, availableAction.GhostRefs);
    }
}
