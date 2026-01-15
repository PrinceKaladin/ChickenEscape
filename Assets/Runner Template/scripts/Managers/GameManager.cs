using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    float scoree = 0;
    bool run = false;
    public GameObject Director;
    public Text scoretext;
    public Text bestscoretext;
    public delegate void _delegate();
    public _delegate OnGameStarted, OnGameOver, OnGamePaused;
    public CinemachineCameraOffset CameraOffset;
    public Text megatext;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        bestscoretext.text = "Best Result: "+ PlayerPrefs.GetInt("bestscore",0).ToString();
        instance = this;
    }

    private void Update()
    {
        if (run)
        {
            scoretext.gameObject.SetActive(true);
            scoree+=Time.deltaTime;
            scoretext.text = "Score: " + ((int)scoree).ToString();
            if (PlayerPrefs.GetInt("bestscore", 1) < scoree)
            {
                PlayerPrefs.SetInt("bestscore", ((int)scoree));
            }

        }
    }
    public void OnPlayerDied()
    {
        if(OnGameOver != null)
        {
            scoretext.gameObject.SetActive(false);
            megatext.text = scoretext.text;
            run = false;
            OnGameOver();

        }
    }

    internal void OnPlayButtonPressed()
    {
        if(OnGameStarted != null)
        {
            run = true;
            OnGameStarted();
            StartCoroutine("yi");
        }

        Director.SetActive(true);
    }


    internal void OnRestartButtonPressed()
    {

        SceneManager.LoadScene(0);
    }
   

        IEnumerator yi() {
        while (CameraOffset.m_Offset.x >= -2)
        {
            yield return new WaitForSeconds(0.01f);
            CameraOffset.m_Offset.x -= 0.02f;
            
        }
    }
}
