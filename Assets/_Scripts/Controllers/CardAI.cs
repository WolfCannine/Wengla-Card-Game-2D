using System.Linq;
using UnityEngine;

public class CardAI : MonoBehaviour
{
    private CardController pc;

    private void Awake()
    {
        pc = GetComponent<CardController>();
    }

    public bool IsImportant => GameController.gc.exposeCardID < pc.cardID_List.LastOrDefault();

    public int ExposeCardID()
    {
        return pc.cardID_List.LastOrDefault();
    }

}
