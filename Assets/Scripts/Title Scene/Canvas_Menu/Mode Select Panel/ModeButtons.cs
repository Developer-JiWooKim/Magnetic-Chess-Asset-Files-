using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class ModeButtons : MonoBehaviour
{
    [SerializeField]
    private List<ModeBase> buttons;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        buttons = GetComponentsInChildren<ModeBase>().ToList();

        if (buttons == null)
        {
            Debug.Log("ModeButtons.cs - Setup() : buttons is null!!");
            return;
        }

        buttons.ForEach(modeButton => {
            modeButton.Setup();
            modeButton.PreparingMode();
        });
    }
}