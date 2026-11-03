using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ManagerGame : MonoBehaviour
{
    public bool timerRunning;
    public TextMeshProUGUI textTimer;
    public TextMeshProUGUI bestTime;
    public Canvas canvas;
    public float countTime = 0f;
    // Update is called once per frame
    void Start()
    {
        if (PlayerPrefs.HasKey("Volume"))
        {
            canvas.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("Volume");
        }
        if (timerRunning)
        {
            LoadGame();
            textTimer.text = DisplayTime(countTime);
        }
    }
    void Update()
    {
        if (timerRunning)
        {
            countTime += Time.deltaTime;
            textTimer.text = DisplayTime(countTime);
        }
    }
    public void LoadScene(int number)
    {
        SceneManager.LoadScene(number);
    }
    public void QuitGame()
    {
        timerRunning = false;
        Application.Quit();
    }
    string DisplayTime(float _time)
    {
        string timeText;
        float min = Mathf.FloorToInt(_time / 60);
        float sec = Mathf.FloorToInt(_time % 60);

        timeText = string.Format("{0:00}:{1:00}", min, sec);
        return timeText;
    }
    public void LoadGame()
    {
        if (File.Exists(UnityEngine.Application.persistentDataPath + "/gamesave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(UnityEngine.Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();
            bestTime.text = string.Format("ксвьее бпелъ: {0:00}:{1:00}", save.minutes, save.seconds);
            Debug.Log("Game Loaded");
        }
        else
        {
            bestTime.text = "";
            Debug.Log("No game saved!");
        }
    }
}
