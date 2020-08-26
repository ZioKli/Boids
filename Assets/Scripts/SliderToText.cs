using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderToText : MonoBehaviour
{
    TMP_Text textComponent;
    public string title;

    private void Start() {
        textComponent = GetComponent<TMP_Text>();
        if(textComponent == null) {
            textComponent = GetComponentInChildren<TMP_Text>(); 
        }
    }

    public void DisplaySliderValue(float sliderValue) {
        textComponent.text = title + " : " + sliderValue;
    }
}
