using UnityEngine;

public class Shell : MonoBehaviour
{
    public Transform shell;
    public Collider shellCollider;

    Vector3 startPosition;
    float time;
    PlayerController player;

    void Start()
    {
        startPosition = shell.localPosition;
    }

    void Update()
    {
        if (player != null)
        {
            return;
        }

        time += Time.deltaTime;
        shell.localPosition = startPosition + 0.3f * Mathf.Sin(time) * Vector3.up;
        shell.Rotate(new Vector3(0, Time.deltaTime * 50, 0));
    }

    public void GetGrabbed(Transform transform, PlayerController player)
    {
        this.player = player;
        this.transform.parent = transform;
        shellCollider.enabled = false;
    }

    public void GetRetrieved()
    {
        player.GetShell();
        Destroy(this.gameObject);
    }
}
