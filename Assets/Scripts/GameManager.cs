using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

public class GameManager : MonoBehaviour
{

    public Transform[] yellowInitialTransforms;
    public Transform[] redInitialTransforms;
    public Transform[] blueInitialTransforms;
    public Transform[] greenInitialTransforms;
    
    [Space(10)]
    public Transform[] greenCounters;
    public Transform[] yellowCounters;
    public Transform[] redCounters;
    public Transform[] blueCounters;

    [Space(10)]
    public GameObject[] dieFaces;

    [Space(10)]
    public Transform Checkpoints;
    public List<Transform> checkpointList;

    private Bot bot;
    private int turn = 0;
    public int[] dieRolls = new int[10];
    public bool canRoll = true;
    private bool canMove = false;
    public int currentRoll = -1;
    private int lastRoll = -1;
    private string front = "";

    // Start is called before the first frame update

    void Awake(){
        checkpointList = new List<Transform>();
        foreach (Transform checkpointSingleTransform in Checkpoints) {
            checkpointList.Add(checkpointSingleTransform);
        }
        bot = GetComponent<Bot>();
    }

    void Start()
    {
        for (int i = 0; i < yellowInitialTransforms.Length; i++){
            yellowCounters[i].position = yellowInitialTransforms[i].position;
        }
        for (int i = 0; i < greenInitialTransforms.Length; i++){
            greenCounters[i].position = greenInitialTransforms[i].position;
        }
        for (int i = 0; i < blueInitialTransforms.Length; i++){
            blueCounters[i].position = blueInitialTransforms[i].position;
        }
        for (int i = 0; i < redInitialTransforms.Length; i++){
            redCounters[i].position = redInitialTransforms[i].position;
        }

        foreach (GameObject o in dieFaces){
            o.SetActive(false);
        }
        dieFaces[0].SetActive(true);
        System.Array.Fill(dieRolls, -1);
    }

