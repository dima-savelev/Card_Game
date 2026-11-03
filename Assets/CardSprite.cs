using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardManager;

public class CardSprite : MonoBehaviour
{
    public Sprite faceCard;
    public Sprite backCard;
    SpriteRenderer displayCard;
    CardManager cardManager;
    Selectable selectable;
    PlayerInput playerInput;
    // Start is called before the first frame update
    void Start()
    {
        SetDeck();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectable.faceUp == true)
        {
            displayCard.sprite= faceCard;
        }
        else
        {
            displayCard.sprite= backCard;
        }
    }
    void SetDeck()
    {
        List<Card> deck = CardManager.GenerateDeck();
        cardManager = FindObjectOfType<CardManager>();
        playerInput= FindObjectOfType<PlayerInput>();
        int index = 0;
        foreach (var card in deck)
        {
            if (this.name == card.Suit + card.Values + card.Color)
            {
                faceCard = cardManager.faceCard[index];
                break;
            }
            index++;
        }
        displayCard = GetComponent<SpriteRenderer>();
        selectable = GetComponent<Selectable>();
    }
}
