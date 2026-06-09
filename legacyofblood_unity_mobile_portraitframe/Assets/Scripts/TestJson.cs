using System.Collections.Generic;
using UnityEngine;
using LegendOfBlood.Combat;

public class TestJson : MonoBehaviour
{
    void Start()
    {
        var cr = new CombatResult();
        cr.EventLog = new List<CombatEvent>();
        cr.EventLog.Add(new CombatEvent { EventType = CombatEventType.Attack, SourceID = "A", TargetID = "B", Value = 10 });
        
        string json = JsonUtility.ToJson(cr);
        Debug.Log("Serialized JSON: " + json);
        
        var cr2 = JsonUtility.FromJson<CombatResult>(json);
        Debug.Log("Deserialized EventLog Count: " + (cr2.EventLog != null ? cr2.EventLog.Count.ToString() : "NULL"));
    }
}
