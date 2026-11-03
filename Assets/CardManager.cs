using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public Sprite[] faceCard;
    public GameObject cardPrefab;
    public GameObject deckButton;
    public GameObject[] posTop;
    public GameObject[] posDown;
    public static List<SuitColor> suitColors = new List<SuitColor>
    {
        new SuitColor("×", "Ê"),
        new SuitColor("Á", "Ê"),
        new SuitColor("Ê", "×"),
        new SuitColor("Ï", "×")
    };
    public List<Card>[] pos;
    public List<Card>[] top;
    List<Card> pos1 = new List<Card>();
    List<Card> pos2 = new List<Card>();
    List<Card> pos3= new List<Card>();
    List<Card> pos4 = new List<Card>();
    List<Card> pos5 = new List<Card>();
    List<Card> pos6 = new List<Card>();
    List<Card> pos7 = new List<Card>();
    public List<GameObject> cardTable = new List<GameObject>();
    public List<Card> deck = new List<Card>();
    public List<Card> tripsOnDispay = new List<Card>();
    public List<Card> myTrips = new List<Card>();
    public List<Card> disCardPile = new List<Card>();
    public int trips;
    public int deckLocation;
    public Sprite emptyDeck;
    public Sprite backCard;
    public float zPos = 0f;
    // Start is called before the first frame update
    void Start()
    {
        pos = new List<Card>[] { pos1, pos2, pos3, pos4, pos5, pos6, pos7};
        DealCard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DealCard()
    {
        foreach(List<Card> item in pos)
        {
            item.Clear();
        }
        deck = GenerateDeck();
        ShuffleDeck(deck);
        SolitaireSort();
        StartCoroutine(CreateDeck());
        SortDeckIntoTrips();
    }
    public void ShuffleDeck(List<Card> card)
    {
        System.Random random= new System.Random();
        int count = card.Count;
        while (count > 1)
        {
            int number = random.Next(count);
            count --;
            Card temp = card[number];
            card[number] = card[count];
            card[count] = temp;
        }
    }
    public static List<Card> GenerateDeck()
    {
        List<Card> newDeck = new List<Card>();
        foreach (var s in suitColors)
        {
            int count = 1;
            for (int i = 0; i < 13; i++)
            {
                Card card = new Card(s, count);
                newDeck.Add(card);
                count++;
            }
        }
        return newDeck;
    }
    IEnumerator CreateDeck()
    {
        for (int i = 0; i < 7; i++)
        {
            int index = 1;
            float ySet = 0f;
            float zSet = 0.089f;
            posDown[i].GetComponent<Selectable>().row = i;
            foreach (var cardName in pos[i])
            {
                yield return new WaitForSeconds(0.02f);
                GameObject newCard = Instantiate(cardPrefab, new Vector3(posDown[i].transform.position.x, posDown[i].transform.position.y - ySet, transform.position.z - zSet), Quaternion.identity, posDown[i].transform);
                newCard.name = cardName.Suit + cardName.Values + cardName.Color;
                newCard.GetComponent<Selectable>().row = i;
                int count = pos[i].Count;
                if (index == count)
                {
                    newCard.GetComponent<Selectable>().faceUp = true;
                    newCard.GetComponent<Collider>().isTrigger = true;
                }
                else
                {
                    newCard.GetComponent<Selectable>().faceUp = false;
                    newCard.GetComponent<Collider>().isTrigger = false;
                }
                ySet = ySet + 0.6f;
                zSet = zSet + 0.001f;
                disCardPile.Add(cardName);
                index++;
            }

            foreach (var item in disCardPile)
            {
                if (deck.Contains(item))
                {
                    deck.Remove(item);
                }
            }
            disCardPile.Clear();
        }
    }
    void SortDeckIntoTrips()
    {
        deckLocation = 0;
        trips = deck.Count;
        for (int i = 0; i < trips; i++)
        {
            myTrips.Add(deck[i]);
        }
    }
    public void DealFromDeck()
    {
        if (deckLocation < trips)
        {
            float xPos = 2.5f;
            GameObject newTopCard = Instantiate(cardPrefab, new Vector3(deckButton.transform.position.x + xPos, deckButton.transform.position.y, deckButton.transform.position.z - zPos), Quaternion.identity, deckButton.transform);
            newTopCard.name = myTrips[deckLocation].Suit + myTrips[deckLocation].Values + myTrips[deckLocation].Color;
            tripsOnDispay.Add(myTrips[deckLocation]);
            newTopCard.GetComponent<Selectable>().faceUp = true;
            newTopCard.GetComponent<Selectable>().isDeckPile = true;
            newTopCard.GetComponent<Collider>().isTrigger = false;
            cardTable.Add(newTopCard.gameObject);
            deckLocation++;
            zPos = zPos + 0.001f;
            if (deckLocation == myTrips.Count())
            {
                deckButton.GetComponent<SpriteRenderer>().sprite = emptyDeck;
            }
            foreach (Transform nameCard in deckButton.transform)
            {

                if (nameCard.CompareTag("Card"))
                {
                    if (deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == nameCard.name) != null)
                    {
                        Card card = deck.First(p => p.Suit + p.Values + p.Color == nameCard.name);
                        deck.Remove(card);
                        disCardPile.Add(card);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < cardTable.Count; i++)
            {
                Destroy(cardTable[i].gameObject);
            }
            if (disCardPile.Count == 0)
            {
                deckButton.GetComponent<SpriteRenderer>().sprite = emptyDeck;
                deckButton.GetComponent<BoxCollider>().enabled  = false;
            }
            else
            {
                deckButton.GetComponent<SpriteRenderer>().sprite = backCard;
                RestackTopDeck();
            }
        }
    }
    void RestackTopDeck()
    {
        deck.Clear();
        zPos = 0;
        foreach (Card nameCard in disCardPile)
        {
            deck.Add(nameCard);
        }
        disCardPile.Clear();
        myTrips.Clear();
        tripsOnDispay.Clear();
        SortDeckIntoTrips();
    }
    void SolitaireSort()
    {
        int count = 0;
        for(int i = 1; i < 8; i++)
        {
            for(int j = 0; j < i; j++)
            {
                pos[count].Add(deck.Last());
                deck.Remove(deck.Last());
            }
            count++;
        }
    }
    public class SuitColor
    {
        public SuitColor() { }
        public SuitColor(string suit, string color)
        {
            Suit = suit;
            Color = color;
        }
        public string Suit;
        public string Color;
    }
    public class Card
    {
        public Card()
        {
            Suit = null;
            Values = 0;
            Color = null;
        }
        public Card(SuitColor suitColors, int values)
        {
            Suit = suitColors.Suit;
            Values = values;
            Color= suitColors.Color;
        }

        public string Suit;
        public string Color;
        public int Values;
    }

}
