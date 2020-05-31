﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitActions {
    //Unit behaviour functions that change values of the unit

    #region hunger // ##################################################################################
    
    public static void HungerEffect(Unit unit) {
        if (unit.amountFed <= 0) {
            UnitActions.TakeDamage(unit, unit.hungerDamage *
                Time.deltaTime * GameManager.gameManager.countsBetweenUpdates);
            unit.amountFed = 0;
        } else {
            unit.amountFed -= unit.hungerPerSecond *
                Time.deltaTime * GameManager.gameManager.countsBetweenUpdates;
        }
    }

    public static void TargetFood(Unit unit) {
        GameObject closestFood = UnitQueries.ClosestFoodInView(unit);
        if (closestFood == null) return;
        unit.GetComponent<Target>().Change(closestFood, closestFood.GetComponent<Food>().radius);
    }

    public static void Eat(Unit unit) {
        //eat from the source at most however much space they have on their stomach
        if (unit.GetComponent<Target>() == null) return;
        unit.amountFed += unit.GetComponent<Target>().targetGameObject.GetComponent<Food>().Eat(unit.maxFed - unit.amountFed);
    }

    public static void TurnFed(Unit unit) {
        unit.hungry = false;
    }
    
    public static void TurnHungryChance(Unit unit) {
        float random = Random.Range(0f, 1f);
        float hungerRatio = (unit.amountFed / unit.maxFed);
        float chanceOfHungerPerSec = Mathf.Pow(1 - hungerRatio, unit.hungerChanceExponent) *
            Time.deltaTime * GameManager.gameManager.countsBetweenUpdates;
        if (random < chanceOfHungerPerSec) {
            unit.hungry = true;
        }
    }

    #endregion

    #region thirst // ################################################################################
   
    public static void ThirstEffect(Unit unit) {
        if (unit.amountQuenched <= 0) {
            UnitActions.TakeDamage(unit, unit.thirstDamage *
                Time.deltaTime * GameManager.gameManager.countsBetweenUpdates);
            unit.amountQuenched = 0;
        } else {
            unit.amountQuenched -= unit.thirstPerSecond *
                Time.deltaTime * GameManager.gameManager.countsBetweenUpdates;
        }
    }

    public static void TurnQuenched(Unit unit) {
        unit.thirsty = false;
    }

    public static void TargetWater(Unit unit) {
        Vector3 closestWaterPoint = UnitQueries.ClosestWaterSource(unit);
        unit.GetComponent<Target>().Change(closestWaterPoint);
    }

    public static void Drink(Unit unit) {
        unit.amountQuenched = Mathf.Min(unit.maxQuenched, unit.amountQuenched + unit.quenchRate *
            Time.deltaTime * GameManager.gameManager.countsBetweenUpdates);
    }

    public static void TurnThirstyChance(Unit unit) {
        float random = Random.Range(0f, 1f);
        float thirstRatio = (unit.amountQuenched / unit.maxQuenched);
        float chanceOfThirstPerSec = Mathf.Pow(1 - thirstRatio, unit.thirstChanceExponent) *
            Time.deltaTime * GameManager.gameManager.countsBetweenUpdates;
        if (random < chanceOfThirstPerSec) {
            unit.thirsty = true;
        }
    }


    #endregion

    #region mating // ################################################################################
  
    public static void TargetMate(Unit unit) {
        GameObject closestMate = UnitQueries.ClosestMateInView(unit);
        if (closestMate == null) return;
        unit.GetComponent<Target>().Change(closestMate, closestMate.GetComponent<Unit>().matingDistance);
    }

    public static void Mate(Unit unit) {
        GameObject closestMateObj = UnitQueries.ClosestMateInView(unit);
        if (closestMateObj == null) return;
        Unit closestMate = closestMateObj.GetComponent<Unit>();
        closestMate.horny = false;
        unit.horny = false;

        List<Unit> allies = GameManager.gameManager.petList;
        List<Unit> enemies = GameManager.gameManager.enemyList;
        if ( allies.Count + enemies.Count < GameManager.gameManager.maxUnits) {
            //todo: spawn in position of parents: force into orbit
            GameManager.gameManager.GetSpeciesFromName(unit.speciesName).Spawn(GameManager.gameManager.unitPrefab);
        }
    }

    public static void TurnHornyChance(Unit unit) {
        float random = Random.Range(0f, 1f);
        float chanceMultiplier = (unit.health / unit.maxHealth) 
            * (unit.amountQuenched / unit.maxQuenched) * (unit.amountFed / unit.maxFed);
        chanceMultiplier = Mathf.Pow(chanceMultiplier, unit.hornyCurveExponent);
        if (random < unit.hornyChancePerSecond * chanceMultiplier *
            Time.deltaTime * GameManager.gameManager.countsBetweenUpdates) {
            unit.horny = true;
        }
    }

    #endregion

    #region genetium // ################################################################################

    public static void TargetGenetium(Unit unit) {
        GameObject closestGenetium = UnitQueries.ClosestGenetiumInView(unit);
        if (closestGenetium == null) return;
        unit.GetComponent<Target>().Change(closestGenetium, closestGenetium.GetComponent<Genetium>().radius);
    }

    public static void TargetBase(Unit unit) {
        //TODO: create variables for base and target and set them in inspector
        GameObject home = GameObject.FindGameObjectWithTag("Base");
        unit.GetComponent<Target>().Change(home, 1f);
    }

    public static void ReachBase(Unit unit) {
        unit.currentGenetiumAmount = 0;
        GameManager.gameManager.GetSpeciesFromName(unit.speciesName).UpdateUnit(unit);
        unit.needsChange = false;
    }

    public static void Harvest(Unit unit) {
        if (unit.currentGenetiumAmount > unit.carryingCapacity) return;
        Genetium genetium = unit.GetComponent<Target>().targetGameObject.GetComponent<Genetium>();
        if (genetium == null) return;
        float amountHarvested = Mathf.Min(genetium.transferRate *
            Time.deltaTime * GameManager.gameManager.countsBetweenUpdates, genetium.currentAmount);
        genetium.currentAmount -= amountHarvested;
        unit.currentGenetiumAmount += amountHarvested;
    }

    #endregion

    #region attacking // ################################################################################

    public static void TargetEnemy(Unit unit) {
        GameObject closestEnemy = UnitQueries.ClosestEnemyInThreatRange(unit);
        if (closestEnemy == null || closestEnemy.GetComponent<Unit>() == null) return;
        unit.GetComponent<Target>().Change(closestEnemy, closestEnemy.GetComponent<Unit>().interactionRadius);
    }

    public static void SetFleeingTarget(Unit unit, Unit enemy) {
        Vector3 position = enemy.transform.position;
        RaycastHit hitInfo = new RaycastHit();

        bool hit = Physics.Raycast(position - 20f * (position - GameManager.gameManager.planet.transform.position),
            (position - GameManager.gameManager.planet.transform.position), out hitInfo,
            Mathf.Infinity, 1 << LayerMask.NameToLayer("Planet"));
        if (hit) unit.GetComponent<Target>().Change(hitInfo.point);
    }

    public static void Attack(Unit unit) {
        if (Time.time - unit.lastAttacked < unit.attackRate) return;
        unit.lastAttacked = Time.time;
        if (unit.GetComponent<Target>().targetGameObject == null) return;
        Unit enemy = unit.GetComponent<Target>().targetGameObject.GetComponent<Unit>();
        UnitActions.TakeDamage(enemy, unit.attackDamage);

        //todo: handle this properly. rethink how attacking will work.
        //todo:attack animation (simulate force)
    }

    public static void Flee(Unit unit) {
        GameObject closestEnemy = UnitQueries.ClosestEnemyInThreatRange(unit);
        if (closestEnemy == null || closestEnemy.GetComponent<Unit>() == null) return;
        SetFleeingTarget(unit, closestEnemy.GetComponent<Unit>());
    }

    #endregion

    #region health // ################################################################################

    public static void Dead(Unit unit) {
        //todo: handle this with a corroutine in die.
        if (Time.time - unit.deathTimeStamp > unit.deathPeriod) {
            Object.Destroy(unit.gameObject);
        }
    }

    public static void Die(Unit unit) {
        unit.animator.enabled = false;
        unit.dead = true;
        unit.deathTimeStamp =
            Time.deltaTime * GameManager.gameManager.countsBetweenUpdates;
    }

    public static void HealthRegenEffect(Unit unit) {
        if (unit.health < unit.maxHealth) {
            unit.health += unit.healthRegen *
                Time.deltaTime * GameManager.gameManager.countsBetweenUpdates;
        }
    }

    public static void TakeDamage(Unit unit, float damage) {
        if (unit.dead) return;
        unit.health -= damage;
        unit.healthbar.size = new Vector2(unit.health / unit.maxHealth * 9f, 1);
        if (unit.health <= 0) UnitActions.Die(unit);
    }

    public static void SetHealthBarPivot(Unit unit) {
        //TODO: make sure thought and healthbar are rotated simultaneously in one function
        unit.healthbarPivot.transform.rotation = Camera.main.transform.rotation;
    }

    #endregion

    #region selection // ################################################################################

    public static void SelectAllUnitsOfSpecies(Species species)
    {
        List<Unit> units = species.GetAllUnitsOfSpecies();
        foreach (Unit unit in units)
        {
            UnitActions.EnableSelectionGraphic(unit);
        }
    }

    //public static void DisableAreaGraphics() {
    //    GameManager.gameManager.areaGraphic.SetActive(false);
    //}

    public static void ResetSelectionGraphicPosition(Unit unit) {
        //check first raycast collision
        RaycastHit hitInfo = new RaycastHit();
        Vector3 directionToCenter = (GameManager.gameManager.planet.transform.position - unit.transform.position).normalized;
        bool hit = Physics.Raycast(unit.transform.position - directionToCenter * 5,
            directionToCenter, out hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Planet"));
        if (hit) unit.selectionGraphic.transform.position = hitInfo.point;
    }

    public static void EnableSelectionGraphic(Unit unit) {
        unit.selectionGraphic.GetComponent<Canvas>().enabled = true;
        ResetSelectionGraphicPosition(unit);
    }

    public static void DisableSelectionGraphic(Unit unit) {
        unit.selectionGraphic.GetComponent<Canvas>().enabled = false;
    }

    public static void DisableAllSelectionGraphics() {
        
        foreach (Unit pet in GameManager.gameManager.petList) {
            DisableSelectionGraphic(pet);
        }
        foreach (Unit hostile in GameManager.gameManager.enemyList) {
            DisableSelectionGraphic(hostile);
        }
    }

    #endregion

    #region movement // ################################################################################

    public static void Wander(Unit unit) {
        if (UnitQueries.NeedsWanderingDestination(unit)) {
            UnitActions.SetWanderingDestination(unit);
        }
    }

    public static void Move(Unit unit) {
        SetHealthBarPivot(unit);

        if (unit.GetComponent<Target>().IsNear(unit, false)) return;
        //project on plane perrpendicular to unit passing throuugh planet center
        Vector3 projectedDestination = Vector3.ProjectOnPlane(unit.GetComponent<Target>().targetVector3, unit.transform.position);
        if (projectedDestination == Vector3.zero) return; //TODO: handle case where target is in the exact opposite side of the planet

        //move in the target direction
        float speed = unit.speed;
        if (unit.swimming) speed = unit.swimspeed;
        Vector3 deltaDestination = unit.transform.position + (Vector3.Normalize(projectedDestination) * Time.deltaTime * speed);//move on surface

        //snap to planet surface
        unit.transform.position = Vector3.Normalize(deltaDestination) * GameManager.gameManager.planetRadius;



        Quaternion targetRotation = Quaternion.LookRotation(projectedDestination, Vector3.up);
        Quaternion deltaTargetRotation = Quaternion.Lerp(unit.transform.rotation, targetRotation, Time.deltaTime * unit.rotationSpeed);
        unit.transform.rotation = deltaTargetRotation;

        //gravity effect
        unit.transform.rotation = Quaternion.FromToRotation(unit.transform.up, deltaDestination) * deltaTargetRotation;




    }

    public static void OverrideTarget(Unit unit, Vector3 target) {
        unit.GetComponent<Target>().Change(target);
        unit.unitState = UnitState.Override;

    }

    public static void ShowTargetGraphic(Unit unit)
    {
        MonoBehaviour.Instantiate(unit.targetGraphic).GetComponent<TargetGraphic>()
            .SetPosition(unit.GetComponent<Target>().targetVector3, GameManager.gameManager.planet.transform.position);
    }

    //Find a random point in planet's surface 
    public static void SetWanderingDestination(Unit unit) {
        //random position somewhere in a sphere around the unit target
        Vector3 position = (Random.onUnitSphere * unit.areaRadius + unit.areaCenter);

        //project on planet. raycast has to be projected from the sky
        RaycastHit hitInfo = new RaycastHit();

        bool hit = Physics.Raycast(position + 20f * (position - GameManager.gameManager.planet.transform.position),
            (GameManager.gameManager.planet.transform.position - position), out hitInfo,
            Mathf.Infinity, 1 << LayerMask.NameToLayer("Planet"));
        if (hit) {
            unit.GetComponent<Target>().Change(hitInfo.point);
        }

        unit.wanderTimeStamp = Time.time;
    }

    public static void WanderIfDeadTarget(Unit unit) {
        if (
            unit.GetComponent<Target>().targetGameObject
            && unit.GetComponent<Target>().targetGameObject.GetComponent<Unit>()
            && unit.GetComponent<Target>().targetGameObject.GetComponent<Unit>().dead
        ) {
            unit.unitState = UnitState.Wander;
        }
    }

    #endregion

    // ###################################################################################################

    //public static void GravityEffect(Unit unit) {
    //    GameManager.gameManager.planet.GetComponent<GravityAttractor>().Attract(unit.transform);
    //}


    public static void SetThought(Unit unit) {
        //unit.thoughtPivot.transform.rotation = Camera.main.transform.rotation;
        if (unit.unitState == UnitState.TargetEnemy || unit.unitState == UnitState.Attack) {
            unit.thoughtRenderer.sprite = unit.attackSprite;
        } else if (unit.unitState == UnitState.TargetGenetium || unit.unitState == UnitState.Harvest) {
            unit.thoughtRenderer.sprite = unit.genetiumSprite;
        } else if (unit.unitState == UnitState.TargetBase) {
            unit.thoughtRenderer.sprite = unit.baseSprite;
        } else if (unit.hungry) {
            unit.thoughtRenderer.sprite = unit.hungrySprite;
        } else if (unit.thirsty) {
            unit.thoughtRenderer.sprite = unit.thirstSprite;
        } else if (unit.horny) {
            unit.thoughtRenderer.sprite = unit.hornySprite;
        } else {
            unit.thoughtRenderer.sprite = null;
        }
    }
}