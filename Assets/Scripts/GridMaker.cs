using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMaker : MonoBehaviour
{

    //Prefab to instantiate to create the grid
    public GameObject gridCellPrefab;

    //Width and height of the grid
    public int gridWidth;
    public int gridHeight;

    //Width of the actual grid cell
    float gridSizeX;

    //Offset between grid cells
    float offset = 0.05f;

    SortedList<string, GameObject> m_gridCellList;

    public GameObject GetGridCellByTag(string tag)
    {
        GameObject obj;
        m_gridCellList.TryGetValue(tag, out obj);
        return obj;
    }

    // Use this for initialization
    void Start()
    {
        //Get the sprite renderer component from our prefab
        SpriteRenderer sr = gridCellPrefab.GetComponent<SpriteRenderer>();
        //Get the width of the sprite by accessing it's bounds memeber.
        gridSizeX = sr.bounds.size.x;

        //Initialise grid cell list
        m_gridCellList = new SortedList<string, GameObject>();

        //Create the actual grid
        CreateGrid();
    }


    void CreateGrid()
    {
        //Loop through how many cells wide we want
        for(int i = 0; i < gridWidth; i++)
        {
            //Loop through how many cells high we want
            for (int j = 0; j < gridHeight; j++)
            {
                //Instantiate a new grid cell
                GameObject instance = Instantiate(gridCellPrefab, this.transform);
                //Get it's position 
                Vector3 position = instance.transform.position;

                //Modify the position based on where we are within the loop
                position.x = (i * (gridSizeX + offset));
                //Flip the y pos so we are creating it top -> bottom not bottom -> top
                position.y = (j * (gridSizeX + offset)) * -1;

                //Set the new position
                instance.transform.position = position;

                //Get the text mesh component from this grid cell instance
                TextMesh gridText = instance.GetComponentInChildren<TextMesh>();

                //Get the row we are on and convert to string
                string asciiString = j.ToString();
                //Convert the row number to it's ascii value
                char c = asciiString[0];
                //Conver to a number so we can modify it mathematically
                int asciiVal = c;

                //Add 17 on so it becomes a letter's ascii value 
                //i.e '0' = 48 and we want that to become 'A'= 65 
                asciiVal += 17;
                
                //Convert the ascii value back to it's character value
                c = (char)asciiVal;

                //Change the text mesh's text to display the character and then the collumn number
                string finalString = c + (i+1).ToString();
                gridText.text = finalString;

                //Add the cell to the list, making the cell name it's 'tag'
                m_gridCellList.Add(finalString, instance);

            }
        }

        //Get the position of the transform parent (the gameobject this script is attached to)
        Vector3 overallPos = this.transform.position;

        //Move the grid to the left by half of it's width
        overallPos.x = -((gridWidth * gridSizeX) / 2);
        //Move the grid up by half of it's height
        overallPos.y = ((gridHeight * gridSizeX) / 2);

        //Set the position of the transform parent to this new value
        this.transform.position = overallPos;
    }

    public void ResetAllCells()
    {
        foreach(KeyValuePair<string,GameObject> cell in m_gridCellList)
        {
            SpriteRenderer sr = cell.Value.GetComponent<SpriteRenderer>();
            sr.color = Color.white;
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
