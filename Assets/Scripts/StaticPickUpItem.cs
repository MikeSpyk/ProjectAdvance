using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.UI;

[RequireComponent(typeof(CursorRayCastTarget))]
public class StaticPickUpItem : MonoBehaviour
{
    [SerializeField] private string m_pickUpText = "Collect item";
    [SerializeField] private string m_itemName = "Item";
    [SerializeField] private int m_itemId = 1;
    [SerializeField] private int m_pickUpSoundIndex = 21;

    CursorRayCastTarget m_rayCastTarget;

    void Awake()
    {
        m_rayCastTarget = GetComponent<CursorRayCastTarget>();
        m_rayCastTarget.cursorStay += cursorStay;
    }

    private void cursorStay(object sender, object unused)
    {
        PlayerUIInterface.Singleton.setActionText(m_pickUpText, new System.Action(pickUpItem));
    }

    private void pickUpItem()
    {
        PlayerController player = CustomGameManager.Singleton.getActivePlayerController();

        if(player != null)
        {
            if(player.addToPlayerInventory(m_itemId))
            {
                PlayerUIInterface.Singleton.printInformationText("+ " + m_itemName);
                Destroy(this.gameObject);
                BasicTools.Audio.SoundManager.singleton.playGlobalSound(m_pickUpSoundIndex);
            }
        }
    }

}
