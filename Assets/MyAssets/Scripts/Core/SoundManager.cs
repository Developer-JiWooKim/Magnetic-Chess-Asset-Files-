using UnityEngine;

namespace Assets.MyAssets.Scripts.Core
{
    public sealed class SoundManager : Singleton<SoundManager>
    {
        public enum E_SFX_Name
        {
            BUTTON_PRESS,
            CHANGE_TURN,
            DROPDOWN_PRESS,
            MAGNETBALL_SPAWN,
            MENU_BUTTON_PRESS,
            SLIDER,
            TIMER,
            SAVE_BUTTON_PRESS,
        }

        public enum E_BGM_Name
        {
            TITLE,
            SCENE_CHANGE,
            GAME,
        }

        [SerializeField] private AudioClip[] bgm = null;
        [SerializeField] private AudioSource bgm_Player = null;
        [SerializeField] private AudioClip[] sfx = null;
        [SerializeField] private AudioSource[] sfx_Player = null;

        // const
        private const float __DEFAULT_VOLUME_VALUE = 0.5f;

        void Start() => Setup();
        private void Setup()
        {
            bgm_Player.playOnAwake = true;
            bgm_Player.loop = true;
            Play_BGM(E_BGM_Name.TITLE);

            SetVolume_BGM(DataManager.Instance.data.volume_value_BGM);
            SetVolume_SFX(DataManager.Instance.data.volume_value_SFX);
        }

        public void SetDefaultVolume()
        {
            SetVolume_BGM(__DEFAULT_VOLUME_VALUE);
            SetVolume_SFX(__DEFAULT_VOLUME_VALUE);
        }

        public void Play_BGM(E_BGM_Name bgm_Name)
        {
            bgm_Player.clip = bgm[(int)bgm_Name];
            bgm_Player.Play();
        }

        public void SetVolume_BGM(float _volume)
        {
            bgm_Player.volume = _volume;
        }

        public void SetVolume_SFX(float _volume)
        {
            for (int i = 0; i < sfx_Player.Length; i++)
            {
                sfx_Player[i].volume = _volume;
            }
        }

        public void Stop_BGM()
        {
            bgm_Player.Stop();
        }

        public void Play_SFX(E_SFX_Name sfx_Name)
        {
            for (int j = 0; j < sfx_Player.Length; j++)
            {

                if (!sfx_Player[j].isPlaying)
                {
                    sfx_Player[j].clip = sfx[(int)sfx_Name];
                    sfx_Player[j].Play();
                    return;
                }
            }
            Debug.Log("All SFX Player is Playing!!");
        }
    }
}
