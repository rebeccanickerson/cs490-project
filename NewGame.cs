using UnityEngine;
using System.Collections;

public class NewGame : MonoBehaviour {

	static public bool isNewGame;

	public void NewGameButton(bool newGame)
	{
		isNewGame = newGame;
		Application.LoadLevel("Small Garden");
	}
}
