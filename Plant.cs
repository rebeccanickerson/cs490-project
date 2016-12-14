using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.EventSystems;

public class Plant : MonoBehaviour {
	public Vector3 location; // Coordinates of the base of the plant
	public float baseRadius; // Radius of the base
	public float increaseInterval; // How long between radius increases in seconds
	public float decreaseRadius; // What to multiply radius by on '!'

	public float unitLength; // Length of one unit.  Scales with baseRadius (?)
	public float baseLeafSize; // Base leaf size.  Scales with baseRadius

	public int iteration; // How many iterations of the L-System to run
	public int maxIteration; // Maximum number of iterations
	public DateTime creationTime; // Time plant was created
	public float iterateInterval; // How long between iterations in seconds

	public Vector3 increaseHeading; // Angle to add on &
	public Vector3 decreaseHeading; // Angle to subtract on ^
	public Vector3 increasePitch; // Angle to add on +
	public Vector3 decreasePitch; // Angle to add on -
	public Vector3 leafIncreasePitch;
	public Vector3 leafDecreasePitch;


	public Vector3 startingGrowthAngle; // Angle growth starts from

	public SerializableDictionary<char, string> rules; // Should include unique leaf & flower rules
	public SerializableDictionary<char, string> finRules; // Expand leaf and flower after iteration is complete

	public string branchMaterial;
	public Vector3 leafMaterial; // RBG values

	public string startingString;

	public GameObject leaves;
	public GameObject flowers;

	public GvrReticle reticle;

	void Awake() {
		this.baseRadius = UnityEngine.Random.Range(0.005f, 0.1f);
		this.baseLeafSize = baseRadius * UnityEngine.Random.Range (0.2f, 0.5f);
		this.decreaseRadius = UnityEngine.Random.Range (0.75f, 0.95f);
		this.unitLength = baseRadius < .1f ? baseRadius * UnityEngine.Random.Range (2f, 5f): baseRadius * UnityEngine.Random.Range(1.1f, 2f);
		this.iteration = 5;
		this.increasePitch = new Vector3(UnityEngine.Random.Range(.01f * Mathf.PI, .1f * Mathf.PI), 0, 0);
		this.decreasePitch = new Vector3(UnityEngine.Random.Range(.01f * Mathf.PI, .1f * Mathf.PI), 0, 0);
		this.increaseHeading = new Vector3 (0, UnityEngine.Random.Range(.01f * Mathf.PI, .1f * Mathf.PI) ,0); // TODO: May want to hold of on this til we know how many &s there are in one branching region
		this.decreaseHeading = new Vector3 (0, UnityEngine.Random.Range(.01f * Mathf.PI, .1f * Mathf.PI) ,0);
		this.leafIncreasePitch = new Vector3(UnityEngine.Random.Range(.05f * Mathf.PI, .1f * Mathf.PI), 0, 0);
		this.leafDecreasePitch = this.leafIncreasePitch;
		this.startingGrowthAngle = new Vector3 (Mathf.PI / 2, 0, 0);
		this.rules = new SerializableDictionary<char, string> ();
		this.finRules = new SerializableDictionary<char, string> ();
		this.gameObject.AddComponent<LSystem>();
		this.gameObject.AddComponent<MeshFilter>();
		this.gameObject.AddComponent<MeshRenderer>();
		this.gameObject.AddComponent<EventTrigger> ();
		this.maxIteration = 6;
		leaves = new GameObject ();
		leaves.transform.parent = this.gameObject.transform;
		leaves.gameObject.AddComponent<MeshFilter>();
		leaves.gameObject.AddComponent<MeshRenderer>();
		leaves.gameObject.tag = "Leaf";
		flowers = new GameObject ();
		flowers.transform.parent = this.gameObject.transform;
		flowers.gameObject.tag = "Flower";
		flowers.gameObject.AddComponent<MeshFilter>();
		flowers.gameObject.AddComponent<MeshRenderer>();
		//this.iterateInterval = 20; // For testing quickly
		//this.increaseInterval = 10; // this.iterateInterval / 2;
		this.iterateInterval = UnityEngine.Random.Range (4500, 100000);
		this.increaseInterval = this.iterateInterval / 2;
	}

	private void GenerateRules() {
		this.GenerateLeafRules ();
		this.GenerateFlowerRules ();
	}

	private void GenerateLeafRules () {
		finRules.Add ('L', "[++{``f,f,f,f,f`|``f,f,f,f}]"); // would draw leaves
		finRules.Add ('l', "[++{`f,f`|`f}]");
	}

	private void GenerateFlowerRules() {
		finRules.Add ('W', "[&&&+++++P--P--P--P]"); // would draw flowers
		finRules.Add ('P', "[{`f,f`|`f}]");
	}

