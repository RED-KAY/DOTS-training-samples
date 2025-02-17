using System;
using DebugTool.UI;
using Mono.Cecil;

namespace AutoFarmers.Tools
{
    using AutoFarmers.Farm;
    using Farmer;
    using System.ComponentModel;
    using TMPro;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UI;

    //using Farmer = AutoFarmers.Farm.Farmer;

    public class DebugToolsDataEntries : Utilities.Singleton.Singleton<DebugToolsDataEntries>
    {
        [SerializeField] private Canvas m_TileDebugCanvas;
        [SerializeField] private Color m_RockIdTextColor;
        private Transform m_DebugParent, m_DebugTilesParent, m_DebugRocksParent;
        private Transform m_DebugRocksIDParent, m_DebugTilesIDParent;

        private void Start()
        {
            m_DebugParent = new GameObject("DebuggingParent").transform;
            m_DebugParent.position = Vector3.zero;
            m_DebugParent.rotation = Quaternion.identity;

            m_DebugTilesParent = new GameObject("Tiles").transform;
            m_DebugTilesIDParent = new GameObject("IDs").transform;
            m_DebugTilesParent.parent = m_DebugParent;
            m_DebugTilesIDParent.parent = m_DebugTilesParent;

            m_DebugRocksParent = new GameObject("Rocks").transform;
            m_DebugRocksIDParent = new GameObject("IDs").transform;
            m_DebugRocksParent.parent = m_DebugParent;
            m_DebugRocksIDParent.parent = m_DebugRocksParent;

            m_TileDebugCanvas = Resources.Load<Canvas>("Debugging/TileCanvas");
        }

        private DTEntry[] m_Entries;

        public DTEntry[] Entries
        {
            get { return m_Entries; }
        }

