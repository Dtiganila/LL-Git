using UnityEngine;
using System.Collections;
public class Player : MonoBehaviour
{
	private bool isGrounded;
	static private float verticalStop = 0.01f;
	private float vel;
	public Vector3 colour;

	void Start(){
		isGrounded = true;
		Screen.lockCursor = true;
		if (GetComponent<NetworkView> ().isMine) {
			GetComponentInChildren<Camera> ().enabled = true;
		} else {
			GetComponentInChildren<Camera> ().enabled = false;
		}
	}

	void Update()
	{
		if (GetComponent<NetworkView>().isMine){
			ChangeColorTo (colour);
			vel = Mathf.Abs(GetComponent<Rigidbody>().velocity.y);
		}
		if (Input.GetKey(KeyCode.Escape))
			Screen.lockCursor = false;
		else
			Screen.lockCursor = true;
		if(Input.GetKeyDown ("f")){
			GetComponent<AudioSource> ().Play ();
		}

	}

	void FixedUpdate(){
		if (GetComponent<NetworkView> ().isMine) {
			InputMovement();
			InputCamera();
		}
	}
	
	[RPC] void ChangeColorTo(Vector3 color)
	{
		GetComponent<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);
		
		if (GetComponent<NetworkView>().isMine)
			GetComponent<NetworkView>().RPC("ChangeColorTo", RPCMode.OthersBuffered, color);
	}
	
	void InputMovement()
	{
		float hor = Input.GetAxis ("Horizontal") * 1200;
		float vir = Input.GetAxis ("Vertical") * 1200;
		float jump = 0f;
		if (isGrounded && vel < verticalStop) {
			jump = Input.GetAxis ("Jump") * 15000;
		} else {
			hor = hor / 4;
			vir = vir / 4;
		}
		GetComponent<Rigidbody> ().AddRelativeForce (new Vector3 (hor, jump, vir) * Time.deltaTime);
	}

	void OnCollisionStay(Collision c){
		isGrounded = true;
	}
	
	void OnCollisionExit(Collision c){
		isGrounded = false;
	}

	void InputCamera(){
		float yaw = Input.GetAxis ("Mouse X") * 200 * Time.deltaTime;
		float pit = Input.GetAxis ("Mouse Y") * -75 * Time.deltaTime;
		GetComponent<Rigidbody> ().rotation = Quaternion.Euler (0, yaw, 0) * GetComponent<Rigidbody> ().rotation;
		GetComponentInChildren<Camera> ().transform.rotation = GetComponentInChildren<Camera> ().transform.rotation * Quaternion.Euler (pit, 0f, 0f);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		if (stream.isWriting)
		{
			syncPosition = GetComponent<Rigidbody>().position;
			stream.Serialize(ref syncPosition);
		}
		else
		{
			stream.Serialize(ref syncPosition);
			GetComponent<Rigidbody>().position = syncPosition;
		}
	}
}