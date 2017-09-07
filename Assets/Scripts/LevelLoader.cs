using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour {

    public struct level
    {
        public tile[,] tiles;
    }

    public struct tile
    {
        public int data;
        public int bonus;
    }

    public GameObject ePillar, iPillar;
    [HideInInspector] public GameObject temp1, temp2;
    public Material eMat, iMat;
    public Texture2D[] eMaps, eBMaps, iMaps, iBMaps;
    public GameObject[] extras;
    public int dimensions, index;
    [HideInInspector] public int height, width, startX, startY;
    public level eLevel, iLevel;
    int temp;
    float initialRotate = 95.4f;

    public void LoadLevel()
    {
        eMat.SetInt("_Dimmensions", dimensions);
        iMat.SetInt("_Dimmensions", dimensions);
        Destroy(temp1);
        Destroy(temp2);
        temp1 = Instantiate(ePillar);
        temp2 = Instantiate(iPillar);
        eMat.SetTexture("_MainTex", eMaps[index]);
        iMat.SetTexture("_MainTex", iMaps[index]);
        height = eMaps[index].height;
        width = eMaps[index].width;
        eMat.SetTextureScale("_MainTex", new Vector2((float)width/height, 1));
        iMat.SetTextureScale("_MainTex", new Vector2((float)width/height, 1));
        iLevel.tiles = new tile[width / dimensions, height / dimensions];
        eLevel.tiles = new tile[width / dimensions, height / dimensions];
        temp1.transform.localScale = new Vector3(ePillar.transform.localScale.x, (height / dimensions), ePillar.transform.localScale.z);
        temp2.transform.localScale = new Vector3(iPillar.transform.localScale.x, (height / dimensions), iPillar.transform.localScale.z);
        for (int i = 0; i < width; i+=dimensions)
        {
            for(int j = 0; j < height; j+=dimensions)
            {
                //External
                temp = RGBData(eMaps[index].GetPixel(i, j));
                eLevel.tiles[i / dimensions, j / dimensions].data = temp;
                //External Bonus
                temp = RGBData(eBMaps[index].GetPixel(i, j));
                if(temp == 4)
                {
                    startX = i / dimensions;
                    startY = j / dimensions;
                }
                eLevel.tiles[i / dimensions, j / dimensions].bonus = temp;
                spawnExtra(temp, 0, i/dimensions, j/dimensions);
                //Internal
                temp = RGBData(iMaps[index].GetPixel(i, j));
                iLevel.tiles[i / dimensions, j / dimensions].data = temp;
                //Internal Bonus
                temp = RGBData(iBMaps[index].GetPixel(i, j));
                iLevel.tiles[i / dimensions, j / dimensions].bonus = temp;
                spawnExtra(temp, 1, i/dimensions, j/dimensions);
            }
        }
        temp1.transform.position = new Vector3(0, ePillar.GetComponent<BoxCollider>().size.y * height / (2 * dimensions) - ePillar.GetComponent<BoxCollider>().size.y / 2, 0);
        temp1.transform.Rotate(0, initialRotate + (360f / ((float)width / (float)dimensions)) * (float)startX, 0);
        temp2.transform.position = new Vector3(0, iPillar.GetComponent<BoxCollider>().size.y * height / (2 * dimensions) - iPillar.GetComponent<BoxCollider>().size.y / 2, 0);
        temp2.transform.Rotate(0, initialRotate + (360f / ((float)width / (float)dimensions)) * (float)startX, 0);
    }

    private int RGBData(Color temp)
    {
        if (temp == Color.white) { return 0; }
        if (temp == Color.red) { return 1; }
        if (temp == Color.green) { return 2; }
        if (temp == Color.blue) { return 3; }
        if (temp == Color.black) { return 4; }
        if (temp == Color.gray) { return 5; }
        if (temp == Color.cyan) { return 6; }
        if (temp == Color.magenta) { return 7; }
        return -1;
    }

    public void spawnExtra(int i, int layer, int x, int y)
    {
        if (i >= 0 && i < extras.Length && extras[i] != null)
        {
            GameObject parent = layer == 0 ? temp1 : temp2;
            GameObject temp = Instantiate(extras[i], parent.transform);
            Obstacle script = temp.GetComponent<Obstacle>();
            temp.transform.localScale = new Vector3(parent.transform.localScale.x, 1f / (height / dimensions), parent.transform.localScale.z);
            temp.transform.Rotate(0, initialRotate - 0.3f + (360f / ((float)width / (float)dimensions)) * (float)x, 0);
            temp.transform.position = temp.transform.up * (2.02f * (y - height / 2) + 1.01f);
            temp.transform.GetChild(0).position = temp.transform.position + temp.transform.forward * (layer == 0 ? -4.91f : -4f);
            if (script != null)
            {
                script.sx = x;
                script.sy = y;
            }
        }
    }
}
