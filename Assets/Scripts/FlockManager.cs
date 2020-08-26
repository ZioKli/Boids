
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject boid;
    public GameObject temp;
    public float boxSize;
    public List<GameObject> boids;
    public OctTree oct;
    public int queryInterval;
    public int count;
    public SimSettings settings;
    Vector3 loc;
    Boid b;
    private void Start() {
        DontDestroyOnLoad(this);
    }
    public void BeginSim(SimSettings newSettings) {
        settings = newSettings;
        count = newSettings.boidCount;
        queryInterval = newSettings.queryInterval;
        oct = new OctTree(new Vector3(boxSize / 2, boxSize / 2, boxSize / 2), boxSize, null);
        for (int i = 0; i < count; i++) {
            loc = new Vector3(Random.Range(5, boxSize - 5), Random.Range(5, boxSize - 5), Random.Range(5, boxSize - 5));
            temp = Instantiate(boid, loc, Quaternion.identity, transform);

            b = temp.GetComponent<Boid>();
            b.queryStep = i % queryInterval;
            b.queryInterval = queryInterval;
            b.viewAngle = newSettings.viewAngle;
            b.viewRange = newSettings.viewRange;

            boids.Add(temp);
            oct.AddBoid(temp);
        }
    }
    //private void OnDrawGizmos() {
    //    if (oct != null) {
    //        oct.Show();
    //    }
    //}

    private void FixedUpdate() {
        //oct = new OctTree(new Vector3(boxSize / 2, boxSize / 2, boxSize / 2), boxSize, null);
        //foreach (GameObject g in GameObject.FindGameObjectsWithTag("Boid")) {
        //    oct.AddBoid(g);
        //}
    }
}


public class OctTree {
    public bool isSubdivided;
    public bool isChild;
    public int capacity;
    public int totalElementsInAllChildren;
    public List<GameObject> boids;
    public OctTree frontNW, frontNE, frontSW, frontSE, backNW, backNE, backSW, backSE, parent;
    public Vector3 center;
    public Boundary bound;
    public float size;
    Vector3 pos;
    Vector3 newCenter;
    Boid currentBoid;

    public bool AddBoid(GameObject boid) {
        pos = boid.transform.position;

        //checks to make sure that the boid is within the boundaries of the current node;
        if    (pos.x < bound.rangeX.x || pos.x >= bound.rangeX.y
            || pos.y < bound.rangeY.x || pos.y >= bound.rangeY.y
            || pos.z < bound.rangeZ.x || pos.z >= bound.rangeZ.y) {
            return false;
        }
        totalElementsInAllChildren++;
        //if the current node has space for the boid it is added to the current node and no subdivision occurs
        if (boids.Count < capacity) {
            currentBoid = boid.GetComponent<Boid>();
            currentBoid.bound = bound;
            currentBoid.currentParent = this;
            boids.Add(boid);
            return true;
        }

        //if the node isn't subdivided then we first subdivide, and then we iterate through all the octets and try to add the boid into one of them
        if (!isSubdivided) {
            Subdivide();
        }
        if (frontNW.AddBoid(boid)) {
            return true;
        }
        if (frontNE.AddBoid(boid)) {
            return true;
        }
        if (frontSW.AddBoid(boid)) {
            return true;
        }
        if (frontSE.AddBoid(boid)) { 
            return true;
        }
        if (backNW.AddBoid(boid)) { 
            return true;
        }
        if (backNE.AddBoid(boid)) {
            return true;
        }
        if (backSW.AddBoid(boid)) {
            return true;
        }
        if (backSE.AddBoid(boid)) {
            return true;
        }
        return false;
    }

