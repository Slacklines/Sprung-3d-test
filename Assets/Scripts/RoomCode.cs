using TMPro;
using UnityEngine;

public class RoomCode : MonoBehaviour
{
    private TMP_Text text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TMP_Text>();
        text.text = "Room Code: " + PlayerManager.instance.roomCode;
    }
}
