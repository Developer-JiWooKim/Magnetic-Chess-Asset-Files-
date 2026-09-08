using UnityEngine;
using Assets.MyAssets.Scripts.MainMenu;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class AddResumeAction : MonoBehaviour
    {
        [SerializeField] private GameObject _background;
        [SerializeField] private GameDirector _gameDirector;
        [SerializeField] private InGameUIManager _inGameUIManager;

        // ResumePanel은 TitleScene의 DontDestroyOnLoad 메뉴 캔버스에 있어
        // GameScene 인스펙터로는 연결할 수 없다. 런타임에 한 번만 찾는다.
        private ResumePanel _resumePanel;
        private MenuManager _menuManager;

        void Start()
        {
            Setup();
        }
        private void Setup()
        {
            _resumePanel = FindObjectOfType<ResumePanel>(true);

            _menuManager = GetComponent<MenuManager>();

            _resumePanel.OnSelect += ResumeOnClickSelectMode;
            _resumePanel.OnReplay += ResumeOnClickReplay;
        }
        private void ResumeOnClickSelectMode()
        {
            _gameDirector.StopGame();
            _background.SetActive(true);
            _inGameUIManager.HideAllPanel();
            _menuManager.ChangeGameSettingToModeSelect();
        }
        private void ResumeOnClickReplay()
        {
            _gameDirector.StopGame();
            _background.SetActive(true);
            _inGameUIManager.HideAllPanel();
            _menuManager.ChangeModeSelectToGameSetting();
        }
    }
}
