using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public string playerID;
    public int fNum, sNum, tNum, totalNum;
    // Start is called before the first frame update
    public Player(string pID, int fN, int sN, int tN, int total)
    {
        playerID = pID;
        fNum = fN;
        sNum = sN;
        tNum = tN;
        totalNum = total;
    }
}
