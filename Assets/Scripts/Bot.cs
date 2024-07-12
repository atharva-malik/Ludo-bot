using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;

public class Bot : MonoBehaviour
{
    /*
        1. Good luck to me
        2. Use minimax, alpha-beta pruning, and fixed depth (3 for now),
        3. First, create a copy of the game board to allow you to make moves, without affecting the original game board.
        4. Then, for evaluation, I will use the following factors(on a scale of 1-10):
            1. Number of 'active' pieces: 7
            2. Number of 'safe' pieces: 2
            3. Number of 'threatened' pieces: 8
            4. Number of pieces in 'home stretch': 10
            5. Number of 'finished' pieces: 6
            6. Capture the enemy's pieces: 9
        
        NEW PLAN: I reread the rules of the game and what I had originally planned would not be legal. RESTART
    */
    public Transform[] blueShadowCounters;
    public Transform[] greenShadowCounters;
    private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UndoMove(){
        Transform[] bc = gm.blueCounters;
        for (int i = 0; i < 4; i++){
            Counter myC = blueShadowCounters[i].GetComponent<Counter>();
            Counter C = bc[i].GetComponent<Counter>();
            myC.isOut = C.isOut;
            myC.isInHome = C.isInHome;
            myC.homeTravelled = C.homeTravelled;
            myC.homeStart = C.homeStart;
            myC.homeOffset = C.homeOffset;
            myC.currentCheckpoint = C.currentCheckpoint;
            myC.isFinished = C.isFinished;
        }
        bc = gm.greenCounters;
        for (int i = 0; i < 4; i++){
            Counter myC = greenShadowCounters[i].GetComponent<Counter>();
            Counter C = bc[i].GetComponent<Counter>();
            myC.isOut = C.isOut;
            myC.isInHome = C.isInHome;
            myC.homeTravelled = C.homeTravelled;
            myC.homeStart = C.homeStart;
            myC.homeOffset = C.homeOffset;
            myC.currentCheckpoint = C.currentCheckpoint;
            myC.isFinished = C.isFinished;
        }
    }

    private Transform[] CreateDeepCopy(Transform[] original){
        Transform[] deepCopy = new Transform[original.Length];
        for (int i = 0; i < original.Length; i++)
        {
            GameObject newObject = new GameObject(original[i].name);
            newObject = original[i].gameObject;
            deepCopy[i] = newObject.transform;
        }
        return deepCopy;
    }

    int getLen(int[] rolls, int lr){
        int len = 1;
        if (lr != 5){
            return 1;
        }
        for (int i = 0; i < 3; i++){
            len ++;
            if (rolls[i] != 5){
                break;
            }
        }
        return len;
    }

    int[] getRoll(int[] rolls, int lr){
        int[] output = new int[1];
        int len;
        len = getLen(rolls, lr);
        if (lr != 5){
            output[0] = lr;
            return output;
        }
        output = new int[len];
        output[0] = 5;
        for (int i = 1; i < len; i++){
            output[i] = rolls[(i-1)];
        }
        return output;
    }

