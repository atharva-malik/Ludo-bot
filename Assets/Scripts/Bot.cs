using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    */
    private GameManager gm;
    private Transform[] bCounters = new Transform[4];
    private Transform[] pCounters = new Transform[4];
    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GameManager>();
        System.Array.Copy(gm.blueCounters, bCounters, gm.blueCounters.Length);
        System.Array.Copy(gm.greenCounters, pCounters, gm.greenCounters.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int[] getRoll(int[] rolls){
        int[] output = new int[3];
        for (int i = 0; i < 3; i++){
            if (rolls[i] != 5){
                output[i] = rolls[i];
                break;
            }
            else{
                output[i] = rolls[i];
            }
        }
        return output;
    }

    // Format: position_bot, position_player, depth, alpha, beta, isMaximizing
    float minimax(Transform[] bCountersInitial, int depth, int alpha, int beta, bool isMaximizing){
        Transform[] bCountersEdited = new Transform[gm.blueCounters.Length];
        Transform[] pCountersEdited = new Transform[gm.blueCounters.Length];
        System.Array.Copy(gm.blueCounters, bCountersInitial, gm.blueCounters.Length);
        System.Array.Copy(gm.blueCounters, bCountersEdited, gm.blueCounters.Length);
        System.Array.Copy(gm.greenCounters, pCountersInitial, gm.greenCounters.Length);
        System.Array.Copy(gm.greenCounters, pCountersEdited, gm.greenCounters.Length);
        if (depth == 0){
            return evaluation();
        }
        if (isMaximizing){
            int maxEval = -2147483648;
            int[] rolls = getRoll(gm.dieRolls);
            for (int i = 0; i < bCountersInitial.Length; i++){
                for (int r = 0; r < 3; r++){
                    if (bCountersEdited[i].GetComponent<Counter>().canMove(r)){
                        move(rolls[r], bCountersEdited[i].gameObject, pCountersEdited);
                        float eval = minimax(depth - 1, alpha, beta, false);
                    }
                } 
            }
            /*
            for each child of position:
                eval = minimax(depth-1, alpha, beta, false)
                maxEval = max(maxEval, eval)
                alpha = max(alpha, eval)
                if (beta <= alpha){
                    break;
                }
            */
            return maxEval;
        }else{
            int minEval = 2147483647;
            /*
            for each child of position:
                eval = minimax(depth-1, alpha, beta, true)
                minEval = min(minEval, eval)
                beta = min(beta, eval)
                if (beta <= alpha){
                    break;
                }
            */
            return minEval;
        }
    }

    public float evaluation(){
        Transform[] botCounters = new Transform[4];
        Transform[] pCounters = new Transform[4];
        System.Array.Copy(gm.blueCounters, botCounters, gm.blueCounters.Length);
        System.Array.Copy(gm.greenCounters, pCounters, gm.greenCounters.Length);
        // The bot side
        int beval = 0;
        foreach (Transform c in botCounters){
            if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == false){
                beval += 7;
            }
            else if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == true && c.GetComponent<Counter>().isFinished == false){
                beval += 17;
            }
            else if (c.GetComponent<Counter>().isFinished == true){
                beval += 13;
            }
        }

        // The player side
        int peval = 0;
        foreach (Transform c in pCounters){
            if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == false){
                peval += 7;
            }
            else if (c.GetComponent<Counter>().isOut == true && c.GetComponent<Counter>().isInHome == true && c.GetComponent<Counter>().isFinished == false){
                peval += 17;
            }
            else if (c.GetComponent<Counter>().isFinished == true){
                peval += 13;
            }
        }
        
        return beval - peval;
    }

    public int makeMove(int cr){
        for (int i = 0; i < 4; i++){
            if (gm.blueCounters[i].GetComponent<Counter>().canMove(cr) == true){
                return i;
            }
        }
        return -1;
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
}
