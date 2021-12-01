using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Segment Types", menuName = "Scriptable Objects/Segment Types")]
public class SegmentTypesSO : ScriptableObject
{
    public GameObject BodyCornerLeftDownUpRightFat;
    public GameObject BodyCornerLeftDownUpRight;
    public GameObject BodyCornerLeftUpDownRightFat;
    public GameObject BodyCornerLeftUpDownRight;
    public GameObject BodyCornerRightDownUpLeftFat;
    public GameObject BodyCornerRightDownUpLeft;
    public GameObject BodyCornerRightUpDownLeftFat;
    public GameObject BodyCornerRightUpDownLeft;
    public GameObject BodyStraightLeftDownFat;
    public GameObject BodyStraightLeftDown;
    public GameObject BodyStraightRightUpFat;
    public GameObject BodyStraightRightUp;
    public GameObject HeadLeftDownEat;
    public GameObject HeadLeftDown;
    public GameObject HeadRightUpEat;
    public GameObject HeadRightUp;
    public GameObject TailLeftDown;
    public GameObject TailRightUp;
}