    private float evaluation(Transform[] bCounters, Transform[] pCounters){
        /*
        Then, for evaluation, I will use the following factors(on a scale of 1-10):
            1. Number of 'active' pieces: 7
            2. Number of 'safe' pieces: 2
            3. Number of 'threatened' pieces: 8 #TODO: Add this here
            4. Number of pieces in 'home stretch': 10
            5. Number of 'finished' pieces: 6
            6. Capture the enemy's pieces: 9 This was implemented in a roundabout way, becuz enemy piece will be in home if captured.
            7. Pieces who have travelled more have a higher value.
        */
        if (whoWon(bCounters, pCounters) != 0){
            if (whoWon(bCounters, pCounters) == 1){
                return 2147483647;
            }else{
                return -2147483648;
            }
        }
        // The bot side
        int beval = 0;
        int numOfCaptures = 0;
        foreach (Transform c in bCounters){
            if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == false){
                beval += 7*c.GetComponent<Counter>().travelled;
                int cc = c.GetComponent<Counter>().currentCheckpoint;
                if (gm.checkpointList[cc].GetComponent<Checkpoints>().isSafe == true){
                    beval += 2;
                }
            }
            else if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == true && c.GetComponent<Counter>().isFinished == false){
                beval += 7*(c.GetComponent<Counter>().travelled - c.GetComponent<Counter>().homeTravelled);
                beval += 10;
            }
            else if (c.GetComponent<Counter>().isFinished == true){
                beval += 13;
            }
        }
        for (int i = 0; i < 6; i++){
            numOfCaptures += checkCaptures(bCounters, pCounters, i);
        }
        beval -= numOfCaptures*9;

        // The player side
        int peval = 0;
        numOfCaptures = 0;
        foreach (Transform c in pCounters){
            if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == false){
                beval += 7*c.GetComponent<Counter>().travelled;
            }
            else if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == true && c.GetComponent<Counter>().isFinished == false){
                beval += 7*(c.GetComponent<Counter>().travelled - c.GetComponent<Counter>().homeTravelled);
                beval += 10;
            }
            else if (c.GetComponent<Counter>().isFinished == true){
                peval += 13;
            }
        }
        for (int i = 0; i < 6; i++){
            numOfCaptures += checkCaptures(pCounters, bCounters, i);
        }
        peval -= numOfCaptures*9;

        return beval - peval;
    }

    public int makeMove(){
        UndoMove();
        Transform[] bCountersEdited = blueShadowCounters.ToArray();
        Transform[] pCountersEdited = greenShadowCounters.ToArray();
        float maxEval = -2147483648;
        int best_move = -1;
        int[] rolls = getRoll(gm.dieRolls, gm.currentRoll);
        for (int i = 0; i < bCountersEdited.Length; i++){
            for (int r = 0; r < rolls.Length; r++){
                if (bCountersEdited[i].GetComponent<Counter>().canMove(rolls[r])){
                    move(rolls[r], bCountersEdited[i].gameObject, pCountersEdited);
                    float eval = evaluation(bCountersEdited, pCountersEdited); // TODO: Implement evaluation
                    if (eval > maxEval){
                        maxEval = eval;
                        best_move = i;
                    }
                    UndoMove();
                }
            }
        }
        return best_move;
    }

    private void move(int cr, GameObject token, Transform[] otherToken){
        doCaptures(token.transform, otherToken, cr);
        // currentRoll = -1; Don't think I need to worry about this
        Counter counter = token.GetComponent<Counter>();
        if (cr == 5 && counter.isOut == false){
            counter.currentCheckpoint = counter.startCheckpoint;
            counter.isOut = true;
            return;
        }
        if (counter.currentCheckpoint+cr+1 > 51 && counter.isInHome == false){
            // counter.currentCheckpoint = counter.currentCheckpoint += cr-51;
            counter.currentCheckpoint += cr-51;
            return;
        }
        if (counter.currentCheckpoint+cr+1 > counter.homeStart && counter.isInHome != true && counter.currentCheckpoint < counter.startCheckpoint){
            counter.currentCheckpoint += cr+counter.homeOffset+1;
            counter.homeTravelled = counter.currentCheckpoint-counter.homeStart-counter.homeOffset;
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
                return;
            }
            else{
                return;
            }
        }
        counter.currentCheckpoint += cr+1;
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
            if (c.GetComponent<Counter>().currentCheckpoint == cp && c.GetComponent<Counter>().isOut == true && gm.checkpointList[cp].GetComponent<Checkpoints>().isSafe == false){
                c.GetComponent<Counter>().isOut = false;
                c.GetComponent<Counter>().currentCheckpoint = -1;
            }
        }
        // canRoll = true; Don't think I need to worry about this
    }

    int checkCaptures(Transform[] bCounters, Transform[] otherCounters, int cRoll){
        // Counter counter = myCounter.GetComponent<Counter>();
        // int cp = counter.currentCheckpoint;
        // if (counter.currentCheckpoint+cRoll+1 > 51 && counter.isInHome == false){
        //     cp += cRoll-51;
        // }else{
        //     cp += cRoll+1;
        // }
        // foreach (Transform c in otherCounters){
        //     if (c.GetComponent<Counter>().currentCheckpoint == cp && c.GetComponent<Counter>().isOut == true && gm.checkpointList[cp].GetComponent<Checkpoints>().isSafe == false){
        //         c.GetComponent<Counter>().isOut = false;
        //         c.GetComponent<Counter>().currentCheckpoint = -1;
        //     }
        // }
        int numOfCaptures = 0;
        foreach (Transform i in bCounters){
            Counter counter = i.GetComponent<Counter>();
            int cp = counter.currentCheckpoint;
            if (counter.currentCheckpoint+cRoll+1 > 51 && counter.isInHome == false){
                cp += cRoll-51;
            }else{
                cp += cRoll+1;
            }
            foreach (Transform c in otherCounters){    
                if (c.GetComponent<Counter>().currentCheckpoint == cRoll+1 && c.GetComponent<Counter>().isOut == true && gm.checkpointList[cRoll+1].GetComponent<Checkpoints>().isSafe == false){
                    numOfCaptures += 1;
                }
            }
        }
        return numOfCaptures;
    }

    bool checkWin(Transform[] blueCounters, Transform[] greenCounters){
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

    int whoWon(Transform[] blueCounters, Transform[] greenCounters){
        bool blueWin = false;
        bool greenWin = false;
        foreach (Transform c in blueCounters){
            if (c.GetComponent<Counter>().isFinished == false){
                blueWin = false;
            }
        }
        foreach (Transform c in greenCounters){
            if (c.GetComponent<Counter>().isFinished == false){
                greenWin = false;
            }
        }
        if (blueWin == true){
            return 1;
        }
        else if (greenWin == true){
            return -1;
        }else{
            return 0;
        }
    }
}
