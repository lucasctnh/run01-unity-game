using UnityEngine;

public class AttachedObstacle : MonoBehaviour {
	[Tooltip("The minimum angle threshold which colliding with obstacles will result in gameover")]
	[SerializeField] private float _angleThreshold = 40f;

	private void OnCollisionEnter(Collision other) {
		if (other.gameObject.CompareTag("Player")) {
			for (int i = 0; i < other.contacts.Length; i++) {
				float currentAngleRefUp = Vector3.Angle(other.contacts[i].normal, Vector3.up);
				float currentAngleRefDown = Vector3.Angle(other.contacts[i].normal, Vector3.down);

				if (currentAngleRefUp <= _angleThreshold || currentAngleRefDown <= _angleThreshold) {
					other.gameObject.GetComponent<PlayerController>().RechargeJumps();
					break;
				}
				else
					GameManager.Instance.GameOver(Sound.Type.Death);
			}
		}
	}
}
