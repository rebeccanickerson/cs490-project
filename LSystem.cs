using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class TurtleState {
	public Vector3 angle; // angle of growth
	public Vector3 baseCenter; //center of the polygon to grow off of
	public Vector3[] baseVertices; //vertices of the polygon to grow off of
	public int[] trianglePoints; //indices of baseVertices in MeshGen.vertices
	public float radius;
	public int uvY;

	public TurtleState (Vector3 angle, Vector3 baseCenter, Vector3[] baseVertices, int[] trianglePoints, float radius, int uvY) {
		this.angle = angle;
		this.baseCenter = baseCenter;
		this.baseVertices = baseVertices;
		this.trianglePoints = trianglePoints;
		this.radius = radius;
		this.uvY = uvY;
	}

	public static TurtleState TurtleFromTurtle(TurtleState turtleState) {
		return new TurtleState(turtleState.angle, turtleState.baseCenter, turtleState.baseVertices, turtleState.trianglePoints, turtleState.radius, turtleState.uvY);
	}
}

public class LSystem : MonoBehaviour {
	private string plantString;
	public Dictionary<char, string> rules = new Dictionary<char, string>();
	private Stack<TurtleState> turtleStack;
	private TurtleState turtleState;
	private MeshGen meshGenerator;
	public float radius;
	private Plant plant;

	public void InitPlant(Plant plant) {
		this.plant = plant;
		this.gameObject.AddComponent <MeshGen> ();
		meshGenerator = this.gameObject.GetComponent<MeshGen> ();
		meshGenerator.gameObject.GetComponent<MeshGen> ().numv = 4; //TODO update this along with the prevVerts in init of the starting turtleState

		DateTime now = System.DateTime.Now;
		System.TimeSpan diff = now - plant.creationTime;
		radius = plant.baseRadius * Mathf.Pow (1.1f, Mathf.Min(plant.maxIteration, (float) Math.Floor(diff.TotalSeconds / plant.increaseInterval))); 
		plantString = plant.startingString;
		meshGenerator.length = plant.unitLength;
		meshGenerator.leafLength = plant.baseLeafSize * Mathf.Pow (1.2f, Mathf.Min(plant.maxIteration, (float) Math.Floor(diff.TotalSeconds / plant.increaseInterval)));

		Vector3[] circleVs = meshGenerator.InitBaseMesh (radius, plant.location, plant.branchMaterial, plant.leafMaterial);

		turtleState = new TurtleState (plant.startingGrowthAngle, plant.location, circleVs, new int[] {0, 1, 2, 3}, radius, 0);
		turtleStack = new Stack<TurtleState> ();

		for (int i = 0; i < plant.iteration; i++) {
			plantString = IterateString (plantString);
		}

		plantString = FinIterateString (plantString);
	    GenPlant (plantString);
	}

	string IterateString (string str) {
		StringBuilder sb = new StringBuilder ();
		foreach (char c in str) {
			if (plant.rules.ContainsKey (c)) {
				sb.Append (plant.rules [c]);
			} else {
				sb.Append (c);
			}
		}
		return sb.ToString ();
	}

	string FinIterateString (string str) {
		StringBuilder sb = new StringBuilder ();
		foreach (char c in str) {
			if (plant.finRules.ContainsKey (c)) {
				sb.Append (plant.finRules [c]);
			} else {
				sb.Append (c);
			}
		}
		return sb.ToString ();
	}

	void GenPlant(string str) {

		foreach (char c in str) {
			switch (c) {
			case 'F':
				turtleState = meshGenerator.DrawUnit (turtleState);
				break;
			case 'f':
				meshGenerator.addLeafVert (turtleState);
				break;
			case '[':
				turtleStack.Push (TurtleState.TurtleFromTurtle(turtleState));
				break;
			case ']':
				turtleState = turtleStack.Pop ();
				break;
			case '&': //increase heading
				turtleState.angle += plant.increaseHeading;
				break;
			case '^': //decrease heading
				turtleState.angle -= plant.decreaseHeading;
				break;
			case '+': //increase pitch
				turtleState.angle += plant.increasePitch;
				break;
			case '-': //decrease pitch
				turtleState.angle -= plant.decreasePitch;
				break;
			case ',': //increase pitch
				turtleState.angle += plant.leafIncreasePitch;
				break;
			case '`': //decrease pitch
				turtleState.angle -= plant.leafDecreasePitch;
				break;
			case '!': //decrease radius of new branches
				turtleState.radius = turtleState.radius * plant.decreaseRadius;
				break;
			case '|':
				turtleState.angle += new Vector3 (Mathf.PI, 0, 0);
				break;
			case '{':
				turtleStack.Push (TurtleState.TurtleFromTurtle(turtleState));
				meshGenerator.beginLeaf (turtleState);
				break;
			case '}':
				meshGenerator.endLeaf ();
				turtleState = turtleStack.Pop ();
				break;
			default:
				// If c within [G,H,I,J,K] range, drawUnit (draw_intnodes rules)
				if (c >= 'G' && c <= 'K') {
					turtleState = meshGenerator.DrawUnit (turtleState);
				}
				break;
			}
		}
		meshGenerator.FinishMesh ();

		foreach (Transform child in transform) {
			if (child.gameObject.tag == "Leaf") {
				meshGenerator.FinishLeafMesh (child.gameObject);
			}
	     }
	}
		
	void printV3(Vector3 v) {
		Debug.Log("(" + v.x + ", " + v.y + ", " + v.z + ")");
	}
}
