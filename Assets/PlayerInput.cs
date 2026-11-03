using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using static CardManager;
using System.Linq;
using DG.Tweening;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Collider currentCollider;
    private Camera mainCamera;
    private Plane dragPlane;
    private Vector3 offset;
    private Vector3 lastPosition;
    CardManager cardManager;
    ManagerGame gameManager;
    public GameObject slot1;
    private List<Card> deck = new List<Card>();
    public bool drop = false;
    private List<Card> cardFamily = new List<Card>();
    public GameObject selected;
    private bool autoStack = false;
    private bool isAbleToDrag = false;
    public GameObject autoStackButton;
    public GameObject winPanel;
    public GameObject newGame;
    public GameObject exitGame;
    public TextMeshProUGUI timeWin;
    bool allOpen;

    private void Start()
    {
        deck = GenerateDeck();
        cardManager = FindObjectOfType<CardManager>();
        gameManager = FindObjectOfType<ManagerGame>();
        slot1 = this.gameObject;
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (autoStack || isAbleToDrag) return;
        if(Input.GetMouseButtonDown(0)) SelectPart();
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(PreDrop());
        }
        DragAndDropObject();
    }
    private void SelectPart()
    {
        if (currentCollider != null) return;
        RaycastHit hit;
        Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out hit))
        {
            if (hit.collider.CompareTag("Card"))
            {
                if (!hit.collider.gameObject.GetComponent<Selectable>().faceUp)
                {
                    return;
                }
                currentCollider = hit.collider;
                lastPosition = currentCollider.transform.position;
                currentCollider.transform.position = new Vector3(lastPosition.x, lastPosition.y, -0.1f);
                print("Card: " +currentCollider.gameObject.name);
                SetOrder(currentCollider.gameObject.transform, 2, true);
                currentCollider.gameObject.tag = "PickenCard";
                currentCollider.gameObject.layer = LayerMask.NameToLayer("Dragging");

                dragPlane = new Plane(mainCamera.transform.forward, currentCollider.transform.position);
                float planeDist;
                dragPlane.Raycast(camRay, out planeDist);
                offset = currentCollider.transform.position - camRay.GetPoint(planeDist);
                if (slot1 == this.gameObject)
                {
                    slot1 = currentCollider.gameObject;
                }
                if (slot1 != currentCollider.gameObject)
                {
                    slot1 = currentCollider.gameObject;
                }
            }
            if (hit.collider.CompareTag("Deck"))
            {
                print(hit.collider.name);
                slot1 = this.gameObject;
                cardManager.DealFromDeck();
            }
            if (hit.collider.CompareTag("PosTop"))
            {
                print("PosTop: " + hit.collider.name);
            }
            if (hit.collider.CompareTag("PosDown"))
            {
                print("PosDown: " + hit.collider.name);
            }
        }
    }
    void SetOrder(Transform root, int order, bool dragging)
    {
        Queue<Transform> queue = new Queue<Transform>();

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            current.gameObject.GetComponent<SpriteRenderer>().sortingOrder = order;
            current.gameObject.GetComponent<Collider>().isTrigger = false;
            if (dragging)
            {
                current.gameObject.layer = LayerMask.NameToLayer("Dragging");
            }
            else current.gameObject.layer = LayerMask.NameToLayer("Default");

            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
            if (drop)
            {
                if (queue.Count == 0)
                {
                    current.gameObject.GetComponent<Collider>().isTrigger = true;
                    current.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }
    void CardFamily(Transform root)
    {
        Queue<Transform> queue = new Queue<Transform>();

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (!current.gameObject.CompareTag("PosDown"))
            {
                if (deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == current.gameObject.name) != null)
                {
                    Card card = deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == current.gameObject.name);
                    cardFamily.Add(card);
                }
            }
            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }
    }
    void CheckFaceUp(Transform root)
    {
        Queue<Transform> queue = new Queue<Transform>();

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (current.GetComponent<Selectable>().faceUp == true || current.gameObject.tag == "PosDown")
            {
                allOpen = true;
            }
            else
            {
                allOpen = false;
                return;
            }
            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }
    }
    private void DragAndDropObject()
    {
        if (currentCollider == null) return;
        Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        float planeDist;
        dragPlane.Raycast(camRay, out planeDist);
        currentCollider.transform.position= camRay.GetPoint(planeDist) + offset;
        
    }
    public IEnumerator PreDrop()
    {
        if (currentCollider == null) yield break;
        currentCollider.gameObject.layer = LayerMask.NameToLayer("Default");
        yield return new WaitForSeconds(0.072f);
        Drop();
    }
    public IEnumerator DoMove(GameObject slot, Vector3 vector, float duration)
    {
        isAbleToDrag = true;
        slot.transform.DOMove(vector, duration);
        yield return new WaitForSeconds(duration);
        isAbleToDrag = false;
    }
    public void Drop()
    {
        if (currentCollider == null) return;
        currentCollider.gameObject.tag = "Card";
        if (selected != null)
        {
            if (Stackable(selected))
            {
                Stack(selected, 0.1f);
                drop = true;
                SetOrder(currentCollider.gameObject.transform, 1, false);
                selected.gameObject.GetComponent<Collider>().isTrigger = false;
            }
            else
            {
                drop = true;
                StartCoroutine(DoMove(slot1, new Vector3(lastPosition.x, lastPosition.y, lastPosition.z), 0.1f));
                if (slot1.GetComponent<Selectable>().isDeckPile)
                {
                    slot1.gameObject.GetComponent<Collider>().isTrigger = false;
                }
                else
                {
                    SetOrder(currentCollider.gameObject.transform, 1, false);
                }
            }
        }
        else
        {
            drop = true;
            StartCoroutine(DoMove(slot1, new Vector3(lastPosition.x, lastPosition.y, lastPosition.z), 0.1f));
            if (slot1.GetComponent<Selectable>().isDeckPile)
            {
                slot1.gameObject.GetComponent<Collider>().isTrigger = false;
            }
            else
            {
                SetOrder(currentCollider.gameObject.transform, 1, false);
            }
        }
        currentCollider.GetComponent<SpriteRenderer>().sortingOrder = 1;
        currentCollider = null;
        slot1 = null;
        NullSlot();
        selected = null;
        drop = false;
        if(IsAbleToAutoStack())
        {
            autoStackButton.SetActive(true);
        }
        if (IsWinGame())
        {
            gameManager.timerRunning = false;
            winPanel.SetActive(true);
            timeWin.text = $"Òâîå âðåìÿ:\n {gameManager.textTimer.text}";
            SaveGame();
            autoStackButton.GetComponent<Button>().enabled = false;
            newGame.GetComponent<Button>().enabled = false;
            exitGame.GetComponent<Button>().enabled = false;
        }
    }
    public bool Stackable(GameObject selected)
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        if (!s2.isDeckPile)
        {
            if (s2.top)
            {
                if (s1.transform.childCount > 0)
                {
                    return false;
                }
                if (s1.card.Suit == s2.card.Suit || (s1.card.Values == 1 && s2.card.Suit == null))
                {
                    if (s1.card.Values == s2.card.Values + 1)
                    {
                        return true;
                    }
                    else return false;
                }
            }
            else
            {
                if (s1.card.Values == s2.card.Values - 1 || (s1.card.Values == 13 && s2.card.Values == 0))
                {
                    if (s1.card.Color == s2.card.Color) return false;
                    else return true;
                }
            }
        }      
        return false;
    }
    void Stack(GameObject _selected, float timeToStuck)
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = _selected.GetComponent<Selectable>();

        float yPos = 0.6f;
        if (s2.top || (!s2.top && s1.card.Values == 13))
        {
            yPos = 0;
        }
        CardFamily(slot1.gameObject.transform);
        StartCoroutine(DoMove(slot1, new Vector3(_selected.transform.position.x, _selected.transform.position.y - yPos, _selected.transform.position.z - 0.001f), timeToStuck));
        slot1.transform.parent = _selected.transform;

        if (s1.isDeckPile)
        {
            if (s2.top)
            {
                s1.top = true;
                if (deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == slot1.name) != null)
                {
                    cardManager.disCardPile.RemoveAt(cardManager.disCardPile.Count - 1);
                    cardManager.cardTable.RemoveAt(cardManager.cardTable.Count - 1);
                    cardFamily.Clear();
                }
            }
            else
            {
                s1.top = false;
                if (deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == slot1.name) != null)
                {
                   cardManager.disCardPile.RemoveAt(cardManager.disCardPile.Count - 1);
                   cardManager.cardTable.RemoveAt(cardManager.cardTable.Count - 1);
                    foreach (Card item in cardFamily)
                    {
                        cardManager.pos[s2.row].Add(item);
                    }
                    cardFamily.Clear();
                }
            }
        }
        else if (s1.top && s2.top && s1.card.Values == 1)
        {
            cardFamily.Clear();
        }
        else if (s1.top)
        {
            s1.top = false;

            foreach (Card item in cardFamily)
            {
                cardManager.pos[s2.row].Add(item);
            }
            cardFamily.Clear();
        }
        else if (s2.top)
        {
            s1.top = true;
            List<Card> cards = new List<Card>();
            cards = cardManager.pos[s1.row];
            int count = cardFamily.Count;
            int index = cards.Count - 1;
            while (count > 0)
            {
                cards.RemoveAt(index);
                count--;
                index--;
            }
            cardManager.pos[s1.row] = cards;
            cardFamily.Clear();
            if (cardManager.pos[s1.row].Count > 0)
            {
                Card card = cardManager.pos[s1.row].Last();
                string name = card.Suit + card.Values + card.Color;
                GameObject lastCard = GameObject.Find(name);
                if (lastCard != null)
                {
                    lastCard.GetComponent<Selectable>().faceUp = true;
                    lastCard.GetComponent<Collider>().isTrigger = true;

                }
            }
        }
        else
        {
            s1.top = false;
            if (deck.FirstOrDefault(p => p.Suit + p.Values + p.Color == slot1.name) != null)
            {
                List<Card> cards = new List<Card>();
                cards = cardManager.pos[s1.row];
                int count = cardFamily.Count;
                int index = cards.Count - 1;
                while (count > 0)
                {
                    cards.RemoveAt(index);
                    count--;
                    index--;
                }
                foreach(Card item in cardFamily)
                {
                    cardManager.pos[s2.row].Add(item);
                }
                cardFamily.Clear();
                if (cardManager.pos[s1.row].Count > 0)
                {
                    Card card = cardManager.pos[s1.row].Last();
                    string name = card.Suit + card.Values + card.Color;
                    GameObject lastCard = GameObject.Find(name);
                    if (lastCard != null)
                    {
                        lastCard.GetComponent<Selectable>().faceUp = true;
                        lastCard.GetComponent<Collider>().isTrigger = true;
                        //lastCard.GetComponent<Collider>().enabled = true;
                    }
                }
            }
        }
        s1.isDeckPile= false;
        CardFamily(s1.gameObject.transform);
        foreach(Card item in cardFamily)
        {
            string name = item.Suit + item.Values + item.Color;
            GameObject rowCard= GameObject.Find(name);
            rowCard.GetComponent<Selectable>().row = s2.row;
        }
        cardFamily.Clear();
        s1.row = s2.row;
        slot1 = this.gameObject;
    }
    void NullSlot()
    {
        foreach(GameObject slot in cardManager.posTop)
        {
            if (slot.transform.childCount == 0)
            {
                slot.GetComponent<Collider>().isTrigger = true;
            }
            if (slot.transform.childCount > 0)
            {
                SetOrder(slot.transform, 1, false);
            }
        }
        foreach (GameObject slot in cardManager.posDown)
        {
            if (slot.transform.childCount == 0)
            {
                slot.GetComponent<Collider>().isTrigger = true;
                slot.GetComponent<Collider>().enabled = true;
            }
        }
    }
    public bool IsAbleToAutoStack()
    {
        foreach (GameObject slot in cardManager.posDown)
        {
            CheckFaceUp(slot.transform);
            if (!allOpen) return false;
        }
        if (cardManager.deck.Count == 0 && allOpen && cardManager.disCardPile.Count == 0)
        {
            return true;
        }
        else return false;
    }
    public void ButtonAutoStuck()
    {
        if (autoStack) return;
        StartCoroutine(AutoStack());
    }
    public IEnumerator AutoStack()
    {
        gameManager.timerRunning = false;
        bool posCount = false;
        autoStack = true;
        while (!posCount)
        {
            foreach (GameObject slot in cardManager.posDown)
            {
                if (slot.transform.childCount != 0)
                {
                    posCount = false;
                    break;
                }
                else posCount = true;
            }
            foreach (List<Card> pos in cardManager.pos)
            {
                if (pos.Count == 0) continue;
                Card card = pos.Last();
                string name = card.Suit + card.Values + card.Color;
                GameObject downCard = GameObject.Find(name);
                slot1 = downCard;
                foreach (GameObject selected in cardManager.posTop)
                {
                    CardFamily(selected.transform);
                    Card cardTop = cardFamily.Last();
                    string nameTop = cardTop.Suit + cardTop.Values + cardTop.Color;
                    GameObject topCard = GameObject.Find(nameTop);
                    if (Stackable(topCard))
                    {
                        cardFamily.Clear();
                        downCard.gameObject.GetComponent<SpriteRenderer>().sortingOrder++;
                        Stack(topCard, 0.5f);
                        yield return new WaitForSeconds(0.5f);
                        downCard.gameObject.GetComponent<SpriteRenderer>().sortingOrder--;
                        break;
                    }
                    cardFamily.Clear();
                }
            }
        }
        winPanel.SetActive(true);
        timeWin.text = $"Òâîå âðåìÿ:\n {gameManager.textTimer.text}";
        SaveGame();
        autoStackButton.GetComponent<Button>().enabled = false;
        newGame.GetComponent<Button>().enabled = false;
        exitGame.GetComponent<Button>().enabled = false;
    }
    public bool IsWinGame()
    {
        bool nullSlot = false;
        foreach (GameObject slot in cardManager.posDown)
        {
            if (slot.transform.childCount == 0)
            {
                nullSlot = true;
            }
            else
            {
                nullSlot= false;
                return false;
            }
        }
        if (cardManager.deck.Count == 0 && nullSlot && cardManager.disCardPile.Count == 0)
        {
            return true;
        }
        else return false;
    }
    private Save CreateSaveGameObject(TextMeshProUGUI timeText)
    {
        Save save = new Save();
        string[] time = timeText.text.Split(new char[] { ':' });
        save.minutes = int.Parse(time[0]);
        save.seconds= int.Parse(time[1]);
        return save;
    }
    public void SaveGame()
    {
        Save save = CreateSaveGameObject(gameManager.textTimer);
        Save fileSave = LoadGame();
        if (fileSave.minutes != 0 && fileSave.seconds != 0)
        {
            if ((save.minutes * 60 + save.seconds) < (fileSave.minutes * 60 + fileSave.seconds))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(UnityEngine.Application.persistentDataPath + "/gamesave.save");
                bf.Serialize(file, save);
                file.Close();
                gameManager.bestTime.text = string.Format("ËÓ×ØÅÅ ÂÐÅÌß: {0:00}:{1:00}", save.minutes, save.seconds);
                Debug.Log("Game Saved");
            }
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(UnityEngine.Application.persistentDataPath + "/gamesave.save");
            bf.Serialize(file, save);
            file.Close();
            gameManager.bestTime.text = string.Format("ËÓ×ØÅÅ ÂÐÅÌß: {0:00}:{1:00}", save.minutes, save.seconds);
            Debug.Log("Game Saved");
        }
    }
    public Save LoadGame()
    {
        Save save = new Save();
        if (File.Exists(UnityEngine.Application.persistentDataPath + "/gamesave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(UnityEngine.Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            save = (Save)bf.Deserialize(file);
            file.Close();
            Debug.Log("Game Loaded");
            return save;
        }
        else
        {
            Debug.Log("No game saved!");
            return save;
        }
    }
}
