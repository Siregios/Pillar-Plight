using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public GameObject centerPoint, cameraObject;
    public AnimationCurve speed;
    Vector3 tempPos;
    float jumpDis, timer, startPos, startRot, endMoveTime, rotateDegrees;
    bool turning = false, jumping = false, fading = false;
    int hDir, vDir, dirTemp;
    int tempInt, x, y, layer;
    LevelLoader.level[] level;

    public int GetX() { return x; }
    public int GetY() { return y; }

    void Init()
    {
        GetComponent<LevelLoader>().LoadLevel();
        level[0] = GetComponent<LevelLoader>().eLevel;
        level[1] = GetComponent<LevelLoader>().iLevel;
        layer = 0;
        x = GetComponent<LevelLoader>().startX;
        y = GetComponent<LevelLoader>().startY;
        transform.position = new Vector3(transform.position.x, jumpDis * y, transform.position.z);
        GetComponent<LevelLoader>().temp1.SetActive(true);
        GetComponent<LevelLoader>().temp2.SetActive(false);
    }

    void Start()
    {
        endMoveTime = speed.keys[speed.keys.Length - 1].time;
        rotateDegrees = 360f / 14f;
        jumpDis = GetComponent<LevelLoader>().ePillar.GetComponent<BoxCollider>().size.y;
        level = new LevelLoader.level[2];
        Init();
    }

    void NextLevel()
    {
        GetComponent<LevelLoader>().index++;
        Init();
    }

    void Update () {
        if (!jumping && !turning && !fading)
        {
            if (Input.GetButton("\\"))
            {
                hDir = (int)Input.GetAxisRaw("\\");
                vDir = -(int)Input.GetAxisRaw("\\");
            }
            if (Input.GetButton("/"))
            {
                hDir = (int)Input.GetAxisRaw("/");
                vDir = (int)Input.GetAxisRaw("/");
            }
            if (Input.GetButton("Vertical"))
            {
                hDir = 0;
                vDir = (int)Input.GetAxisRaw("Vertical");
            }
            if (Input.GetButton("Horizontal"))
            {
                hDir = (int)Input.GetAxisRaw("Horizontal");
                vDir = 0;
            }
            if (Input.GetButtonDown("Enter") && level[layer].tiles[x, y].data == 1)
            {
                fading = true;
                SwapCameraLayer();
                layer = layer == 0 ? 1 : 0;
                hDir = vDir = 0;
                timer = 0;
                startRot = cameraObject.transform.rotation.eulerAngles.x;
                tempPos = cameraObject.transform.position;
            }
            if (hDir != 0 || vDir != 0)
            {
                tempInt = (int)Mathf.Repeat(x + hDir * (-2 * layer + 1), 14);
                if (level[layer].tiles[tempInt, y + (int)vDir].data < 3)
                {
                    x = tempInt;
                    y += vDir;
                    turning = hDir != 0;
                    jumping = vDir != 0;
                    startRot = centerPoint.transform.rotation.eulerAngles.y;
                    startPos = transform.position.y;
                    timer = 0;
                }
            }
        }
        else
        {
            timer += Time.deltaTime;
            if (fading)
            {
                cameraObject.transform.localRotation = Quaternion.Euler(Mathf.Lerp(startRot, 0, speed.Evaluate(Mathf.PingPong(timer, endMoveTime))), 0, 0);
                cameraObject.transform.position = new Vector3(Mathf.Lerp(tempPos.x, transform.position.x, speed.Evaluate(Mathf.PingPong(timer, endMoveTime))),
                                                              Mathf.Lerp(tempPos.y, transform.position.y, speed.Evaluate(Mathf.PingPong(timer, endMoveTime))),
                                                              Mathf.Lerp(tempPos.z, transform.position.z, speed.Evaluate(Mathf.PingPong(timer, endMoveTime))));
                cameraObject.GetComponentInChildren<SpriteRenderer>().color = new Color(0,0,0, Mathf.Lerp(0, 2.55f, Mathf.PingPong(timer, endMoveTime)));
                if (timer > endMoveTime*2){fading = false;}
            }
            else
            {
                if (turning) { centerPoint.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(startRot, startRot + (-1 * hDir * (-2 * layer + 1) * rotateDegrees), speed.Evaluate(timer)), 0); }
                if (jumping) { transform.position = new Vector3(transform.position.x, Mathf.Lerp(startPos, startPos + (vDir * jumpDis), speed.Evaluate(timer)), transform.position.z); }
                if (timer > endMoveTime)
                {
                    hDir = vDir = 0;
                    turning = jumping = false;
                    if (y >= level[0].tiles.GetLength(1) - 1) { NextLevel(); }
                }
            }
        }
    }

    void SwapCameraLayer()
    {
        bool layer = GetComponent<LevelLoader>().temp1.activeSelf;
        transform.Rotate(0, 180, 0);
        GetComponent<LevelLoader>().temp1.SetActive(!layer);
        GetComponent<LevelLoader>().temp2.SetActive(layer);
    }
}
