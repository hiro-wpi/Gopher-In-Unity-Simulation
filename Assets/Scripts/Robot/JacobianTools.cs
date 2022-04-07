using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JacobianTools
{
    public static void Print(ArticulationJacobian jacobian)
    {
        string jacobianString = "";
        for (int i = 0; i < jacobian.rows; i++)
        {
            string row = "";
            for (int j = 0; j < jacobian.columns; j++)
            {
                row += jacobian[i, j] + ", ";
            }
            jacobianString += row + "\n";
        }
        Debug.Log(jacobianString);
    }
    public static List<float> Multiply(ArticulationJacobian jacobian, List<float> targetDelta)
    {
        List<float> result = new List<float>(jacobian.rows);
        for (int i = 0; i < jacobian.rows; i++)
        {
            result.Add(0.0f);
            for (int j = 0; j < jacobian.columns; j++)
            {
                result[i] += jacobian[i, j] * targetDelta[j];
            }
        }

        return result;
    }

    public static ArticulationJacobian Multiply(ArticulationJacobian jacobian1, ArticulationJacobian jacobian2)
    {
        if (jacobian1.columns != jacobian2.rows)
            throw new Exception("Can't multiply jacobians, jacobian1.columns != jacobian2.rows!");
        ArticulationJacobian result = new ArticulationJacobian(jacobian1.rows, jacobian2.columns);
        for (int row = 0; row < jacobian1.rows; row++)
            for (int column = 0; column < jacobian2.columns; column++)
                for (int i = 0; i < jacobian1.columns; i++)
                    result[row, column] += jacobian1[row, i] * jacobian2[i, column];
        return result;
    }

    public static ArticulationJacobian Multiply(ArticulationJacobian jacobian, float value)
    {
        ArticulationJacobian result = new ArticulationJacobian(jacobian.rows, jacobian.columns);
        for (int row = 0; row < jacobian.rows; row++)
            for (int column = 0; column < jacobian.columns; column++)
                result[row, column] = jacobian[row, column] * value;
        return result;
    }

    public static ArticulationJacobian Add(ArticulationJacobian jacobian1, ArticulationJacobian jacobian2)
    {
        if (jacobian1.rows != jacobian2.rows || jacobian1.columns != jacobian2.columns)
            throw new Exception("Can't add jacobians, matrix dimensions are not equal!");
        ArticulationJacobian result = new ArticulationJacobian(jacobian1.rows, jacobian1.columns);
        for (int row = 0; row < jacobian1.rows; row++)
            for (int column = 0; column < jacobian1.columns; column++)
                result[row, column] = jacobian1[row, column] + jacobian2[row, column];
        return result;
    }

    public static ArticulationJacobian Transpose(ArticulationJacobian jacobian)
    {
        ArticulationJacobian jacobianT = new ArticulationJacobian(jacobian.columns, jacobian.rows);
        for (int i = 0; i < jacobian.rows; i++)
        {
            for (int j = 0; j < jacobian.columns; j++)
                jacobianT[j, i] = jacobian[i, j];
        }

        return jacobianT;
    }

    public static ArticulationJacobian SwapRows(ArticulationJacobian jacobian, int row1, int row2)
    {
        if (row1 == row2)
            return jacobian;
        for (int i = 0; i < jacobian.columns; i++)
        {
            float temp = jacobian[row1, i];
            jacobian[row1, i] = jacobian[row2, i];
            jacobian[row2, i] = temp;
        }
        return jacobian;
    }

    public static ArticulationJacobian EulerToQuaternion(ArticulationJacobian jacobian)
    {
        if (jacobian.rows != 6)
            throw new Exception("Can't convert Euler to Quaternion, jacobian.columns != 6!");
        ArticulationJacobian result = new ArticulationJacobian(7, jacobian.columns);
        for (int c = 0; c < jacobian.columns; c++)
        {
            result[0, c] = jacobian[0, c];
            result[1, c] = jacobian[1, c];
            result[2, c] = jacobian[2, c];

            Quaternion q = Quaternion.Euler(jacobian[3, c], jacobian[4, c], jacobian[5, c]);
            result[3, c] = q.x;
            result[4, c] = q.y;
            result[5, c] = q.z;
            result[6, c] = q.w;
        }
        return result;
    }

    // J* = (J.T*J)^-1*J.T
    public static ArticulationJacobian PsuedoInverse(ArticulationJacobian jacobian)
    {
        ArticulationJacobian jT = Transpose(jacobian);
        ArticulationJacobian jTj = Multiply(jT, jacobian);
        try
        {
            ArticulationJacobian jTj_inv = Inverse(jTj);
            ArticulationJacobian psuedoInverseJ = Multiply(jTj_inv, jT);
            Debug.Log("Inverse of jacobian was successful!");
            Print(jTj_inv);
            Print(psuedoInverseJ);
            return psuedoInverseJ;
        }
        catch (Exception e)
        {
            Debug.Log("Inverse failed: " + e.Message);
            return jT;
        }
    }

    public static ArticulationJacobian DampedLeastSquares(ArticulationJacobian jacobian, float lambda)
    {
        ArticulationJacobian jT = Transpose(jacobian);
        ArticulationJacobian jjT = Multiply(jacobian, jT);
        ArticulationJacobian identity = new ArticulationJacobian(jjT.rows, jjT.columns);
        AssignIdentity(ref identity);
        ArticulationJacobian inverseTerm = Inverse(Add(jjT, Multiply(identity, lambda * lambda)));
        ArticulationJacobian result = Multiply(jT, inverseTerm);
        return result;
    }

    public static ArticulationJacobian Inverse(ArticulationJacobian jacobian)
    {
        const float deltaE = 1e-8f;
        if (jacobian.rows != jacobian.columns)
            throw new Exception("Can't find inverse for non square rows != columns jacobian!");
        ArticulationJacobian jacobianInv = new ArticulationJacobian(jacobian.rows, jacobian.columns);
        // Initialize to identity
        for (int diagonal = 0; diagonal < jacobianInv.rows; diagonal++)
            jacobianInv[diagonal, diagonal] = 1.0f;

        for (int diagonal = 0; diagonal < jacobian.rows; diagonal++)
        {
            int maxRow = diagonal;
            float maxValue = Math.Abs(jacobian[diagonal, diagonal]);
            float currentValue;
            for (int row = diagonal + 1; row < jacobian.rows; row++)
            {
                currentValue = Math.Abs(jacobian[row, diagonal]);
                if (currentValue > maxValue)
                {
                    maxRow = row;
                    maxValue = currentValue;
                }
            }
            if (maxValue < deltaE)
            {
                Print(jacobian);
                throw new Exception("Jacobian is degenerate on row " + (diagonal + 1) + "!");
            }
            SwapRows(jacobian, diagonal, maxRow);
            SwapRows(jacobianInv, diagonal, maxRow);

            float inverseElement = 1 / jacobian[diagonal, diagonal];
            for (int col = diagonal; col < jacobian.columns; col++)
                jacobian[diagonal, col] *= inverseElement;
            for (int col = 0; col < jacobianInv.columns; col++)
                jacobianInv[diagonal, col] *= inverseElement;
            for (int row = 0; row < jacobian.rows; row++)
            {
                float value = jacobian[row, diagonal];
                if (row != diagonal)
                {
                    for (int column = diagonal; column < jacobian.columns; column++)
                        jacobian[row, column] -= value * jacobian[diagonal, column];
                    for (int column = 0; column < jacobianInv.columns; column++)
                        jacobianInv[row, column] -= value * jacobianInv[diagonal, column];

                }
            }

        }
        return jacobianInv;
    }

    public static void AssignIdentity(ref ArticulationJacobian jacobian)
    {
        for (int row = 0; row < jacobian.rows; row++)
        {
            for (int column = 0; column < jacobian.columns; column++)
                jacobian[row, column] = row != column ? 0.0f : 1.0f;
        }
    }

    public static ArticulationJacobian FillMatrix(int startRow, List<int> cols, ArticulationJacobian oldJ)
    {
        ArticulationJacobian minJ = new ArticulationJacobian(6, cols.Count);
        int row = -1;
        for (int r = startRow; r < startRow + 6; r++)
        {
            row += 1;
            int col = -1;
            foreach (int c in cols)
            {
                col += 1;
                minJ[row, col] = oldJ[r, c];
            }
        }
        return minJ;
    }

    public static float rotationError(Quaternion q1, Quaternion q2)
    {
        Quaternion q = q1 * Quaternion.Inverse(q2);
        float theta = Mathf.Clamp(Mathf.Abs(q.w), -1, 1); // avoid overflow
        float errRotation = 2 * Mathf.Acos(theta);
        return errRotation;
    }

    public static Vector3 quaternionToEuler(Quaternion q)
    {
        float sqw = q.w * q.w;
        float sqx = q.x * q.x;
        float sqy = q.y * q.y;
        float sqz = q.z * q.z;
        float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        float test = q.x * q.w - q.y * q.z;
        Vector3 v;

        if (test > 0.499 * unit)
        { // singularity at north pole
            v.x = 2 * Mathf.Atan2(q.x, q.w);
            v.y = Mathf.PI / 2;
            v.z = 0;
            return NormalizeAngle(v);
        }
        if (test < -0.499 * unit)
        { // singularity at south pole
            v.x = -2 * Mathf.Atan2(q.x, q.w);
            v.y = -Mathf.PI / 2;
            v.z = 0;
            return NormalizeAngle(v);
        }
        Quaternion q2 = new Quaternion(q.w, -q.x, -q.y, -q.z);
        v.x = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, sqx - sqy - sqz + sqw);
        v.y = Mathf.Asin(2 * test / unit);
        v.z = Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, -sqx + sqy - sqz + sqw);
        return NormalizeAngle(v);
    }

    public static Vector3 NormalizeAngle(Vector3 v)
    {
        v.x = WrapToPI(v.x);
        v.y = WrapToPI(v.y);
        v.z = WrapToPI(v.z);
        return v;
    }

    public static float WrapToPI(float angle)
    {
        angle = angle % (2 * Mathf.PI);
        if (angle < -Mathf.PI)
            angle += 2 * Mathf.PI;
        else if (angle > Mathf.PI)
            angle -= 2 * Mathf.PI;
        return angle;
    }

    public static void Set(ArticulationJacobian jacobian, int i, Vector3 position, Vector3 rotation)
    {
        jacobian[0, i] = position.x;
        jacobian[1, i] = position.y;
        jacobian[2, i] = position.z;
        jacobian[3, i] = rotation.x;
        jacobian[4, i] = rotation.y;
        jacobian[5, i] = rotation.z;
    }
}
