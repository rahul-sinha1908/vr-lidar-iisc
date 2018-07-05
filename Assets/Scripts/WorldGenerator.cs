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
	}
	public GameObject objPrefab;
	public Transform parentObj;
	TextAsset asset;
	string[] lines;
	Vector3[] vertices;
	// Use this for initialization
	void Start () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		asset = Resources.Load("data") as TextAsset;
		lines = asset.text.Split('\n');
		Debug.Log("Total lines : "+lines.Length);

		// Generate();
		spawnSceene();
		//parentObj.Rotate(30,0,0,Space.World);
	}
	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";
		xSize=10;
		ySize=10;
		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		for (int i = 0, y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, y);
			}
		}
		mesh.vertices = vertices;

		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
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
		vertices = new Vector3[(width) * (height)];
		foreach(LoadData data in myList){
			int y = (data.laserId%2==2)?data.laserId/2-1:15+data.laserId/2;
			int x = data.azimuth;
			int index=y*width+x;
			//Debug.Log("Index : "+)
			if(index<vertices.Length)
				vertices[index]=new Vector3(data.x, data.y, data.z);
		}

		for (int i = 0; i < vertices.Length; i++)
			if(vertices[i]!=null){
				vertices[0]=vertices[i];
				break;
			}
		for (int i = 1; i < vertices.Length; i++) {
			if(vertices[i]==null){
				vertices[i]=vertices[i-1];
			}
		}

		// for (int x = 0; x <= xSize; x++) {
		// 	for (int y = 0; y <= ySize; y++) {
		// 		int i = y*width+x;
		// 		if(y==0)
		// 			continue;
		// 		int pi = (y-1)*width+x;
		// 		if(Vector3.SqrMagnitude(vertices[i]-vertices[pi])>clearingDistance)
		// 			vertices[i]=vertices[pi];
		// 	}
		// }

		mesh.vertices=vertices;
		
		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				Vector3 v1=vertices[vi], v2= vertices[vi+1], v3=vertices[vi+xSize+1], v4=vertices[vi+xSize+2];
				if(Vector3.SqrMagnitude(v1-v2)>clearingDistance
					|| Vector3.SqrMagnitude(v2-v3)>clearingDistance
					||Vector3.SqrMagnitude(v3-v4)>clearingDistance
					||Vector3.SqrMagnitude(v4-v1)>clearingDistance)
					continue;
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + xSize + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + 1;
				triangles[ti + 5] = vi + xSize + 2;
				// if(Vector3.SqrMagnitude(vertices[i]-vertices[pi])>clearingDistance)
				// 	vertices[i]=vertices[pi];
			}
		}
		mesh.triangles=triangles;
		mesh.RecalculateNormals();

		Debug.Log("Done Drawing"+vertices.Length);
		//StartCoroutine(Generate());
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
		// 	Gizmos.DrawSphere(vertices[i], 0.1f);
		// }
	}

	private void ceateAMesh(){

	}

}
