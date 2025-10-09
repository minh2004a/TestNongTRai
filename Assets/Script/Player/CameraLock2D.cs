using UnityEngine;

public class CameraLock2D : MonoBehaviour
{
    [SerializeField] Transform target;   // kéo Player vào đây
    [SerializeField] Vector2 offset = Vector2.zero; // lệch khung nếu cần

    void LateUpdate()
    {
        if (!target) return;
        Vector3 p = target.position;
        transform.position = new Vector3(p.x + offset.x, p.y + offset.y, -10f);
    }
}
