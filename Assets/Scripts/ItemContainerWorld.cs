using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI;

[RequireComponent(typeof(CursorRayCastTarget))]
public class ItemContainerWorld : MonoBehaviour
{
    [SerializeField] private CursorRayCastTarget m_cursorRayCastTarget;
    [SerializeField] private int m_itemSlots = 1;

    private bool m_cursorIsOver = false;
    private System.Tuple<int,Dictionary<string,string>>[] m_itemIndex_additionalData = null;

    public int itemSlots{get{return m_itemSlots;}}
    public System.Tuple<int,Dictionary<string,string>>[] items_itemIndex_additionalData
    {
        get{ return m_itemIndex_additionalData;}
    }

    void Awake()
    {
        m_cursorRayCastTarget.cursorStay += onCursorStay;
        m_cursorRayCastTarget.cursorEntered += onCursorEntered;
        m_cursorRayCastTarget.cursorExited += onCursorExited;

        m_itemIndex_additionalData = new System.Tuple<int,Dictionary<string,string>>[m_itemSlots];

        for(int i = 0; i < m_itemSlots; i++)
        {
            m_itemIndex_additionalData[i] = new System.Tuple<int, Dictionary<string, string>>(-1,null);
        }
    }

    void Update()
    {
        if(m_cursorIsOver)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                PlayerUIInterface.Singleton.onPlayerOpenItemContainer(this);
            }
        }   
    }

    private void onCursorStay(object sender, object unused)
    {
        PlayerUIInterface.Singleton.setActionText("E to open container", new System.Action(()=>{}));
    }

    private void onCursorEntered(object sender, object unused)
    {
        m_cursorIsOver = true;
    }

    private void onCursorExited(object sender, object unused)
    {
        m_cursorIsOver = false;
    }

    public void updateItems(System.Tuple<int,Dictionary<string,string>>[] items_itemIndex_additionalData)
    {
        if(m_itemIndex_additionalData.Length != items_itemIndex_additionalData.Length)
        {
            throw new System.NotSupportedException("can't update item-collection with different length");
        }

        /*Debug.Log("updateItems");

        for(int i = 0; i <  items_itemIndex_additionalData.Length; i++)
        {
            Debug.Log("item: " + items_itemIndex_additionalData[i].Item1);
        }*/

        m_itemIndex_additionalData = items_itemIndex_additionalData;
    }
}
