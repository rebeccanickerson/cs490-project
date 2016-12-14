using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {

	private Vector3[] newVertices;
	private Vector2[] newUV;
	private int[] newTriangles;

	//private Vector3 baseCoordinates = new Vector3 (0, 0, 0);
	public float height;
	public float width;
	public Material material;

	private List<Vector3> vertices;
	private List<int> triangles;
	private List<Vector2> uvs;

	void Initialize () {

	}

	void Awake () {
		vertices = new List<Vector3> ();
		triangles = new List<int> ();
		uvs = new List<Vector2> ();
		print ("finished init");
	}

	public Vector3[] DrawUnit (Vector3 baseCoordinates, Vector3 rotation, Vector3 pivot, float width) {
		newUV = new Vector2[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1),
			new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1),
			new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};

		//print ("base: " + baseCoordinates);

		foreach (Vector2 v in newUV) {
			uvs.Add (v);
		}

		float a = baseCoordinates.x;
		float b = baseCoordinates.y;
		float c = baseCoordinates.z;

		int size = vertices.Count;
		Vector3 center = new Vector3(a, b, c); // Pivot point
		//Vector3 center = pivot; //redundant, i know
		//print ("pivot around: " + center);

		Quaternion newRotation = new Quaternion();
		newRotation.eulerAngles = rotation;


		vertices.Add(newRotation * (new Vector3(a, b, c) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + width/2, b, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + 3 * width / 2, b, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + 2 * width, b, c) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + 3 * width / 2, b, c - Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + width / 2, b, c - Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a, b + height, c) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + width/2, b + height, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + 3 * width / 2, b + height, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + 2 * width, b + height, c) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + 3 * width / 2, b + height, c - Mathf.Sqrt(3) / 2 * width) - center) + center);
		vertices.Add(newRotation * (new Vector3(a + width / 2, b + height, c - Mathf.Sqrt(3) / 2 * width) - center) + center);

		Vector3 topCenter = 
			((newRotation * new Vector3(a, b + height, c) - center) + center) + 
			(newRotation * (new Vector3(a + width/2, b + height, c + Mathf.Sqrt(3) / 2 * width) - center) + center) +
			(newRotation * (new Vector3(a + 3 * width / 2, b + height, c + Mathf.Sqrt(3) / 2 * width) - center) + center) +
			(newRotation * (new Vector3(a + 2 * width, b + height, c) - center) + center) +
			(newRotation * (new Vector3(a + 3 * width / 2, b + height, c - Mathf.Sqrt(3) / 2 * width) - center) + center) +
			(newRotation * (new Vector3(a + width / 2, b + height, c - Mathf.Sqrt(3) / 2 * width) - center) + center);

		/*printV3(newRotation * (new Vector3(a, b, c) - center) + center);
		printV3(newRotation * (new Vector3(a + width/2, b, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a + 3 * width / 2, b, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a + 2 * width, b, c) - center) + center);
		printV3(newRotation * (new Vector3(a + 3 * width / 2, b, c - Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a + width / 2, b, c - Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a, b + height, c) - center) + center);
		printV3(newRotation * (new Vector3(a + width/2, b + height, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a + 3 * width / 2, b + height, c + Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a + 2 * width, b + height, c) - center) + center);
		printV3(newRotation * (new Vector3(a + 3 * width / 2, b + height, c - Mathf.Sqrt(3) / 2 * width) - center) + center);
		printV3(newRotation * (new Vector3(a + width / 2, b + height, c - Mathf.Sqrt(3) / 2 * width) - center) + center);*/


		topCenter /= 6;
		//print (topCenter);



		//print("MG");
		//print (newRotation * (new Vector3(a, b + height, c) - center) + center);

		newTriangles = new int[] { size+0,size+1,size+5, size+1,size+4,size+5, size+1,size+2,size+4, 
			size+2,size+3,size+4, size+6,size+7,size+11, size+7,size+10,size+11, size+7,size+8,size+10,
			size+8,size+9,size+10, size+0,size+6,size+5, size+5,size+6,size+11, size+5,size+11,size+4,
			size+4,size+11,size+10, size+4,size+10,size+3, size+10,size+9,size+3, size+3,size+9,size+2,
			size+2,size+9,size+8, size+2,size+7,size+1, size+2,size+8,size+7, size+1,size+7,size+0,
			size+7,size+6,size+0 };

		foreach (int i in newTriangles) {
			triangles.Add (i);
		}

		// Return the base position for the next unit to start from
		//print( "MG " + (newRotation * (new Vector3 (a, b + height, c) - center) + center));
		return new Vector3[] {
			newRotation * (new Vector3 (a, b + height, c) - center) + center,
			topCenter
		};
	}


	public void FinishMesh () {
		Mesh mesh = new Mesh ();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray ();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals ();
		GetComponent<Renderer> ().material = material;
	}

	// Update is called once per frame
	void Update () {
	    
	}

	void printV3(Vector3 v) {
		print("(" + v.x + ", " + v.y + ", " + v.z + ")");
	}
}
