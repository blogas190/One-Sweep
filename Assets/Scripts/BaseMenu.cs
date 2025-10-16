using UnityEngine;

public abstract class BaseMenu : MonoBehaviour
{
    [SerializeField] protected GameObject menuUI;
    public virtual void Show()
    {
        menuUI.SetActive(true);
    }

    public virtual void Hide()
    {
        menuUI.SetActive(false);
    }
}