        public void Init()
        {
            m_Entries = new[]
            {
                new DTEntry() //id = 1
                {
                    m_Title = "Show Tile IDs",
                    m_Type = DTType.Toggles,
                    m_Data = new DTToggles()
                    {
                        m_Labels = new string[]
                        {
                            "All",
                            "OG",
                            "None"
                        },

                        m_Values = new[] { false, false, true }
                    },
                    m_Action = (id, data) =>
                    {
                        if (!Initialized) return;
                        string l = (string)data;

                        for (int i = 0; i < m_DebugTilesIDParent.childCount; i++)
                        {
                            Destroy(m_DebugTilesIDParent.GetChild(i).gameObject);
                        }

                        if (l == "None")
                        {
                            return;
                        }

                        using (NativeArray<Entity> entities = m_TileEntityQuery.ToEntityArray(Allocator.TempJob))
                        {
                            foreach (Entity entity in entities)
                            {
                                // Access the components of the entity
                                Tile tile = m_EntityManager.GetComponentData<Tile>(entity);
                                if (tile.m_Status == TileStatus.Rock && l == "OG")
                                {
                                    continue;
                                }

                                Canvas c = Instantiate(m_TileDebugCanvas, m_DebugTilesIDParent);
                                c.transform.position = new Vector3(tile.m_Position.x, tile.m_Position.y + 5f,
                                    tile.m_Position.z);
                                c.transform.rotation = Quaternion.Euler(90, 90, 0);

                                c.GetComponentInChildren<TextMeshProUGUI>().text = tile.m_Id.ToString();

                                // Do something with the components
                                //Debug.Log($"Entity {entity.Index}: Tile = {tile.m_Id}, Status = {tile.m_Status}");
                            }
                        }
                    }
                },

                new DTEntry() //id = 2
                {
                    m_Title = "Hit A Tile",
                    m_Type = DTType.InputField,
                    m_Data = (DTTypeData)new DTInputField(mButtonLabel: "A", mInputFieldPlaceholder: "id"),
                    m_Action = (id, data) =>
                    {
                        if (!Initialized) return;
                        string command = (string)data;

                        //eg. - __id__ -> Moves Farmer to the Tile specified using the ID.
                        //eg. r __id__ -> where r signifies a rock and the __id__ is the Rock ID that needs to mined.

                        if (string.IsNullOrWhiteSpace(command))
                        {
                            Debug.LogError("Command is empty or null.");
                            return;
                        }

                        // Remove any extra whitespace
                        command = command.Trim();

                        // Check if the command starts with "r " (for a rock command)
                        if (command.StartsWith("r "))
                        {
                            // Expected format: "r <id>"
                            string[] parts = command.Split(new char[] { ' ' },
                                System.StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length != 2)
                            {
                                Debug.LogError("Invalid rock command format. Expected format: \"r <id>\".");
                                return;
                            }

                            // Try parsing the id
                            if (!int.TryParse(parts[1], out int rockId) || rockId <= 0)
                            {
                                Debug.LogError("Invalid rock id. It must be a positive integer.");
                                return;
                            }

                            // Process the rock command
                            Debug.Log("Processing rock command. Rock id: " + rockId);
                            // TODO: Clear the rock with the provided id.
                        }
                        else
                        {
                            // Otherwise, assume the command is just a tile id.
                            if (!int.TryParse(command, out int tileId) || tileId <= 0)
                            {
                                Debug.LogError("Invalid tile id. It must be a positive integer.");
                                return;
                            }

                            // Process the tile command
                            Debug.Log("Processing tile command. Tile id: " + tileId);
                            // TODO: Process the tile with the provided id.
                        } 
                    }
                },

                new DTEntry() //id = 3
                {
                    m_Title = "Test Button Debug Tool",
                    m_Type = DTType.Button,
                    m_Data = (DTTypeData)new DTButton(string.Empty)
                },

                new DTEntry() //id = 4
                {
                    m_Title = "Show Rock IDs",
                    m_Type = DTType.Toggles,
                    m_Data = new DTToggles()
                    {
                        m_Labels = new string[]
                        {
                            "All",
                            "None"
                        },

                        m_Values = new[] { false, true }
                    },
                    m_Action = (id, data) =>
                    {
                        if (!Initialized) return;
                        string l = (string)data;

                        for (int i = 0; i < m_DebugRocksIDParent.childCount; i++)
                        {
                            Destroy(m_DebugRocksIDParent.GetChild(i).gameObject);
                        }

                        if (l == "None")
                        {
                            return;
                        }

                        using (NativeArray<Entity> entities = m_RockEntityQuery.ToEntityArray(Allocator.TempJob))
                        {
                            foreach (var entity in entities)
                            {
                                Rock rock = m_EntityManager.GetComponentData<Rock>(entity);
                                Canvas c = Instantiate(m_TileDebugCanvas, m_DebugRocksIDParent);
                                c.transform.position =
                                    new Vector3(rock.m_Center.x, rock.m_Center.y + 1f, rock.m_Center.z);
                                c.transform.rotation = Quaternion.Euler(90, 90, 0);

                                var tmp = c.GetComponentInChildren<TextMeshProUGUI>();
                                tmp.text = rock.m_RockId.ToString();
                                tmp.color = m_RockIdTextColor;

                                c.GetComponentInChildren<TextMeshProUGUI>().text = rock.m_RockId.ToString();
                            }
                        }
                    }
                },
            };
        }

        private bool m_Initialized;
        public bool Initialized => m_Initialized;

        private EntityManager m_EntityManager;
        private EntityQuery m_TileEntityQuery, m_RockEntityQuery, m_FarmerEntityQuery;

