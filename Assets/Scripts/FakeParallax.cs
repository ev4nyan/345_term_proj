using UnityEngine;

public class FakeParallax : MonoBehaviour
{
    public float speed = 0.02f;

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * 0.02f;
        transform.localScale = new Vector3(1f + offset, 1f + offset, 1f);
    }
}
