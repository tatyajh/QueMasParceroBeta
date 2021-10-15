using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour {

    public Text empanadaText, scoreText, maxScoreText;
    private PlayerController controller;

	// Use this for initialization
	void Start () {
        controller = GameObject.Find("Player").GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
        if(GameManager.sharedInstance.currentGameState == GameState.inGame){
            int empanadas = GameManager.sharedInstance.collectedObject;
            float score = controller.GetTravelledDistance();
            float maxScore = PlayerPrefs.GetFloat("maxscore", 0);

            empanadaText.text = empanadas.ToString();
            scoreText.text = "Score: " + score.ToString("f1");
            maxScoreText.text = "MaxScore: " + maxScore.ToString("f1");
        }
	}
}
