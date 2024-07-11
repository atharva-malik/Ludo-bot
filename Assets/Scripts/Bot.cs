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
    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int makeMove(){
        int move = Random.Range(0,4);
        while (gm.greenCounters[move].GetComponent<Counter>().canMove(move) == false)
            move = Random.Range(0, 4);
        return move;
    }

    float evaluation(){
        return -1;
    }
}
