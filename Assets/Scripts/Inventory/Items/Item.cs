[System.Serializable]
public struct Item
{
    public int itemID;
    public int amount;

    public bool IsEmpty => itemID == -1;

    public Item(int id, int qty)
    {
        itemID = id;
        amount = qty;
    }

    public static Item Empty => new Item(-1, 0);
}