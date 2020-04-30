﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    private GameObject target;
    public float bioFuel;
    public CameraController cameraController;
    public GameObject dummyTargets;
    public GameObject units;
    public GameObject unitPrefab;
    public GameState gameState;
    public List<Species> speciesList = new List<Species>();

    // to update a species first change the attributes in the species class using GetSpecies("Tall").speed = 100 etc.
    // then set this attribute (recall species) to the species name (i.e. "Tall") to recall all pets
    public string recallSpecies = null;

    private float lastMouseX;
    private float lastMouseY;
    public float shortClickDuration=.3f;
    private bool isDragging;
    public bool wasButtonDown;
    public string selectedSpecies;

    private void Awake()
    {
        if (gameManager != null && gameManager != this) Destroy(this.gameObject);
        else gameManager = this;

        lastMouseX = Mathf.Infinity;
        lastMouseY = Mathf.Infinity;
        target = null;
        selectedSpecies = null;

        AddSpecies("Tall", 1.5f, 0.6f, 0.2f, 0.2f,new Vector3(-0.09f, 5.48f, -2.99f), 2f,"Pet");
        AddSpecies("Fast", 3f,   0.2f, 0.2f, 0.2f,new Vector3(0, -5f, 5.5f),          2f,"Hostile");



        GetSpecies("Tall").Spawn(unitPrefab);
        GetSpecies("Tall").Spawn(unitPrefab);
        GetSpecies("Tall").Spawn(unitPrefab);

        GetSpecies("Fast").Spawn(unitPrefab);
        GetSpecies("Fast").Spawn(unitPrefab);
        GetSpecies("Fast").Spawn(unitPrefab);

    }

    public void AddSpecies(string name, float speed, float legsLength, float bodySize, float headSize,Vector3 areaCenter, float areaRadius,string tag) {
        speciesList.Add(new Species(name, speed, legsLength, bodySize, headSize, areaCenter, areaRadius,tag));
    }

    public Species GetSpecies(string name) {
        foreach (Species species in speciesList) {
            if (species.speciesName == name) {
                return species;
            }
        }
        Debug.Log("species not found");
        return null;
    }

    void Update()
    {

        if (recallSpecies != null) {
            GameObject[] pets = GameObject.FindGameObjectsWithTag("Pet");
            GameObject[] filteredSpecied = UnitHelperFunctions.FilterSpecies(pets, recallSpecies);
            foreach (GameObject pet in filteredSpecied) {
                pet.GetComponent<Unit>().needsChange = true;
            }
            recallSpecies = null;
        }
        
        RaycastHit hitInfo;
        bool hit;

        SetDraggingState();
        if (target == null && gameState == GameState.Following)
        {
            gameState = GameState.Panning;
            cameraController.StartPanning();
        }

        //stop following unit if species selected
        if (selectedSpecies != null && target != null)
        {
            UnitActions.DisableSelectionGraphic(target.GetComponent<Unit>());
            target = null;
            //select all units of species

        }


        //SHORT CLICK
        if (Input.GetMouseButtonUp(0) && !isDragging)
        {

            //check first raycast collision
            hitInfo = new RaycastHit();
            hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);
            //todo: check if ui click and return.


            if (hit)//unit clicked
            {

                if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Units"))
                {
                    gameState = GameState.Following;
                    cameraController.StartFollowing(hitInfo.transform);

                    //unit clicked is a new target
                    if (target != hitInfo.transform.gameObject)
                    {
                        selectedSpecies = null;
                        //disable last selection graphic
                        if (target != null) UnitActions.DisableSelectionGraphic(target.GetComponent<Unit>());
                        target = hitInfo.transform.gameObject;

                        UnitActions.EnableSelectionGraphic(target.GetComponent<Unit>());
                    }

                }
                else
                {
                    //only focus on planet
                    hitInfo = new RaycastHit();
                    hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo,
                        Mathf.Infinity, 1 << LayerMask.NameToLayer("Planet"));

                    //if a pet is being controlled and user hits map: override
                    if (target != null && hit)
                    {
                        if (target.tag == "Pet" || target.transform.parent.tag == "Pet")
                            UnitActions.OverrideTarget(target.GetComponent<Unit>(), hitInfo.point);

                    }
                    else if (selectedSpecies != null)
                    {
                        //change species area and update all the relevant units with the new info

                        //show area selection animation
                    }
                }
            }




        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (isDragging && gameState == GameState.Following)
        {
            gameState = GameState.Panning;
            cameraController.StartPanning();
        }
        
        

        
    }


    //check for dragging behaviour if mouse was pressed on the previous frame in a different position
    // and it's still pressed now
    public void SetDraggingState()
    {
        if (Input.GetMouseButton(0))
        {
            

            if (wasButtonDown)
            {
                isDragging = Mathf.Pow(Input.GetAxis("Mouse X") - lastMouseX, 2) +
                Mathf.Pow(Input.GetAxis("Mouse Y") - lastMouseY, 2) > 0.005f || isDragging;
            }

            lastMouseX = Input.GetAxis("Mouse X");
            lastMouseY = Input.GetAxis("Mouse Y");
            wasButtonDown = true;
        }
        else
        {
            wasButtonDown = false;

        }
    }

}

public enum GameState
{
 
    Panning, //not following anything. 
    Following,//only able to exit when unit dies
}

