using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

	public GameObject objPrefab;
	TextAsset asset;
	string[] lines;
	// Use this for initialization
	void Start () {
		asset = Resources.Load("data") as TextAsset;
		lines = asset.text.Split('\n');
		Debug.Log("Total lines : "+lines.Length);

		spawnSceene();
	}
	private void spawnSceene(){
		foreach(string line in lines){
			string[] xyz=line.Split(',');
			if(xyz.Length>=3){
				float x, y, z;
				if(!float.TryParse(xyz[0], out x))continue;
				if(!float.TryParse(xyz[1], out y))continue;
				if(!float.TryParse(xyz[2], out z))continue;

				GameObject.Instantiate(objPrefab,new Vector3(x,y,z), Quaternion.identity);
			}
		}
	}
	// Update is called once per frame
	void Update () {
		
	}
}
