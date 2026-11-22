using UnityEngine;

namespace MobileGameProject.SplineNetwork
{
    public static class SplineMath
    {
        public static Vector3 EvaluateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return uuu * p0
                 + 3f * uu * t * p1
                 + 3f * u * tt * p2
                 + ttt * p3;
        }

        public static Vector3 EvaluateCubicBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            return 3f * u * u * (p1 - p0)
                 + 6f * u * t * (p2 - p1)
                 + 3f * t * t * (p3 - p2);
        }

        public static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps = 32)
        {
            float length = 0f;
            Vector3 prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 point = SplineMath.EvaluateCubicBezier(p0, p1, p2, p3, t);
                length += Vector3.Distance(prev, point);
                prev = point;
            }
            return length;
        }

        public static void GetFrame(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, Quaternion startRotation, Quaternion endRotation, out Vector3 tangent, out Vector3 normal, out Vector3 binormal)
        {
            tangent = EvaluateCubicBezierTangent(p0, p1, p2, p3, t).normalized;
            if (tangent.sqrMagnitude < 0.0001f)
                tangent = Vector3.forward;

            Quaternion nodeRot = Quaternion.Slerp(startRotation, endRotation, t);
            Vector3 up = nodeRot * Vector3.up;

            if (Mathf.Abs(Vector3.Dot(tangent, up)) > 0.99f)
                up = nodeRot * Vector3.forward;

            binormal = Vector3.Cross(tangent, up).normalized;
            normal = Vector3.Cross(binormal, tangent).normalized;
        }
    }
}
