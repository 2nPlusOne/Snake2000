using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class SnakeSegment : MonoBehaviour
{
    [SerializeField] protected SegmentTypesSO SegmentTypes;

    public List<SnakeSegment> Segments =  // List of all segments
        new List<SnakeSegment>();

    protected List<Vector3> DigestPositions = // List of coordinates where food was picked up
        new List<Vector3>();

    protected bool isDigesting = false;   // Whether to switch this segment to its enlarged version
    int index;                            // Index of this segment in the list of segments
    Vector2 prevDirection;                // Direction of the previous segment
    protected Vector2 direction;          // Direction of this segment


    void OnEnable()
    {
        GameEvents.Instance.OnSegmentsListUpdated += UpdateSegmentList;
        GameEvents.Instance.OnDigestPositionsListUpdated += UpdateDigestPositionsList;
    }

    void FixedUpdate()
    {
        HandleDigestion();
        direction = GetDirection();

        // Make sure this segment isn't the tail
        if (index < Segments.Count - 1)
        {
            prevDirection = GetPreviousDirection();
            HandleBodyGraphics();
        }

        else if (index == Segments.Count - 1)
        {
            HandleTailGraphics();
            return;
        }
    }

    void HandleDigestion()
    {
        // If not digesting food at this position, set isDigesting to false and do nothing.
        if (!DigestPositions.Contains(this.transform.position))
        {
            isDigesting = false;
            return;
        }


        // If this is the tail segment, remove this position from the list
        else if (index == Segments.Count - 1)
            DigestPositions.Remove(this.transform.position);
        
        isDigesting = true;
    }

    Vector2 GetPreviousDirection()
    {
        Vector2 _prevDirection = Segments[index].transform.position - Segments[index + 1].transform.position;
        if (Mathf.Abs(_prevDirection.x) > 60) 
            _prevDirection.x *= -1; // Invert direction on horizontal edge wrap
        if (Mathf.Abs(_prevDirection.y) > 20) 
            _prevDirection.y *= -1; // Invert for vertical edge wrap
        return _prevDirection.normalized;
    }
    Vector2 GetDirection()
    {
        Vector2 _direction = Segments[index - 1].transform.position - this.transform.position;
        if (Mathf.Abs(_direction.x) > 60) 
            _direction.x *= -1; // Invert direction on horizontal edge wrap
        if (Mathf.Abs(_direction.y) > 20)
            _direction.y *= -1; // Invert for vertical edge wrap
        return _direction.normalized;
    }

    void HandleBodyGraphics()
    {
        // Straight segment types
        if (prevDirection == Vector2.right && direction == Vector2.right)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyStraightRightUpFat);
            } 
            else
                SwapToSegmentType(SegmentTypes.BodyStraightRightUp);
        }
        else if (prevDirection == Vector2.left && direction == Vector2.left)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyStraightLeftDownFat);
            }
            else
                SwapToSegmentType(SegmentTypes.BodyStraightLeftDown);
        }
        else if (prevDirection == Vector2.up && direction == Vector2.up)
        {
            if (isDigesting)
            {
                Transform seg = SwapToSegmentType(SegmentTypes.BodyStraightRightUpFat);
                seg.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                Transform seg = SwapToSegmentType(SegmentTypes.BodyStraightRightUp);
                seg.Rotate(new Vector3(0, 0, 90));
            }
        }
        else if (prevDirection == Vector2.down && direction == Vector2.down)
        {
            if (isDigesting)
            {
                Transform seg = SwapToSegmentType(SegmentTypes.BodyStraightLeftDownFat);
                seg.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                Transform seg = SwapToSegmentType(SegmentTypes.BodyStraightLeftDown);
                seg.Rotate(new Vector3(0, 0, 90));
            }
        }

        // Corner segment types
        else if (prevDirection == Vector2.left && direction == Vector2.down)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftDownUpRightFat);
            }
            else
                SwapToSegmentType(SegmentTypes.BodyCornerLeftDownUpRight);
        }
        else if (prevDirection == Vector2.up && direction == Vector2.right)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftDownUpRightFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftDownUpRight);
            }
        }
        else if (prevDirection == Vector2.left && direction == Vector2.up)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftUpDownRightFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftUpDownRight);
            }
        }
        else if (prevDirection == Vector2.down && direction == Vector2.right)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftUpDownRightFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerLeftUpDownRight);
            }
        }
        else if (prevDirection == Vector2.right && direction == Vector2.down)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightDownUpLeftFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightDownUpLeft);
            }
        }
        else if (prevDirection == Vector2.up && direction == Vector2.left)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightDownUpLeftFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightDownUpLeft);
            }
        }
        else if (prevDirection == Vector2.right && direction == Vector2.up)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightUpDownLeftFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightUpDownLeft);
            }
        }
        else if (prevDirection == Vector2.down && direction == Vector2.left)
        {
            if (isDigesting)
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightUpDownLeftFat);
            }
            else
            {
                SwapToSegmentType(SegmentTypes.BodyCornerRightUpDownLeft);
            }
        }
    }

    void HandleTailGraphics()
    {
        if (direction == Vector2.right)
        {
            SwapToSegmentType(SegmentTypes.TailRightUp);
        }
        if (direction == Vector2.left)
        {
            SwapToSegmentType(SegmentTypes.TailLeftDown);
        }
        if (direction == Vector2.up)
        {
            Transform seg = SwapToSegmentType(SegmentTypes.TailRightUp);
            seg.Rotate(new Vector3(0, 0, 90));
        }
        if (direction == Vector2.down)
        {
            Transform seg = SwapToSegmentType(SegmentTypes.TailLeftDown);
            seg.Rotate(new Vector3(0, 0, 90));
        }
    }

    Transform SwapToSegmentType(GameObject segToInstantiate)
    {
        foreach (Transform child in this.transform)
            Destroy(child.gameObject);

        GameObject segment = Instantiate(segToInstantiate);
        segment.transform.position = this.transform.position;
        segment.transform.SetParent(this.transform);

        return segment.transform;
    }

    void UpdateSegmentList(List<SnakeSegment> segmentList)
    {
        // Upon instantiation, a segment is always the tail
        index = Segments.IndexOf(this);
        Segments = segmentList;
    }

    void UpdateDigestPositionsList(List<Vector3> _digestPositions)
    {
        DigestPositions = _digestPositions;
    }

    void OnDisable()
    {
        if (GameEvents.Instance) // Prevents null reference error when closing game or changing scenes
        {
            GameEvents.Instance.OnSegmentsListUpdated -= UpdateSegmentList;
            GameEvents.Instance.OnDigestPositionsListUpdated -= UpdateDigestPositionsList;
        }
    }
}
