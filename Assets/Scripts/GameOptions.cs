using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOptions : MonoBehaviour
{
    public static GameOptions instance = null;

    [SerializeField] TMP_Text timeStepText;
    [SerializeField] Slider timeStepSlider;

    Dictionary<int, float> timeStepValues =
        new Dictionary<int, float>();
    
    Dictionary<float, int> timeStepSliderValues =
        new Dictionary<float, int>();

    void Awake()
    {
        EnforceSingleton();
        InitializeDicts();
    }

    void InitializeDicts()
    {
        timeStepValues.Add(1, 0.01f);
        timeStepValues.Add(2, 0.02f);
        timeStepValues.Add(3, 0.03f);
        timeStepValues.Add(4, 0.04f);
        timeStepValues.Add(5, 0.05f);
        timeStepValues.Add(6, 0.06f);
        timeStepValues.Add(7, 0.07f);
        timeStepValues.Add(8, 0.08f);
        timeStepValues.Add(9, 0.09f);
        timeStepValues.Add(10, 0.10f);
        timeStepValues.Add(11, 0.11f);
        timeStepValues.Add(12, 0.12f);

        timeStepSliderValues.Add(0.01f, 1);
        timeStepSliderValues.Add(0.02f, 2);
        timeStepSliderValues.Add(0.03f, 3);
        timeStepSliderValues.Add(0.04f, 4);
        timeStepSliderValues.Add(0.05f, 5);
        timeStepSliderValues.Add(0.06f, 6);
        timeStepSliderValues.Add(0.07f, 7);
        timeStepSliderValues.Add(0.08f, 8);
        timeStepSliderValues.Add(0.09f, 9);
        timeStepSliderValues.Add(0.10f, 10);
        timeStepSliderValues.Add(0.11f, 11);
        timeStepSliderValues.Add(0.12f, 12);
    }

    void Start()
    {
        timeStepSlider.value = timeStepSliderValues[Time.fixedDeltaTime];
        timeStepText.text = "Timestep: 0." + timeStepSlider.value.ToString("00") + " seconds";
    }

    public void TimestepSliderChanged(float value)
    {
        Time.fixedDeltaTime = timeStepValues[(int)value];
        timeStepText.text = "Timestep: 0." + value.ToString("00") + " seconds";
    }

    void EnforceSingleton()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
}
