namespace AutoFarmers.Tools
{
    using AutoFarmers.Farm;
    using AutoFarmers.Farmers;
    using System.ComponentModel;
    using TMPro;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UI;
    //using Farmer = AutoFarmers.Farm.Farmer;
    public class DebuggingCanvas : MonoBehaviour
    {
        private bool _initialized = false, _doneOperation = false;
        EntityManager _entityManager;
        private EntityQuery _tileEntityQuery, _rockEntityQuery, _farmerEntityQuery;
        [SerializeField] private Canvas _debugCanvas;
        [SerializeField] private Transform _parent;

        [SerializeField] private Color _rockIdTextColor;

        [SerializeField] private TMP_InputField _rockId;
        [SerializeField] private TMP_InputField _locationId;



        private void Update()
        {
            if(!_initialized)
                TryInitialize();
            

            if(_initialized && !_doneOperation)
            {
                using (NativeArray<Entity> entities = _tileEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (Entity entity in entities)
                    {
                        // Access the components of the entity
                        Tile tile = _entityManager.GetComponentData<Tile>(entity);
                        if (tile.m_Status == TileStatus.Rock)
                        {
                            continue;
                        }
                        Canvas c = Instantiate(_debugCanvas, _parent);
                        c.transform.position = new Vector3( tile.m_Position.x, tile.m_Position.y + 1f, tile.m_Position.z);
                        c.transform.rotation = Quaternion.Euler(90, 90, 0);

                        c.GetComponentInChildren<TextMeshProUGUI>().text = tile.m_Id.ToString();

                        // Do something with the components
                        //Debug.Log($"Entity {entity.Index}: Tile = {tile.m_Id}, Status = {tile.m_Status}");
                        _doneOperation = true;
                    }
                }

                using (NativeArray<Entity> entities = _rockEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        Rock rock = _entityManager.GetComponentData<Rock>(entity);
                        Canvas c = Instantiate(_debugCanvas, _parent);
                        c.transform.position = new Vector3(rock.m_Center.x, rock.m_Center.y + 1f, rock.m_Center.z);
                        c.transform.rotation = Quaternion.Euler(90, 90, 0);

                        var tmp = c.GetComponentInChildren<TextMeshProUGUI>();
                        tmp.text = rock.m_RockId.ToString();
                        tmp.color = _rockIdTextColor;

                        c.GetComponentInChildren<TextMeshProUGUI>().text = rock.m_RockId.ToString();
                    }
                }
            }
        }

        void TryInitialize()
        {
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                _tileEntityQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<Tile>());
                _rockEntityQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<Rock>());
                _farmerEntityQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<Farmer>());

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

            using (NativeArray<Entity> entities = _rockEntityQuery.ToEntityArray(Allocator.TempJob))
            {
                foreach (var entity in entities)
                {
                    Rock rock = _entityManager.GetComponentData<Rock>(entity);
                    if(rock.m_RockId == idToRemove)
                    {
                        if (_entityManager.HasComponent<RockHitTag>(entity))
                        {
                            var removeTag = _entityManager.GetComponentData<RockHitTag>(entity);
                            removeTag.m_Hit = true;
                            _entityManager.SetComponentData(entity, removeTag);
                        }
                        else
                        {
                            _entityManager.AddComponentData(entity, new RockHitTag { m_Hit = true });
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
                    using (NativeArray<Entity> entities = _rockEntityQuery.ToEntityArray(Allocator.TempJob))
                    {
                        foreach (var entity in entities)
                        {
                            Rock rock = _entityManager.GetComponentData<Rock>(entity);
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
                using (NativeArray<Entity> entities = _tileEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        Tile tile = _entityManager.GetComponentData<Tile>(entity);
                        if (tile.m_Id == tileId)
                        {
                            location = tile.m_Position;
                        }
                    }
                }
            }

            if (!location.Equals(float3.zero)) {
                using (NativeArray<Entity> entities = _farmerEntityQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        Farmer farmer = _entityManager.GetComponentData<Farmer>(entity);
                        if (farmer.m_Id == farmerId)
                        {
                            _entityManager.AddComponentData(entity, new FarmerMove { _Location = location });
                        }
                    }
                }
            }


            //int id = int.Parse(_locationId.text);

            //using (NativeArray<Entity> entities = _rockEntityQuery.ToEntityArray(Allocator.TempJob))
            //{
            //    foreach (var entity in entities)
            //    {
            //        Rock rock = _entityManager.GetComponentData<Rock>(entity);
            //        if (rock.m_RockId == id)
            //        {
            //            if (_entityManager.HasComponent<RockHitTag>(entity))
            //            {
            //                var removeTag = _entityManager.GetComponentData<RockHitTag>(entity);
            //                removeTag.m_Hit = true;
            //                _entityManager.SetComponentData(entity, removeTag);
            //            }
            //            else
            //            {
            //                _entityManager.AddComponentData(entity, new RockHitTag { m_Hit = true });
            //            }
            //        }
            //    }
            //}
        }
    }
}