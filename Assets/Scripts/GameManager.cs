using System.Collections;
using System.Collections.Generic;
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
    private List<Transform> checkpointList;

    private int turn = 1;
    public bool canRoll = true;
    private bool canMove = false;
    public int currentRoll = -1;

    // Start is called before the first frame update

    void Awake(){
        checkpointList = new List<Transform>();
        foreach (Transform checkpointSingleTransform in Checkpoints) {
            checkpointList.Add(checkpointSingleTransform);
        }
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && canRoll) {
                if (hit.collider.tag == "Die"){
                    currentRoll = roll();
                }
                canRoll = false;
            }
            if (turn == 0){ // Player's turn
                foreach (Transform c in greenCounters){
                    if (c.GetComponent<Counter>().canMove(currentRoll)){
                        canMove = true;
                        break;
                    }
                }
                if (!canMove){
                    canRoll = true;
                }
                else{
                    foreach (Transform t in greenCounters){
                        if (hit.collider.gameObject.transform == t){
                            if (hit.collider.gameObject.GetComponent<Counter>().canMove(currentRoll)){
                                move(currentRoll, hit.collider.gameObject);
                                canRoll = true;
                                canMove = false;
                            }
                        }
                    }
                }
            }
            else if (turn == 1){ // This will be the bot's turn, but bot is not yet implemented
                foreach (Transform c in blueCounters){
                    if (c.GetComponent<Counter>().canMove(currentRoll)){
                        canMove = true;
                        break;
                    }
                }
                if (!canMove){
                    canRoll = true;
                }
                else{
                    foreach (Transform t in blueCounters){
                        if (hit.collider.gameObject.transform == t){
                            if (hit.collider.gameObject.GetComponent<Counter>().canMove(currentRoll)){
                                move(currentRoll, hit.collider.gameObject);
                                canRoll = true;
                                canMove = false;
                            }
                        }
                    }
                }
            }
        }
    }

    void move(int currentRoll, GameObject token){
        Counter counter = token.GetComponent<Counter>();
        if (currentRoll == 5 && counter.isOut == false){
            token.transform.position = checkpointList[counter.startCheckpoint].position;
            counter.currentCheckpoint = counter.startCheckpoint;
            counter.isOut = true;
            return;
        }
        if (counter.currentCheckpoint+currentRoll+1 > 51 && counter.isInHome == false){
            counter.currentCheckpoint = counter.currentCheckpoint += currentRoll-51;
            token.transform.position = checkpointList[counter.currentCheckpoint].position;
            return;
        }
        if (counter.currentCheckpoint+currentRoll+1 > counter.homeStart && counter.isInHome != true && counter.currentCheckpoint < counter.startCheckpoint){
            counter.currentCheckpoint += currentRoll+counter.homeOffset+1;
            counter.homeTravelled = counter.currentCheckpoint-counter.homeStart-counter.homeOffset;
            token.transform.position = checkpointList[counter.currentCheckpoint].position;
            counter.isInHome = true;
            return;
        }
        if (counter.isInHome){
            //counter.homeTravelled < 6 && 
            if(counter.homeTravelled+currentRoll+1 <= 6){
                if(counter.homeTravelled == 6 || counter.homeTravelled+currentRoll+1 == 6){
                    // token.transform.position = checkpointList[^1].position;
                    token.transform.position = new Vector2(0,0);
                    counter.isFinished = true;
                    return;
                }
                counter.currentCheckpoint += currentRoll+1;
                counter.homeTravelled += currentRoll+1;
                token.transform.position = checkpointList[counter.currentCheckpoint].position;
                return;
            }
            else{
                return;
            }
        }
        counter.currentCheckpoint += currentRoll+1;
        token.transform.position = checkpointList[counter.currentCheckpoint].position;
    }

    int roll(){
        foreach (GameObject o in dieFaces){
            o.SetActive(false);
        }
        int num = Random.Range(0, 6);
        dieFaces[num].SetActive(true);
        if (num == 5){
            canRoll = true;
        }
        else{
            if (turn == 0){
                turn++;
            }
            else{
                turn--;
            }
        }
        return num;
    }
}
