﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitHelperFunctions {
    public static GameObject GetClosest(Unit unit, GameObject[] objects) {
        float closestDistance = Mathf.Infinity;
        GameObject closestObj = null;
        foreach (GameObject obj in objects) {
            float distance = (unit.transform.position - obj.transform.position).magnitude;
            if (distance < closestDistance) {
                closestDistance = distance;
                closestObj = obj;
            }
        }
        return closestObj;
    }
    public static bool InRangeOf(Unit unit, GameObject[] objects, float range) {
        foreach (GameObject obj in objects) {
            float distance = (unit.transform.position - obj.transform.position).magnitude;
            if (distance < range) {
                return true;
            }
        }
        return false;
    }

    public static GameObject[] FilterNonHornyPetsAndSelf(Unit unit, GameObject[] pets) {
        List<GameObject> hornyPets = new List<GameObject>();
        foreach (GameObject pet in pets) {
            Unit petUnit = pet.GetComponent<Unit>();
            if (petUnit && petUnit.horny && petUnit != unit) {
                hornyPets.Add(pet);
            }
        }
        return hornyPets.ToArray();
    }

    public static GameObject[] FilterEmptyFoods(GameObject[] foods) {
        List<GameObject> nonEmptyFoods = new List<GameObject>();
        foreach (GameObject food in foods) {
            Food foodComponent = food.GetComponent<Food>();
            if (foodComponent.availableFood > foodComponent.consideredEmpty) {
                nonEmptyFoods.Add(food);
            }
        }
        return nonEmptyFoods.ToArray();
    }

    
}