using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;



public class TileBuilder : MonoBehaviour {
	//Will Read from a file set up as an 2d array of binary values that can be used to
	//get information and build tiled maps

	//File 
	//2d array
	//type : contains what information 

	//Builder::
	//Position of mouse
	//place object
	public int objectSelection = 1;
	public GameObject currentTile;
	public GameObject chosenGO;
	public List<GameObject> buildObjects;
	MeshRenderer tileRend;
	Transform tileTrans;
	public Material canPlace;
	public Material cantPlace;
	public string filePath="Assets/Untitled.txt";
	public TextAsset binMap;
	private int[][] map;
	private int maxObjects = 5;
	//select object
	//remove object
	//show a hovering green tile for positioning item
	// Use this for initialization
	void Start () {

		tileTrans = currentTile.transform;
		tileRend = currentTile.GetComponent<MeshRenderer>();
		tileRend.enabled = false;

		//Check if file exists , if so open file and read contents
		map = createMap(map,45,45);
		map = zeroFillMap (map);

		//if (binMap != null) {
		debugPrintMap(map);
		//}
		readFile ();
		instantiateMap ();
	}
	
	// Update is called once per frame
	void Update (){
		chooseObject ();
		// go through array and instantiate prefabs in proper positions
		//createMap ();
		tilePlacer ();

	}
	//for now changes object selection to write to map
	private void chooseObject()
	{
		if (Input.GetKeyDown (KeyCode.DownArrow) && objectSelection > 0) {
			objectSelection--;
		} else if (Input.GetKeyDown (KeyCode.UpArrow) && objectSelection < maxObjects) {
			objectSelection++;
		}
	}

	//Read from a local file WILL NEED TO CHANGE**
	private void readFile(){	
		//check if the file contains data needed
		//if no map found create a new map
		StreamReader reader = new StreamReader(filePath);
		string file = reader.ReadLine();
		if (file != null) {
			string[] splitStr = file.Split (' ');
			//for splitStr length had to account for terminating character?
			if (splitStr.Length - 1 == map.Length * map [0].Length) {
				for (int i = 0; i < map.Length; i++) {
					for (int j = 0; j < map [i].Length; j++) {
						map [i] [j] = int.Parse (splitStr [(i * map [i].Length) + j]);
					}
				}
			}
		}
		reader.Close();

	}
	//Save to local file WILL NEED TO CHANGE**
	private void saveMap(){
		StreamWriter writer = new StreamWriter(filePath, false);
		for (int i = 0; i < map.Length; i++) {
			for (int j = 0; j < map [i].Length; j++) {
				writer.Write (map[i][j].ToString() + ' ');
			}
		}
		writer.Close();
	}

	//Instantiate the map to the game
	private void instantiateMap(){
		
		for (int i = 0; i < map.Length; i++) {
			for (int j = 0; j < map [i].Length; j++) {
				if (map [i] [j] != 0) {
					instantiateObject (map [i] [j], i, j);
				}
			}
		}
	}
	private void instantiateObject(int choice, int xPos, int zPos){
		chosenGO = buildObjects[choice -1];
		Transform mapObjects = GameObject.Find ("MapObjects").transform;
		if (mapObjects != null && chosenGO != null) {
			Vector3 goPosition;
			goPosition.x = xPos+0.5f;
			goPosition.z = zPos+0.5f;
			goPosition.y = 0.0f;
			Transform go = Instantiate(chosenGO.transform, goPosition, Quaternion.identity);
			go.SetParent (mapObjects);
			go.gameObject.name = "X : "+(int)Mathf.Floor(goPosition.x)+" Z : "+(int)Mathf.Floor(goPosition.z);
		}
	}
	private void tilePlacer(){
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
			if (hit.collider.tag == "buildMap") {
				tileRend.material = canPlace;
				showTile (hit.point);

				if (Input.GetKeyDown(KeyCode.A) || Input.GetMouseButtonDown(0)) {
					chosenGO = buildObjects [objectSelection - 1];
					Transform mapObjects = GameObject.Find ("MapObjects").transform;
					if (mapObjects != null && chosenGO != null) {
						Transform go = Instantiate (chosenGO.transform, tileTrans.position, Quaternion.identity);
						go.SetParent (mapObjects);
						go.gameObject.name = "X : " + (int)Mathf.Floor (tileTrans.position.x) + " Z : " + (int)Mathf.Floor (tileTrans.position.z);
						//write to map
						map [(int)Mathf.Floor (tileTrans.position.x)] [(int)Mathf.Floor (tileTrans.position.z)] = objectSelection;
						//save to file
						saveMap ();
					}
				}


			} 
			else if (hit.collider.tag == "invadeMap") {
				//same as above fo different x y positions
				tileRend.material = cantPlace;
				showTile (hit.point);
			}
		} 
		else {
			tileRend.enabled = false;
		}
	}
	private void showTile(Vector3 mousePosition)
	{
		Vector3 position = mousePosition;
		//if collider is within bounds of map then show cube, if no object is already in that space canPlace material
		//else cantPlace material
		//Y is height

		float x = position.x;
		float z = position.z;
		x = Mathf.Round (x);
		z = Mathf.Round (z);
		x += .5f;
		z += .5f;

		position.x = x;
		position.z = z;
		position.y = 0.0f;

		tileTrans.position = position;

		tileRend.enabled = true;
	}

	private int[][] createMap(int [][] newMap, int sizeX, int sizeY){
		int [][] createdMap = new int[sizeX][];
		for (int i = 0; i < sizeX; i++) {
			createdMap [i] = new int[sizeY];
		}
		return createdMap;
		
	}
	private int[][] zeroFillMap(int [][] mapToFill){
		for(int i = 0; i < mapToFill.Length; i++){
			for (int j = 0; j < mapToFill[i].Length; j++) {
				mapToFill[i][j] = 0;
			}
		}
		return mapToFill;
	}
	private void debugPrintMap(int[][] map)
	{
		string row = "";
		for (int i = 0; i < map.Length; i++) {
			row += "Row "+i+" :\t";
			for (int j = 0; j < map [i].Length; j++) {
				row += map[i][j].ToString ();
			}
			row += "\n";
		}
		Debug.Log (row);
	}
}
			
	

