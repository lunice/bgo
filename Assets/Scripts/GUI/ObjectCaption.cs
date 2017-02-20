using UnityEngine;
using System.Collections;

public class ObjectCaption : MonoBehaviour {
	public Transform target;
	public string caption = "";
	public bool useRayCast;
	public Vector3 shift = new Vector3(0,2.0f,0);
	public Vector2 size = new Vector2(200, 25);
	public GUISkin captionGUISkin;
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI() {
		if ( !target ) 
			return;
		Vector3 targetPos = new Vector3(	target.transform.position.x + shift.x, 
											target.transform.position.y + shift.y,
											target.transform.position.z + shift.z );
		
		Vector3 cameraRelative = Camera.main.transform.InverseTransformPoint(target.transform.position);
		if ( cameraRelative.z > 0 ){
			if ( useRayCast ) {
				RaycastHit hit;
				Vector3 rayDirection = target.position - Camera.main.transform.position;
				Ray ray = new Ray(Camera.main.transform.position, rayDirection);
				if ( !Physics.Raycast(ray, out hit) )
					if( hit.distance >= rayDirection.magnitude)
						return;
			}
			Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetPos);
			if ( captionGUISkin )
				GUI.Label(new Rect(	screenPosition.x - size.x * 0.5f, Screen.height - screenPosition.y - size.y * 0.5f, size.x, size.y ), caption,captionGUISkin.label);
			else
				GUI.Label(new Rect(	screenPosition.x - size.x * 0.5f, Screen.height - screenPosition.y - size.y * 0.5f, size.x, size.y ), caption);
		}
	}
}
