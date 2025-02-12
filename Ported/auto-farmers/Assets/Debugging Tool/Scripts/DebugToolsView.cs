using UnityEngine;

namespace DebugTool.UI
{
    public class DebugToolsView : MonoBehaviour
    {
        protected DTEntry m_EntryData;

        public int Id
        {
            get
            {
               // if (m_EntryData == null)
               // {
               //     Debug.LogError("DTEntry not set and a class is trying to access it!");
               //     return -1;
               // }
                return m_EntryData.m_Id;
            }
        }

        public virtual void Initialize(DTEntry entryData)
        {
            m_EntryData = entryData;
        }

        protected bool m_StartCalled;
    }
}