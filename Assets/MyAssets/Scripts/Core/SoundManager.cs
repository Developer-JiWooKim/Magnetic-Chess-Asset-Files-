using UnityEngine;

namespace Assets.MyAssets.Scripts.Core
{
    public sealed class SoundManager : Singleton<SoundManager>
    {
        public enum SfxName
        {
            ButtonPress,
            ChangeTurn,
            DropdownPress,
            MagnetBallSpawn,
            MenuButtonPress,
            Slider,
            Timer,
            SaveButtonPress,
        }

        public enum BgmName
        {
            Title,
            SceneChange,
            Game,
        }

        [SerializeField] private AudioClip[] _bgm = null;
        [SerializeField] private AudioSource _bgmPlayer = null;
        [SerializeField] private AudioClip[] _sfx = null;
        [SerializeField] private AudioSource[] _sfxPlayer = null;

        // const
        private const float DEFAULT_VOLUME = 0.5f;

        void Start() => Setup();
        private void Setup()
        {
            _bgmPlayer.playOnAwake = true;
            _bgmPlayer.loop = true;
            PlayBGM(BgmName.Title);

            SetVolumeBGM(DataManager.Instance.data.volumeBgm);
            SetVolumeSFX(DataManager.Instance.data.volumeSfx);
        }

        public void SetDefaultVolume()
        {
            SetVolumeBGM(DEFAULT_VOLUME);
            SetVolumeSFX(DEFAULT_VOLUME);
        }

        public void PlayBGM(BgmName bgmName)
        {
            _bgmPlayer.clip = _bgm[(int)bgmName];
            _bgmPlayer.Play();
        }

        public void SetVolumeBGM(float volume)
        {
            _bgmPlayer.volume = volume;
        }

        public void SetVolumeSFX(float volume)
        {
            for (int i = 0; i < _sfxPlayer.Length; i++)
            {
                _sfxPlayer[i].volume = volume;
            }
        }

        public void StopBGM()
        {
            _bgmPlayer.Stop();
        }

        public void PlaySFX(SfxName sfxName)
        {
            for (int j = 0; j < _sfxPlayer.Length; j++)
            {

                if (!_sfxPlayer[j].isPlaying)
                {
                    _sfxPlayer[j].clip = _sfx[(int)sfxName];
                    _sfxPlayer[j].Play();
                    return;
                }
            }
            Debug.Log("All SFX Player is Playing!!");
        }
    }
}
