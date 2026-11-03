using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TriggerCard : MonoBehaviour
{
    public static GameObject selectedCard;
    private PlayerInput playerInput;
    private void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != null)
        {
            if (selectedCard != other.gameObject)
            {
                if (other.CompareTag("Card"))
                {
                    print(other.gameObject.name);
                    selectedCard = other.gameObject;
                    playerInput.selected = other.gameObject;
                }
                if (other.CompareTag("PosTop"))
                {
                    print(other.gameObject.name);
                    selectedCard = other.gameObject;
                    playerInput.selected = other.gameObject;
                }
                if (other.CompareTag("PosDown"))
                {
                    print(other.gameObject.name);
                    selectedCard = other.gameObject;
                    playerInput.selected = other.gameObject;
                }
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject != null && selectedCard != null)
        {
            if (selectedCard == other.gameObject)
            {
                selectedCard = null;
                playerInput.selected = null;
            }
        }
    }
}
