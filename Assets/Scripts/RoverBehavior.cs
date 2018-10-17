using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class RoverBehavior : MonoBehaviour
{
    struct GridNode
    {
        public float G;
        public float H;
        public float F;
        public Vector3 position;

        public GridNode(float _G, float _H, float _F, Vector3 _position)
        {
            G = _G;
            H = _H;
            F = _F;
            position = _position;
        }
    }

    //int tempCounter = 0;
    //Color tempColor = Color.green;

    public bool isBusy = false;

    public float moveSpeed = 3f;
    public float rotatingSpeed = 100f;

    private bool isWandering = false;
    private bool isRotatingLeft = false;
    private bool isRotatingRight = false;
    private bool isWalking = false;
    private bool foundPath = false;

    private List<Vector3> turningPoints;
    private List<Vector3> endingPoints;
    private Vector3 staringBlockPos;
    private List<GameObject> blocksToDestroy;
    private Rigidbody rBody;
    private List<GridNode> closedGridPoints;
    private List<GridNode> myPath;

    private Thread pathFinding;

    void Start ()
    {
        blocksToDestroy = new List<GameObject>();
        rBody = GetComponent<Rigidbody>();
        closedGridPoints = new List<GridNode>();
        myPath = new List<GridNode>();
        turningPoints = new List<Vector3>();
        endingPoints = new List<Vector3>();
    }
	
	void Update ()
    {
        if (blocksToDestroy.Count > 0)
        {
            isBusy = true;
            if (myPath.Count > 0)
            {
                for (int i = 0; i < endingPoints.Count; ++i)
                    if (myPath[myPath.Count - 1].position == endingPoints[i])
                        foundPath = true;

                if(!foundPath)
                    FindPath(myPath[myPath.Count - 1].position, blocksToDestroy[0].transform.position);
            }
            else
            {
                FindPath(staringBlockPos, blocksToDestroy[0].transform.position);
                //Debug.Log("Seeking from: " + transform.position + " to " + blocksToDestroy[0].transform.position);
            }
            for (int i = 0; i < myPath.Count; ++i)
                Debug.DrawRay(myPath[i].position, Vector3.up, Color.blue);
            //for (int i = 0; i < turningPoints.Count; ++i)
            //    Debug.DrawLine(turningPoints[i], Vector3.up * 8, Color.white);
            //for (int i = 0; i < endingPoints.Count; ++i)
            //    Debug.DrawLine(endingPoints[i], Vector3.up * 8, Color.yellow);
        }

        if (!isBusy)
        {
            if (!isWandering)
            {
                StartCoroutine("Wander");
            }
            if (isRotatingRight)
            {
                transform.Rotate(transform.up * Time.deltaTime * rotatingSpeed); //transform.right
            }
            if (isRotatingLeft)
            {
                transform.Rotate(transform.up * Time.deltaTime * -rotatingSpeed); //transform.right
            }
            if (isWalking)
            {
                transform.position += (transform.forward * moveSpeed * Time.deltaTime);
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Block")
        {
            isWalking = false;
            staringBlockPos = other.transform.position;
        }
    }
    void AddWork(GameObject _blockToAdd)
    {
        if (_blockToAdd.GetComponent<BreakableBlockBehavior>().isAccessible)
        {
            blocksToDestroy.Add(_blockToAdd);

            RaycastHit checkRight;
            RaycastHit checkLeft;
            RaycastHit checkFront;
            RaycastHit checkBack;
            Physics.Raycast(_blockToAdd.transform.position, Vector3.right, out checkRight, 1.05f);
            Physics.Raycast(_blockToAdd.transform.position, Vector3.left, out checkLeft, 1.05f);
            Physics.Raycast(_blockToAdd.transform.position, Vector3.forward, out checkFront, 1.05f);
            Physics.Raycast(_blockToAdd.transform.position, Vector3.back, out checkBack, 1.05f);
            //Debug.DrawRay(_blockToAdd.transform.position, Vector3.right, Color.magenta, 10000);
            //Debug.DrawRay(_blockToAdd.transform.position, Vector3.left, Color.magenta, 10000);
            //Debug.DrawRay(_blockToAdd.transform.position, Vector3.forward, Color.magenta, 10000);
            //Debug.DrawRay(_blockToAdd.transform.position, Vector3.back, Color.magenta, 10000);

            if (checkRight.transform.GetComponent<BreakableBlockBehavior>().isWalkable)
                endingPoints.Add(checkRight.transform.position);
            if (checkLeft.transform.GetComponent<BreakableBlockBehavior>().isWalkable)
                endingPoints.Add(checkLeft.transform.position);
            if (checkFront.transform.GetComponent<BreakableBlockBehavior>().isWalkable)
                endingPoints.Add(checkFront.transform.position);
            if (checkBack.transform.GetComponent<BreakableBlockBehavior>().isWalkable)
                endingPoints.Add(checkBack.transform.position);

            //Debug.DrawRay(_blockToAdd.transform.position, Vector3.up, Color.cyan, 10000);
            //Debug.Log("First to destroy: " + _blockToAdd.transform.position);
            //Debug.Log("Parent: " + blocksToDestroy[0].transform.root.name);
        }
    }

    void RemoveWork(GameObject _blockToRemove)
    {
        blocksToDestroy.Remove(_blockToRemove);
    }

    void FindPath(Vector3 currBlock, Vector3 _destination)
    {
        List<GridNode> tempList = new List<GridNode>();
        GridNode toAdd = new GridNode(0, 0, 10000, Vector3.zero);

        RaycastHit blockCheck_front;
        RaycastHit blockCheck_back;
        RaycastHit blockCheck_right;
        RaycastHit blockCheck_left;
        Physics.Raycast(currBlock, Vector3.forward, out blockCheck_front, 1.05f);
        Physics.Raycast(currBlock, Vector3.back, out blockCheck_back, 1.05f);
        Physics.Raycast(currBlock, Vector3.right, out blockCheck_right, 1.05f);
        Physics.Raycast(currBlock, Vector3.left, out blockCheck_left, 1.05f);

        //Thread.Sleep(500);
        //Debug.DrawRay(currBlock, Vector3.right, Color.red, 10000);
        //Debug.DrawRay(currBlock, Vector3.left, Color.red, 10000);
        //Debug.DrawRay(currBlock, Vector3.forward, Color.red, 10000);
        //Debug.DrawRay(currBlock, Vector3.back, Color.red, 10000);

        tempList.Add(BlockCheck(blockCheck_front, _destination));
        tempList.Add(BlockCheck(blockCheck_back, _destination));
        tempList.Add(BlockCheck(blockCheck_right, _destination));
        tempList.Add(BlockCheck(blockCheck_left, _destination));

        for (int i = 0; i < tempList.Count; i++)
            for (int j = 0; j < tempList.Count; ++j)
                if (toAdd.F < 10000 && (tempList[i].H == tempList[j].H))
                    tempList.Remove(RSDTTP(tempList[i], tempList[j]));


        for (int i = 0; i < tempList.Count - 1; ++i)
            if (toAdd.F > tempList[i].F)
                toAdd = tempList[i];

        if (myPath.Count > 2)
        {
            RaycastHit cornerCheck;
            Physics.Raycast(myPath[myPath.Count - 2].position,
                (myPath[myPath.Count - 2].position - myPath[myPath.Count - 3].position).normalized, out cornerCheck, 1.05f);
            if (cornerCheck.transform.position != myPath[myPath.Count - 1].position)
                turningPoints.Add(myPath[myPath.Count - 2].position);
        }

        if (toAdd.F < 10000)
        {
            myPath.Add(toAdd);
            //Debug.Log("Block on " + toAdd.position + " was ADDED to the path");
        }
        else
        {
            myPath.Remove(myPath[myPath.Count - 1]);
            //for(int i = myPath.Count - 1; i >= 0; --i)
            //{
            //    if (turningPoints.Count > 0 && myPath[i].position == turningPoints[turningPoints.Count - 1])
            //    {
            //        turningPoints.Remove(turningPoints[turningPoints.Count - 1]);
            //        break;
            //    }
            //    else
            //    {
            //        Thread.Sleep(200);
            //        //Debug.Log("Block on " + myPath[i].position + " was REMOVED from the path");
            //        myPath.Remove(myPath[i]);
            //    }
            //}
        }
        closedGridPoints.Add(toAdd);
    }

    GridNode BlockCheck(RaycastHit _toCheck, Vector3 _Destination)
    {
        bool canAdd = true;
        float tempG = 1;
        float tempH = 0;
        float tempF = 0;

        if (_toCheck.collider.tag == "Block")
        {
            GameObject temp = _toCheck.collider.gameObject;
            Vector3 tempPosition = temp.transform.position;
            GridNode tempGrid;
            RaycastHit[] distanceCheck;

            if (temp.GetComponent<BreakableBlockBehavior>().isWalkable)
            {
                for (int i = 0; i < closedGridPoints.Count; ++i)
                {
                    if (temp.transform.position == closedGridPoints[i].position)
                    {
                        canAdd = false;
                        break;
                    }
                }
                if (canAdd)
                {
                    if (myPath.Count > 0)
                    {
                        tempG = myPath[myPath.Count - 1].G + 1;
                    }
                    //distanceCheck = Physics.RaycastAll(temp.transform.position, (_Destination - temp.transform.position).normalized,
                    //    (_Destination - temp.transform.position).magnitude);

                    //if (tempCounter > 8)
                    //    tempColor = Color.red;
                    //Debug.DrawLine(temp.transform.position, _Destination, tempColor, 10000);
                    //tempCounter++;

                    //for (int i = 0; i < distanceCheck.Length - 1; ++i)
                    //    if (distanceCheck[i].collider.tag == "GridPoint")
                    //        ++tempH;

                    //Debug.Log("_Destination's position: " + _Destination);
                    //Debug.Log("CurrBlock position: " + temp.transform.position);
                    //Debug.Log("TempH = " + tempH);
                    //Debug.Log("TemG = " + tempG);

                    tempH = (_Destination - temp.transform.position).magnitude;               //kinda works, but not exactly 'fastest' way
                    tempF = tempG + tempH;
                    //Debug.Log("TempF = " + tempF);
                    tempGrid = new GridNode(tempG, tempH, tempF, tempPosition);
                    return tempGrid;
                }
            }
            else
            {
                return new GridNode(0, 0, 10000, Vector3.zero);
            }
        }
            return new GridNode(0, 0, 10000, Vector3.zero);
    }

    GridNode RSDTTP(GridNode _first, GridNode _second) //ResolveSameDistanceToTargetProblem (RSDTTP)
    {
        Vector3 shortestFirst = endingPoints[0] - _first.position;
        Vector3 shortestSecond = endingPoints[0] - _second.position;

        for (int i = 1; i < endingPoints.Count; ++i)
        {
            if ((endingPoints[i] - _first.position).magnitude < shortestFirst.magnitude)
                shortestFirst = endingPoints[i] - _first.position;
            if ((endingPoints[i] - _second.position).magnitude < shortestSecond.magnitude)
                shortestSecond = endingPoints[i] - _second.position;
        }

        return shortestFirst.magnitude > shortestSecond.magnitude ? _first : _second;
    }

    IEnumerator Wander()
    {
        int RotateTime = Random.Range(1, 3);
        int RotateWait = Random.Range(1, 3);
        int RotateLorR = Random.Range(1, 3);
        int WalkWait = Random.Range(1, 3);
        int WalkTime = Random.Range(1, 3);

        isWandering = true;

        yield return new WaitForSecondsRealtime(WalkWait);
        isWalking = true;
        yield return new WaitForSecondsRealtime(WalkTime);
        isWalking = false;
        yield return new WaitForSecondsRealtime(RotateWait);
        if (RotateLorR == 1)
        {
            isRotatingRight = true;
            yield return new WaitForSecondsRealtime(RotateTime);
            isRotatingRight = false;
        }
        if (RotateLorR == 2)
        {
            isRotatingLeft = true;
            yield return new WaitForSecondsRealtime(RotateTime);
            isRotatingLeft = false;
        }
        isWandering = false;
    }
}
