using System.Collections.Generic;
using UnityEngine;

public class TrailLine : MonoBehaviour
{
    public Transform player;
    public Transform playerRopeMount;
    public Transform worldRopeMount;
    public LineRenderer rope;
    public float segmentDistance = 2f;

    List<Vector3> positions;

    void Start()
    {
        rope.transform.parent = player.parent;
        rope.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        positions = new List<Vector3> {
            worldRopeMount.position,
            playerRopeMount.position,
        };

        rope.SetPositions(positions.ToArray());
    }

    void Update()
    {
        var lastPos = positions[positions.Count - 2];
        var currentPos = playerRopeMount.position;

        positions[^1] = currentPos;
        if (Vector3.Distance(lastPos, currentPos) > segmentDistance)
        {
            Debug.Log("Add point");
            positions.Add(currentPos);
            rope.SetPositions(positions.ToArray());
        }
        else
        {
            // rope.SetPosition(rope.positionCount - 1, currentPos);
            rope.SetPositions(positions.ToArray());
        }
    }
}
