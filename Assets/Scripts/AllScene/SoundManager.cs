using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
    private static SoundManager instance;
    public static SoundManager Instance 
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("Sound Manager instance is null!!");
                return null;
            }

            return instance;
        }
    }

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

    [SerializeField]
    private AudioClip[] bgm = null;
    [SerializeField]
    private AudioClip[] sfx = null;
    
    [SerializeField]
    private AudioSource bgm_Player = null;
    [SerializeField]
    private AudioSource[] sfx_Player = null;

    private const float __DEFAULT_VOLUME_VALUE = 0.5f;

    private void Awake()
    {
        SingletonSetup();
    }
    void Start()
    {
        Setup();
    }
    /// <summary>
    /// 싱글톤 처리하는 함수
    /// </summary>
    private void SingletonSetup()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Setup()
    {
        // 사운드 플레이어 설정, 타이틀 BGM재생
        {
            bgm_Player.playOnAwake = true;
            bgm_Player.loop = true;
            Play_BGM(E_BGM_Name.TITLE);

            SetVolume_BGM(DataManager.Instance.data.volume_value_BGM);
            SetVolume_SFX(DataManager.Instance.data.volume_value_SFX);
        }
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
            // SFX 플레이어 중 재생 중이지 않은 AudioSource를 발견하면
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
