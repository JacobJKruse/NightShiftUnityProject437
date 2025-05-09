using UnityEngine;

public class GameInput : MonoBehaviour
{
    public int colum;             // Column number this object represents
    public GameManager manager;   // Reference to the game manager

    private void OnMouseEnter()
    {
        manager.SetHoverColumn(colum); // Tell the GameManager what column is hovered
    }

    private void OnMouseDown()
    {
        manager.SelectColum(colum);    // Tell the GameManager to place token
    }
}
