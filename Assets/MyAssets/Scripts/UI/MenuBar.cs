using System.Collections;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class MenuBar : MonoBehaviour
    {
        [SerializeField]
        private RectTransform background;
        [SerializeField]
        private MenuList menu;
        [SerializeField]
        private float sizeUpSpeed = 1000f;
        [SerializeField]
        private bool isShowList = false;

        private struct MenuBarSize
        {
            public float beforeX;
            public float afterX;
        }

        private MenuBarSize title_menuBarSize;
        private MenuBarSize game_menuBarSize;

        private const float __TITLE_BEFORE_X = 130f;
        private const float __TITLE_AFTER_X = 365f;

        private const float __GAME_BEFORE_X = 130f;
        private const float __GAME_AFTER_X = 470f;

        private Coroutine currCor = null;

        private void Start()
        {
            Setup();
        }

        private void Setup()
        {
            title_menuBarSize = new MenuBarSize();
            title_menuBarSize.beforeX = __TITLE_BEFORE_X;
            title_menuBarSize.afterX = __TITLE_AFTER_X;

            game_menuBarSize = new MenuBarSize();
            game_menuBarSize.beforeX = __GAME_BEFORE_X;
            game_menuBarSize.afterX = __GAME_AFTER_X;

            background.GetComponent<RectTransform>().sizeDelta =
                new Vector2(__TITLE_BEFORE_X, background.GetComponent<RectTransform>().sizeDelta.y);
        }

        private IEnumerator IncreaseBar(float __beforeX, float __afterX)
        {
            background.sizeDelta = new Vector2(__beforeX, background.sizeDelta.y);

            while (background.sizeDelta.x <= __afterX)
            {
                background.sizeDelta = new Vector2(background.sizeDelta.x + Time.deltaTime * sizeUpSpeed, background.sizeDelta.y);
                yield return null;
            }
            background.sizeDelta = new Vector2(__afterX, background.sizeDelta.y);
            EndAnimation();
        }

        private IEnumerator DecreaseBar(float __beforeX, float __afterX)
        {
            background.sizeDelta = new Vector2(__afterX, background.sizeDelta.y);
            EndAnimation();
            while (background.sizeDelta.x >= __beforeX)
            {
                background.sizeDelta = new Vector2(background.sizeDelta.x - Time.deltaTime * sizeUpSpeed, background.sizeDelta.y);
                yield return null;
            }
            background.sizeDelta = new Vector3(__beforeX, background.sizeDelta.y);
        }

        private void EndAnimation()
        {
            isShowList = !isShowList;
            menu.OnClickListButton();
        }
        public void OnClickMenuListButton()
        {
            if (currCor != null)
            {
                StopCoroutine(currCor);
            }

            DontDestroy_Menu.SceneName curScene = DontDestroy_Menu.Instance.CurrentScene;

            MenuBarSize currentScene =
                curScene == DontDestroy_Menu.SceneName.Title ? title_menuBarSize : game_menuBarSize;

            if (isShowList)
            {
                currCor = StartCoroutine(DecreaseBar(currentScene.beforeX, currentScene.afterX));
            }
            else
            {
                currCor = StartCoroutine(IncreaseBar(currentScene.beforeX, currentScene.afterX));
            }
        }
        public void PlaySound_Menu_Button_Press()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MENU_BUTTON_PRESS);
        }
        public void PlaySound_Button_Press()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
        }
    }
}
