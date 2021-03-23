using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Slower version of the level generator to show how it works
public class SlowGenerator : MonoBehaviour {
	public float timeBetweenLoops = 0.001f, timeBetweenLoopsLong = 0.1f;
	public float chanceWalkerChangeDir = 0.5f, chanceWalkerSpawn = 0.05f;
	public float chanceWalkerDestoy = 0.05f;
	public float chanceChestSpawn = 0.00f, chanceChestWallFavor = 0.005f;
	public float chanceEnemySpawn = 0.02f;
	public int maxHallwayLength = 3;
	enum gridSpace { empty, floor, wall, chest, enemy };
	gridSpace[,] grid;
	GameObject[,] gridObjects;
	int roomHeight, roomWidth;
	Vector2 roomSizeWorldUnits = new Vector2(30, 30);
	float worldUnitsInOneGridCell = 1;
	struct walker
	{
		public Vector2 dir;
		public Vector2 pos;
		public int hallwayLength;
	}
	List<walker> walkers;
	int maxWalkers = 10;
	int maxChests = 3;
	int maxEnemies = 10;
	float percentToFill = 0.2f;
	public GameObject wallObj, floorObj, chestObj, enemyObj;

	void Start()
	{
		Setup();
		StartCoroutine(CreateFloors());
	}