        public void TryInitialize()
        {
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                m_TileEntityQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Tile>());
                m_RockEntityQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Rock>());
                m_FarmerEntityQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Farmer>());

                m_Initialized = true;
            }
        }

        private void Update()
        {
            if (!m_Initialized)
                TryInitialize();
        }


        public void SetDebugPanel(bool value)
        {
            gameObject.SetActive(value);
        }

        private void OnDestroy()
        {
            m_FarmerEntityQuery.Dispose();
            m_TileEntityQuery.Dispose();
            m_RockEntityQuery.Dispose();
        }
    }

    /*

    public class DebuggingCanvas : Utilities.Singleton.Singleton<DebuggingCanvas>
    {
        private bool _initialized = false, _doneOperation = false;
        EntityManager m_EntityManager;
        private EntityQuery m_TileEntityQuery, m_RockEntityQuery, m_FarmerEntityQuery;
        [SerializeField] private Canvas m_TileDebugCanvas;
        [SerializeField] private Transform m_DebugParent;

        [SerializeField] private Color m_RockIdTextColor;

        [SerializeField] private TMP_InputField _rockId;
        [SerializeField] private TMP_InputField _locationId;

        [SerializeField] private GameObject _debugPanel;

        public void DebugToolsDataEntries()
        {

        }

        private void Update()
        {
            if(!_initialized)
                TryInitialize();

            if(_initialized && !_doneOperation)
            {
                using (NativeArray<Entity> entities = m_TileEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (Entity entity in entities)
                    {
                        // Access the components of the entity
                        Tile tile = m_EntityManager.GetComponentData<Tile>(entity);
                        if (tile.m_Status == TileStatus.Rock)
                        {
                            continue;
                        }
                        Canvas c = Instantiate(m_TileDebugCanvas, m_DebugParent);
                        c.transform.position = new Vector3( tile.m_Position.x, tile.m_Position.y + 1f, tile.m_Position.z);
                        c.transform.rotation = Quaternion.Euler(90, 90, 0);

                        c.GetComponentInChildren<TextMeshProUGUI>().text = tile.m_Id.ToString();

                        // Do something with the components
                        //Debug.Log($"Entity {entity.Index}: Tile = {tile.m_Id}, Status = {tile.m_Status}");
                        _doneOperation = true;
                    }
                }

                using (NativeArray<Entity> entities = m_RockEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        Rock rock = m_EntityManager.GetComponentData<Rock>(entity);
                        Canvas c = Instantiate(m_TileDebugCanvas, m_DebugParent);
                        c.transform.position = new Vector3(rock.m_Center.x, rock.m_Center.y + 1f, rock.m_Center.z);
                        c.transform.rotation = Quaternion.Euler(90, 90, 0);

                        var tmp = c.GetComponentInChildren<TextMeshProUGUI>();
                        tmp.text = rock.m_RockId.ToString();
                        tmp.color = m_RockIdTextColor;

                        c.GetComponentInChildren<TextMeshProUGUI>().text = rock.m_RockId.ToString();
                    }
                }
            }
        }

        public void SetDebugPanel(bool value)
        {
           _debugPanel.SetActive(value);
        }

        void TryInitialize()
        {
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                m_TileEntityQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Tile>());
                m_RockEntityQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Rock>());
                m_FarmerEntityQuery = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Farmer>());

                _initialized = true;
            }

        }
        public void RemovePressed()
        {
            if(_rockId == null)
            {
                Debug.LogError("No InputField assigned to read the id from!");
                return;
            }

            int idToRemove = int.Parse(_rockId.text);

            using (NativeArray<Entity> entities = m_RockEntityQuery.ToEntityArray(Allocator.TempJob))
            {
                foreach (var entity in entities)
                {
                    Rock rock = m_EntityManager.GetComponentData<Rock>(entity);
                    if(rock.m_RockId == idToRemove)
                    {
                        if (m_EntityManager.HasComponent<RockHitTag>(entity))
                        {
                            var removeTag = m_EntityManager.GetComponentData<RockHitTag>(entity);
                            removeTag.m_Hit = true;
                            m_EntityManager.SetComponentData(entity, removeTag);
                        }
                        else
                        {
                            m_EntityManager.AddComponentData(entity, new RockHitTag { m_Hit = true });
                        }
                    }
                }
            }
        }
        public void MoveFarmer()
        {
            if (_locationId == null)
            {
                Debug.LogError("No InputField assigned to read the id from!");
                return;
            }

            string input = _locationId.text;
            string[] splitText = _locationId.text.Split(" ");
            foreach (string line in splitText) {
                Debug.Log(line);
            }

            float3 location = float3.zero;
            int farmerId = int.Parse(splitText[0]);


            if (splitText.Length == 3)
            {
                if (splitText[2] == "r")
                {
                    int rockId = int.Parse(splitText[1]);
                    using (NativeArray<Entity> entities = m_RockEntityQuery.ToEntityArray(Allocator.TempJob))
                    {
                        foreach (var entity in entities)
                        {
                            Rock rock = m_EntityManager.GetComponentData<Rock>(entity);
                            if (rock.m_RockId == rockId)
                            {
                                location = rock.m_Center;
                            }
                        }
                    }
                }
            }
            else if (splitText.Length == 2) {
                int tileId = int.Parse(splitText[1]);
                using (NativeArray<Entity> entities = m_TileEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        Tile tile = m_EntityManager.GetComponentData<Tile>(entity);
                        if (tile.m_Id == tileId)
                        {
                            location = tile.m_Position;
                        }
                    }
                }
            }

            if (!location.Equals(float3.zero)) {
                using (NativeArray<Entity> entities = m_FarmerEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        Farmer farmer = m_EntityManager.GetComponentData<Farmer>(entity);
                        if (farmer.m_Id == farmerId)
                        {
                            m_EntityManager.AddComponentData(entity, new FarmerMove { _Location = location });
                        }
                    }
                }
            }
        }
    }*/
}