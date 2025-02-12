using UnityEngine;

namespace DebugTool.UI
{
    public struct DTEntry
    {
        public int m_Id;
        public string m_Title;
        public DTType m_Type;
        public DTTypeData m_Data;
    }

    public interface DTTypeData
    {
        
    }

    public struct DTToggles : DTTypeData
    {
        public string[] m_Labels;
        public bool[] m_Values;
    }

    public struct DTInputField : DTTypeData
    {
        public string m_InputFieldPlaceholder;
        public string m_ButtonLabel;

        public DTInputField(string mButtonLabel, string mInputFieldPlaceholder)
        {
            m_ButtonLabel = mButtonLabel;
            m_InputFieldPlaceholder = mInputFieldPlaceholder;
        }
    }

    public struct DTButton : DTTypeData
    {
        public string m_Label;

        public DTButton(string mLabel)
        {
            m_Label = mLabel;
        }
    }

    public enum DTType
    {
        None,
        Toggles,
        InputField,
        Button
    }

    public class DebuggingToolSet : MonoBehaviour
    {
        private DTEntry[] m_Entries;
        
        [SerializeField] private DebugToggleView m_DebugToggleViewPrefab;
        [SerializeField] private DebugInputFieldView m_DebugInputFieldPrefab;
        [SerializeField] private DebugButtonView m_DebugButtonPrefab;

        void DefineEntriesHERE()
        {
            m_Entries = new[]
            {
                new DTEntry() //id = 1
                {
                    m_Title = "Debug Tool 1",
                    m_Type = DTType.Toggles,
                    m_Data = new DTToggles()
                    {
                        m_Labels = new string[]
                        {
                            "Test 1",
                            "Test 2",
                        },

                        m_Values = new[] { false, false }
                    }
                },

                new DTEntry() //id = 2
                {
                    m_Title = "Debug Tool 2",
                    m_Type = DTType.InputField,
                    m_Data = (DTTypeData)new DTInputField(mButtonLabel: "A", mInputFieldPlaceholder: "id")
                },

                new DTEntry() //id = 3
                {
                    m_Title = "Debug Tool 3",
                    m_Type = DTType.Button,
                    m_Data = (DTTypeData)new DTButton(mLabel: "Test 3")
                }
            };
        }
        
        
        void Awake()
        {
            DefineEntriesHERE();
            InitilizeTool();

        }

        private void InitilizeTool()
        {
            for (int i=0; i<m_Entries.Length; i++)
            {
                DTEntry entry = m_Entries[i];
                entry.m_Id = i+1;

                switch (entry.m_Type)
                {
                    case DTType.Toggles:
                        break;
                    
                    case DTType.InputField:
                        break;
                    
                    case DTType.Button:
                        break;
                    
                    default:
                        Debug.LogError("Unknown DTType: " + entry.m_Type);
                        return;
                }
            }
        }
    }


}