    // Update is called once per frame
    void Update()
    {
        if (checkWin() == true){
            Debug.Log("Game Over");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && canRoll) {
                if (hit.collider.tag == "Die"){
                    roll();
                    currentRoll = dieRolls[0];
                    if(lastRoll != 5){
                        switchTurn();
                        lastRoll = currentRoll;
                    }else{
                        lastRoll = currentRoll;
                    }
                    canRoll = false;
                    changeRoll();
                }
            }
        }
        if (turn == 0){ // Player's turn
            if (front != "green"){
                front = "green";
                moveToFront(greenCounters, blueCounters);
            }
            playerTurn(greenCounters, blueCounters);
        }
        else if (turn == 1){ // This will be the bot's turn, but bot is not yet implemented
            canMove = false;
            foreach (Transform c in blueCounters){
                if (c.GetComponent<Counter>().canMove(currentRoll)){
                    canMove = true;
                    break;
                }
            }
            if (canMove == false){
                canRoll = true;
            }
            if (front != "blue"){
                front = "blue";
                moveToFront(blueCounters, greenCounters);
            }
            int moveNum = bot.makeMove(currentRoll);
            if (moveNum != -1){
                Debug.Log(bot.evaluation());
                move(currentRoll, blueCounters[moveNum].gameObject, greenCounters);
                canRoll = true;
                canMove = false;
            }
            // playerTurn(blueCounters, greenCounters);
        }
    }

    void playerTurn(Transform[] counters, Transform[] otherCounters){
        foreach (Transform c in counters){
            if (c.GetComponent<Counter>().canMove(currentRoll)){
                canMove = true;
                break;
            }
        }
        if (canMove == false){
            canRoll = true;
        }
        else if (canMove == true){
            if (Input.GetKeyDown(KeyCode.Keypad1) && counters[0].GetComponent<Counter>().canMove(currentRoll))
            {
                move(currentRoll, counters[0].gameObject, otherCounters);
                canRoll = true;
                canMove = false;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2) && counters[1].GetComponent<Counter>().canMove(currentRoll))
            {
                move(currentRoll, counters[1].gameObject, otherCounters);
                canRoll = true;
                canMove = false;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3) && counters[2].GetComponent<Counter>().canMove(currentRoll))
            {
                move(currentRoll, counters[2].gameObject, otherCounters);
                canRoll = true;
                canMove = false;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4) && counters[3].GetComponent<Counter>().canMove(currentRoll))
            {
                move(currentRoll, counters[3].gameObject, otherCounters);
                canRoll = true;
                canMove = false;
            }
        }
    }

    void move(int cr, GameObject token, Transform[] otherToken){
        doCaptures(token.transform, otherToken, cr);
        currentRoll = -1;
        // changeRoll();
        Counter counter = token.GetComponent<Counter>();
        if (cr == 5 && counter.isOut == false){
            token.transform.position = checkpointList[counter.startCheckpoint].position;
            counter.currentCheckpoint = counter.startCheckpoint;
            counter.isOut = true;
            return;
        }
        if (counter.currentCheckpoint+cr+1 > 51 && counter.isInHome == false){
            // counter.currentCheckpoint = counter.currentCheckpoint += cr-51;
            counter.currentCheckpoint += cr-51;
            token.transform.position = checkpointList[counter.currentCheckpoint].position;
            return;
        }
        if (counter.currentCheckpoint+cr+1 > counter.homeStart && counter.isInHome != true && counter.currentCheckpoint < counter.startCheckpoint){
            counter.currentCheckpoint += cr+counter.homeOffset+1;
            counter.homeTravelled = counter.currentCheckpoint-counter.homeStart-counter.homeOffset;
            token.transform.position = checkpointList[counter.currentCheckpoint].position;
            counter.isInHome = true;
            if(counter.homeTravelled >= 6){
                token.transform.position = new Vector2(0,0);
                counter.isFinished = true;
                return;
            }
            return;
        }
        if (counter.isInHome){
            if(counter.homeTravelled+cr+1 <= 6){
                if(counter.homeTravelled >= 6 || counter.homeTravelled+cr+1 >= 6){
                    token.transform.position = new Vector2(0,0);
                    counter.isFinished = true;
                    return;
                }
                counter.currentCheckpoint += cr+1;
                counter.homeTravelled += cr+1;
                token.transform.position = checkpointList[counter.currentCheckpoint].position;
                return;
            }
            else{
                return;
            }
        }
        counter.currentCheckpoint += cr+1;
        token.transform.position = checkpointList[counter.currentCheckpoint].position;
    }

    void roll(){
        foreach (GameObject o in dieFaces){
            o.SetActive(false);
        }
        int num;
        for(int i = 0; i < 10; i++){
            if (i >= 10){
                break;
            }
            if (i > 1 && dieRolls[i-1] == 5 && dieRolls[i-2] == 5){
                num = Random.Range(0, 5);
            }else{
                num = Random.Range(0, 6);
            }
            if (dieRolls[i] == -1){
                dieRolls[i] = num;
            }
        }
        dieFaces[dieRolls[0]].SetActive(true);
    }

    void changeRoll(){
        for (int i = 0; i < dieRolls.Length-1; i++){
            dieRolls[i] = dieRolls[i+1];
        }
        dieRolls[9] = -1;
    }

    void switchTurn(){
        if (turn <= 0){
            turn++;
        }
        else{
            turn--;
        }
    }

    void doCaptures(Transform myCounter, Transform[] otherCounters, int cRoll){
        Counter counter = myCounter.GetComponent<Counter>();
        int cp = counter.currentCheckpoint;
        if (counter.currentCheckpoint+cRoll+1 > 51 && counter.isInHome == false){
            cp += cRoll-51;
        }else{
            cp += cRoll+1;
        }
        foreach (Transform c in otherCounters){
            if (c.GetComponent<Counter>().currentCheckpoint == cp && c.GetComponent<Counter>().isOut == true && checkpointList[cp].GetComponent<Checkpoints>().isSafe == false){
                c.GetComponent<Counter>().isOut = false;
                if (otherCounters == blueCounters){
                    int i = getIndex(c, blueCounters);
                    otherCounters[i].transform.position = blueInitialTransforms[i].position;
                }else if (otherCounters == greenCounters){
                    int i = getIndex(c, greenCounters);
                    otherCounters[i].transform.position = greenInitialTransforms[i].position;
                }
            }
        }
        canRoll = true;
    }

    int getIndex(Transform target, Transform[] objects){
        int index = -1;
        for (int i = 0; i < objects.Length; i++){
            if (objects[i] == target){
                index = i;
                break;
            }
        }
        return index;
    }

    bool checkWin(){
        foreach (Transform c in blueCounters){
            if (c.GetComponent<Counter>().isFinished == false){
                return false;
            }
        }
        foreach (Transform c in greenCounters){
            if (c.GetComponent<Counter>().isFinished == false){
                return false;
            }
        }
        return true;
    }

    void moveToFront(Transform[] counters, Transform[] otherCounters){
        foreach (Transform c in counters){
            c.transform.position += new Vector3(0,0,1);
        }
        foreach (Transform c in otherCounters){
            c.transform.position += new Vector3(0,0,-1);
        }
    }
}
