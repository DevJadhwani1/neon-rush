using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool isTriggered = false;

    void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
            box.size = new Vector3(14f, 8f, 4f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // Check if vehicle or player passed through
        if (other.CompareTag("Player") || other.name.Contains("Vehicle") || other.transform.root.name.Contains("Vehicle"))
        {
            isTriggered = true;
            Debug.Log("Goal triggered by: " + other.name);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCourseCompleted();
            }
        }
    }
}
