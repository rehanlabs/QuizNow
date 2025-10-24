using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

public class generateQuestions : MonoBehaviourPunCallbacks
{
	public string[] items;
	public Text question;
	public Button optionA;
	public Button optionB;
	public Button optionC;
	public Button optionD;
	public Text optionAText;
	public Text optionBText;
	public Text optionCText;
	public Text optionDText;
	public int questionCounter=0;
	public int totalScore = 0;
	public Text player1;
	public Text player2;
	public float timeRemaining = 30;
	public TMP_Text timer;
	public TMP_Text questionNumber;
	public Button leaveRoom;
	void Awake()
    {
		PhotonNetwork.AutomaticallySyncScene = true;
	}
	IEnumerator getQuestion()
    {
		redo:
		WWW itemsData = new WWW("https://strixplasma.com/quizgame/getQuestionSingle.php");
		yield return itemsData;
		string itemsDataString = itemsData.text;
		//print(itemsDataString);
		items = itemsDataString.Split(';');
		//foreach (Player player in PhotonNetwork.PlayerList)
		//{
		//	ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
		//	playerHash.Add("score", 0);
		//	//player.SetCustomProperties(new )
		//	//PhotonHashTable setRace = new PhotonHashTable();
		//	//setRace.Add("race", tmp_race_float);
		//	player.SetCustomProperties(playerHash);
		//}
		//print(items[10]);
		//print(GetDataValue(items[0], "optionD:"));
		if(questionsStorage.questions.Contains(GetDataValue(items[0], "question:")))
        {
			goto redo;
        }
		questionsStorage.id.Add(GetDataValue(items[0], "id:"));
		questionsStorage.questions.Add(GetDataValue(items[0], "question:"));
		questionsStorage.optionA.Add(GetDataValue(items[0], "optionA:"));
		questionsStorage.optionB.Add(GetDataValue(items[0], "optionB:"));
		questionsStorage.optionC.Add(GetDataValue(items[0], "optionC:"));
		questionsStorage.optionD.Add(GetDataValue(items[0], "optionD:"));
		questionsStorage.correctOption.Add(GetDataValue(items[0], "correctAnswer:"));
		questionsStorage.weight.Add(GetDataValue(items[0], "weight:"));
			
		question.text = questionsStorage.questions[questionCounter];
		optionAText.text = questionsStorage.optionA[questionCounter];
		optionBText.text = questionsStorage.optionB[questionCounter];
		optionCText.text = questionsStorage.optionC[questionCounter];
		optionDText.text = questionsStorage.optionD[questionCounter];
		optionA.gameObject.GetComponent<Image>().color = new Color(0.18f, 0.6f, 0.79f);
		optionB.gameObject.GetComponent<Image>().color = new Color(0.18f, 0.6f, 0.79f);
		optionC.gameObject.GetComponent<Image>().color = new Color(0.18f, 0.6f, 0.79f);
		optionD.gameObject.GetComponent<Image>().color = new Color(0.18f, 0.6f, 0.79f);
		//optionA.onClick.AddListener(changeQuestionA);
		//optionB.onClick.AddListener(changeQuestionB);
		//optionC.onClick.AddListener(changeQuestionC);
		//optionD.onClick.AddListener(changeQuestionD);
		questionNumber.text=(questionCounter+1)+"";
		updateScores();
		

		player1.text = PhotonNetwork.PlayerList[0].NickName;
		player2.text = PhotonNetwork.PlayerList[1].NickName;
	}
	IEnumerator Start()
	{

		WWW itemsData = new WWW("https://strixplasma.com/quizgame/getQuestionSingle.php");
		yield return itemsData;
		string itemsDataString = itemsData.text;
		//print(itemsDataString);
		items = itemsDataString.Split(';');
		foreach (Player player in PhotonNetwork.PlayerList)
		{
			ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
			playerHash.Add("score", 0);
			//player.SetCustomProperties(new )
			//PhotonHashTable setRace = new PhotonHashTable();
			//setRace.Add("race", tmp_race_float);
			player.SetCustomProperties(playerHash);
		}
		//print(items[10]);
		//print(GetDataValue(items[0], "optionD:"));

		questionsStorage.id.Add(GetDataValue(items[0], "id:"));
		questionsStorage.questions.Add(GetDataValue(items[0], "question:"));
		questionsStorage.optionA.Add(GetDataValue(items[0], "optionA:"));
		questionsStorage.optionB.Add(GetDataValue(items[0], "optionB:"));
		questionsStorage.optionC.Add(GetDataValue(items[0], "optionC:"));
		questionsStorage.optionD.Add(GetDataValue(items[0], "optionD:"));
		questionsStorage.correctOption.Add(GetDataValue(items[0], "correctAnswer:"));
		questionsStorage.weight.Add(GetDataValue(items[0], "weight:"));

		question.text = questionsStorage.questions[0];
		optionAText.text = questionsStorage.optionA[0];
		optionBText.text = questionsStorage.optionB[0];
		optionCText.text = questionsStorage.optionC[0];
		optionDText.text = questionsStorage.optionD[0];

		optionA.onClick.AddListener(changeQuestionA);
		optionB.onClick.AddListener(changeQuestionB);
		optionC.onClick.AddListener(changeQuestionC);
		optionD.onClick.AddListener(changeQuestionD);
		questionNumber.text = (questionCounter + 1) + "";
		updateScores();
		player1.text = PhotonNetwork.PlayerList[0].NickName;
		player2.text = PhotonNetwork.PlayerList[1].NickName;
		//question.text = questionsStorage.questions[0];
		//print(questionsStorage.optionA.Count);
	}
	public void changeQuestionA()
    {
		print(questionCounter);
		print(questionsStorage.correctOption.Count);
		if(questionsStorage.correctOption[questionCounter] =="A")
        {
			totalScore = totalScore + int.Parse(questionsStorage.weight[questionCounter]);
			updateScores();
			optionA.gameObject.GetComponent<Image>().color = Color.green;
			
			//Hashtable hash = new Hashtable();

			//hash.Add("score", totalScore);
			//PhotonNetwork.PlayerList[0].SetCustomProperties(hash);

		}
        else
        {
			optionA.gameObject.GetComponent<Image>().color = Color.red;
		}
		questionCounter = questionCounter + 1;
		//getQuestion();
		StartCoroutine("getQuestion");
		//if (questionCounter>=10)
  //      {
		//	return;
		//	//SceneManager.LoadScene("scoreScreen", LoadSceneMode.Single);
		//	//PhotonNetwork.LoadLevel("scoreScreen");
		//}
		//question.text = questionsStorage.questions[questionCounter];
		//optionAText.text = questionsStorage.optionA[questionCounter];
		//optionBText.text = questionsStorage.optionB[questionCounter];
		//optionCText.text = questionsStorage.optionC[questionCounter];
		//optionDText.text = questionsStorage.optionD[questionCounter];
		
	}
	public void changeQuestionB()
	{
		
		if (questionsStorage.correctOption[questionCounter] == "B")
		{
			totalScore = totalScore + int.Parse(questionsStorage.weight[questionCounter]);
			optionB.gameObject.GetComponent<Image>().color = Color.green;
			updateScores();
		}
		else
		{
			optionB.gameObject.GetComponent<Image>().color = Color.red;
			
		}
		questionCounter = questionCounter + 1;
		//getQuestion();
		StartCoroutine("getQuestion");
		//if (questionCounter >= 10)
		//{
		//	return;
		//	//SceneManager.LoadScene("scoreScreen", LoadSceneMode.Single);
		//	//PhotonNetwork.LoadLevel("scoreScreen");
		//}
		//question.text = questionsStorage.questions[questionCounter];
		//optionAText.text = questionsStorage.optionA[questionCounter];
		//optionBText.text = questionsStorage.optionB[questionCounter];
		//optionCText.text = questionsStorage.optionC[questionCounter];
		//optionDText.text = questionsStorage.optionD[questionCounter];

	}
	public void changeQuestionC()
	{
		
		if (questionsStorage.correctOption[questionCounter] == "C")
		{
			totalScore = totalScore + int.Parse(questionsStorage.weight[questionCounter]);
			optionC.gameObject.GetComponent<Image>().color = Color.green;
			updateScores();
		}
		else
		{
			
			optionC.gameObject.GetComponent<Image>().color = Color.red;
		}
		questionCounter = questionCounter + 1;
		//getQuestion();
		StartCoroutine("getQuestion");
		//if (questionCounter >= 10)
		//{
		//	return;
		//	//PhotonNetwork.LoadLevel("scoreScreen");
		//}
		//question.text = questionsStorage.questions[questionCounter];
		//optionAText.text = questionsStorage.optionA[questionCounter];
		//optionBText.text = questionsStorage.optionB[questionCounter];
		//optionCText.text = questionsStorage.optionC[questionCounter];
		//optionDText.text = questionsStorage.optionD[questionCounter];

	}
	public void changeQuestionD()
	{
		
		if (questionsStorage.correctOption[questionCounter] == "D")
		{
			totalScore = totalScore + int.Parse(questionsStorage.weight[questionCounter]);
			optionD.gameObject.GetComponent<Image>().color = Color.green;
			updateScores();
			
		}
		else
		{
			optionD.gameObject.GetComponent<Image>().color = Color.red;
			
		}
		questionCounter = questionCounter + 1;
		//getQuestion();
		StartCoroutine("getQuestion");
		//if (questionCounter >= 10)
		//{
		//	return;
		//	//PhotonNetwork.LoadLevel("scoreScreen");
		//}
		//question.text = questionsStorage.questions[questionCounter];
		//optionAText.text = questionsStorage.optionA[questionCounter];
		//optionBText.text = questionsStorage.optionB[questionCounter];
		//optionCText.text = questionsStorage.optionC[questionCounter];
		//optionDText.text = questionsStorage.optionD[questionCounter];

	}

