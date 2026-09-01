using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : MonoBehaviour
{
    [SerializeField]
    private Slider slider_BGM;
    [SerializeField]
    private Slider slider_SFX;

    private void Start()
    {
        Setup();
    }
    private void Setup()
    {
        slider_BGM.value = DataManager.Instance.data.volume_value_BGM;
        slider_SFX.value = DataManager.Instance.data.volume_value_SFX;

        SetVolume_BGM();
        SetVolume_SFX();
    }
    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
    public void SetOption()
    {
        DataManager.Instance.SaveGameOptionData();
    }
    public void SetVolume_BGM()
    {
        DataManager.Instance.data.volume_value_BGM = slider_BGM.value;
        SoundManager.Instance.SetVolume_BGM(slider_BGM.value);
    }
    public void SetVolume_SFX()
    {
        DataManager.Instance.data.volume_value_SFX = slider_SFX.value;
        SoundManager.Instance.SetVolume_SFX(slider_SFX.value);
    }
    public void PlaySound_DropDown_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.DROPDOWN_PRESS);
    }
    public void PlaySound_Slider()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.SLIDER);
    }
    public void PlaySound_Save_Button_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.SAVE_BUTTON_PRESS);
    }
}
