using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CardManager;

public class Selectable : MonoBehaviour
{
    public bool faceUp = false;
    public bool top = false;
    public bool isDeckPile = false;
    public int row;
    public string suit;
    private List<Card> deck = new List<Card>();
    public Card card;
    // Start is called before the first frame update
    void Start()
    {
        deck = GenerateDeck();
        CheckValue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CheckValue()
    {
        if (CompareTag("Card"))
        {
            suit = transform.name.ToString();
            if (deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == suit) != null)
            {
                card = deck.First(p => p.Suit + p.Values + p.Color == suit);
            }
        }
        if (CompareTag("PosTop"))
        {
            card = new Card();
            top = true;
        }
        if (CompareTag("PosDown"))
        {
            card = new Card();
            top = false;
        }
    }
}