	string GetDataValue(string data, string index)
	{
		string value = data.Substring(data.IndexOf(index) + index.Length);
		if (value.Contains("|")) value = value.Remove(value.IndexOf("|"));
		return value;
	}
	public void updateScores()
	{
        ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
        playerHash.Add("score", totalScore);
		playerHash.Add("timer", timeRemaining);
		playerHash.Add("done", false);
		PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        //if (PhotonNetwork.LocalPlayer.IsMasterClient)
        //      {
        //          player1.text = PhotonNetwork.PlayerList[0].NickName + ":";
        //          player1.text+= PhotonNetwork.LocalPlayer.CustomProperties["score"].ToString();
        //	player2.text = PhotonNetwork.PlayerList[1].NickName + ":" + (string)PhotonNetwork.PlayerList[1].CustomProperties["score"].ToString();
        //}
        //      else
        //      {
        //	player2.text = PhotonNetwork.PlayerList[1].NickName + ":" + (string)PhotonNetwork.LocalPlayer.CustomProperties["score"].ToString();
        //	player1.text = PhotonNetwork.PlayerList[0].NickName + ":" + (string)PhotonNetwork.PlayerList[0].CustomProperties["score"].ToString();
        //	//PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        //}
    }
	public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable playerHash)
	{
		if (PhotonNetwork.PlayerList.Length == 1)
		{
			PhotonNetwork.LoadLevel("scoreScreen");
		}
		//ExitGames.Client.Photon.Hashtable playerHash1 = new ExitGames.Client.Photon.Hashtable();
		//playerHash.Add("score", totalScore);
		//PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash1);
		//target.SetCustomProperties(playerHash);
		try {
			player2.text = PhotonNetwork.PlayerList[1].NickName + ":" + (string)PhotonNetwork.PlayerList[1].CustomProperties["score"].ToString();
			player1.text = PhotonNetwork.PlayerList[0].NickName + ":" + (string)PhotonNetwork.PlayerList[0].CustomProperties["score"].ToString();
		}
		catch(Exception e)
        {

        }
		if((float)PhotonNetwork.PlayerList[0].CustomProperties["timer"]<= 0 && (int)PhotonNetwork.PlayerList[0].CustomProperties["score"] == (int)PhotonNetwork.PlayerList[1].CustomProperties["score"])
        {
			timeRemaining = 10f;
			updateScores();

		}
		if ((bool)PhotonNetwork.PlayerList[0].CustomProperties["done"] && (bool)PhotonNetwork.PlayerList[1].CustomProperties["done"])
		{
			PhotonNetwork.LoadLevel("scoreScreen");
		}



	}
	void Update()
    {
		
		if (timeRemaining > 0)
		{
			timeRemaining -= Time.deltaTime;
			//if(questionCounter < 10)
			//{
			ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
			playerHash.Add("score", totalScore);
			playerHash.Add("timer", timeRemaining);
			playerHash.Add("done", false);
			PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
			timer.text = timeRemaining.ToString("0.00") + " Seconds Remaining";
			//}
   //         else
   //         {
			//	timer.text = "Please wait for Other player to finish his questions";
			//	optionA.enabled = false;
			//	optionB.enabled = false;
			//	optionC.enabled = false;
			//	optionD.enabled = false;
			//	ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
			//	playerHash.Add("score", totalScore);
			//	playerHash.Add("timer", timeRemaining);
			//	playerHash.Add("done", true);
			//	PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
			//}
			

		}
        else
        {
			ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
			playerHash.Add("score", totalScore);
			playerHash.Add("timer", timeRemaining);
			playerHash.Add("done", true);
			PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);

		}
	}
	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.CloseConnection(PhotonNetwork.LocalPlayer);
		SceneManager.LoadScene("homeScreen");
	}
}
