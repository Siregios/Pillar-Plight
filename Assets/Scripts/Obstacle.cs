using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    GameObject player;
    public int sx, sy, speed;
    public float waitTime;
    public int dir, dist = 0;
    public bool HZ;
    int dirStore;
    int count;
    float t;
    bool wait;
    bool moved;
    int x, y;
    Vector3 startPosition, startScale;
    Quaternion startRotation;

    void Start()
    {
        dirStore = dir;
        player = GameObject.FindGameObjectWithTag("Player");
        x = sx;
        y = sy;
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
    }

	void Update () {
        t += Time.deltaTime;
        if (!wait)
        {
            t = Mathf.Clamp(t, 0, (float)Mathf.Abs(speed * dir));
            if (HZ)
                transform.position = new Vector3(transform.position.x, transform.position.y - 2.02f * Time.deltaTime * speed * dir, transform.position.z);
            if (!HZ)
                transform.Rotate(new Vector3(0, Time.deltaTime * 360 / 14 * speed * dir, 0));
            if (!moved)
            {
                if (t > 1f / (2f * Mathf.Abs(speed * dir)))
                {
                    moved = true;
                    x += dir;
                }
            }
            if (t >= 1f / (float)Mathf.Abs(speed * dir))
            {
                t = 0; ;
                count++;
                moved = false;
            }
            if (dist != 0 && count >= dist)
            {
                dir *= -1;
                count = 0;
                wait = true;
            }
        }
        else if (t > waitTime)
        {
            t = 0;
            wait = false;
        }
	}

    void FixedUpdate()
    {
        if (player.GetComponent<PlayerController>().GetX() == x && player.GetComponent<PlayerController>().GetY() == y)
        {
           
        }
    }

    void OnDisable()
    {
        x = sx;
        y = sy;
        count = 0;
        dir = dirStore;
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
    }
}
