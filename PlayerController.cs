using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	private Rigidbody rb;
	private Camera cam;
	private GameObject go;
	private float speed;
	public GameObject cube;
	private bool inMotion;
	private bool mouseHold;
	public PlantManager plantManager;
	public GvrReticle reticle;
	private GameObject seed;
	private readonly object _lock = new object();

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		cam = GetComponent<Camera> ();
		go = cam.gameObject;
		speed = 6.0f;
		seed = (GameObject) Instantiate(Resources.Load("Seed"));
		seed.GetComponent<Renderer> ().enabled = false;
		mouseHold = false;
	}
		
	void FixedUpdate () {

		RaycastHit hit;
		Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, cam.nearClipPlane));
		bool intersect = Physics.Raycast (ray, out hit, 3f);
		lock (_lock) {
			if (intersect && withinEpsilon (hit.point.y) && !inMotion && farFromOtherPlants (hit.point) && !mouseHold) {
			// Show option to plant plant
			seed.GetComponent<Renderer> ().enabled = true;
			seed.transform.position = hit.point;
		} else {
			seed.GetComponent<Renderer> ().enabled = false;
		}

		if (Input.GetMouseButton(0)) {
			if (intersect && withinEpsilon(hit.point.y) && !inMotion && farFromOtherPlants(hit.point) && !mouseHold) { 
				// Plant plant using plantManager
				plantManager.AddNewPlant (hit.point);
				inMotion = false;
				seed.GetComponent<Renderer> ().enabled = false;
			} else { // Continue moving
				Vector3 forwardVector = go.transform.forward;
				forwardVector.y = 0;
				rb.velocity = forwardVector * speed;
				inMotion = true;
			}
			mouseHold = true; // Here so we can plant exactly one seed
		}
		else {
			rb.velocity = Vector3.zero;
			inMotion = false;
			mouseHold = false;
		}
		}
		cam.gameObject.transform.position = new Vector3 (cam.gameObject.transform.position.x, 22, cam.gameObject.transform.position.z);
	}

	bool withinEpsilon(float height) {
		float eps = 0.01f;
		if (height > (20f - eps) && height < (20f + eps))
			return true;
		else
			return false;
	}

	bool farFromOtherPlants (Vector3 point) {
		List<Plant> currentPlants = plantManager.GetAllPlants ();
		foreach (Plant plant in currentPlants) {
			Vector3 distVector = plant.location - point;
			if (distVector.magnitude < 3.0f)
				return false;
		}
		return true;
	}
}﻿