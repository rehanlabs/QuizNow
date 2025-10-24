using System;
using UnityEngine;

public class ButtonEventCenter : MonoBehaviour
{
    public static ButtonEventCenter Instance { get; private set; }

    // Event triggered when *any* button is clicked
    public event Action OnAnyButtonClicked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void NotifyButtonClicked()
    {
        OnAnyButtonClicked?.Invoke();
    }
}
