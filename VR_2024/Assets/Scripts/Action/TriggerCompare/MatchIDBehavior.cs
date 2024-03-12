using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MatchIDBehavior : IDBehavior
{
    [System.Serializable]
    public struct PossibleMatch
    {
        public ID id;
        public UnityEvent triggerEvent;
    }
    
    private WaitForFixedUpdate _wffu = new WaitForFixedUpdate();
    public bool debug;
    public List<PossibleMatch> triggerEnterMatches;

    private void OnTriggerEnter(Collider other)
    {
        IDBehavior idBehavior = other.GetComponent<IDBehavior>();
        if (idBehavior == null) return;
        StartCoroutine(CheckId(other, idBehavior.idObj, triggerEnterMatches));
    }
    
    private IEnumerator CheckId(Collider other, ID otherId, List<PossibleMatch> possibleMatches)
    {
        bool noMatch = true;
        foreach (var obj in possibleMatches)
        {
            if (otherId != obj.id) continue;
            if (debug) Debug.Log($"Match found on: '{this}' with '{obj.id}' while checking '{other}' with '{otherId}'");
            noMatch = false;
            obj.triggerEvent.Invoke();
            yield return _wffu;
        }

        if (noMatch && debug)
        {
            Debug.Log(
                $"No match found on: '{this}' While checking '{other}' with: '{otherId}' with {possibleMatches.Count}" +
                $" possible matches.");
        }
    }
}