    public void Subdivide() {
        newCenter = new Vector3(center.x - size / 4, center.y + size / 4, center.z + size / 4);
        backNW = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x + size / 4, center.y + size / 4, center.z + size / 4);
        backNE = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x - size / 4, center.y - size / 4, center.z + size / 4);
        backSW = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x + size / 4, center.y - size / 4, center.z + size / 4);
        backSE = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x - size / 4, center.y + size / 4, center.z - size / 4);
        frontNW = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x + size / 4, center.y + size / 4, center.z - size / 4);
        frontNE = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x - size / 4, center.y - size / 4, center.z - size / 4);
        frontSW = new OctTree(newCenter, size / 2, this);

        newCenter = new Vector3(center.x + size / 4, center.y - size / 4, center.z - size / 4);
        frontSE = new OctTree(newCenter, size / 2, this);

        isSubdivided = true;
    }

    public OctTree(Vector3 center, float size, OctTree parent, int capacity = 4) {
        if(parent != null) {
            this.parent = parent;
            isChild = true;
        }
        else {
            isChild = false;
        }

        boids = new List<GameObject>();
        this.center = center;
        this.size = size;
        this.capacity = capacity;
        bound = new Boundary();
        bound.rangeX = new Vector2(center.x - size / 2, center.x + size / 2);
        bound.rangeY = new Vector2(center.y - size / 2, center.y + size / 2);
        bound.rangeZ = new Vector2(center.z - size / 2, center.z + size / 2);
        totalElementsInAllChildren = 0;
    }

    public void Show() {
        Gizmos.color = Color.red;
        if (!isSubdivided) {
            Gizmos.DrawWireCube(center, new Vector3(size, size, size));
        }
        else {
            frontNW.Show();
            frontNE.Show();
            frontSW.Show();
            frontSE.Show();
            backNW.Show();
            backNE.Show();
            backSW.Show();
            backSE.Show();
        }
    }

    public bool CubeIntersects(Vector3 center, float radius) {
        return center.x - radius < bound.rangeX.y && center.x + radius > bound.rangeX.x
            && center.y - radius < bound.rangeY.y && center.y + radius > bound.rangeY.x
            && center.z - radius < bound.rangeZ.y && center.z + radius > bound.rangeZ.x;
    }

    public List<GameObject> Query(Vector3 center, float radius) {
        List<GameObject> elements = new List<GameObject>();
        if (CubeIntersects(center, radius)) { 
            foreach(GameObject b in boids) {
                elements.Add(b);
            }
            if (isSubdivided) {
                elements.AddRange(frontNW.Query(center, radius));
                elements.AddRange(frontNE.Query(center, radius));
                elements.AddRange(frontSW.Query(center, radius));
                elements.AddRange(frontSE.Query(center, radius));
                elements.AddRange(backNW.Query(center, radius));
                elements.AddRange(backNE.Query(center, radius));
                elements.AddRange(backSW.Query(center, radius));
                elements.AddRange(backSE.Query(center, radius));
            }
        }
        return elements;
    }

    public void Disolve() {
        if (totalElementsInAllChildren < capacity) {
            if (isSubdivided) {
                frontNW.PushBoidsToParent();
                frontNE.PushBoidsToParent();
                frontSW.PushBoidsToParent();
                frontSE.PushBoidsToParent();
                backNW.PushBoidsToParent();
                backNE.PushBoidsToParent();
                backSW.PushBoidsToParent();
                backSE.PushBoidsToParent();
                frontNW = null;
                frontNE = null;
                frontSW = null;
                frontSE = null;
                backNW = null;
                backNE = null;
                backSW = null;
                backSE = null;
                isSubdivided = false;
            }
        }
    }

    public void PushBoidsToParent() {
        if (isSubdivided) {
            frontNW.PushBoidsToParent();
            frontNE.PushBoidsToParent();
            frontSW.PushBoidsToParent();
            frontSE.PushBoidsToParent();
            backNW.PushBoidsToParent();
            backNE.PushBoidsToParent();
            backSW.PushBoidsToParent();
            backSE.PushBoidsToParent();
        }
        foreach(GameObject b in boids) {
            parent.AddBoid(b);
        }
        boids.Clear();
    }

    public bool RecalculatePositionInTree(GameObject b) {
        if (boids.Contains(b)) {
            boids.Remove(b);
        }
        if (b.transform.position.x < bound.rangeX.x || b.transform.position.x >= bound.rangeX.y 
            || b.transform.position.y < bound.rangeY.x || b.transform.position.y >= bound.rangeY.y 
            || b.transform.position.z < bound.rangeZ.x || b.transform.position.z >= bound.rangeZ.y) {
            if(parent == null) {
                return false;
            }
            Disolve();
            totalElementsInAllChildren--;
            return parent.RecalculatePositionInTree(b);
        }
        else {
            totalElementsInAllChildren--;
            return(AddBoid(b));
        }
    }
}
public struct Boundary{
    public Vector2 rangeY;
    public Vector2 rangeX;
    public Vector2 rangeZ;
}