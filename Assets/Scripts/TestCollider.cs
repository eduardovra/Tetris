using UnityEngine;
using System.Collections;

public class TestCollider : MonoBehaviour {

	public float distance;
	public Vector3 point;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;

		Physics.Raycast (transform.position, Vector3.down, out hit);

		Debug.DrawLine(transform.position, hit.point);

		distance = hit.distance;
		point = hit.point;
	}
}
