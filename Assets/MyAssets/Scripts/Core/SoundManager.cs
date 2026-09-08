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

            // 어떤 곡을 트는지는 씬의 컨트롤러가 정한다(TitleUIController · MatchUIController).
            // 여기서 타이틀 곡을 틀면 타이틀로 "돌아왔을" 때와 처음 켰을 때가 달라진다.

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
                    Debug.Log("사운드 재생");
                    return;
                }
            }
        }
    }
}
