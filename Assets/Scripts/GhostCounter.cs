using Unity.Netcode;
using UnityEngine;

public class GhostCounter : MonoBehaviour
{
    public enum ActionType { Move, WeakAttack, StrongAttack, N, NE, E, SE, S, SW, W, NW};
    public ActionType actionType;
    public string gridPosString;
    Renderer counterRenderer;
    GameManager gameManager;

    void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        counterRenderer = gameObject.GetComponent<Renderer>();
    }

    void Update() 
    {
        if (actionType == ActionType.StrongAttack) 
        {
            counterRenderer.material.color = new Color (1f, 0f, 0f, Mathf.PingPong(Time.time, 0.5f));
        }
        else if (actionType == ActionType.WeakAttack) 
        {
            counterRenderer.material.color = new Color (1f, 0.5f, 0f, Mathf.PingPong(Time.time, 0.5f));
        }
        else
        {
            counterRenderer.material.color = new Color(0f, 0.5f, 0f, Mathf.PingPong(Time.time, 0.5f));
        }
    }

    void OnMouseDown()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (Action availableAction in gameManager.blueSelectedCard.availableActionsObj.GetComponents<Action>())
            {
                foreach(GhostCounter gc in availableAction.ghostCounters)
                {
                    if (gc == this)
                    {
                        //Debug.Log(this + " - " + gc);
                        //Debug.Log(availableAction.ghostRefs[0]);
                        gameManager.ActionSelected(gameManager.blueHero, availableAction.actionType, availableAction.ghostRefs);
                    }
                }
            }
        }
        else
        {
            foreach (Action availableAction in gameManager.redSelectedCard.availableActionsObj.GetComponents<Action>())
            {
                foreach (GhostCounter gc in availableAction.ghostCounters)
                {
                    if (gc == this)
                    {
                        //Debug.Log(this + " - " + gc);
                        //Debug.Log(availableAction.ghostRefs[0]);
                        gameManager.ActionSelected(gameManager.redHero, availableAction.actionType, availableAction.ghostRefs);
                    }
                }
            }
        }
    }
}
