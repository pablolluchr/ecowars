﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesButton : MonoBehaviour
{
    public bool selected;
    public Color selectedColor;
    public Color normalColor;
    public Species species;
    public SpeciesSelectionPanel speciesSelectionPanel;
    public string unitType;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SelectSpecies);
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = species.speciesName;
        speciesSelectionPanel = transform.parent.GetComponent<SpeciesSelectionPanel>();

    }

    public void SelectSpecies()
    {
        GameManager.gameManager.UpdateSelectedSpecies(species);
        //speciesSelectionPanel.Hide();
        //todo: close species selection?
        //if (selected)
        //{ //Second click (only for pets)
        //    if (unitType == "Pet")
        //    {
        //        //TODO: UPDATE THE LOGIC
        //        GameManager.gameManager.selectedSpecies = null;
        //        GameManager.gameManager.cameraController.StartPanning();

        //        //Show attribute panel for the species and hide everything else
        //        GameManager.gameManager.attributePanel.GetComponent<AttributePanel>().speciesName = speciesName;
        //        GameManager.gameManager.attributePanel.SetActive(true);
        //        GameManager.gameManager.bottomControls.SetActive(false);
        //        transform.parent.gameObject.SetActive(false);
        //    }


        //}
        //else //first click (select)
        //{
        //    GameManager.gameManager.cameraController.ResetMovingToSpeed();


        //    //unselect the other species
        //    SpeciesButton[] speciesButtons = transform.parent.GetComponentsInChildren<SpeciesButton>();
        //    foreach (SpeciesButton speciesButton in speciesButtons)
        //    {
        //        speciesButton.GetComponent<Image>().color = normalColor;
        //        speciesButton.selected = false;
        //    }
        //    //update button state and graphics
        //    GetComponent<Image>().color = selectedColor;
        //    selected = true;

        //}
    }
}
