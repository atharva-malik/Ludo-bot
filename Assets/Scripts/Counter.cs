using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public bool isOut = false;
    public int currentCheckpoint = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool canMove(int roll){
        if (roll == 5){
            return true;
        }
        if (isOut == false){
            return false;
        }
        return true;
    }
}
