using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JEngine.Core
{
    public class ClassBind: MonoBehaviour
    {
        public _ClassBind[] Classes = new _ClassBind[1];
    }

    [System.Serializable]
    public class _ClassBind
    {
        public string Namespace = "HotUpdateScripts";
        public string Class = "";
    }
}
