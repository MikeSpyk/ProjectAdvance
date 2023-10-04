using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using BasicTools.UI.Inventory;

public class ItemsInterface : MonoBehaviour
{
    public static ItemsInterface singleton;

    [SerializeField] private Sprite[] m_itemIcons;
    [SerializeField] private GameObject[] m_itemEquipablePrefabs;
    [SerializeField] private GameObject[] m_itemDroppedPrefabs;
    [Header("Items XML")]
    [SerializeField] private TextAsset m_storableItemsXMLFile;
    [SerializeField] private bool m_debug_reload = false;

    private Dictionary<int, StorableItemTemplate> m_itemID_itemStorableTemplate = new Dictionary<int, StorableItemTemplate>();
    private Dictionary<string, int> m_ItemStorableName_index = new Dictionary<string, int>();

    private void Awake()
    {
        singleton = this;
        initialize();
    }

    void FixedUpdate()
    {
        if(m_debug_reload)
        {
            m_debug_reload = false;
            initialize();
        }
    }

    public GameObject spawnItemDropWorld(int itemID, UIItemData data)
    {
        GameObject worldItem = Instantiate(m_itemDroppedPrefabs[m_itemID_itemStorableTemplate[itemID].m_dropModelIndex]);
        worldItem.GetComponent<UIItemData>().copyFrom(data);

        return worldItem;
    }
    public GameObject spawnItemDropWorld(int itemID)
    {
        GameObject worldItem = Instantiate(m_itemDroppedPrefabs[m_itemID_itemStorableTemplate[itemID].m_dropModelIndex]);
        UIItemData itemdata = worldItem.GetComponent<UIItemData>();

        Tuple<string,string>[] randomStuff = ItemsInterface.singleton.rollRandomAttributes(itemID,Time.time, Time.time * Time.deltaTime).ToArray();

        itemdata.updateItemData(itemID,randomStuff);

        return worldItem;
    }
    public GameObject spawnItemDropWorld(int itemID, Tuple<string,string>[] additionalData)
    {
        GameObject worldItem = Instantiate(m_itemDroppedPrefabs[m_itemID_itemStorableTemplate[itemID].m_dropModelIndex]);
        UIItemData itemdata = worldItem.GetComponent<UIItemData>();

        itemdata.updateItemData(itemID,additionalData);

        return worldItem;
    }

    public GameObject spawnEquipableItem(int itemID)
    {
        return Instantiate(m_itemEquipablePrefabs[getWorldPrefabIndex(itemID)]);
    }

    public Sprite getGUIIconSprite(int itemID)
    {
        return m_itemIcons[getGUIIconIndex(itemID)];
    }

    public int getGUIIconIndex(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_GUIIconIndex;
    }

    public int getWorldPrefabIndex(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_worldModelIndex;
    }

    public bool getItemStackable(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_stackable;
    }

    public int getMaxStackSize(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_maxStackSize;
    }

    public string getDisplayName(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_displayName;
    }

    public StorableItemTemplate.ItemType getItemType(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_itemType;
    }

    public string getDescription(int itemID)
    {
        return m_itemID_itemStorableTemplate[itemID].m_description;
    }

    public List<Tuple<string,string>> rollRandomAttributes(int itemID, float seedX, float seedY)
    {
        List<RandomItemAttributeMinMax> attributes = m_itemID_itemStorableTemplate[itemID].m_randomAttributesMinMax;
        List<Tuple<string,string>> result = new List<Tuple<string, string>>();

        for(int i = 0; i < attributes.Count; i++)
        {
            if(BasicTools.Random.RandomValuesSeed.getRandomBoolProbability(seedX + 0.33f * i,seedY + 0.33f * i,attributes[i].m_chance))
            {
                int amount = (int)BasicTools.Random.RandomValuesSeed.getRandomValueSeed(seedX+ 0.66f * i,seedY + 0.66f * i,attributes[i].m_minValue,attributes[i].m_maxValue);
                result.Add(new Tuple<string,string>(attributes[i].m_name, amount.ToString()));
            }
        }

        return result;
    }

    private void initialize()
    {
        m_itemID_itemStorableTemplate.Clear();
        m_ItemStorableName_index.Clear();
        loadStorableItemsXMLAsset(m_storableItemsXMLFile.text, Application.systemLanguage);
    }

