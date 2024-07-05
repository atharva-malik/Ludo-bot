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

    private int turn = 0;
    public bool canRoll = true;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && canRoll) {
                if (hit.collider.name == "Board"){
                    currentRoll = roll();
                }
                //canRoll = false;
            }
            if (turn == 1){ // Player's turn
                foreach (Transform t in yellowCounters){
                    if (hit.collider.gameObject.transform == t){
                        if (hit.collider.gameObject.GetComponent<Counter>().canMove(currentRoll)){
                            move(currentRoll, hit.collider.gameObject, turn);
                            canRoll = true;
                        }
                    }
                }
            }
        }
    }

    void move(int currentRoll, GameObject token, int turn){
        if (currentRoll == 5 && token.GetComponent<Counter>().isOut == false){
            token.transform.position = checkpointList[startingPositionFromTurn(turn)].position;
            token.GetComponent<Counter>().isOut = true;
            return;
        }
        token.GetComponent<Counter>().currentCheckpoint += currentRoll+1;
        token.transform.position = checkpointList[token.GetComponent<Counter>().currentCheckpoint].position;

    }

    int startingPositionFromTurn(int turn){
        if (turn == 1){
            foreach(Transform t in checkpointList){
                if (t.gameObject.GetComponent<Checkpoints>().yellowStartPoint){
                    for (int i = 0; i < checkpointList.Count; i++){
                        if (checkpointList[i] == t){
                            return i;
                        }
                    }
                }
            }
        }
        return 0;
    }

    int roll(){
        foreach (GameObject o in dieFaces){
            o.SetActive(false);
        }
        int num = Random.Range(0, 6);
        dieFaces[num].SetActive(true);
        if (turn == 0){
            turn++;
        }
        else{
            turn--;
        }
        return num;
    }
}
