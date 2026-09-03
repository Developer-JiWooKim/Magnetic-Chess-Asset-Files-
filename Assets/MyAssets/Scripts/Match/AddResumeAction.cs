using UnityEngine;
using Assets.MyAssets.Scripts.MainMenu;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class AddResumeAction : MonoBehaviour
    {
        [SerializeField] private GameObject background;
        [SerializeField] private GameDirector gameDirector;
        [SerializeField] private InGameUI_Manager inGameUI_Manager;

        // ResumePanel은 TitleScene의 DontDestroyOnLoad 메뉴 캔버스에 있어
        // GameScene 인스펙터로는 연결할 수 없다. 런타임에 한 번만 찾는다.
        private ResumePanel resumePanel;
        private Menu_Manager menuManager;

        void Start()
        {
            Setup();
        }
        private void Setup()
        {
            resumePanel = FindObjectOfType<ResumePanel>(true);

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
}