	/// <summary>
	/// Setup room dimensions and create first walker
	/// </summary>
	void Setup(){
		//find grid size
		roomHeight = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsInOneGridCell);
		roomWidth = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsInOneGridCell);
		//create grid
		grid = new gridSpace[roomWidth,roomHeight];
		gridObjects = new GameObject[roomWidth, roomHeight];
		//set grid's default state
		for (int x = 0; x < roomWidth-1; x++){
			for (int y = 0; y < roomHeight-1; y++){
				//make every cell "empty"
				grid[x,y] = gridSpace.empty;
			}
		}
		//set first walker
		//init list
		walkers = new List<walker>();
		//create a walker 
		walker newWalker = new walker();
		newWalker.dir = RandomDirection();
		//find center of grid
		Vector2 spawnPos = new Vector2(Mathf.RoundToInt(roomWidth/ 2.0f),
										Mathf.RoundToInt(roomHeight/ 2.0f));
		newWalker.pos = spawnPos;
		//add walker to list
		walkers.Add(newWalker);
	}
	
	/// <summary>
	/// Changes the walker's direction to up,down,left,right randomly when called
	/// </summary>
	/// <returns> New direction of walker</returns>
	Vector2 RandomDirection(){
		//pick random int between 0 and 3
		int choice = Mathf.FloorToInt(Random.value * 3.99f);
		//use that int to chose a direction
		switch (choice){
			case 0:
				return Vector2.down;
			case 1:
				return Vector2.left;
			case 2:
				return Vector2.up;
			default:
				return Vector2.right;
		}
	}

	/// <summary>
	/// Counts the number of tiles that are floors
	/// </summary>
	/// <returns> The number of floors</returns>
	int NumberOfFloors()
	{
		int count = 0;
		foreach (gridSpace space in grid)
		{
			if (space == gridSpace.floor)
			{
				count++;
			}
		}
		return count;
	}

	/// <summary>
	/// Creates floors while moving/spawning/destroying walkers
	/// </summary>
	IEnumerator CreateFloors(){
		int iterations = 0;//loop will not run forever
		do
		{
			bool spawned = false;
			//create floor at position of every walker
			foreach (walker myWalker in walkers)
			{
				if (grid[(int)myWalker.pos.x, (int)myWalker.pos.y] != gridSpace.floor)
				{
					Spawn(myWalker.pos.x, myWalker.pos.y, floorObj);//update visuals
					spawned = true;
				}
				grid[(int)myWalker.pos.x, (int)myWalker.pos.y] = gridSpace.floor;
			}
			//chance: destroy walker
			int numberChecks = walkers.Count; //might modify count while in this loop
			for (int i = 0; i < numberChecks; i++)
			{
				//only if its not the only one, and at a low chance
				if (Random.value < chanceWalkerDestoy && walkers.Count > 1)
				{
					walkers.RemoveAt(i);
					break; //only destroy one per iteration
				}
			}
			//chance: walker pick new direction
			for (int i = 0; i < walkers.Count; i++)
			{
				walker thisWalker = walkers[i];
				if (thisWalker.hallwayLength >= maxHallwayLength)
				{
					thisWalker.dir = Vector2.Perpendicular(thisWalker.dir);
					thisWalker.hallwayLength = 0;
				}
				else if (Random.value < chanceWalkerChangeDir)
				{
					thisWalker.dir = RandomDirection();
					thisWalker.hallwayLength = 0;
				}
				else
					thisWalker.hallwayLength++;
				walkers[i] = thisWalker;
			}
			//chance: spawn new walker
			numberChecks = walkers.Count; //might modify count while in this loop
			for (int i = 0; i < numberChecks; i++)
			{
				//only if # of walkers < max, and at a low chance
				if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers)
				{
					//create a walker 
					walker newWalker = new walker();
					newWalker.dir = RandomDirection();
					newWalker.pos = walkers[i].pos;
					walkers.Add(newWalker);
				}
			}
			//move walkers
			for (int i = 0; i < walkers.Count; i++)
			{
				walker thisWalker = walkers[i];
				thisWalker.pos += thisWalker.dir;
				walkers[i] = thisWalker;
			}
			//avoid boarder of grid
			for (int i = 0; i < walkers.Count; i++)
			{
				walker thisWalker = walkers[i];
				//clamp x,y to leave a 1 space boarder: leave room for walls
				thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 1, roomWidth - 2);
				thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 1, roomHeight - 2);
				walkers[i] = thisWalker;
			}
			//check to exit loop
			if ((float)NumberOfFloors() / (float)grid.Length > percentToFill)
			{
				break;
			}
			iterations++;
			if (spawned){
				yield return new WaitForSeconds(timeBetweenLoops);//make it wait
			}
		}while(iterations < 100000);
		StartCoroutine(CreateWalls());//move to next step
	}

	/// <summary>
	/// Creates walls along the edges of floors
	/// </summary>
	IEnumerator CreateWalls(){
		//loop though every grid space
		for (int x = 0; x < roomWidth-1; x++){
			for (int y = 0; y < roomHeight-1; y++){
				//if theres a floor, check the spaces around it
				if (grid[x,y] == gridSpace.floor){
					bool placed = false;
					//if any surrounding spaces are empty, place a wall
					if (grid[x,y+1] == gridSpace.empty){
						Spawn(x,y+1,wallObj);
						grid[x,y+1] = gridSpace.wall;
						placed = true;
					}
					if (grid[x,y-1] == gridSpace.empty){
						Spawn(x,y-1,wallObj);
						grid[x,y-1] = gridSpace.wall;
						placed = true;
					}
					if (grid[x+1,y] == gridSpace.empty){
						Spawn(x+1,y,wallObj);
						grid[x+1,y] = gridSpace.wall;
						placed = true;
					}
					if (grid[x-1,y] == gridSpace.empty){
						Spawn(x-1,y,wallObj);
						grid[x-1,y] = gridSpace.wall;
						placed = true;
					}
					if (placed){
						yield return new WaitForSeconds(timeBetweenLoops/2);
					}
				}
			}
		}
		StartCoroutine(RemoveSingleWalls());
	}

	/// <summary>
	/// Removes any single walls within rooms
	/// </summary>
	IEnumerator RemoveSingleWalls(){
		//loop though every grid space
		for (int x = 0; x < roomWidth-1; x++){
			for (int y = 0; y < roomHeight-1; y++){
				//if theres a wall, check the spaces around it
				if (grid[x,y] == gridSpace.wall){
					//assume all space around wall are floors
					bool allFloors = true;
					//check each side to see if they are all floors
					for (int checkX = -1; checkX <= 1 ; checkX++){
						for (int checkY = -1; checkY <= 1; checkY++){
							if (x + checkX < 0 || x + checkX > roomWidth - 1 || 
								y + checkY < 0 || y + checkY > roomHeight - 1){
								//skip checks that are out of range
								continue;
							}
							if ((checkX != 0 && checkY != 0) || (checkX == 0 && checkY == 0)){
								//skip corners and center
								continue;
							}
							if (grid[x + checkX,y+checkY] != gridSpace.floor){
								allFloors = false;
							}
						}
					}
					if (allFloors){
						grid[x,y] = gridSpace.floor;
						Destroy(gridObjects[x,y]);
						gridObjects[x,y] = null;
						Spawn(x,y,floorObj);
						yield return new WaitForSeconds(timeBetweenLoopsLong);
					}
				}
			}
		}
		StartCoroutine(CreateChests());
	}

	IEnumerator CreateChests()
	{
		//To keep from too many chests
		int chestCount = 0;
		for (int x = 0; x < roomWidth - 1; x++)
		{
			
			for (int y = 0; y < roomHeight - 1; y++)
			{
				if (grid[x, y] == gridSpace.floor && chestCount < maxChests)
				{
					//check each side to see if they are all floors
					for (int checkX = -1; checkX <= 1; checkX++)
					{
						for (int checkY = -1; checkY <= 1; checkY++)
						{
							if (x + checkX < 0 || x + checkX > roomWidth - 1 ||
								y + checkY < 0 || y + checkY > roomHeight - 1)
							{
								//skip checks that are out of range
								continue;
							}
							if ((checkX != 0 && checkY != 0) || (checkX == 0 && checkY == 0))
							{
								//Skip center and corners
								continue;
							}
							if (grid[x + checkX, y + checkY] == gridSpace.wall)
							{
								//Chest spawns favors more walls
								chanceChestSpawn += chanceChestWallFavor;
							}
						}
					}
					bool placed = false;
					if (Random.value < chanceChestSpawn)
					{
						Spawn(x, y, chestObj);
						chestCount++;
						placed = true;
					}
					if (placed)
					{
						yield return new WaitForSeconds(timeBetweenLoops / 2);
					}
					chanceChestSpawn = 0;
				}
			}
		}
		StartCoroutine(CreateEnemies());
	}

	/// <summary>
	/// Creates enemies within rooms
	/// </summary>
	IEnumerator CreateEnemies()
	{
		//To keep from too many enemies
		int enemyCount = 0;
		bool placed = false;
		for (int x = 0; x < roomWidth - 1; x++)
		{
			for (int y = 0; y < roomHeight - 1; y++)
			{
				if ((grid[x, y] == gridSpace.floor) && (enemyCount < maxEnemies))
				{
					if (Random.value < chanceEnemySpawn)
					{
						Spawn(x, y, enemyObj);
						placed = true;
						enemyCount++;
					}
					if (placed)
						yield return new WaitForSeconds(timeBetweenLoops / 2);
				}	
			}
		}
	}

	/// <summary>
	/// Instantites appropriate object
	/// </summary>
	/// <param name="x">Object's x position</param>
	/// <param name="y">Object's y position</param>
	/// <param name="toSpawn">Object to be spawned</param>
	void Spawn(float x, float y, GameObject toSpawn){
		//find the position to spawn
		Vector2 offset = roomSizeWorldUnits / 2.0f;
		Vector2 spawnPos = new Vector2(x,y) * worldUnitsInOneGridCell - offset;
		//spawn object
		GameObject obj = Instantiate(toSpawn, spawnPos, Quaternion.identity);
		gridObjects[(int)x,(int)y] = obj;
	}
}
