using UnityEngine;

public class AddResumeAction : MonoBehaviour
{
    [SerializeField]
    private GameObject background;
    [SerializeField]
    private GameDirector gameDirector;

    private ResumePanel resumePanel;
    private InGameUI_Manager inGameUI_Manager;
    private Menu_Manager menuManager;

    void Start()
    {
        Setup();
    }
    private void Setup()
    {
        resumePanel = FindObjectOfType<ResumePanel>(true);
        inGameUI_Manager = FindObjectOfType<InGameUI_Manager>();

        menuManager = GetComponent<Menu_Manager>();

        resumePanel.OnSelect += Resume_OnClickSelectMode;
        resumePanel.OnReplay += Resume_OnClickReplay;
    }
    private void Resume_OnClickSelectMode()
    {
        gameDirector.StopGame();
        background.SetActive(true);
        inGameUI_Manager.Hide_All_Panel();
        menuManager.Change_GameSetting_to_ModeSelect();
    }
    private void Resume_OnClickReplay()
    {
        gameDirector.StopGame();
        background.SetActive(true);
        inGameUI_Manager.Hide_All_Panel();
        menuManager.Change_ModeSelect_to_GameSetting();
    }
}
