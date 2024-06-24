using AutoFarmers.Farm;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AutoFarmers.Tools
{
    public class DebuggingCanvas : MonoBehaviour
    {
        private bool _initialized = false, _doneOperation = false;
        EntityManager _entityManager;
        private EntityQuery _tileEntityQuery, _rockEntityQuery;
        [SerializeField] private Canvas _debugCanvas;
        [SerializeField] private Transform _parent;

        [SerializeField] private Color _rockIdTextColor;

        [SerializeField] private TMP_InputField _id;


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

                _initialized = true;
            }
            
        }

        public void RemovePressed()
        {
            if(_id == null)
            {
                Debug.LogError("No InputField assigned to read the id from!");
                return;
            }

            int idToRemove = int.Parse(_id.text);

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
    }
}