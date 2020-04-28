﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitActions {


    public static void Wander(Unit unit) {
        if (unit.NeedsDestination()) {
            unit.GetDestination();
        }
        unit.Move(unit.destination);
    }

    public static void MoveToWater(Unit unit) {

    }

    public static void MoveToFood(Unit unit) {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        Food closestFood = UnitHelperFunctions.GetClosest(unit, foods).GetComponent<Food>();
        unit.Move(closestFood.transform.position);
    }

    public static void MoveToMate(Unit unit) {
        GameObject[] pets = GameObject.FindGameObjectsWithTag("Pet");
        GameObject[] hornyPets = UnitHelperFunctions.GetOtherHornyPets(unit, pets);
        Unit closestMate = UnitHelperFunctions.GetClosest(unit, hornyPets).GetComponent<Unit>();
        if (closestMate != null) {
            unit.Move(closestMate.transform.position);
        } else {
            unit.GravityEffect();
        }
    }

    public static void MoveToFuel(Unit unit) {

    }

    public static void MoveToBase(Unit unit) {

    }

    public static void Drink(Unit unit) {

    }

    public static void Eat(Unit unit) {
        unit.amountFed += unit.feedingPerSecond * Time.deltaTime;
        unit.GravityEffect();
    }

    public static void Mate(Unit unit) {
        GameObject[] pets = GameObject.FindGameObjectsWithTag("Pet");
        GameObject[] hornyPets = UnitHelperFunctions.GetOtherHornyPets(unit, pets);
        Unit closestMate = UnitHelperFunctions.GetClosest(unit, hornyPets).GetComponent<Unit>();
        closestMate.horny = false;
        unit.horny = false;
        MonoBehaviour.Instantiate(unit);
    }

    public static void Harvest(Unit unit) {

    }

    public static void EngageEnemy(Unit unit) {

    }

    public static void Attack(Unit unit) {

    }

    public static void Flee(Unit unit) {

    }

    public static void TurnHorny(Unit unit) {
        unit.horny = true;
    }
}