    private void loadStorableItemsXMLAsset(string inputContent, SystemLanguage language)
    {
        XmlDocument inputFile = null;
        XmlNamespaceManager namespaceManager = null;

        try
        {
            if (inputContent == null || inputContent == "")
            {
                Debug.LogError("ItemManager: could not load storableItems-XML-File: File is null or empty");
                return;
            }

            inputFile = new XmlDocument();
            inputFile.LoadXml(inputContent);
            namespaceManager = new XmlNamespaceManager(inputFile.NameTable);
            namespaceManager.AddNamespace("ehd", "urn:ehd/001");
        }
        catch (Exception ex)
        {
            Debug.LogError("ItemManager: could not load storableItems-XML-File: " + ex);
            return;
        }

        try
        {
            XmlNodeList itemsNodes = inputFile.SelectNodes("//ehd:StorableItem", namespaceManager);
            XmlNode myLanguageNode = null;
            XmlNode englishLanguageNode = null;

            //Debug.Log("ItemManager: language = \"" + language.ToString() + "\"");

            for (int i = 0; i < itemsNodes.Count; i++)
            {
                try
                {
                    myLanguageNode = null;
                    englishLanguageNode = null;
                    List<RandomItemAttributeMinMax> randomAttributeMinMaxList = new List<RandomItemAttributeMinMax>();

                    for (int j = 0; j < itemsNodes[i].ChildNodes.Count; j++)
                    {
                        if (itemsNodes[i].ChildNodes[j].Name == "Language")
                        {
                            if (itemsNodes[i].ChildNodes[j].Attributes["name"].Value.ToUpper() == SystemLanguage.English.ToString().ToUpper())
                            {
                                englishLanguageNode = itemsNodes[i].ChildNodes[j];
                            }
                            if (itemsNodes[i].ChildNodes[j].Attributes["name"].Value.ToUpper() == language.ToString().ToUpper())
                            {
                                myLanguageNode = itemsNodes[i].ChildNodes[j];
                            }
                        }
                        else if(itemsNodes[i].ChildNodes[j].Name == "RandomAttributeMinMax")
                        {
                            try
                            {
                                string attributeName = itemsNodes[i].ChildNodes[j].Attributes["Name"].Value;
                                float chance = float.Parse(itemsNodes[i].ChildNodes[j].Attributes["Chance"].Value.Replace("%",""));
                                float minValue = float.Parse(itemsNodes[i].ChildNodes[j].Attributes["MinValue"].Value);
                                float maxValue = float.Parse(itemsNodes[i].ChildNodes[j].Attributes["MaxValue"].Value);

                                randomAttributeMinMaxList.Add(new RandomItemAttributeMinMax(attributeName, chance, minValue, maxValue));
                            }
                            catch(Exception ex)
                            {
                                Debug.LogError("ItemManager: failed to load RandomAttributeMinMax: " + ex);
                            }
                        }
                    }

                    // load item

                    if (myLanguageNode == null)
                    {
                        Debug.LogWarning("ItemManager: language \"" + language.ToString() + "\" could not be found. Loading english !");
                        myLanguageNode = englishLanguageNode;

                        if (englishLanguageNode == null)
                        {
                            Debug.LogError("ItemManager: Error: language \"" + SystemLanguage.English.ToString() + "\" could not be found.");
                            continue;
                        }
                    }

                    string temp_name = itemsNodes[i].Attributes["StorableItemName"].Value;
                    StorableItemTemplate.ItemType temp_itemType = (StorableItemTemplate.ItemType)int.Parse(itemsNodes[i].Attributes["ItemType"].Value);
                    int temp_GUIIconIndex = int.Parse(itemsNodes[i].Attributes["GUIIconIndex"].Value);
                    int temp_maxStackSize = int.Parse(itemsNodes[i].Attributes["StackSize"].Value);
                    int temp_ID = int.Parse(itemsNodes[i].Attributes["ID"].Value);
                    int temp_worldModelID = int.Parse(itemsNodes[i].Attributes["WorldModelIndex"].Value);
                    int temp_dropModelIndex = int.Parse(itemsNodes[i].Attributes["DropModelIndex"].Value);
                    int temp_lootRarity = int.Parse(itemsNodes[i].Attributes["LootRarity"].Value);

                    string temp_description = myLanguageNode.SelectSingleNode("ehd:Description", namespaceManager).Attributes["Text"].Value;
                    string temp_displayName = myLanguageNode.SelectSingleNode("ehd:DisplayName", namespaceManager).Attributes["Text"].Value;

                    bool temp_stackable = temp_maxStackSize > 1;

                    StorableItemTemplate temp_itemTemplate = new StorableItemTemplate(temp_name, temp_displayName, temp_itemType, temp_GUIIconIndex, temp_stackable, temp_maxStackSize, temp_ID, temp_description, temp_worldModelID, temp_dropModelIndex, randomAttributeMinMaxList);
                    m_itemID_itemStorableTemplate.Add(temp_ID, temp_itemTemplate);
                    m_ItemStorableName_index.Add(temp_name, temp_ID);
                }
                catch (Exception ex2)
                {
                    Debug.LogError("ItemManager: failed to load a StorableItem: " + ex2);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("ItemManager: error reading storableItems-XML-File: " + ex);
        }
    }


}
