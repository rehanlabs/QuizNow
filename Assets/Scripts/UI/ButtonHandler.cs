using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonHandler : MonoBehaviour
{
    private Button targetButton;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
        targetButton.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        ButtonEventCenter.Instance.NotifyButtonClicked();
    }
}
