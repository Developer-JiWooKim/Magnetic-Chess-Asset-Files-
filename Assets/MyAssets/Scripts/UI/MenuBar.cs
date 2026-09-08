using System.Collections;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class MenuBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private MenuList _menu;
        [SerializeField] private float _sizeUpSpeed = 1000f;
        [SerializeField] private bool _isShowList = false;

        private struct MenuBarSize
        {
            public float beforeX;
            public float afterX;
        }

        private MenuBarSize _titleMenuBarSize;
        private MenuBarSize _gameMenuBarSize;

        // consts
        private const float TITLE_BEFORE_X = 130f;
        private const float TITLE_AFTER_X = 365f;

        private const float GAME_BEFORE_X = 130f;
        private const float GAME_AFTER_X = 470f;

        private Coroutine _currCor = null;

        private void Start() => Setup();
        private void Setup()
        {
            _titleMenuBarSize = new MenuBarSize
            {
                beforeX = TITLE_BEFORE_X,
                afterX = TITLE_AFTER_X
            };

            _gameMenuBarSize = new MenuBarSize
            {
                beforeX = GAME_BEFORE_X,
                afterX = GAME_AFTER_X
            };

            _background.GetComponent<RectTransform>().sizeDelta =
                new Vector2(TITLE_BEFORE_X, _background.GetComponent<RectTransform>().sizeDelta.y);
        }

        private IEnumerator IncreaseBar(float beforeX, float afterX)
        {
            _background.sizeDelta = new Vector2(beforeX, _background.sizeDelta.y);

            while (_background.sizeDelta.x <= afterX)
            {
                _background.sizeDelta = new Vector2(_background.sizeDelta.x + Time.deltaTime * _sizeUpSpeed, _background.sizeDelta.y);
                yield return null;
            }
            _background.sizeDelta = new Vector2(afterX, _background.sizeDelta.y);
            EndAnimation();
        }

        private IEnumerator DecreaseBar(float beforeX, float afterX)
        {
            _background.sizeDelta = new Vector2(afterX, _background.sizeDelta.y);
            EndAnimation();
            while (_background.sizeDelta.x >= beforeX)
            {
                _background.sizeDelta = new Vector2(_background.sizeDelta.x - Time.deltaTime * _sizeUpSpeed, _background.sizeDelta.y);
                yield return null;
            }
            _background.sizeDelta = new Vector3(beforeX, _background.sizeDelta.y);
        }

        private void EndAnimation()
        {
            _isShowList = !_isShowList;
            _menu.OnClickListButton();
        }
        public void OnClickMenuListButton()
        {
            if (_currCor != null)
            {
                StopCoroutine(_currCor);
            }

            DontDestroyMenu.SceneName curScene = DontDestroyMenu.Instance.CurrentScene;

            MenuBarSize currentScene =
                curScene == DontDestroyMenu.SceneName.Title ? _titleMenuBarSize : _gameMenuBarSize;

            if (_isShowList)
            {
                _currCor = StartCoroutine(DecreaseBar(currentScene.beforeX, currentScene.afterX));
            }
            else
            {
                _currCor = StartCoroutine(IncreaseBar(currentScene.beforeX, currentScene.afterX));
            }
        }
        public void PlaySoundMenuButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.MenuButtonPress);
        }
        public void PlaySoundButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.ButtonPress);
        }
    }
}
