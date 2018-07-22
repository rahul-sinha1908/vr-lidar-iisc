using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {
	private MeshFilter meshFilter;
	private Mesh mesh;
	private int xSize=-1, ySize=-1;
	public float clearingDistance=0;
	class LoadData{
		public float x, y, z;
		public int laserId, azimuth;

		public LoadData(float x, float y, float z, int laserId, int azimuth){
			this.x=x;
			this.y=y;
			this.z=z;
			this.laserId= laserId;
			this.azimuth=azimuth;
		}

		public Vector3 point(){
			return new Vector3(x,y,z);
		}
	}
	public GameObject objPrefab;
	public Transform parentObj;
	public bool isDrawCubes;
	TextAsset asset;
	string[] lines;
	Vector3[] vertices;
	// Use this for initialization
	void Start () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		asset = Resources.Load("data1") as TextAsset;
		lines = asset.text.Split('\n');
		Debug.Log("Total lines : "+lines.Length);

		// Generate();
		spawnSceene();
		//parentObj.Rotate(30,0,0,Space.World);
	}
	private void spawnSceene(){
		int totColumns=0;
		int totRows=0;

		float x, y, z;
		float  temp1, temp2;
		int  laserId, azimuth;
		int previousLID=100;

		List<LoadData> myList=new List<LoadData>();
		foreach(string line in lines){
			string[] xyz=line.Split(',');
			if(xyz.Length>=3){

				if(!float.TryParse(xyz[0], out x))continue;
				if(!float.TryParse(xyz[1], out y))continue;
				if(!float.TryParse(xyz[2], out z))continue;
				if(!float.TryParse(xyz[7], out temp1))continue;
				if(!float.TryParse(xyz[8], out temp2))continue;

				laserId=Mathf.RoundToInt(temp1);
				azimuth=Mathf.RoundToInt(temp2);

				if(previousLID>laserId)
					totColumns++;
				if(totRows<laserId)
					totRows=laserId;
				
				previousLID=laserId;
				// laser - 7, verticalangle - 12
				myList.Add(new LoadData(x,y,z,laserId,totColumns-1));
				// GameObject obj = GameObject.Instantiate(objPrefab,new Vector3(x,y,z), Quaternion.identity, parentObj);
				// obj.name="Cube : "+laserId+" - "+azimuth;
			}
		}
		Debug.Log("Total Rows : "+totColumns+" : "+totRows);

		initTheVertices(myList, totColumns, totRows);
	}
	void initTheVertices(List<LoadData> myList, int width, int height){
		xSize=width-1;
		ySize=height-1;

		GameObject obj = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity);
		Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
		int count = mesh.vertices.Length;
		int triCount = mesh.triangles.Length;

		vertices = new Vector3[myList.Count*count];
		int[] triangles = new int[myList.Count * triCount];

		for(int i=0;i<myList.Count;i++){
			for(int j=0;j<count;j++){
				vertices[i*count+j]=myList[i].point()+mesh.vertices[j];
			}
			for(int j=0;j<triCount;j++){
				triangles[i*triCount+j]=i*count+mesh.triangles[j];
			}
		}
		//drawCubes();
		
		mesh.vertices=vertices;
		mesh.triangles=triangles;
		mesh.RecalculateNormals();

		Debug.Log("Done Drawing"+vertices.Length);
		//StartCoroutine(Generate());
	}

	private void drawCubes(){
		if(vertices==null){
			Debug.Log("Vertices null");
			return;
		}
		Debug.Log("Its drawing : "+vertices.Length);
		for (int i = 0; i < vertices.Length; i++) {
			GameObject.Instantiate(objPrefab, vertices[i], Quaternion.identity, transform);
		}
	}
	// Update is called once per frame
	void Update () {
		
	}
	private void OnDrawGizmos () {
		// if(vertices==null){
		// 	Debug.Log("Vertices null");
		// 	return;
		// }
		// Debug.Log("Its drawing : "+vertices.Length);
		// Gizmos.color = Color.black;
		// for (int i = 0; i < vertices.Length; i++) {
		// 	//Gizmos.DrawSphere(vertices[i], 0.1f);
		// 	Gizmos.DrawCube(vertices[i], new Vector3(1,1,1)*0.05f);
		// }
	}

	private void ceateAMesh(){

	}

}
