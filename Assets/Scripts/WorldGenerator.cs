﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {
    public string fileName;
    public bool isDirectory=false;
	private MeshFilter meshFilter;
	private Mesh mesh;
	public int totalRenderVertex=-1;
	private int xSize=-1, ySize=-1;
	public float clearingDistance=0;
    private List<GameObject> myCapsules;
	class LoadData{
		public float x, y, z;
		public int laserId, azimuth, intensity;

		public LoadData(float x, float y, float z, int laserId, int intensity, int azimuth){
			this.x=x;
			this.y=y;
			this.z=z;
			this.laserId= laserId;
			this.intensity=intensity;
			this.azimuth=azimuth;
		}
	}
	public GameObject objPrefab;
	public Transform parentObj;
	TextAsset asset;
	string[] lines;
    List<string[]> linesArray;
	Vector3[] vertices;
	Color[] colors;
    bool firstLoad = true;

	// Use this for initialization
	void Start () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";
		meshFilter=GetComponent<MeshFilter>();
        myCapsules = new List<GameObject>();

        linesArray=new List<string[]>();

        StartCoroutine(loadScenes());
	}

	public void BasicMerge()
	{
		// All our children (and us)
		MeshFilter[] filters = GetComponentsInChildren<MeshFilter> (false);

		Mesh finalMesh=new Mesh();
		finalMesh.name="Bundle of Objects";

		CombineInstance[] combiners=new CombineInstance[filters.Length];
		for(int i=0;i<filters.Length;i++){
			if(filters[i].transform == transform)
				continue;
			combiners[i].subMeshIndex=0;
			combiners[i].mesh=filters[i].mesh;
			combiners[i].transform=filters[i].transform.localToWorldMatrix;
		}
		finalMesh.CombineMeshes(combiners);
		meshFilter.mesh=finalMesh;
	}

	public void AdvancedMerge()
	{
		// All our children (and us)
		MeshFilter[] filters = GetComponentsInChildren<MeshFilter> (false);

		// All the meshes in our children (just a big list)
		List<Material> materials = new List<Material>();
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer> (false); // <-- you can optimize this
		foreach (MeshRenderer renderer in renderers)
		{
		if (renderer.transform == transform)
			continue;
		Material[] localMats = renderer.sharedMaterials;
		foreach (Material localMat in localMats)
			if (!materials.Contains (localMat))
			materials.Add (localMat);
		}

		// Each material will have a mesh for it.
		List<Mesh> submeshes = new List<Mesh>();
		foreach (Material material in materials)
		{
			// Make a combiner for each (sub)mesh that is mapped to the right material.
			List<CombineInstance> combiners = new List<CombineInstance> ();
			foreach (MeshFilter filter in filters)
			{
				if (filter.transform == transform) continue;
				// The filter doesn't know what materials are involved, get the renderer.
				MeshRenderer renderer = filter.GetComponent<MeshRenderer> ();  // <-- (Easy optimization is possible here, give it a try!)
				if (renderer == null)
				{
				Debug.LogError (filter.name + " has no MeshRenderer");
				continue;
				}

				// Let's see if their materials are the one we want right now.
				Material[] localMaterials = renderer.sharedMaterials;
				for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
				{
				if (localMaterials [materialIndex] != material)
				continue;
				// This submesh is the material we're looking for right now.
				CombineInstance ci = new CombineInstance();
				ci.mesh = filter.sharedMesh;
				ci.subMeshIndex = materialIndex;
				ci.transform = Matrix4x4.identity;
				combiners.Add (ci);
				}
			}
			// Flatten into a single mesh.
			Mesh mesh = new Mesh ();
			mesh.CombineMeshes (combiners.ToArray(), true);
			submeshes.Add (mesh);
		}

		// The final mesh: combine all the material-specific meshes as independent submeshes.
		List<CombineInstance> finalCombiners = new List<CombineInstance> ();
		foreach (Mesh mesh in submeshes)
		{
			CombineInstance ci = new CombineInstance ();
			ci.mesh = mesh;
			ci.subMeshIndex = 0;
			ci.transform = Matrix4x4.identity;
			finalCombiners.Add (ci);
		}
		Mesh finalMesh = new Mesh();
		finalMesh.name="Bunch Of Objects";
		finalMesh.CombineMeshes (finalCombiners.ToArray(), false);
		//meshFilter.sharedMesh = finalMesh;
		Debug.Log ("Final mesh has " + submeshes.Count + " materials.");
	}
    private IEnumerator loadScenes()
    {
        if (!isDirectory)
        {
            asset = Resources.Load(fileName) as TextAsset;
            lines = asset.text.Split('\n');
            Debug.Log("Total lines : " + lines.Length);
            spawnScene(totalRenderVertex);

			Debug.Log("Its merging Now");
			//AdvancedMerge();
			//BasicMerge();
			Debug.Log("Its merged Now");
			// foreach(Transform children in transform)
			// 	Destroy(children.gameObject);
			Debug.Log("All children deleted");

        }
        else
        {
            int i = 1;
            string actualFile = fileName + " (" + i + ")";
            asset = Resources.Load(actualFile) as TextAsset;
            Debug.Log("Loaded : " + actualFile +" : "+(asset!=null));
            while (asset != null)
            {
                Debug.Log("It got in");
                lines = asset.text.Split('\n');
                linesArray.Add(lines);
                Debug.Log("Total lines : " + lines.Length);
                //spawnScene();
                i++;
                actualFile = fileName + " (" + i + ")";
                asset = Resources.Load(actualFile) as TextAsset;
                Debug.Log("Loaded : " + actualFile + " : " + (asset != null));
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("Loading Done");

            int ind=0;
            while(spawnSceneInSequence(ind++)){
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("Display Done");
        }
    }
    // Update is called once per frame
    void Update()
    {
        //loadNextScene();
    }

    private bool spawnSceneInSequence(int ind){
        if(ind>linesArray.Count)
            return false;
        
		int totColumns=0;
		int totRows=0;

		float x, y, z;
		float  temp1, temp2, temp3;
		int  laserId, azimuth, intensity;
		int previousLID=100;

		List<LoadData> myList=new List<LoadData>();
		foreach(string line in linesArray[ind]){
			string[] xyz=line.Split(',');
			if(xyz.Length>=3){

				if(!float.TryParse(xyz[0], out x))continue;
				if(!float.TryParse(xyz[1], out y))continue;
				if(!float.TryParse(xyz[2], out z))continue;
				if(!float.TryParse(xyz[6], out temp3))continue;
				if(!float.TryParse(xyz[7], out temp1))continue;
				if(!float.TryParse(xyz[8], out temp2))continue;

				laserId=Mathf.RoundToInt(temp1);
				azimuth=Mathf.RoundToInt(temp2);
				intensity=Mathf.RoundToInt(temp3);

				if(previousLID>laserId)
					totColumns++;
				if(totRows<laserId)
					totRows=laserId;
				
				previousLID=laserId;
				// laser - 7, verticalangle - 12
				myList.Add(new LoadData(x,y,z,laserId, intensity,totColumns-1));
				// GameObject obj = GameObject.Instantiate(objPrefab,new Vector3(x,y,z), Quaternion.identity, parentObj);
				// obj.name="Cube : "+laserId+" - "+azimuth;
			}
		}
		Debug.Log("Total Rows : "+totColumns+" : "+totRows);

		initTheVertices(myList, totColumns, totRows);

        if(firstLoad){
            firstLoad = false;
		}

        return true;
    }


    private void spawnScene(int totalVertices=-1){
		int totColumns=0;
		int totRows=0;

		float x, y, z;
		float  temp1, temp2, temp3;
		int  laserId, azimuth, intensity;
		int previousLID=100;

		List<LoadData> myList=new List<LoadData>();
		int i=0;
		foreach(string line in lines){
			string[] xyz=line.Split(',');
			if(xyz.Length>=3){

				if(!float.TryParse(xyz[0], out x))continue;
				if(!float.TryParse(xyz[1], out y))continue;
				if(!float.TryParse(xyz[2], out z))continue;
				if(!float.TryParse(xyz[6], out temp3))continue;
				if(!float.TryParse(xyz[7], out temp1))continue;
				if(!float.TryParse(xyz[8], out temp2))continue;

				laserId=Mathf.RoundToInt(temp1);
				azimuth=Mathf.RoundToInt(temp2);
				intensity=Mathf.RoundToInt(temp3);

				if(previousLID>laserId)
					totColumns++;
				if(totRows<laserId)
					totRows=laserId;
				
				previousLID=laserId;
				// laser - 7, verticalangle - 12
				myList.Add(new LoadData(x,y,z,laserId, intensity, totColumns-1));
				// GameObject obj = GameObject.Instantiate(objPrefab,new Vector3(x,y,z), Quaternion.identity, parentObj);
				// obj.name="Cube : "+laserId+" - "+azimuth;
			}
			if(totalVertices!=-1){
				if(i++>totalVertices)
					break;
			}
		}
		Debug.Log("Total Rows : "+totColumns+" : "+totRows);

		initTheVertices(myList, totColumns, totRows);

        if(firstLoad){			
            firstLoad = false;
		}
    }
	void initTheVertices(List<LoadData> myList, int width, int height){
        if (firstLoad)
        {
            xSize = width - 1;
            ySize = height - 1;
            vertices = new Vector3[(width) * (height)];
			colors = new Color[(width) * (height)];
        }
		float maxIntensity=0;
		foreach(LoadData data in myList){
			int intense = data.intensity;
			maxIntensity+=intense;
		}
		maxIntensity=maxIntensity/myList.Count;
		
		foreach(LoadData data in myList){
			int y = (data.laserId%2==2)?data.laserId/2-1:15+data.laserId/2;
			int x = data.azimuth;
			int index=y*width+x;
			//Debug.Log("Index : "+)
			if(index<vertices.Length){
				vertices[index]=new Vector3(data.x, data.y, data.z);
				colors[index]=Color.Lerp(Color.green, Color.red, data.intensity*1.0f/maxIntensity);
			}
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

        bool b = true;
        if (b)
        {
            drawSpheres();
            return;
        }


        //This wont be excuted in this branch
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

	
	private void OnDrawGizmos () {
        //if (vertices == null)
        //{
        //    Debug.Log("Vertices null");
        //    return;
        //}
        //Debug.Log("Its drawing : " + vertices.Length);
        //Gizmos.color = Color.green;
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Gizmos.DrawSphere(vertices[i], 0.04f);
        //}
    }
    private void drawSpheres()
    {
        if (vertices == null)
        {
            Debug.Log("Vertices null");
            return;
        }
        Debug.Log("Its drawing : " + vertices.Length);
        //Gizmos.color = Color.green;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (firstLoad)
            {
                GameObject obj = GameObject.Instantiate(objPrefab, vertices[i], Quaternion.identity, parentObj);
				obj.GetComponent<MeshRenderer>().material.SetColor("_Color", colors[i]);
                myCapsules.Add(obj);
            }
            else
            {
                if (myCapsules[i] != null){
                    myCapsules[i].transform.position = vertices[i];
					myCapsules[i].GetComponent<MeshRenderer>().material.SetColor("_Color", colors[i]);
				}
            }
        }
    }

	private void ceateAMesh(){

	}

}
