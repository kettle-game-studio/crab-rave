using System.Collections;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Transform player;
    public Transform hookTip;
    public Transform hookTipMount;
    public float speed;
    public float distance = 30;
    public LineRenderer rope;
    public ParticleSystem hookBubbles;

    bool stopFlying = false;
    Shell shell;

    public bool IsFlying => !stopFlying;
    public Vector3 TipPosition => hookTip.transform.position;
    public AudioSource catchAudio;
    public AudioSource wroomAudio;

    public void Start()
    {
        rope.transform.parent = player.parent;
        rope.transform.position = new Vector3(0, 0, 0);
        rope.transform.rotation = Quaternion.Euler(0, 0, 0);
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
        hookBubbles.Play();

        while (!stopFlying)
        {
            hookTip.position += speed * Time.deltaTime * hookTip.forward;
            yield return null;

            if (Vector3.Distance(hookTip.position, hookTipMount.position) > distance || shell != null)
            {
                hookBubbles.Stop();
                yield return FlyBack(controller);
                wroomAudio.Stop();
                yield break;
            }
        }
        catchAudio.Play();
        hookBubbles.Stop();
        controller.GrappleOk();
    }

    public IEnumerator FlyBack(PlayerController controller, float totalTime = 0.2f)
    {
        var start = hookTip.position;
        // hookBubbles.Play();

        if (shell != null){
            totalTime = 0.5f;
        }

        wroomAudio.Play();
        var time = 0f;
        while (time < totalTime)
        {
            var finish = hookTipMount.position;
            hookTip.position = Vector3.Lerp(start, finish, time / totalTime);
            time += Time.deltaTime;
            yield return null;
        }
        wroomAudio.Stop();
        hookTip.SetPositionAndRotation(hookTipMount.position, hookTipMount.rotation);
        hookTip.parent = hookTipMount;
        // hookBubbles.Stop();
        if (shell != null){
            shell.GetRetrieved();
        }
        controller.GrappleFailed();
    }

    void OnTriggerEnter(Collider other)
    {
        var shell = other.GetComponent<Shell>();
        if (shell == null)
        {
            stopFlying = true;
            return;
        }

        catchAudio.Play();
        this.shell = shell;
        shell.GetGrabbed(hookTip, player.GetComponent<PlayerController>());
    }
}
