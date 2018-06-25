using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

	public GameObject objPrefab;
	TextAsset asset;
	
    List<string[]> lineSet;
    List<GameObject> myObjs;
    int curInd = 1;
	// Use this for initialization
	void Start () {

        lineSet = new List<string[]>();
        myObjs = new List<GameObject>();

        for(int i = 1; i <= 15; i++)
        {
            asset = Resources.Load("VelodyneData/data ("+i+")") as TextAsset;
            string[] lines = asset.text.Split('\n');
            lineSet.Add(lines);
            Debug.Log("Total lines : " + lines.Length);
        }
        Debug.Log("All Loaded");

		spawnSceene();
        StartCoroutine(playScene());
	}
	private void spawnSceene(){
		foreach(string line in lineSet[0]){
			string[] xyz=line.Split(',');
			if(xyz.Length>=3){
				float x, y, z;
				if(!float.TryParse(xyz[0], out x))continue;
				if(!float.TryParse(xyz[1], out y))continue;
				if(!float.TryParse(xyz[2], out z))continue;

				GameObject obj = GameObject.Instantiate(objPrefab,new Vector3(x,y,z), Quaternion.identity);
                myObjs.Add(obj);
			}
		}
	}
    private void reloadScene(int pos)
    {
        int i = 0;
        foreach (string line in lineSet[pos])
        {
            string[] xyz = line.Split(',');
            if (xyz.Length >= 3)
            {
                float x, y, z;
                if (!float.TryParse(xyz[0], out x)) continue;
                if (!float.TryParse(xyz[1], out y)) continue;
                if (!float.TryParse(xyz[2], out z)) continue;

                if (i < myObjs.Count)
                {
                    myObjs[i].transform.position = new Vector3(x, y, z);
                    i++;
                }
                else
                {
                    break;
                }
            }
        }
    }
	// Update is called once per frame
	void Update () {
        
	}

    private IEnumerator playScene()
    {
        while (curInd < lineSet.Count)
        {
            yield return new WaitForSeconds(0.2f);
            reloadScene(curInd++);
        }
    }
}
