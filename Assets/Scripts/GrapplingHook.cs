using System.Collections;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Transform player;
    public Transform hookTip;
    public Transform hookTipMount;
    public float speed;
    public float distance = 30;
    public LineRenderer rope;

    bool stopFlying = false;

    public bool IsFlying => !stopFlying;

    public void Start()
    {
        rope.transform.parent = player.parent;
        rope.transform.position = new Vector3(0,0,0);
        rope.transform.rotation = Quaternion.Euler(0,0,0);
        rope.SetPositions(new Vector3[]{
            hookTipMount.position,
            hookTip.position,
        });
    }

    public void Update()
    {
        rope.SetPosition(0, hookTipMount.position);
        rope.SetPosition(1, hookTip.position);
    }

    public IEnumerator FlyForward(PlayerController controller)
    {
        stopFlying = false;
        hookTip.parent = player.parent;

        while (!stopFlying)
        {
            hookTip.position += speed * Time.deltaTime * hookTip.forward;
            yield return null;

            if (Vector3.Distance(hookTip.position, hookTipMount.position) > distance)
            {
                yield return FlyBack(controller);
                hookTip.parent = hookTipMount;
                yield break;
            }
        }

        hookTip.parent = hookTipMount;
        controller.GrappleOk();
    }

    public IEnumerator FlyBack(PlayerController controller)
    {
        var start = hookTip.position;
        var finish = hookTipMount.position;

        var time = 0f;
        const float TotalTime = 0.2f;
        while (time < TotalTime)
        {
            hookTip.position = Vector3.Lerp(start, finish, time / TotalTime);
            time += Time.deltaTime;
            yield return null;
        }
        hookTip.position = finish;
        controller.GrappleFailed();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger enter {other.gameObject.name}");
        stopFlying = true;
    }
}
