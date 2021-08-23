using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform camPos;

    void Update()
    {
        transform.position = camPos.position;
    }
}
