using UnityEngine;

public class CoreSystems : MonoBehaviour
{
    private void Awake() { DontDestroyOnLoad(this.gameObject); }
}