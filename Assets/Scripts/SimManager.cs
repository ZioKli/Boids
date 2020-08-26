using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SimManager : MonoBehaviour{
    SimSettings settings;
    FlockManager flock;
    bool simIsRunning = false;
    public float defaultSpeed, defaultViewRange, defaultViewAngle, defaultCount, defaultQueryInterval;
    public GameObject pauseMenu;
    public void Quit() {
        Application.Quit();
    }

    private void Start() {
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(pauseMenu);
        settings = new SimSettings() {
            boidCount = Mathf.FloorToInt(defaultCount),
            viewRange = defaultViewRange,
            viewAngle = defaultViewAngle,
            speed = defaultSpeed,
            queryInterval = Mathf.FloorToInt(defaultQueryInterval)
        };
    }

    public void BeginSim() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        flock = GameObject.FindGameObjectWithTag("Flock").GetComponent<FlockManager>();
        if(flock != null) {
            flock.BeginSim(settings);
            simIsRunning = true;
        }
        Cursor.visible = false;
    }
    
    public void TogglePauseMenu(InputAction.CallbackContext context) {
        if (simIsRunning) {
            if (context.started) {
                pauseMenu.SetActive(!pauseMenu.activeSelf);
                Cursor.visible = pauseMenu.activeSelf;
                if (pauseMenu.activeSelf) {
                    Time.timeScale = 0;
                }
                else {
                    Time.timeScale = 1;
                }
            }
            
        }
    }

    #region setters
    public void SetBoidCount(float sliderVal) {
        settings.boidCount = Mathf.FloorToInt(sliderVal);
    }

    public void SetQueryInterval(float sliderVal) {
        settings.queryInterval = Mathf.FloorToInt(sliderVal);
    }

    public void SetViewAngle (float sliderVal) {
        settings.viewAngle = sliderVal;
    }

    public void SetBoidSpeed(float sliderVal) {
        settings.speed = sliderVal;
    }

    public void SetViewRange(float sliderVal) {
        settings.viewRange = sliderVal;
    }

    #endregion

}

public struct SimSettings{
    public int boidCount;
    public float viewRange;
    public float viewAngle;
    public float speed;
    public int queryInterval;
}