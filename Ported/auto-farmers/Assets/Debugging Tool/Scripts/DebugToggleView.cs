using TMPro;

namespace DebugTool.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEditor;
    
    [RequireComponent(typeof(ToggleGroup))]
    public class DebugToggleView : MonoBehaviour
    {
        private ToggleGroup m_ToggleGroup;
        
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private Toggle m_Toggle;
        
        private void Start()
        {
            m_ToggleGroup = GetComponent<ToggleGroup>();
        }

        public void Initialize(ref string[] labels, ref bool[] values)
        {
            for (int i=0; i<labels.Length; i++)
            { 
               Toggle t = Instantiate(m_Toggle, m_Content);
               t.gameObject.SetActive(true);
               TextMeshProUGUI tmp = t.transform.Find("Label").GetComponent<TextMeshProUGUI>();
               tmp.text = labels[i];
               if (values.Length <= 0 || values.Length - 1 < i)
                   t.isOn = false;
               else
                   t.isOn = values[i];
                
               t.group = m_ToggleGroup;

               t.onValueChanged.AddListener(OnValueChanged) ;
            }
            
        }

        private void OnValueChanged(bool arg0)
        {
            
        }
    }
}