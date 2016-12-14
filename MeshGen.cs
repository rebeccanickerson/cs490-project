using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class MeshGen : MonoBehaviour {

	public int numv; // Number of vertices in the polygon

	public float length;
	public float leafLength;
	public float width;
	public string branchMaterial;
	public Vector3 leafMaterial; //RBG values

	private List<Vector3> vertices;
	private List<int> triangles;
	private List<Vector2> uvs;

	private List<Vector3> leafVertices;
	private List<int> leafTriangles;
	private List<Vector2> leafUvs;
	private List<Vector3> newLeafVs;

	void Awake () {
		vertices = new List<Vector3> ();
		triangles = new List<int> ();
		uvs = new List<Vector2> ();
		leafVertices = new List<Vector3> ();
		leafTriangles = new List<int> ();
		leafUvs = new List<Vector2> ();
	}

	public TurtleState DrawUnit (TurtleState turtleState) {

		Vector3 prevCenter = turtleState.baseCenter;
		Vector3 rotation = turtleState.angle;
		int[] prevTrianglePoints = turtleState.trianglePoints;
		int[] newTrianglePoints = new int[numv];
		float radius = turtleState.radius;

		int size = vertices.Count;

		float xAngle = rotation.x;
		float yAngle = rotation.y;

		Vector3 newCenter = new Vector3 (
			prevCenter.x + length * Mathf.Cos (xAngle) * Mathf.Cos(yAngle),
			prevCenter.y + length * Mathf.Sin (xAngle),
			prevCenter.z + length * Mathf.Cos (xAngle) * Mathf.Sin (yAngle));

		Vector3[] circleVs = new Vector3[numv];

		for (int i = 0; i < numv; i++) {
			circleVs[i] = new Vector3(newCenter.x, 
			    radius * Mathf.Sin(2 * Mathf.PI * i / numv) + newCenter.y,
				radius * Mathf.Cos(2 * Mathf.PI * i / numv) + newCenter.z);
			uvs.Add (new Vector2 (Mathf.Sin(2 * Mathf.PI * i / (numv * 2)), turtleState.uvY));
		}

		Quaternion newRotation = new Quaternion();
		newRotation.eulerAngles = new Vector3(xAngle * 180 / Mathf.PI, yAngle * 180 / Mathf.PI, 0); //ignoring z comp

		Vector3 tempRotation = new Vector3 (0, - yAngle * 180 / Mathf.PI, xAngle * 180 / Mathf.PI);

		for (int i = 0; i < circleVs.Length; i++) {
			circleVs [i] = rotateAroundCenter (circleVs[i], newCenter, tempRotation);
		}

		foreach (Vector3 vert in circleVs) {
			vertices.Add (vert);
		}
		//Add center so we can make triangle at top
		vertices.Add (newCenter);
		uvs.Add (new Vector2 (.5f, .5f));
		for (int i = 0; i < numv; i++) {
			newTrianglePoints[i]= i + size;
			int j = (i == numv - 1 ? 0 : i + 1);
			lock (triangles) {
				triangles.Add (prevTrianglePoints [i]);
				triangles.Add (i + size);
				triangles.Add (prevTrianglePoints [j]);

				triangles.Add (prevTrianglePoints[j]);
				triangles.Add (i + size);
				triangles.Add (j + size);

				triangles.Add (i + size);
				triangles.Add (numv + size); // newCenter
				triangles.Add (j + size);
			}
		}

		return new TurtleState (rotation, newCenter, circleVs, newTrianglePoints, radius, (turtleState.uvY + 1) % 2);
	}
		
	void printV3(Vector3 v) {
		Debug.Log("(" + v.x + ", " + v.y + ", " + v.z + ")");
	}

	Vector3 rotateAroundCenter (Vector3 point, Vector3 center, Vector3 rotation) {
		Vector3 dir = point - center;
		dir = Quaternion.Euler (rotation) * dir;
		return dir + center;
	}

	public void FinishMesh () {
		Mesh mesh = new Mesh ();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray ();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals ();
		GetComponent<Renderer> ().material = (Material) Resources.Load(branchMaterial);
		//Debug.Log ((Material) Resources.Load(branchMaterial));
	}

	public void beginLeaf (TurtleState turtleState) {
		newLeafVs = new List<Vector3> ();
		newLeafVs.Add (turtleState.baseCenter); // TODO - start from edge of branch, not center. will need to modify the turtlestate.baseCenter for this {}
	}

	public void endLeaf () {

		int num = newLeafVs.Count;
		int size = leafVertices.Count;

		Vector3 leafCenter = new Vector3(0, 0, 0);

		int count = 0;
		foreach (Vector3 vert in newLeafVs) {
			leafVertices.Add (vert);
			if (count < num / 2) {
				leafUvs.Add (new Vector2 (count * 2 / (num / 2), 0));
			} else {
				leafUvs.Add (new Vector2 (count * 2 / (num / 2), 1));
			}
			leafCenter += vert;
			count++;
		}
		leafCenter /= num;
		leafVertices.Add (leafCenter);
		leafUvs.Add (new Vector2(.5f, .5f));

		Vector3 altLeafCenter = leafCenter;
		Vector3 a = newLeafVs [0];
		Vector3 b = newLeafVs [1];
		Vector3 c = newLeafVs [2];

		Vector3 normal = Vector3.Cross (b - a, c - a);
		normal /= normal.magnitude;
		altLeafCenter += normal * -0.03f;

		leafVertices.Add (altLeafCenter);
		leafUvs.Add (new Vector2(.5f, .5f));

		int j;
		for (int i = 0; i < num; i++) {
			j = (i == num - 1) ? 0 : i + 1;
			leafTriangles.Add (i + size);
			leafTriangles.Add (j + size);
			leafTriangles.Add (num + size); // center of leaf
			leafTriangles.Add (i + size);
			leafTriangles.Add (num + 1 + size); // other center of leaf
			leafTriangles.Add (j + size);
		}
	}

	public void addLeafVert (TurtleState turtleState) {
		Vector3 center = turtleState.baseCenter;
		Vector3 rotation = turtleState.angle;

		float xAngle = rotation.x;
		float yAngle = rotation.y;

		Vector3 newCenter = new Vector3 (
			center.x + length * Mathf.Cos (xAngle) * Mathf.Cos(yAngle),
			center.y + length * Mathf.Sin (xAngle),
			center.z + length * Mathf.Cos (xAngle) * Mathf.Sin (yAngle));

		newLeafVs.Add (newCenter);
		turtleState.baseCenter = newCenter;
	}

	public void FinishLeafMesh (GameObject childGO) {
		Mesh mesh = new Mesh ();
		childGO.GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = leafVertices.ToArray();
		mesh.uv = leafUvs.ToArray ();
		mesh.triangles = leafTriangles.ToArray();
		Material leafmat = new Material ((Material) Resources.Load("LeafDefault"));
		leafmat.EnableKeyword ("_EMISSION");
		leafmat.SetColor ("_Color", new Color (leafMaterial.x, leafMaterial.y, leafMaterial.z));
		leafmat.SetColor ("_EmissionColor", new Color (Math.Max(leafMaterial.x - 0.35f, 0.2f), Math.Max(leafMaterial.y - 0.35f, 0.2f), leafMaterial.z));
		mesh.RecalculateNormals ();
		childGO.GetComponent<Renderer> ().material = leafmat;//(Material) Resources.Load(leafMaterial);
	}

	public Vector3[] InitBaseMesh(float radius, Vector3 location, string bm, Vector3 lm) {

		this.branchMaterial = bm;
		this.leafMaterial = lm;

		Vector3[] circleVs = new Vector3[numv];
		for (int i = 0; i < numv; i++) {
			circleVs[i] = new Vector3(radius * Mathf.Cos(2 * Mathf.PI * i / numv) + location.x,
				location.y,
				radius * Mathf.Sin(2 * Mathf.PI * i / numv) + location.z);
			vertices.Add (circleVs[i]);
		}
		uvs.Add (new Vector2 (0, 0));
		uvs.Add (new Vector2 (0, 1));
		uvs.Add (new Vector2 (1, 0));
		uvs.Add (new Vector2 (1, 1));
		return circleVs;
	}
}
