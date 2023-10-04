using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI.Inventory;

[RequireComponent(typeof(UIItemData))]
[RequireComponent(typeof(Rigidbody))]
public class PickUpItem : MonoBehaviour
{
    private UIItemData m_itemData;
    private Rigidbody m_rigidbody;

    void Awake()
    {
        m_itemData = GetComponent<UIItemData>();
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        m_rigidbody.velocity = Vector3.down * 0.1f;
    }

    void Update()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider.gameObject == this.gameObject)
            {
                PlayerUIInterface.Singleton.setActionText(string.Format("Collect {0}", ItemsInterface.singleton.getDisplayName(m_itemData.itemIndex)), new System.Action(pickUpItem));
            }
        }

        if(!m_rigidbody.isKinematic)
        {
            if(m_rigidbody.velocity.y < 0.001f)
            {
                m_rigidbody.isKinematic = true;
            }
        }
    }

    private void pickUpItem()
    {
        PlayerController player = CustomGameManager.Singleton.getActivePlayerController();

        if(player != null)
        {
            if(player.addToPlayerInventory(m_itemData))
            {
                PlayerUIInterface.Singleton.printInformationText(string.Format("+ {0}",ItemsInterface.singleton.getDisplayName(m_itemData.itemIndex)));
                Destroy(this.gameObject);
            }
        }
    }
}
