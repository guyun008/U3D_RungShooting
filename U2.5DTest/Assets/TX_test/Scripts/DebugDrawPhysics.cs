using UnityEngine;
using System.Collections;

namespace TestTools
{
    public static class DebugDrawPhysics {

        public static void DebugDrawCircle(Vector3 center, float radius, Color color, float duration, int noOfVerts = 8) {

#if UNITY_EDITOR
            for (int i = 0; i < noOfVerts; i++) {
                Debug.DrawLine(center + new Vector3(radius * Mathf.Sin(2 * Mathf.PI * i / noOfVerts), radius * Mathf.Cos(2 * Mathf.PI * i / noOfVerts), 0),
                               center + new Vector3(radius * Mathf.Sin(2 * Mathf.PI * (i + 1) / noOfVerts), radius * Mathf.Cos(2 * Mathf.PI * (i + 1) / noOfVerts), 0), color, duration);
            }
#endif
        }

        public static void DebugDrawCircle(Vector3 center, float radius, Color color, int noOfVerts = 8) {

#if UNITY_EDITOR
            for (int i = 0; i < noOfVerts; i++) {
                Debug.DrawLine(center + new Vector3(radius * Mathf.Sin(2 * Mathf.PI * i / noOfVerts), radius * Mathf.Cos(2 * Mathf.PI * i / noOfVerts), 0),
                               center + new Vector3(radius * Mathf.Sin(2 * Mathf.PI * (i + 1) / noOfVerts), radius * Mathf.Cos(2 * Mathf.PI * (i + 1) / noOfVerts), 0), color);
            }
#endif
        }

        public static void DebugDrawCross(Vector3 center, float size, Color color) {
#if UNITY_EDITOR
            Vector3 sizeVec = new Vector3(size / 2, size / 2, 0);
            Debug.DrawLine(center - sizeVec, center + sizeVec);
            sizeVec.y *= -1;
            Debug.DrawLine(center - sizeVec, center + sizeVec);
#endif
        }

        public static void DebugDrawCircleCast(Vector2 start, float radius, Vector2 direction, float distance, Color color) {
#if UNITY_EDITOR

            DebugDrawCircle(start, radius, color);
            if (distance == 0) return;
            Vector2 end = start + direction * distance;
            Vector2 perp = new Vector2(direction.y, direction.x);
            Debug.DrawLine(start + perp * radius, end + perp * radius, color);
            Debug.DrawLine(start - perp * radius, end - perp * radius, color);

            DebugDrawCircle(end, radius, color);
#endif
        }

        public static void DebugDrawBounds(Bounds bounds, Color color, float duration = 0) {
            DebugDrawBox(bounds.center, bounds.size * 2, 0, color, duration);
        }

        public static void DebugDrawBox(Vector2 center, Vector2 size, float angle, Color color, float duration = 0) {
#if UNITY_EDITOR

            Vector2 pos1 = center + new Vector2(size.x / 2, size.y / 2).Rotate(angle);
            Vector2 pos2 = center + new Vector2(size.x / 2, -size.y / 2).Rotate(angle);
            if(duration == 0)
                Debug.DrawLine(pos1, pos2, color);
            else
                Debug.DrawLine(pos1, pos2, color, duration);

            pos1 = center + new Vector2(size.x / 2, size.y / 2).Rotate(angle);
            pos2 = center + new Vector2(-size.x / 2, size.y / 2).Rotate(angle);
            if (duration == 0)
                Debug.DrawLine(pos1, pos2, color);
            else
                Debug.DrawLine(pos1, pos2, color, duration);

            pos1 = center + new Vector2(-size.x / 2, size.y / 2).Rotate(angle);
            pos2 = center + new Vector2(-size.x / 2, -size.y / 2).Rotate(angle);
            if (duration == 0)
                Debug.DrawLine(pos1, pos2, color);
            else
                Debug.DrawLine(pos1, pos2, color, duration);

            pos1 = center + new Vector2(-size.x / 2, -size.y / 2).Rotate(angle);
            pos2 = center + new Vector2(size.x / 2, -size.y / 2).Rotate(angle);
            if (duration == 0)
                Debug.DrawLine(pos1, pos2, color);
            else
                Debug.DrawLine(pos1, pos2, color, duration);

#endif
        }

        public static void DebugDrawBoxCast(Vector2 start, Vector2 size, float angle, Vector2 direction, float distance, Color color, float duration = 0) {
#if UNITY_EDITOR
            DebugDrawBox(start, size, angle, color, duration);
           

            if (distance == 0) return;

            Vector2 move = direction * distance;
            DebugDrawBox(start + move, size, angle, color, duration);
                        
#endif
        }

        private static Vector2 Rotate(this Vector2 v, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}