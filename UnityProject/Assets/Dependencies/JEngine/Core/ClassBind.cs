using System;
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
        public bool ActiveAfter = false;
        public bool RequireBindFields = false;
        [Tooltip("如果是GameObject，请填写完整路径，并且Active为true;\r\n" +
                 "如果是Unity脚本，需要填写GameObject全路径.脚本名称（脚本名称无空格，例如：Canvas/Text.Text，并且GameObject的Active为true）")]public _ClassField[] Fields;
        public bool BoundData
        {
            get;
            set;
        } = false;
    }

    [System.Serializable]
    public class _ClassField
    {
        public enum FieldType
        {
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            Float,
            Decimal,
            Double,
            String,
            GameObject,
            UnityComponent
        }

        public FieldType fieldType;
        public string fieldName;
        public string value;
    }
}