	public void InitNewPlant(Vector3 location, SerializableDictionary<char, string> dict, string[] br_intnodes) {
		this.location = location;
		this.decreasePitch = this.increasePitch;
		int bark = UnityEngine.Random.Range(1, 15);
		this.branchMaterial = "Bark " + bark.ToString ("00") + Path.DirectorySeparatorChar + "Bark pattern " + bark.ToString("00");
		this.leafMaterial = GenerateLeafColor();
		this.rules = dict;
		this.startingString = br_intnodes[UnityEngine.Random.Range (0, br_intnodes.Length)];
		EventTrigger et = this.gameObject.AddComponent<EventTrigger> ();
		EventTrigger.Entry entry = new EventTrigger.Entry ();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener ( (eventData) => { Debug.Log(eventData); });
		et.triggers.Add(entry);
		this.creationTime = System.DateTime.Now;
		this.GenerateLeafRules ();
		this.GenerateFlowerRules ();
	}

	private Vector3 GenerateLeafColor() {
		return new Vector3 (UnityEngine.Random.Range(0.25f, 1.0f), UnityEngine.Random.Range(0.25f, 1.0f), UnityEngine.Random.Range (0.0f, 0.05f));
	}

	public void UpdateIterations () {
		DateTime now = System.DateTime.Now;
		System.TimeSpan diff = now - this.creationTime;
		this.iteration = (int) Math.Min (this.maxIteration, Math.Floor(diff.TotalSeconds / iterateInterval));
	}
}

public class SerializablePlant {
	public Vector3 location; // Coordinates of the base of the plant
	public float baseRadius; // Radius of the base
	public float increaseInterval; // How long between radius increases in seconds
	public float decreaseRadius; // What to multiply radius by on '!'

	public float unitLength; // Length of one unit.  Scales with baseRadius (?)
	public float baseLeafSize; // Base leaf size.  Scales with baseRadius

	public int iteration; // How many iterations of the L-System to run
	public int maxIteration; // Maximum number of iterations
	public DateTime creationTime; // Time plant was created
	public float iterateInterval; // How long between iterations

	public Vector3 increaseHeading; // Angle to add on &
	public Vector3 decreaseHeading; // Angle to subtract on ^
	public Vector3 increasePitch; // Angle to add on +
	public Vector3 decreasePitch; // Angle to add on -
	public Vector3 leafIncreasePitch;
	public Vector3 leafDecreasePitch;


	public Vector3 startingGrowthAngle; // Angle growth starts from

	public SerializableDictionary<char, string> rules; // Should include unique leaf & flower rules
	public SerializableDictionary<char, string> finRules; // Expand leaf and flower after iteration is complete

	// Name of the .mat file to get from Resources
	public string branchMaterial;
	public Vector3 leafMaterial;  // RBG values

	public string startingString;

	public SerializablePlant () {
		
	}

	public void ConvertPlantToSP(Plant plant) {
		this.location = plant.location;
		this.baseRadius = plant.baseRadius;
		this.decreaseRadius = plant.decreaseRadius;
		this.unitLength = plant.unitLength;
		this.baseLeafSize = plant.baseLeafSize;
		this.iteration = plant.iteration;
		this.maxIteration = plant.maxIteration;
		this.increaseHeading = plant.increaseHeading;
		this.decreaseHeading = plant.decreaseHeading;
		this.increasePitch = plant.increasePitch;
		this.decreasePitch = plant.decreasePitch;
		this.leafIncreasePitch = plant.leafIncreasePitch;
		this.leafDecreasePitch = plant.leafDecreasePitch;
		this.startingGrowthAngle = plant.startingGrowthAngle;
		this.rules = plant.rules;
		this.finRules = plant.finRules;
		this.branchMaterial = plant.branchMaterial; // May need to be name?
		this.leafMaterial = plant.leafMaterial; // May need to be name?
		this.startingString = plant.startingString;
		this.creationTime = plant.creationTime;
		this.iterateInterval = plant.iterateInterval;
		this.increaseInterval = plant.increaseInterval;
	}

	public void ConvertSPToPlant(Plant plant) {
		plant.location = this.location;
		plant.baseRadius = this.baseRadius;
		plant.decreaseRadius = this.decreaseRadius;
		plant.unitLength = this.unitLength;
		plant.baseLeafSize = this.baseLeafSize;
		plant.iteration = this.iteration;
		plant.maxIteration = this.maxIteration;
		plant.increaseHeading = this.increaseHeading;
		plant.decreaseHeading = this.decreaseHeading;
		plant.increasePitch = this.increasePitch;
		plant.decreasePitch = this.decreasePitch;
		plant.leafIncreasePitch = this.leafIncreasePitch;
		plant.leafDecreasePitch = this.leafDecreasePitch;
		plant.startingGrowthAngle = this.startingGrowthAngle;
		plant.rules = this.rules;
		plant.finRules = this.finRules;
		plant.branchMaterial = this.branchMaterial; // May need to be name?
		plant.leafMaterial = this.leafMaterial; // May need to be name?
		plant.startingString = this.startingString;
		plant.creationTime = this.creationTime;
		plant.iterateInterval = this.iterateInterval;
		plant.increaseInterval = this.increaseInterval;
	}
}
