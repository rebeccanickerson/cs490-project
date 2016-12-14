using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class PlantManager : MonoBehaviour {
	public Plant plant;
	public int numberPlants;
	public float radius;
	private List<GameObject> allPlantsList;
	public string branchMaterial;
	public string leafMaterial;
	public CFGRuleGenerator ruleGenerator;
	private PlantContainer pc;

	void Awake () {
		allPlantsList = new List<GameObject>();
		ruleGenerator.Init ();
	}

	void Start () {
		pc = new PlantContainer();
		if (!NewGame.isNewGame) {
			pc = PlantContainer.Load(Path.Combine(Application.persistentDataPath, "plantData.xml"));
			foreach (SerializablePlant sp in pc.Plants) {
				GameObject newPlant = new GameObject ();
				newPlant.gameObject.AddComponent<Plant> ();
				sp.ConvertSPToPlant (newPlant.gameObject.GetComponent<Plant> ());
				allPlantsList.Add (newPlant);
				newPlant.gameObject.GetComponent<Plant> ().UpdateIterations ();
				newPlant.gameObject.GetComponent<Plant> ().gameObject.GetComponent<LSystem> ().InitPlant (newPlant.gameObject.GetComponent<Plant> ());
				newPlant.gameObject.AddComponent <BoxCollider> ();
				newPlant.GetComponent<BoxCollider> ().enabled = true;
				newPlant.GetComponent<BoxCollider> ().center = newPlant.gameObject.GetComponent<Plant>().location + new Vector3 (0, 2, 0);
				newPlant.GetComponent<BoxCollider> ().size = new Vector3 (1, 4, 1);
			}
			return;
		}
		// Otherwise, don't do anything.
	}

	void printV3(Vector3 v) {
		Debug.Log("(" + v.x + ", " + v.y + ", " + v.z + ")");
	}

	public List<Plant> GetAllPlants () {
		return (this.allPlantsList.ConvertAll (x => x.gameObject.GetComponent<Plant> ()));
	}

	public void AddNewPlant (Vector3 point) {
		//Debug.Log ("add plants to " + point);

		GameObject newPlant = new GameObject ();
		newPlant.gameObject.AddComponent<Plant> ();
		newPlant.gameObject.GetComponent<Plant> ().location = point;
		allPlantsList.Add(newPlant);

		GameObject seed = (GameObject) Instantiate(Resources.Load("Seed"));
		seed.transform.position = point;


		/* All possible node types
		string[] br_intnodes = new string[] { "A", "B", "C", "D", "E" }; // Branching internodes
		string[] draw_intnodes = new string[] { "F", "G", "H", "I", "J", "K" }; // Internodes that are drawn
		string[] branch_types = new string[] { "S", "T", "U", "V", "X" }; // Different types of branches
		string[] delay_nodes = new string[] {}; // Instead of draw_intnodes?  */

		int numNodes = Random.Range(1, 6);
		string[] br_intnodes = new string[numNodes];
		for (int i = 0; i < numNodes; i++) {
			br_intnodes [i] = ((char)(65 + i)).ToString ();
		}

		numNodes = Random.Range(1, 7);
		string[] draw_intnodes = new string[numNodes];
		for (int i = 0; i < numNodes; i++) {
			draw_intnodes [i] = ((char)(70 + i)).ToString ();
		}

		numNodes = Random.Range(1, 5);
		string[] branch_types = new string[numNodes];
		for (int i = 0; i < numNodes; i++) {
			branch_types [i] = ((char)(83 + i)).ToString ();
		}

		ruleGenerator.AddCFGRule ("(BrIn)", br_intnodes);
		ruleGenerator.AddCFGRule ("(DrIn)", draw_intnodes);
		ruleGenerator.AddCFGRule ("(BrTyp)", branch_types);

		SerializableDictionary <char, string> rules = new SerializableDictionary <char, string> ();

		// Must set rules for all internodes.
		string rule;
		for (int j = 0; j < draw_intnodes.Length; j++) {
			rule = ruleGenerator.GenerateRule (draw_intnodes[j][0], 2);
			rules.Add (draw_intnodes [j] [0], rule);
		}
		for (int j = 0; j < br_intnodes.Length; j++) {
			rule = ruleGenerator.GenerateRule (br_intnodes[j][0], 3);
			rules.Add (br_intnodes [j] [0], rule);
		}
		for (int j = 0; j < branch_types.Length; j++) {
			rule = ruleGenerator.GenerateRule (branch_types [j] [0], 3);
			rules.Add (branch_types [j] [0], rule);
		}
		rules.Add ('l', "L"); // Leaves mature over time
		newPlant.gameObject.GetComponent<Plant>().InitNewPlant (
			new Vector3 (point.x, 20, point.z), rules, br_intnodes);

		pc.Plants = new SerializablePlant[this.allPlantsList.Count];
		for (int i = 0; i < this.allPlantsList.Count; i++) {
			GameObject g = this.allPlantsList[i];
			SerializablePlant sp = new SerializablePlant();
			sp.ConvertPlantToSP(g.GetComponent<Plant>());
			pc.Plants[i] = sp;
		}

		pc.Save(Path.Combine(Application.persistentDataPath, "plantData.xml"));

	}
}
