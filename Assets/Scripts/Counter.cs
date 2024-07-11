using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public bool isOut = false;
    public bool isInHome = false;
    public int homeTravelled = 0;
    public int homeStart = 0;
    public int homeOffset = 0;
    public int currentCheckpoint = -1;
    public bool isFinished = false;
    public int startCheckpoint;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool canMove(int roll){
        if (isFinished == true){
            return false;
        }
        else if (isInHome && homeTravelled+roll+1 > 6){
            return false;
        }
        if (roll < 0){
            return false;
        }
        if (roll == 5){
            return true;
        }
        if (isOut == false){
            return false;
        }
        return true;
    }
}
