using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {
    LevelLoader loader;
    public int objectID;
    public float time;
    public float dif;
    PlayerController player;
    int layer = 0;
    float t;
    int temp = 0;


    void Start()
    {
        loader = GameObject.FindGameObjectWithTag("Player").GetComponent<LevelLoader>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

	// Update is called once per frame
	void Update () {
        if (player.GetY() < loader.height - 8)
        {
            if (t <= 0)
                t = Random.Range(-dif, dif) + time;
            t -= Time.deltaTime;
            if (t <= 0)
            {
                temp = (int)Mathf.PingPong(Mathf.Abs(player.GetX() - Random.Range(0, 13)), 7);
                if (temp >= 5)
                    temp = Random.Range(player.GetX() - 1, player.GetX() + 1);
                loader.spawnExtra(objectID, layer, temp, player.GetY() + 8);

            }
        }
	}
}
