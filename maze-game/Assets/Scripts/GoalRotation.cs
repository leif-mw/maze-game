using UnityEngine;

public class GoalRotation : MonoBehaviour
{
    [SerializeField]
    private GameObject goal;

    // Update is called once per frame
    void Update()
    {
        goal.transform.Rotate(0, 0, 100 * Time.deltaTime);
    }
}
