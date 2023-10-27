using System;
using Unity.Netcode;
using UnityEngine;

public class Action : MonoBehaviour
{
    [SerializeField] int actionId;
    public enum ActionType { Move, WeakAttack, StrongAttack, Arrow};
    public ActionType actionType;
    [SerializeField] string[] _ghostRefs;
    [SerializeField] GhostCounter[] _ghostCounters;

    public int ActionId { get => actionId; set => actionId = value; }
    public string[] GhostRefs { get => _ghostRefs; set => _ghostRefs = value; }
    public GhostCounter[] GhostCounters { get => _ghostCounters; set => _ghostCounters = value; }
    
    public void AssignGhostCounters()
    {
        _ghostCounters = new GhostCounter[_ghostRefs.Length]; 

        for (int i = 0; i < _ghostRefs.Length; i++)
        {
            //Debug.Log(_ghostRefs[i]);
            if (!NetworkManager.Singleton.IsServer)
            {
                if (_ghostRefs.Length > 1 && i < _ghostRefs.Length)
                {
                    //Debug.Log(_ghostRefs[i]);
                    switch (_ghostRefs[i].Substring(0, 2))
                    {
                        case "N ":
                            _ghostRefs[i] = _ghostRefs[i].Replace("N", "S");
                            break;
                        case "E ":
                            _ghostRefs[i] = _ghostRefs[i].Replace("E", "W");
                            break;
                        case "S ":
                            _ghostRefs[i] = _ghostRefs[i].Replace("S", "N");
                            break;
                        case "W ":
                            _ghostRefs[i] = _ghostRefs[i].Replace("W", "E");
                            break;
                        case "NE":
                            _ghostRefs[i] = _ghostRefs[i].Replace("NE", "SW");
                            break;
                        case "SE":
                            _ghostRefs[i] = _ghostRefs[i].Replace("SE", "NW");
                            break;
                        case "SW":
                            _ghostRefs[i] = _ghostRefs[i].Replace("SW", "NE");
                            break;
                        case "NW":
                            _ghostRefs[i] = _ghostRefs[i].Replace("NW", "SE");
                            break;
                    }
                    //Debug.Log(_ghostRefs[i]);
                }
            }
            if (_ghostRefs[i] != "00") 
            {
                //print(_ghostRefs[i]);
                GameObject gc = GameObject.Find(_ghostRefs[i]);
                if (gc != null)
                {
                    _ghostCounters[i] = gc.GetComponent<GhostCounter>();
                }
            }
        }
    }
}
