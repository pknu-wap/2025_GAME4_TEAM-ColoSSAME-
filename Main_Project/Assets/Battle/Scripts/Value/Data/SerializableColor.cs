using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    [System.Serializable]
    public struct SerializableColor
    {
        public float r, g, b, a;

        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToUnityColor()
        {
            return new Color(r, g, b, a);
        }
    }
}