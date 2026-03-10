using System;
using UnityEngine;

namespace BattleK.Scripts.Utils
{
    // 인스펙터에서 드롭다운을 띄우기 위한 마커 속성입니다.
    [AttributeUsage(AttributeTargets.Field)]
    public class SelectableReferenceAttribute : PropertyAttribute { }
}