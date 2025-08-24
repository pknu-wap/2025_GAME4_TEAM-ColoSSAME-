using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Square,
    Wedge,
    InvertedTriangle
}

public class StrategyDB : MonoBehaviour
{
    // 중심이 (0, 0) 기준일 때 상대적인 좌표
    private readonly Dictionary<FormationType, Vector2[]> formationPositions = new()
    {
        { FormationType.Square, new Vector2[]
            {
                new Vector2(-50, 40),
                new Vector2(50, 40),
                new Vector2(-50, -20),
                new Vector2(50, -20)
            }
        },
        { FormationType.Wedge, new Vector2[]
            {
                new Vector2(0, 45),
                new Vector2(0, -45),
                new Vector2(-75, 0),
                new Vector2(75, 0)
            }
        },
        { FormationType.InvertedTriangle, new Vector2[]
            {
                new Vector2(-50, 45),
                new Vector2(-50, -45),
                new Vector2(0, 0),
                new Vector2(75, 0)
            }
        }
    };

    public Vector2[] GetFormationPositions(FormationType type)
    {
        return formationPositions.TryGetValue(type, out var pos) ? pos : null;
    }
}