using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EnterRoom : MonoBehaviour
{

    public int roomNumber;
    // Start is called before the first frame update
    public void setGameRoom()
    {
        switch (roomNumber)
        {
            case 1:
                GameManager.Instance.gameRoomID = GameManager.Instance.roomNumbers[0];
                SceneManager.LoadScene("ClientScene");
                break;
            case 2:
                GameManager.Instance.gameRoomID = GameManager.Instance.roomNumbers[1];
                SceneManager.LoadScene("ClientScene");
                break;
            case 3:
                GameManager.Instance.gameRoomID = GameManager.Instance.roomNumbers[2];
                SceneManager.LoadScene("ClientScene");
                break;
            case 4:
                GameManager.Instance.gameRoomID = GameManager.Instance.roomNumbers[3];
                SceneManager.LoadScene("ClientScene");
                break;
        }
    }
}
