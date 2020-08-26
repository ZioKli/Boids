using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Boid : MonoBehaviour
{
    [HideInInspector]
    public Vector3 heading;
    public int queryInterval;
    public int queryStep;
    public float speed = 20;
    public float viewRange = 20;
    public int maxNearby = 4;
    public List<Boid> nearbyBoids;

    [Range(0,180)]
    public float viewAngle = 180;
    
    [Range(0,.1f)]
    public float turningFraction;

    public float avoidDistance = 5;
    [HideInInspector]
    public Boundary bound;
    public OctTree currentParent;
    [Range(0,1)]
    public float separateWeight, alignWeight, coheseWeight;
    public float boxSize;
    public FlockManager flock;
    private Vector3 target, separateTarget, coheseTarget, alignTarget;
    private float angleTo;
    private float distanceTo;
    private List<GameObject> unverifiedLocalBoids;
    GameObject b;



    void Start() {
        heading = Random.onUnitSphere;
        flock = GetComponentInParent<FlockManager>();
        boxSize = flock.boxSize;
    }

    private void FixedUpdate() {
        //move should come last after the final heading is calculated
        alignTarget = coheseTarget = separateTarget = Vector3.zero;
        if (NeedsQuery()) {
            GetNearby();    
        }
        if(nearbyBoids.Count != 0) {
            Cohese();
            Align();
            Separate();
            WeightTargets();
            heading = Vector3.Lerp(heading, target, turningFraction);
        }
        Move();
        Reposition();
    }

    private void OnCollisionEnter(Collision collision) {
        Warp(collision);
    }

    void Separate() {
        if (nearbyBoids.Count == 0) {
            return;
        }
        foreach (Boid b in nearbyBoids) {

            distanceTo = (b.transform.position - transform.position).magnitude;
            if (distanceTo < avoidDistance) {
                separateTarget -= b.transform.position - transform.position; 
            }
            //Debug.DrawRay(transform.position, separateTarget, Color.green);
            separateTarget.Normalize();
        }
    }

    void Align() {
        if (nearbyBoids.Count == 0) {
            return;
        }
        foreach (Boid b in nearbyBoids) {
            alignTarget += b.heading;
        }
        //Debug.DrawRay(transform.position, alignTarget, Color.magenta);
        alignTarget.Normalize();
    }

    void Cohese() {
        if (nearbyBoids.Count == 0) {
            return;
        }
        float x, y, z;
        x = y = z = 0; 
        foreach(Boid b in nearbyBoids) {
            x += b.transform.position.x;
            y += b.transform.position.y;
            z += b.transform.position.z;
        }
        x /= nearbyBoids.Count;
        y /= nearbyBoids.Count;
        z /= nearbyBoids.Count;
        coheseTarget = new Vector3(x, y, z) - transform.position;
        //Debug.DrawRay(transform.position, coheseTarget, Color.red);
        coheseTarget.Normalize();
    }

    void Warp(in Collision other) {
        //depending on what type of wall was collided with the boid will teleport to the opposite side of the field
        if (other.gameObject.CompareTag("xWall")) {
            if(transform.position.x > boxSize / 2) {
                transform.position = new Vector3(2, transform.position.y, transform.position.z);
            }
            else {
                transform.position = new Vector3(boxSize - 2, transform.position.y, transform.position.z);
            }
        }
        else if (other.gameObject.CompareTag("yWall")){
            if(transform.position.y > boxSize / 2) {
                transform.position = new Vector3(transform.position.x, 2, transform.position.z);
            }
            else {
                transform.position = new Vector3(transform.position.x, boxSize - 2, transform.position.z);
            }
        }
        else if (other.gameObject.CompareTag("zWall")) {
            if (transform.position.z > boxSize / 2) {
                transform.position = new Vector3(transform.position.x, transform.position.y, 2);
            }
            else {
                transform.position = new Vector3(transform.position.x, transform.position.y, boxSize - 2);
            }
        }
    }

    void Move() {
        transform.LookAt(transform.position + heading); // this makes the boid look where it is going
        Debug.DrawRay(transform.position, heading * 5, Color.green);
        transform.position += heading * speed * Time.fixedDeltaTime; // move in the along the heading
    }

    void GetNearby() {
        if(unverifiedLocalBoids == null) {
            unverifiedLocalBoids = new List<GameObject>();
        }
        nearbyBoids.Clear();
        unverifiedLocalBoids.Clear();
        unverifiedLocalBoids = flock.oct.Query(transform.position, viewRange);
        int found = 0;
        int index = 0;
        while (found < maxNearby && index < unverifiedLocalBoids.Count) {
            b = unverifiedLocalBoids[index];
            angleTo = Vector3.SignedAngle(transform.position, b.transform.position, Vector3.Cross(transform.position, b.transform.position).normalized);
            distanceTo = (b.transform.position - transform.position).magnitude;
            if (Mathf.Abs(angleTo) < viewAngle && distanceTo <= viewRange) {
                found++;
                nearbyBoids.Add(b.GetComponent<Boid>());
            }
            index++;
        }
    }

    void WeightTargets() {
        target += (coheseTarget * coheseWeight) + (alignTarget * alignWeight) + (separateTarget * separateWeight);
        //Debug.DrawRay(transform.position, target, Color.blue);
        target.Normalize();
    }

    bool NeedsQuery() {
        if(queryStep == queryInterval) {
            queryStep = 0;
            return true;
        }
        else {
            queryStep++;
            return false;
        }
    }
    void Reposition() {
        if(transform.position.x < bound.rangeX.x || transform.position.x >= bound.rangeX.y
            || transform.position.y < bound.rangeY.x || transform.position.y >= bound.rangeY.y
            || transform.position.z < bound.rangeZ.x || transform.position.z >= bound.rangeZ.y) {
            currentParent.RecalculatePositionInTree(gameObject);
        }
    }
}
 