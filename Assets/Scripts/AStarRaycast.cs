using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AStarRaycast : MonoBehaviour
{
    public Transform currentFinalTarget = null;

    public bool solved = false;
    public float collisionRetreat = 1f;

    public int maxIterations = 200;
    public List<PathPoint> openPathPoints = new List<PathPoint>();
    public List<PathPoint> closedPathPoints = new List<PathPoint>();
    public List<Transform> path = new List<Transform>();

    List<GameObject> toDestroyTransforms = new List<GameObject>();

    PathPoint currentPoint;

    public Vector3Int characterPositionInGrid;

    public float rayDistance = 0.2f;

    public List<Transform> FindPathLoop(Transform target)
    {
        List<Transform> pathsTargets = new List<Transform>();

        currentFinalTarget = target;

        solved = false;

        int j = 0;

        RaycastHit[] tempRays;

        PathPoint[] tempPoints;

        Vector3 orign = transform.position;

        RaycastHit hit = new RaycastHit();

        currentPoint = new PathPoint(GetPositionInGrid(orign), hit);

        currentPoint.parent = null;

        for (int i = 0; i <= j; i++)
        {
            tempRays = CastPathfindingRays(orign, target);

            tempPoints = CreatePathPoints(tempRays);

            PathPoint nextPoint = PickNextTarget(tempPoints);

            currentPoint = nextPoint;

            openPathPoints.Remove(currentPoint);
            closedPathPoints.Add(currentPoint);

            if (nextPoint == null) { Debug.Log("next point null"); break; }

            orign = nextPoint.hit.point + (nextPoint.hit.normal * 0.01f);

            if (nextPoint.collisionTransform == currentFinalTarget)
            {

                int parentsIndex = 1;

                PathPoint parent = nextPoint.parent;

                for (int p = 0; p < parentsIndex; p++)
                {

                    if (parent != null)
                    {

                        pathsTargets.Add(parent.newTargetTransform);

                        parent = parent.parent;

                        parentsIndex++;

                        if (parentsIndex > closedPathPoints.Count) { Debug.Log("failed creating path"); }

                    }

                }

                solved = true;

                path = pathsTargets;

                return pathsTargets;

            }
            else
            {
                if (j > maxIterations)
                {
                    Debug.Log("returning the best solution");

                    int parentsIndex = 1;

                    PathPoint parent = nextPoint.parent;

                    for (int p = 0; p < parentsIndex; p++)
                    {

                        if (parent != null)
                        {   
                            if(parent.newTargetTransform != null){
                            pathsTargets.Add(parent.newTargetTransform);
                            }

                            parent = parent.parent;

                            parentsIndex++;

                            if (parentsIndex > closedPathPoints.Count) {Debug.Log("failed creating path"); break; }

                        }

                    }

                    path = pathsTargets;

                    return pathsTargets;
                }

                j++;
            }

        }

        return pathsTargets;
    }

    public PathPoint PickNextTarget(PathPoint[] options)
    {

        float maxCost = float.MaxValue;

        float tempCost = maxCost;

        for (int i = 0; i < options.Length; i++)
        {
            options[i].parent = currentPoint;
        }

        int openIndex = 0;

        int k = 0;

        foreach (PathPoint p in openPathPoints)
        {
            bool walkable = (!p.colided || p.collisionTransform == currentFinalTarget);

            if (walkable)
            {
                if (p.cost < tempCost)
                {
                    openIndex = k;

                    tempCost = p.cost;

                }
            }

            k++;
        }
        
        return openPathPoints[openIndex];

    }

    public PathPoint[] CreatePathPoints(RaycastHit[] hits)
    {
        PathPoint[] pathPoints = new PathPoint[hits.Length];

        for (int i = 0; i < hits.Length; i++)
        {
            pathPoints[i] = new PathPoint(GetPositionInGrid(hits[i].point), hits[i]);

            Transform hitColision = hits[i].transform;

            bool alreadyInList = false;

            GameObject temp = new GameObject();

            if (hitColision != null)
            {
                pathPoints[i].collisionTransform = hitColision;

                pathPoints[i].newTargetTransform = temp.transform;

                pathPoints[i].newTargetTransform.position =  hits[i].point + ((transform.position - hits[i].point).normalized * collisionRetreat);

                pathPoints[i].colided = true;

            }
            else
            {

                pathPoints[i].newTargetTransform = temp.transform;

                // no collision, parameter created manualy in CastRay
                pathPoints[i].newTargetTransform.position = hits[i].point;

                pathPoints[i].colided = false;
            }

            toDestroyTransforms.Add(temp);

            pathPoints[i].cost = Vector3.Distance(currentFinalTarget.position, pathPoints[i].hit.point);

            foreach (PathPoint p in openPathPoints)
            {
                if (p.gridPosition == pathPoints[i].gridPosition) { alreadyInList = true; }
            }

            foreach (PathPoint p in closedPathPoints)
            {
                if (p.gridPosition == pathPoints[i].gridPosition) { alreadyInList = true; }
            }

            if (!alreadyInList)
            {

                openPathPoints.Add(pathPoints[i]);
            }

        }

        return pathPoints;
    }

    public RaycastHit[] CastPathfindingRays(Vector3 _orign, Transform target, int _rays = 8)
    {
        int rays = _rays;

        RaycastHit[] hits = new RaycastHit[rays];

        Vector3 orign = _orign;

        Vector3 targetDirection = target.position - orign;

        float targetDistance = targetDirection.magnitude;

        for (int i = 0; i < rays; i++)
        {
            RaycastHit raycastHit;

            float angle = (360 / rays) * i;

            targetDirection = Quaternion.AngleAxis(angle, Vector3.up) * targetDirection;

            raycastHit = CastRay(orign, targetDirection);

            hits[i] = raycastHit;
        }
        return hits;
    }

    public RaycastHit CastRay(Vector3 _pos, Vector3 _dir)
    {
        RaycastHit temp = new RaycastHit();

        Vector3 startPosition = _pos;

        Vector3 targetDirection = _dir.normalized;

        float distanceToTarget = rayDistance;

        if (!Physics.Raycast(startPosition, targetDirection, out temp, distanceToTarget))
        {
            temp = new RaycastHit();

            temp.point = startPosition + ((targetDirection * distanceToTarget));
            temp.distance = rayDistance;
            temp.normal = targetDirection;
        }

        temp.point -= (targetDirection) * 0.5f;

        Debug.DrawRay(startPosition, targetDirection * distanceToTarget, Color.red, 100f);

        return temp;

    }

    public Vector3Int GetPositionInGrid(Vector3 position)
    {

        Vector3Int pos = Vector3Int.FloorToInt(position);

        return pos;

    }


    public void Clear()
    {
        openPathPoints.Clear();
        closedPathPoints.Clear();
        path.Clear();

        foreach(GameObject t in toDestroyTransforms){

            if(t.gameObject != null){

            Destroy(t);

            }
        }

    }

}

[Serializable]
public class PathPoint
{
    public Vector3Int gridPosition;

    public Transform newTargetTransform;

    public Transform collisionTransform;

    public PathPoint parent;

    public float cost;

    public RaycastHit hit;

    public bool colided = false;

    public PathPoint(Vector3Int _gridPosition, RaycastHit _hit)
    {

        gridPosition = _gridPosition;

        hit = _hit;

    }

    private void OnDestroy()
    {

        GameObject.DestroyImmediate(newTargetTransform);

    }


}