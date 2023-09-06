using Unity.Netcode;
using UnityEngine;

public class Action : MonoBehaviour
{
    public int actionId;
    public enum ActionType { Move, WeakAttack, StrongAttack, Arrow};
    public ActionType actionType;
    public string[] ghostRefs;
    public GhostCounter[] ghostCounters; 

    public void AssignGhostCounters()
    {
        ghostCounters = new GhostCounter[ghostRefs.Length]; 

        for (int i = 0; i < ghostRefs.Length; i++)
        {
            //Debug.Log(ghostRefs[i]);
            if (!NetworkManager.Singleton.IsServer)
            {
                if (ghostRefs.Length > 1 && i < ghostRefs.Length)
                {
                    Debug.Log(ghostRefs[i]);
                    switch (ghostRefs[i].Substring(0, 2))
                    {
                        case "N ":
                            ghostRefs[i] = ghostRefs[i].Replace("N", "S");
                            break;
                        case "E ":
                            ghostRefs[i] = ghostRefs[i].Replace("E", "W");
                            break;
                        case "S ":
                            ghostRefs[i] = ghostRefs[i].Replace("S", "N");
                            break;
                        case "W ":
                            ghostRefs[i] = ghostRefs[i].Replace("W", "E");
                            break;
                        case "NE":
                            ghostRefs[i] = ghostRefs[i].Replace("NE", "SW");
                            break;
                        case "SE":
                            ghostRefs[i] = ghostRefs[i].Replace("SE", "NW");
                            break;
                        case "SW":
                            ghostRefs[i] = ghostRefs[i].Replace("SW", "NE");
                            break;
                        case "NW":
                            ghostRefs[i] = ghostRefs[i].Replace("NW", "SE");
                            break;
                    }
                    Debug.Log(ghostRefs[i]);
                }
            }
            if (ghostRefs[i] != "00")
                ghostCounters[i] = GameObject.Find(ghostRefs[i]).GetComponent<GhostCounter>();
        }
    }
}
